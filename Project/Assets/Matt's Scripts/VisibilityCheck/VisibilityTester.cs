using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public abstract class VisibilityTester : MonoBehaviour
{
    protected bool canSeeSomething;
    public LayerMask viewableLayers = ~0;
    public UnityEvent OnStartViewing;
    public UnityEvent OnStopViewing;

    public abstract void GetCullingMatrices(out Matrix4x4 view, out Matrix4x4 proj, out FrustumCulling.CullingFrustum frustum);
    public abstract Plane[] GetFrustumPlanes();
    
    // Update is called once per frame
    void Update()
    {
        uint visibleCount = 0; // how many visible this frame
        
        if (VisibilityMesh.s_objects.Count > 0)
        {
            GetCullingMatrices(out var view, out var proj, out var frustum);
            float4x4 viewBurst = view;
            
            foreach (var obj in VisibilityMesh.s_objects)
            {
                if ((obj.gameObject.layer & viewableLayers) == 0)
                    continue;
                
                if (!obj.mf.sharedMesh)
                    continue;

                if (obj.firstAppearanceOnly && obj.hasBeenSeen)
                    continue;

                if (FrustumCulling.burst_test_using_separating_axis_theorem(frustum, viewBurst, obj.transform.localToWorldMatrix, obj.mf.sharedMesh.bounds))
                {
                    visibleCount++;
                    obj.numViewers++;
                }
            }
        }
        
        if (VisibilitySphere.s_objects.Count > 0)
        {
            GetCullingMatrices(out var view, out var proj, out var frustum);

            //Plane[] planes = new Plane[6];
            //GeometryUtility.CalculateFrustumPlanes(proj * view, planes);
            Plane[] planes = GetFrustumPlanes();
            
            foreach (var obj in VisibilitySphere.s_objects)
            {
                if (obj.firstAppearanceOnly && obj.hasBeenSeen)
                    continue;

                if (FrustumCulling.test_sphere(planes, obj.transform.position, obj.radius))
                {
                    visibleCount++;
                    obj.numViewers++;
                }
            }
        }

        if (canSeeSomething)
        {
            if (visibleCount == 0)
            {
                canSeeSomething = false;
                OnStopViewing.Invoke();
            }
        }
        else if (visibleCount > 0)
        {
            canSeeSomething = true;
            OnStartViewing.Invoke();
        }
    }

    protected void DrawFrustumPlanes(in Plane[] planes, in Vector3 origin)
    {
        Gizmos.color = Color.red;
        DrawPlane(planes[1], origin);
        Gizmos.color = Color.green;
        DrawPlane(planes[2], origin);
        Gizmos.color = Color.blue;
        DrawPlane(planes[5], origin);
    }

    protected void DrawPlane(Plane p, Vector3 origin)
    {
        Gizmos.DrawRay(p.ClosestPointOnPlane(origin), p.normal);
    }
    
    protected void DrawFrustum(in Matrix4x4 view, in Matrix4x4 projection)
    {
        Matrix4x4 clip2world = (projection * view).inverse;
        
        Vector4[] corners =
        {
            new(-1, -1, -1, 1),
            new(1, -1, -1, 1),
            new(1, 1, -1, 1),
            new(-1, 1, -1, 1),

            new(-1, -1, 1, 1),
            new(1, -1, 1, 1),
            new(1, 1, 1, 1),
            new(-1, 1, 1, 1),
        };

        for (int i = 0; i < corners.Length; i++)
        {
            corners[i] = clip2world * corners[i];
            corners[i] /= corners[i].w;
        }

        Gizmos.DrawLine(corners[0], corners[1]);
        Gizmos.DrawLine(corners[1], corners[2]);
        Gizmos.DrawLine(corners[2], corners[3]);
        Gizmos.DrawLine(corners[3], corners[0]);
        
        Gizmos.DrawLine(corners[4], corners[5]);
        Gizmos.DrawLine(corners[5], corners[6]);
        Gizmos.DrawLine(corners[6], corners[7]);
        Gizmos.DrawLine(corners[7], corners[4]);
        
        Gizmos.DrawLine(corners[0], corners[4]);
        Gizmos.DrawLine(corners[1], corners[5]);
        Gizmos.DrawLine(corners[2], corners[6]);
        Gizmos.DrawLine(corners[3], corners[7]);
    }
    
    protected static void ExtractFrustumPlanes(in Matrix4x4 VP, in Plane[] planes)
    {
        // Left clipping plane
        planes[0] = new Plane(
            new Vector3(VP.m30 + VP.m00, VP.m31 + VP.m01, VP.m32 + VP.m02), 
            VP.m33 + VP.m03);
        // Right clipping plane
        planes[1] = new Plane(
            new Vector3(VP.m30 - VP.m00, VP.m31 - VP.m01, VP.m32 - VP.m02), 
            VP.m33 - VP.m03);
        // Top clipping plane
        planes[2] = new Plane(
            new Vector3(VP.m30 - VP.m10, VP.m31 - VP.m11, VP.m32 - VP.m12), 
            VP.m33 - VP.m13);
        // Bottom clipping plane
        planes[3] = new Plane(
            new Vector3(VP.m30 + VP.m10, VP.m31 + VP.m11, VP.m32 + VP.m12), 
            VP.m33 + VP.m13);
        // Near clipping plane
        planes[4] = new Plane(
            new Vector3(VP.m20, VP.m21, VP.m22), 
            VP.m23);
        // Far clipping plane
        planes[5] = new Plane(
            new Vector3(VP.m30 - VP.m20, VP.m31 - VP.m21, VP.m32 - VP.m22), 
            VP.m33 - VP.m23);
    }
    
    protected static void ExtractFrustumPlanesStereo(in Matrix4x4 leftVP, in Matrix4x4 rightVP, in Plane[] planes)
    {
        // Left clipping plane
        planes[0] = new Plane(
            new Vector3(leftVP.m30 + leftVP.m00, leftVP.m31 + leftVP.m01, leftVP.m32 + leftVP.m02), 
            leftVP.m33 + leftVP.m03);
        // Right clipping plane
        planes[1] = new Plane(
            new Vector3(rightVP.m30 - rightVP.m00, rightVP.m31 - rightVP.m01, rightVP.m32 - rightVP.m02), 
            rightVP.m33 - rightVP.m03);
        // Top clipping plane
        planes[2] = new Plane(
            new Vector3(leftVP.m30 - leftVP.m10, leftVP.m31 - leftVP.m11, leftVP.m32 - leftVP.m12), 
            leftVP.m33 - leftVP.m13);
        // Bottom clipping plane
        planes[3] = new Plane(
            new Vector3(leftVP.m30 + leftVP.m10, leftVP.m31 + leftVP.m11, leftVP.m32 + leftVP.m12), 
            leftVP.m33 + leftVP.m13);
        // Near clipping plane
        planes[4] = new Plane(
            new Vector3(leftVP.m20, leftVP.m21, leftVP.m22), 
            leftVP.m23);
        // Far clipping plane
        planes[5] = new Plane(
            new Vector3(leftVP.m30 - leftVP.m20, leftVP.m31 - leftVP.m21, leftVP.m32 - leftVP.m22), 
            leftVP.m33 - leftVP.m23);
    }
}
