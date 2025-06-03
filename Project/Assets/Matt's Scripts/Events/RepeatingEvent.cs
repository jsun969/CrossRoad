using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[AddComponentMenu("Matt's Scripts/Events/Repeating Event")]
public class RepeatingEvent : MonoBehaviour
{
    [Tooltip("How many seconds between invocations")]
    [Min(0.001f)] public float coolDownTime = 1f;
    [Tooltip(("Enable to prevent invoking more than once in a single frame"))]
    public bool limitOncePerFrame = true;
    [Tooltip("Enable to account for time scale")]
    public bool useScaledTime = true;
    public UnityEvent Event;

    private double lastTime = 0;
    private double lastTimeUnscaled = 0;
    
    private void Update()
    {
        int invocationCount = 0;
        
        if (useScaledTime)
        {
            while (lastTime < Time.timeAsDouble)
            {
                lastTime += coolDownTime;
                invocationCount++;
            }
            lastTimeUnscaled = Time.unscaledTimeAsDouble;
        }
        else
        {
            while (lastTimeUnscaled < Time.unscaledTimeAsDouble)
            {
                lastTimeUnscaled += coolDownTime;
                invocationCount++;
            }
            lastTime = Time.timeAsDouble;
        }

        if (invocationCount > 0)
        {
            if (limitOncePerFrame) 
                invocationCount = 1;
                
            while (invocationCount-- > 0)
                Invoke();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Invoke()
    {
        Event.Invoke();
    }
}
