using BNG;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using WSWhitehouse.TagSelector;

[RequireComponent(typeof(SnapZone))]
public class SnapZoneEvent: MonoBehaviour
{
    [Serializable]
    public class TagPairing
    {
        [TagSelector(UseDefaultTagFieldDrawer = false)] public string tagName;
        public UnityEvent OnObjectPlaced;
        public UnityEvent OnObjectRemoved;
    }
    
    [Serializable]
    public class GrabbalePairing
    {
        public Grabbable target;
        public UnityEvent OnObjectPlaced;
        public UnityEvent OnObjectRemoved;
    }
    
    public List<TagPairing> tagPairings;
    public List<GrabbalePairing> objectPairing;
    private SnapZone snapZone;

    private void Awake()
    {
        snapZone = GetComponent<SnapZone>();
    }

    public void OnEnable()
    {
        snapZone.OnSnapEvent.AddListener(PlaceObject);
        snapZone.OnDetachEvent.AddListener(RemoveObject);
    }

    public void OnDisable()
    {
        snapZone.OnSnapEvent.RemoveListener(PlaceObject);
        snapZone.OnDetachEvent.RemoveListener(RemoveObject);
    }

    private void PlaceObject(Grabbable grabbable)
    {
        foreach(var pairing in tagPairings)
        {
            if (grabbable.CompareTag(pairing.tagName))
            {
                pairing.OnObjectPlaced.Invoke();
                break;
            }
        }
        
        foreach (var pairing in objectPairing)
        {
            if (grabbable == pairing.target)
            {
                pairing.OnObjectPlaced.Invoke();
                break;
            }
        }
    }

    private void RemoveObject(Grabbable grabbable)
    {
        foreach(var pairing in tagPairings)
        {
            if (grabbable.CompareTag(pairing.tagName))
            {
                pairing.OnObjectRemoved.Invoke();
                break;
            }
        }

        foreach (var pairing in objectPairing)
        {
            if (grabbable == pairing.target)
            {
                pairing.OnObjectRemoved.Invoke();
                break;
            }
        }
    }
}
