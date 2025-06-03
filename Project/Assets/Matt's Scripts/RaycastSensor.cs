using System;
using UnityEngine;
using UnityEngine.Events;

public class RaycastSensor : MonoBehaviour
{
    public LayerMask allowedLayers = ~0;
    public float length = 3f;
    public bool bidirectional = true;
    private bool canSeeSomething;
    private RaycastHit hit = new RaycastHit();

    public UnityEvent OnSensorTrigger;
    public UnityEvent OnSensorReset;

    private void Update()
    {
        var origin = transform.position;
        var direction = transform.forward;
        
        bool seeThisFrame = Physics.Raycast(origin, direction, out hit, length, allowedLayers);
        
        if (!seeThisFrame && bidirectional)
            seeThisFrame = Physics.Raycast(origin + direction * length, -direction, out hit, length, allowedLayers);

        if (seeThisFrame != canSeeSomething)
        {
            canSeeSomething = seeThisFrame;
            
            if (canSeeSomething)
                OnSensorTrigger.Invoke();
            else
                OnSensorReset.Invoke();
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = canSeeSomething ? Color.green : Color.red;
        }

        if (canSeeSomething)
        {
            Gizmos.DrawLine(transform.position, hit.point);
        }
        else
        {
            Gizmos.DrawRay(transform.position, transform.forward * length);
        }
    }
}
