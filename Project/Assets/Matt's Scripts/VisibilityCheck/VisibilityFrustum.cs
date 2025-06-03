using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Matt's Scripts/Visibility/Visibility Frustum")]
public class VisibilityFrustum : VisibilityTester
{
    [Min(0)] public float near = 0.01f;
    [Min(0)] public float far = 10f;
    [Range(0.5f, 189)] public float angle = 15;
    
    private Plane[] planes = new Plane[6];

    private Matrix4x4 GetViewMatrix()
    {
        transform.GetPositionAndRotation(out var p, out var r);
        return Matrix4x4.TRS(p, r, new Vector3(1, 1, -1)).inverse;
    }

    private Matrix4x4 GetProjectionMatrix()
    {
        return Matrix4x4.Perspective(angle, 1, near, far);
    }
    
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = canSeeSomething ? Color.green : Color.red;
        }

        DrawFrustum(GetViewMatrix(), GetProjectionMatrix());
    }

    public override void GetCullingMatrices(out Matrix4x4 view, out Matrix4x4 proj, out FrustumCulling.CullingFrustum frustum)
    {
        view = GetViewMatrix();
        proj = GetProjectionMatrix();
        frustum = FrustumCulling.CalculateFrustum(angle, 1, near, far);
    }

    public override Plane[] GetFrustumPlanes()
    {
        var view = GetViewMatrix();
        var proj = GetProjectionMatrix();
        GeometryUtility.CalculateFrustumPlanes(proj * view, planes);
        //ExtractFrustumPlanes(proj * view, planes);
        return planes;
    }
}
