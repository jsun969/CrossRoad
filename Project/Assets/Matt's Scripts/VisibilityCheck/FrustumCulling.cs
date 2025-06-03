using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.XR;

[BurstCompile]
public static class FrustumCulling
{
    public struct CullingFrustum
    {
        public float near_right;
        public float near_top;
        public float near_plane;
        public float far_plane;
    }

    public struct OBB
    {
        public Vector3 center;
        public Vector3 extents;
        public Vector3[] axes; // Should be 3 elements
    }
    
    public struct OBB_burst
    {
        public float3 center;
        public float3 extents;
        public float3x3 axes;
    }

    public static CullingFrustum CalculateFrustum(float fov, float aspect, float near, float far)
    {
        CullingFrustum frustum = new();
        
        float tan_fov = Mathf.Tan(0.5f * fov * Mathf.Deg2Rad);
        frustum.near_right = aspect * near * tan_fov;
        frustum.near_top = near * tan_fov;
        frustum.near_plane = -near;
        frustum.far_plane = -far;

        return frustum;
    }

    public static CullingFrustum CalculateFrustum(in Camera c)
    {
        Vector4 PerspectiveMul(in Matrix4x4 m, in Vector4 v)
        {
            var t = m * v;
            return t *= 1f / t.w;
        }
        
        CullingFrustum frustum = new();

        if (c.stereoEnabled && XRSettings.enabled)
        {
            Matrix4x4 clip2view = c.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right).inverse;
            var c1 = PerspectiveMul(clip2view, new(-1, -1, -1, 1));
            var c2 = PerspectiveMul(clip2view, new(1, -1, -1, 1));
            var c3 = PerspectiveMul(clip2view, new(1, 1, -1, 1));
            var c4 = PerspectiveMul(clip2view, new(-1, 1, -1, 1));
            
            frustum.near_right = Mathf.Max(Mathf.Max(c1.x, c2.x), Mathf.Max(c3.x, c4.x));
            frustum.near_top = Mathf.Max(Mathf.Max(c1.y, c2.y), Mathf.Max(c3.y, c4.y));
        }
        else
        {
            Matrix4x4 clip2view = c.projectionMatrix.inverse;
            var c1 = PerspectiveMul(clip2view, new(-1, -1, -1, 1));
            var c2 = PerspectiveMul(clip2view, new(1, -1, -1, 1));
            var c3 = PerspectiveMul(clip2view, new(1, 1, -1, 1));
            var c4 = PerspectiveMul(clip2view, new(-1, 1, -1, 1));
            
            frustum.near_right = Mathf.Max(Mathf.Max(c1.x, c2.x), Mathf.Max(c3.x, c4.x));
            frustum.near_top = Mathf.Max(Mathf.Max(c1.y, c2.y), Mathf.Max(c3.y, c4.y));
        }
        
        frustum.near_plane = -c.nearClipPlane;
        frustum.far_plane = -c.farClipPlane;

        return frustum;
    }
    
    public static bool ShouldFrustumCull(Camera c, MeshFilter meshFilter)
    {
        float tan_fov = Mathf.Tan(0.5f * c.fieldOfView * Mathf.Deg2Rad);

        CullingFrustum frustum = CalculateFrustum(c);

        return burst_test_using_separating_axis_theorem(frustum, c.worldToCameraMatrix, meshFilter.transform.localToWorldMatrix, meshFilter.sharedMesh.bounds);
        //return test_using_separating_axis_theorem(frustum, c.worldToCameraMatrix * transform, bounds);
    }
    
    public static bool test_using_separating_axis_theorem(in CullingFrustum frustum, in Matrix4x4 vs_transform, in Bounds aabb)
    {
        // Near, far
        float z_near = frustum.near_plane;
        float z_far = frustum.far_plane;
        // half width, half height
        float x_near = frustum.near_right;
        float y_near = frustum.near_top;

        // So first thing we need to do is obtain the normal directions of our OBB by transforming 4 of our AABB vertices
        Vector3[] corners = {
            new (aabb.min.x, aabb.min.y, aabb.min.z),
            new (aabb.max.x, aabb.min.y, aabb.min.z),
            new (aabb.min.x, aabb.max.y, aabb.min.z),
            new (aabb.min.x, aabb.min.y, aabb.max.z),
        };

        // Transform corners
        // This only translates to our OBB if our transform is affine
        for (int corner_idx = 0; corner_idx < 4; corner_idx++) {
            corners[corner_idx] = vs_transform.MultiplyPoint3x4(corners[corner_idx]);
        }

        OBB obb = new ();
        obb.axes = new []
        {
            corners[1] - corners[0],
            corners[2] - corners[0],
            corners[3] - corners[0]
        };
        
        obb.center = corners[0] + 0.5f * (obb.axes[0] + obb.axes[1] + obb.axes[2]);
        obb.extents = new Vector3(obb.axes[0].magnitude, obb.axes[1].magnitude, obb.axes[2].magnitude);
        obb.axes[0] = obb.axes[0] / obb.extents.x;
        obb.axes[1] = obb.axes[1] / obb.extents.y;
        obb.axes[2] = obb.axes[2] / obb.extents.z;
        obb.extents *= 0.5f;
        
        {
            // Projected center of our OBB
            float MoC = obb.center.z;
            // Projected size of OBB
            float radius = 0.0f;
            for (int i = 0; i < 3; i++) {
                // Vector3.Dot(M, axes[i]) == axes[i].z;
                radius += Mathf.Abs(obb.axes[i].z) * obb.extents[i];
            }
            float obb_min = MoC - radius;
            float obb_max = MoC + radius;

            float tau_0 = z_far; // Since z is negative, far is smaller than near
            float tau_1 = z_near;

            if (obb_min > tau_1 || obb_max < tau_0) {
                return false;
            }
        }

        {
            Vector3[] M = {
                new ( z_near, 0.0f, x_near ), // Left Plane
                new ( -z_near, 0.0f, x_near ), // Right plane
                new ( 0.0f, -z_near, y_near ), // Top plane
                new ( 0.0f, z_near, y_near ), // Bottom plane
            };
            
            for (int m = 0; m < 4; m++) {
                float MoX = Mathf.Abs(M[m].x);
                float MoY = Mathf.Abs(M[m].y);
                float MoZ = M[m].z;
                float MoC = Vector3.Dot(M[m], obb.center);

                float obb_radius = 0.0f;
                for (int i = 0; i < 3; i++) {
                    obb_radius += Mathf.Abs(Vector3.Dot(M[m], obb.axes[i])) * obb.extents[i];
                }
                
                float obb_min = MoC - obb_radius;
                float obb_max = MoC + obb_radius;

                float p = x_near * MoX + y_near * MoY;

                float tau_0 = z_near * MoZ - p;
                float tau_1 = z_near * MoZ + p;

                if (tau_0 < 0.0f) {
                    tau_0 *= z_far / z_near;
                }
                if (tau_1 > 0.0f) {
                    tau_1 *= z_far / z_near;
                }

                if (obb_min > tau_1 || obb_max < tau_0) {
                    return false;
                }
            }
        }

        // OBB Axes
        {
            for (int m = 0; m < 3; m++) {
                float MoX = Mathf.Abs(obb.axes[m].x);
                float MoY = Mathf.Abs(obb.axes[m].y);
                float MoZ = obb.axes[m].z;
                float MoC = Vector3.Dot(obb.axes[m], obb.center);

                float obb_radius = obb.extents[m];

                float obb_min = MoC - obb_radius;
                float obb_max = MoC + obb_radius;

                // Frustum projection
                float p = x_near * MoX + y_near * MoY;
                float tau_0 = z_near * MoZ - p;
                float tau_1 = z_near * MoZ + p;
                if (tau_0 < 0.0f) {
                    tau_0 *= z_far / z_near;
                }
                if (tau_1 > 0.0f) {
                    tau_1 *= z_far / z_near;
                }

                if (obb_min > tau_1 || obb_max < tau_0) {
                    return false;
                }
            }
        }

        // Now let's perform each of the Vector3.Cross products between the edges
        // First R x A_i
        {
            for (int m = 0; m < 3; m++) {
                Vector3 M = new( 0.0f, -obb.axes[m].z, obb.axes[m].y );
                float MoX = 0.0f;
                float MoY = Mathf.Abs(M.y);
                float MoZ = M.z;
                float MoC = M.y * obb.center.y + M.z * obb.center.z;

                float obb_radius = 0.0f;
                for (int i = 0; i < 3; i++) {
                    obb_radius += Mathf.Abs(Vector3.Dot(M, obb.axes[i])) * obb.extents[i];
                }

                float obb_min = MoC - obb_radius;
                float obb_max = MoC + obb_radius;

                // Frustum projection
                float p = x_near * MoX + y_near * MoY;
                float tau_0 = z_near * MoZ - p;
                float tau_1 = z_near * MoZ + p;
                if (tau_0 < 0.0f) {
                    tau_0 *= z_far / z_near;
                }
                if (tau_1 > 0.0f) {
                    tau_1 *= z_far / z_near;
                }

                if (obb_min > tau_1 || obb_max < tau_0) {
                    return false;
                }
            }
        }

        // U x A_i
        {
            for (int m = 0; m < 3; m++) {
                Vector3 M = new Vector3(obb.axes[m].z, 0.0f, -obb.axes[m].x);
                float MoX = Mathf.Abs(M.x);
                float MoY = 0.0f;
                float MoZ = M.z;
                float MoC = M.x * obb.center.x + M.z * obb.center.z;

                float obb_radius = 0.0f;
                for (int i = 0; i < 3; i++) {
                    obb_radius += Mathf.Abs(Vector3.Dot(M, obb.axes[i])) * obb.extents[i];
                }

                float obb_min = MoC - obb_radius;
                float obb_max = MoC + obb_radius;

                // Frustum projection
                float p = x_near * MoX + y_near * MoY;
                float tau_0 = z_near * MoZ - p;
                float tau_1 = z_near * MoZ + p;
                if (tau_0 < 0.0f) {
                    tau_0 *= z_far / z_near;
                }
                if (tau_1 > 0.0f) {
                    tau_1 *= z_far / z_near;
                }

                if (obb_min > tau_1 || obb_max < tau_0) {
                    return false;
                }
            }
        }

        // Frustum Edges X Ai
        {
            for (int obb_edge_idx = 0; obb_edge_idx < 3; obb_edge_idx++) {
                Vector3[] M = {
                    Vector3.Cross(new Vector3(-x_near, 0.0f, z_near ), obb.axes[obb_edge_idx]), // Left Plane
                    Vector3.Cross(new Vector3( x_near, 0.0f, z_near ), obb.axes[obb_edge_idx]), // Right plane
                    Vector3.Cross(new Vector3( 0.0f, y_near, z_near ), obb.axes[obb_edge_idx]), // Top plane
                    Vector3.Cross(new Vector3( 0.0f, -y_near, z_near ), obb.axes[obb_edge_idx]) // Bottom plane
                };

                for (int m = 0; m < 4; m++) {
                    float MoX = Mathf.Abs(M[m].x);
                    float MoY = Mathf.Abs(M[m].y);
                    float MoZ = M[m].z;

                    const float epsilon = 1e-4f;
                    if (MoX < epsilon && MoY < epsilon && Mathf.Abs(MoZ) < epsilon) continue;

                    float MoC = Vector3.Dot(M[m], obb.center);

                    float obb_radius = 0.0f;
                    for (int i = 0; i < 3; i++) {
                        obb_radius += Mathf.Abs(Vector3.Dot(M[m], obb.axes[i])) * obb.extents[i];
                    }

                    float obb_min = MoC - obb_radius;
                    float obb_max = MoC + obb_radius;

                    // Frustum projection
                    float p = x_near * MoX + y_near * MoY;
                    float tau_0 = z_near * MoZ - p;
                    float tau_1 = z_near * MoZ + p;
                    if (tau_0 < 0.0f) {
                        tau_0 *= z_far / z_near;
                    }
                    if (tau_1 > 0.0f) {
                        tau_1 *= z_far / z_near;
                    }

                    if (obb_min > tau_1 || obb_max < tau_0) {
                        return false;
                    }
                }
            }
        }

        // No intersections detected
        return true;
    }
    
    [BurstCompile]
    public static bool burst_test_using_separating_axis_theorem(in CullingFrustum frustum, in float4x4 viewMatrix, in float4x4 modelMatrix, in Bounds aabb)
    {
        // Near, far
        float z_near = frustum.near_plane; 
        float z_far = frustum.far_plane;
        // half width, half height
        float x_near = frustum.near_right;
        float y_near = frustum.near_top;

        // So first thing we need to do is obtain the normal directions of our OBB by transforming 4 of our AABB vertices
        float3x4 corners = new (
            new (aabb.min.x, aabb.min.y, aabb.min.z),
            new (aabb.max.x, aabb.min.y, aabb.min.z),
            new (aabb.min.x, aabb.max.y, aabb.min.z),
            new (aabb.min.x, aabb.min.y, aabb.max.z));
        

        // Transform corners
        // This only translates to our OBB if our transform is affine
        for (int corner_idx = 0; corner_idx < 4; corner_idx++)
        {
            corners[corner_idx] = math.transform(viewMatrix, math.transform(modelMatrix, corners[corner_idx]));
        }

        OBB_burst obb = new ();
        obb.axes = new (
            corners[1].xyz - corners[0].xyz,
            corners[2].xyz - corners[0].xyz,
            corners[3].xyz - corners[0].xyz 
        );
        
        obb.center = corners[0].xyz + 0.5f * (obb.axes[0] + obb.axes[1] + obb.axes[2]);
        obb.extents = new float3(math.length(obb.axes[0]), math.length(obb.axes[1]), math.length(obb.axes[2]));
        obb.axes[0] = obb.axes[0] / obb.extents.x;
        obb.axes[1] = obb.axes[1] / obb.extents.y;
        obb.axes[2] = obb.axes[2] / obb.extents.z;
        obb.extents *= 0.5f;
        
        {
            // Projected center of our OBB
            float MoC = obb.center.z;
            // Projected size of OBB
            float radius = 0.0f;
            for (int i = 0; i < 3; i++) {
                // Vector3.Dot(M, axes[i]) == axes[i].z;
                radius += math.abs(obb.axes[i].z) * obb.extents[i];
            }
            float obb_min = MoC - radius;
            float obb_max = MoC + radius;

            float tau_0 = z_far; // Since z is negative, far is smaller than near
            float tau_1 = z_near;

            if (obb_min > tau_1 || obb_max < tau_0) {
                return false;
            }
        }

        {
            float3x4 M = new float3x4(
                new (z_near, 0.0f, x_near), // Left Plane
                new(-z_near, 0.0f, x_near), // Right plane
                new (0.0f, -z_near, y_near), // Top plane
                new (0.0f, z_near, y_near) // Bottom plane
            );
            
            for (int m = 0; m < 4; m++) {
                float MoX = math.abs(M[m].x);
                float MoY = math.abs(M[m].y);
                float MoZ = M[m].z;
                float MoC = math.dot(M[m], obb.center);

                float obb_radius = 0.0f;
                for (int i = 0; i < 3; i++) {
                    obb_radius += math.abs(math.dot(M[m], obb.axes[i])) * obb.extents[i];
                }
                
                float obb_min = MoC - obb_radius;
                float obb_max = MoC + obb_radius;

                float p = x_near * MoX + y_near * MoY;

                float tau_0 = z_near * MoZ - p;
                float tau_1 = z_near * MoZ + p;

                if (tau_0 < 0.0f) {
                    tau_0 *= z_far / z_near;
                }
                if (tau_1 > 0.0f) {
                    tau_1 *= z_far / z_near;
                }

                if (obb_min > tau_1 || obb_max < tau_0) {
                    return false;
                }
            }
        }

        // OBB Axes
        {
            for (int m = 0; m < 3; m++) {
                float MoX = math.abs(obb.axes[m].x);
                float MoY = math.abs(obb.axes[m].y);
                float MoZ = obb.axes[m].z;
                float MoC = math.dot(obb.axes[m], obb.center);

                float obb_radius = obb.extents[m];

                float obb_min = MoC - obb_radius;
                float obb_max = MoC + obb_radius;

                // Frustum projection
                float p = x_near * MoX + y_near * MoY;
                float tau_0 = z_near * MoZ - p;
                float tau_1 = z_near * MoZ + p;
                if (tau_0 < 0.0f) {
                    tau_0 *= z_far / z_near;
                }
                if (tau_1 > 0.0f) {
                    tau_1 *= z_far / z_near;
                }

                if (obb_min > tau_1 || obb_max < tau_0) {
                    return false;
                }
            }
        }

        // Now let's perform each of the Vector3.Cross products between the edges
        // First R x A_i
        {
            for (int m = 0; m < 3; m++) {
                float3 M = new ( 0.0f, -obb.axes[m].z, obb.axes[m].y );
                float MoX = 0.0f;
                float MoY = math.abs(M.y);
                float MoZ = M.z;
                float MoC = M.y * obb.center.y + M.z * obb.center.z;

                float obb_radius = 0.0f;
                for (int i = 0; i < 3; i++) {
                    obb_radius += math.abs(math.dot(M, obb.axes[i])) * obb.extents[i];
                }

                float obb_min = MoC - obb_radius;
                float obb_max = MoC + obb_radius;

                // Frustum projection
                float p = x_near * MoX + y_near * MoY;
                float tau_0 = z_near * MoZ - p;
                float tau_1 = z_near * MoZ + p;
                if (tau_0 < 0.0f) {
                    tau_0 *= z_far / z_near;
                }
                if (tau_1 > 0.0f) {
                    tau_1 *= z_far / z_near;
                }

                if (obb_min > tau_1 || obb_max < tau_0) {
                    return false;
                }
            }
        }

        // U x A_i
        {
            for (int m = 0; m < 3; m++)
            {
                float3 M = new (obb.axes[m].z, 0.0f, -obb.axes[m].x);
                float MoX = math.abs(M.x);
                float MoY = 0.0f;
                float MoZ = M.z;
                float MoC = M.x * obb.center.x + M.z * obb.center.z;

                float obb_radius = 0.0f;
                for (int i = 0; i < 3; i++)
                {
                    obb_radius += math.abs(math.dot(M, obb.axes[i])) * obb.extents[i];
                }

                float obb_min = MoC - obb_radius;
                float obb_max = MoC + obb_radius;

                // Frustum projection
                float p = x_near * MoX + y_near * MoY;
                float tau_0 = z_near * MoZ - p;
                float tau_1 = z_near * MoZ + p;
                if (tau_0 < 0.0f)
                {
                    tau_0 *= z_far / z_near;
                }

                if (tau_1 > 0.0f)
                {
                    tau_1 *= z_far / z_near;
                }

                if (obb_min > tau_1 || obb_max < tau_0)
                {
                    return false;
                }
            }
        }
        
        // Frustum Edges X Ai
        {
            for (int obb_edge_idx = 0; obb_edge_idx < 3; obb_edge_idx++)
            {
                float3x4 M = new(
                    math.cross(new float3(-x_near, 0.0f, z_near), obb.axes[obb_edge_idx]), // Left Plane
                    math.cross(new float3(x_near, 0.0f, z_near), obb.axes[obb_edge_idx]), // Right plane
                    math.cross(new float3(0.0f, y_near, z_near), obb.axes[obb_edge_idx]), // Top plane
                    math.cross(new float3(0.0f, -y_near, z_near), obb.axes[obb_edge_idx]) // Bottom plane
                );

                for (int m = 0; m < 4; m++) {
                    float MoX = math.abs(M[m].x);
                    float MoY = math.abs(M[m].y);
                    float MoZ = M[m].z;

                    const float epsilon = 1e-4f;
                    if (MoX < epsilon && MoY < epsilon && math.abs(MoZ) < epsilon) continue;

                    float MoC = math.dot(M[m], obb.center);

                    float obb_radius = 0.0f;
                    for (int i = 0; i < 3; i++) {
                        obb_radius += math.abs(math.dot(M[m], obb.axes[i])) * obb.extents[i];
                    }

                    float obb_min = MoC - obb_radius;
                    float obb_max = MoC + obb_radius;

                    // Frustum projection
                    float p = x_near * MoX + y_near * MoY;
                    float tau_0 = z_near * MoZ - p;
                    float tau_1 = z_near * MoZ + p;
                    if (tau_0 < 0.0f) {
                        tau_0 *= z_far / z_near;
                    }
                    if (tau_1 > 0.0f) {
                        tau_1 *= z_far / z_near;
                    }

                    if (obb_min > tau_1 || obb_max < tau_0) {
                        return false;
                    }
                }
            }
        }

        // No intersections detected
        return true;
    }

    public static bool test_sphere(Plane[] planes, in Vector3 sphereOrigin, in float sphereRadius)
    {
        /*Debug.Log($"Left {planes[0].GetDistanceToPoint(sphereOrigin) < sphereRadius}\n" +
                  $"Right {planes[1].GetDistanceToPoint(sphereOrigin) < sphereRadius}\n" +
                  $"Top {planes[2].GetDistanceToPoint(sphereOrigin) < sphereRadius}\n" +
                  $"Bottom {planes[3].GetDistanceToPoint(sphereOrigin) < sphereRadius}\n" +
                  $"Near {planes[4].GetDistanceToPoint(sphereOrigin) < sphereRadius}\n" +
                  $"Far {planes[5].GetDistanceToPoint(sphereOrigin) < sphereRadius}\n");*/
        
        for (int i=0; i < planes.Length; i++)
        {
            if (planes[i].GetDistanceToPoint(sphereOrigin) < -sphereRadius)
                return false;
        }

        return true;
    }
}