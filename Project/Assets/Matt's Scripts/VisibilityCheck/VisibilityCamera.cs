using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;

[AddComponentMenu("Matt's Scripts/Visibility/Visibility Camera")]
[RequireComponent(typeof(Camera))]
public class VisibilityCamera : VisibilityTester
{
    private Camera viewCamera;
    private Plane[] planes = new Plane[6];

    private void Awake()
    {
        viewCamera = GetComponent<Camera>();
    }

    public override void GetCullingMatrices(out Matrix4x4 view, out Matrix4x4 proj, out FrustumCulling.CullingFrustum frustum)
    {
        view = viewCamera.worldToCameraMatrix;
        proj = viewCamera.projectionMatrix;
        frustum = FrustumCulling.CalculateFrustum(viewCamera);
    }

    public override Plane[] GetFrustumPlanes()
    {
        if (viewCamera.stereoEnabled && XRSettings.enabled)
        {
            var left = viewCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left) *
                       viewCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
            var right = viewCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right) *
                       viewCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Right);

            int leftPlaneIndex = 0;
            GeometryUtility.CalculateFrustumPlanes(left, planes);
            Plane leftPlane = planes[leftPlaneIndex];
            GeometryUtility.CalculateFrustumPlanes(right, planes);
            planes[leftPlaneIndex] = leftPlane;
            //ExtractFrustumPlanesStereo(left, right, planes);
        }
        else
        {
            var viewProj = viewCamera.projectionMatrix * viewCamera.worldToCameraMatrix;
            GeometryUtility.CalculateFrustumPlanes(viewCamera.projectionMatrix * viewCamera.worldToCameraMatrix, planes);
            //ExtractFrustumPlanes(viewProj, planes);
        }
        
        return planes;
    }

    private void OnValidate()
    {
        if (!Application.isPlaying) 
            viewCamera = GetComponent<Camera>();
    }
    
    protected void OnDrawGizmos()
    {
        if (!viewCamera)
            return;
        
        if (Application.isPlaying)
        {
            Gizmos.color = canSeeSomething ? Color.green : Color.red;
        }

        if (viewCamera.stereoEnabled && XRSettings.enabled)
        {
            DrawFrustum(viewCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Left), viewCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left));
            DrawFrustum(viewCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Right), viewCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right));
        }
        else
            DrawFrustum(viewCamera.worldToCameraMatrix, viewCamera.projectionMatrix);
    }
}
