using System;
using UnityEngine;

public class AdvancedLookat : MonoBehaviour
{
    public Transform target;
    [Min(0)] public float speed = 180;
    
    // had to add this to fix objects not rotating when game view was closed (i.e. no Application.onBeforeRender)
    #if UNITY_EDITOR
    private void LateUpdate()
    {
        OnBeforeRender();
    }
    #endif

    private void OnEnable()
    {
        Application.onBeforeRender += OnBeforeRender;
    }

    private void OnDisable()
    {
        Application.onBeforeRender -= OnBeforeRender;
    }

    private void OnBeforeRender()
    {
        Quaternion lookingAt = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookingAt, speed * Time.deltaTime);
    }
}
