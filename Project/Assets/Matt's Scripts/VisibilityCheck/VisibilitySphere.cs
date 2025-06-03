using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Matt's Scripts/Visibility/Visibility Sphere")]
public class VisibilitySphere : VisibilityTestedObject
{
    public static List<VisibilitySphere> s_objects = new();
    
    [Min(0)] public float radius;
    
    private void OnEnable()
    {
        s_objects.Add(this);
    }

    private void OnDisable()
    {
        s_objects.Remove(this);
    }
    
    private void OnDrawGizmos()
    {
        if (radius <= 0)
            return;
        
        if (Application.isPlaying)
        {
            Gizmos.color = isVisible ? Color.green : Color.red;
        }

        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
