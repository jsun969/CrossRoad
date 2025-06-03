using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Matt's Scripts/Events/Probability Event")]
public class ProbabilityEvent : MonoBehaviour
{
    [Range(0, 100)] public float eventChance = 50;
    public UnityEvent Event;

    public void InvokeEvent()
    {
        if (eventChance <= 0)
            return;

        if (Random.value * 100f <= eventChance)
            Event.Invoke();
    }
}
