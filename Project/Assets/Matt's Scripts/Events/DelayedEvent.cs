using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[AddComponentMenu("Matt's Scripts/Events/Delayed Event")]
public class DelayedEvent : MonoBehaviour
{
    public bool playOnAwake;
    [Min(0)] public float delayTime;
    public bool useScaledTime = true;
    public UnityEvent Event;
    
    private void Start()
    {
        if (playOnAwake)
            Invoke();
    }

    public void Invoke()
    {
        StartCoroutine(EventCoroutine());
    }

    private IEnumerator EventCoroutine()
    {
        if (useScaledTime)
            yield return new WaitForSeconds(delayTime);
        else
            yield return new WaitForSecondsRealtime(delayTime);
        
        Event.Invoke();
    }
}
