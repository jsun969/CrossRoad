using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using WSWhitehouse.TagSelector;

public class TriggerObject : MonoBehaviour
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
    public class ColliderEvent
    {
        public Collider triggerCollider;
        public UnityEvent OnEnter;
        public UnityEvent OnExit;
    }

    private class Overlap
    {
        public Collider trigger;
        public int count;
        public int index;

        public Overlap(Collider trigger, int count, int index)
        {
            this.trigger = trigger;
            this.count = count;
            this.index = index;
        }
    }

    public bool allowMutlipleOverlaps = true;
    public List<TagEvent> tagEvents = new ();
    public List<ColliderEvent> colliderEvents = new ();

    private Collider currentOverlap;
    private List<Overlap> overlapTags = new();
    private List<Overlap> overlapTriggers = new();
    
    private void OnTriggerEnter(Collider other)
    {
        if (!allowMutlipleOverlaps && currentOverlap != null)
        {
            return;
        }
        
        // skip if rigidbody is already overlapping
        {
            bool alreadyOverlapping = false;
            
            int index = GetOverlapIndex(overlapTriggers, other);
            if (index >= 0)
            {
                overlapTriggers[index].count++;
                alreadyOverlapping = true;
            }
            
            index = GetOverlapIndex(overlapTags, other);
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
            if (other.CompareTag(tagEvents[i].tagName))
            {
                overlapTags.Add(new (other, 1, i));
                TagEnter(i, other);
                break;
            }
        }

        for (int i = 0; i<colliderEvents.Count; i++)
        {
            if (colliderEvents[i].triggerCollider == other)
            {
                overlapTriggers.Add(new (other, 1, i));
                currentOverlap = other;
                colliderEvents[i].OnEnter.Invoke();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!allowMutlipleOverlaps && currentOverlap != other.attachedRigidbody)
        {
            return;
        }
        
        int index = GetOverlapIndex(overlapTriggers, other);
        if (index >= 0)
        {
            overlapTriggers[index].count--;
            
            if (overlapTriggers[index].count == 0)
            {
                int eventIndex = overlapTriggers[index].index;
                overlapTriggers.RemoveAt(index);
                currentOverlap = null;
                colliderEvents[eventIndex].OnExit.Invoke();
            }
        }
        
        index = GetOverlapIndex(overlapTags, other);
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
            int triggerIndex = Time.frameCount % overlapTags.Count;
            if (!overlapTags[triggerIndex].trigger || !overlapTags[triggerIndex].trigger.gameObject.activeInHierarchy)
            {
                int tagIndex = overlapTags[triggerIndex].index;
                overlapTags.RemoveAt(triggerIndex);
                TagExit(tagIndex);
            }
        }
    }

    private void TagEnter(int index, Collider collider)
    {
        tagEvents[index].activeCount++;
        if (tagEvents[index].activeCount == 1 || !tagEvents[index].oneAtATime)
        {
            currentOverlap = collider;
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
    
    private int GetOverlapIndex(in List<Overlap> overlaps, Collider collider)
    {
        for (int j = 0; j < overlaps.Count; j++)
        {
            if (overlaps[j].trigger == collider)
            {
                return j;
            }
        }

        return -1;
    }
}
