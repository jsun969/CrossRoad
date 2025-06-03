using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Matt's Scripts/Visibility/Visibility Mesh")]
[RequireComponent(typeof(MeshFilter))]
public class VisibilityMesh : VisibilityTestedObject
{
    public static List<VisibilityMesh> s_objects = new();

    [NonSerialized] public MeshFilter mf;
    
    private void Awake()
    {
        mf = GetComponent<MeshFilter>();
    }

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
        if (mf == null || mf.sharedMesh == null)
            return;
        
        if (Application.isPlaying)
        {
            Gizmos.color = isVisible ? Color.green : Color.red;
        }

        var bounds = mf.sharedMesh.bounds;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(bounds.center, 2 * bounds.extents);
    }

    private void OnValidate()
    {
        if (!Application.isPlaying) 
            mf = GetComponent<MeshFilter>();
    }
}
