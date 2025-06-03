using System;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Matt's Scripts/Events/Cooldown Event")]
public class CoolDownEvent : MonoBehaviour
{
    [Tooltip("How much time must pass before the event can be invoked again")]
    [Min(0)] public float coolDownTime = 5f;
    public UnityEvent Event;

    private double lastInvokeTime = Double.NegativeInfinity;
    
    public void Invoke()
    {
        if ((float)(Time.timeAsDouble - lastInvokeTime) >= coolDownTime)
        {
            lastInvokeTime = Time.timeAsDouble;
            Event.Invoke();
        }
    }
}
