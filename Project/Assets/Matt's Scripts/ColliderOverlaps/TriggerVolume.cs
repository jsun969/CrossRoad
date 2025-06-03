using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using WSWhitehouse.TagSelector;

[RequireComponent(typeof(Collider))]
public class TriggerVolume : MonoBehaviour
{
    [Serializable]
    public class TagEvent
    {
        [TagSelector] public string tagName;
        public bool oneAtATime;
        public UnityEvent OnEnter;
        public UnityEvent OnExit;
        [NonSerialized] public int activeCount;
    }
    
    [Serializable]
    public class RBEvent
    {
        public Rigidbody rigidbody;
        public UnityEvent OnEnter;
        public UnityEvent OnExit;
    }

    private class Overlap
    {
        public Rigidbody rb;
        public int count;
        public int index;

        public Overlap(Rigidbody rb, int count, int index)
        {
            this.rb = rb;
            this.count = count;
            this.index = index;
        }
    }
    
    public bool allowMutlipleOverlaps = true;
    public List<TagEvent> tagEvents = new ();
    public List<RBEvent> rbEvents = new ();
    
    private Rigidbody currentOverlap;
    private List<Overlap> overlapTags = new();
    private List<Overlap> overlapRBs = new();
    
    private void OnTriggerEnter(Collider other)
    {
        if (!allowMutlipleOverlaps && currentOverlap != null)
        {
            return;
        }
        
        // skip if rigidbody is already overlapping
        {
            bool alreadyOverlapping = false;
            
            int index = GetOverlapIndex(overlapRBs, other.attachedRigidbody);
            if (index >= 0)
            {
                overlapRBs[index].count++;
                alreadyOverlapping = true;
            }
            
            index = GetOverlapIndex(overlapTags, other.attachedRigidbody);
            if (index >= 0)
            {
                overlapTags[index].count++;
                alreadyOverlapping = true;
            }
            
            if (alreadyOverlapping)
                return;
        }
        
        for (int i = 0; i<tagEvents.Count; i++)
        {
            if (other.attachedRigidbody.CompareTag(tagEvents[i].tagName))
            {
                overlapTags.Add(new (other.attachedRigidbody, 1, i));
                TagEnter(i, other.attachedRigidbody);
                break;
            }
        }

        for (int i = 0; i<rbEvents.Count; i++)
        {
            if (rbEvents[i].rigidbody == other.attachedRigidbody)
            {
                overlapRBs.Add(new (other.attachedRigidbody, 1, i));
                currentOverlap = other.attachedRigidbody;
                rbEvents[i].OnEnter.Invoke();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!allowMutlipleOverlaps && currentOverlap != other.attachedRigidbody)
        {
            return;
        }
        
        int index = GetOverlapIndex(overlapRBs, other.attachedRigidbody);
        if (index >= 0)
        {
            overlapRBs[index].count--;
            
            if (overlapRBs[index].count == 0)
            {
                int eventIndex = overlapRBs[index].index;
                overlapRBs.RemoveAt(index);
                currentOverlap = null;
                rbEvents[eventIndex].OnExit.Invoke();
            }
        }
        
        index = GetOverlapIndex(overlapTags, other.attachedRigidbody);
        if (index >= 0)
        {
            overlapTags[index].count--;
            
            if (overlapTags[index].count == 0)
            {
                int eventIndex = overlapTags[index].index;
                overlapTags.RemoveAt(index);
                TagExit(eventIndex);
            }
        }
        
    }

    private void Update()
    {
        if (overlapTags.Count > 0)
        { 
            // check one object per frame if has been deleted or disabled 
            int rbIndex = Time.frameCount % overlapTags.Count;
            if (!overlapTags[rbIndex].rb || !overlapTags[rbIndex].rb.gameObject.activeInHierarchy)
            {
                int tagIndex = overlapTags[rbIndex].index;
                overlapTags.RemoveAt(rbIndex);
                TagExit(tagIndex);
            }
        }
    }

    private void TagEnter(int index, Rigidbody rb)
    {
        tagEvents[index].activeCount++;
        if (tagEvents[index].activeCount == 1 || !tagEvents[index].oneAtATime)
        {
            currentOverlap = rb;
            tagEvents[index].OnEnter.Invoke();
        }
    }
    
    private void TagExit(int index)
    {
        tagEvents[index].activeCount--;
        if (tagEvents[index].activeCount == 0 || !tagEvents[index].oneAtATime)
        {
            currentOverlap = null;
            tagEvents[index].OnExit.Invoke();
        }
    }
    
    private int GetOverlapIndex(in List<Overlap> overlaps, Rigidbody rb)
    {
        for (int j = 0; j < overlaps.Count; j++)
        {
            if (overlaps[j].rb == rb)
            {
                return j;
            }
        }

        return -1;
    }
}
