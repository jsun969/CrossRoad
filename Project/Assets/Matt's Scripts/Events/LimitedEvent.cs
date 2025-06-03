using System;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Matt's Scripts/Events/Limited Event")]
public class LimitedEvent : MonoBehaviour
{
    [Tooltip("How many times can this event be invoked")]
    [Min(0)] public int limitAmount;
    [Tooltip("Whether the call count will be reset if this object is disabled")]
    public bool resetOnDisable;
    public UnityEvent Event;

    private int callCount = 0;

    private void OnDisable()
    {
        if (resetOnDisable)
            callCount = 0;
    }

    public void Invoke()
    {
        if (callCount >= limitAmount)
            return;

        callCount++;
        Event.Invoke();
    }
}
