using UnityEngine;
using UnityEngine.Events;

public class ProximityCheck : MonoBehaviour
{
    [Tooltip("How close the target must be to count as within proximity")]
    [Min(0)] public float maxDistance = 1f;
    [Tooltip("How far target must be beyond the threshold to leave the proximity")]
    [Min(0)] public float escapeDistance = 0f;
    public Transform target;
    
    private bool isWithinProximity = false;
    
    public UnityEvent OnEnterProximity;
    public UnityEvent OnLeaveProximity;

    // Update is called once per frame
    void Update()
    {
        if (isWithinProximity)
        {
            if (Vector3.Distance(target.position, transform.position) > maxDistance + escapeDistance)
            {
                isWithinProximity = false;
                OnLeaveProximity.Invoke();
            }
        }
        else
        {
            if (Vector3.Distance(target.position, transform.position) <= maxDistance)
            {
                isWithinProximity = true;
                OnEnterProximity.Invoke();
            }
        }
    }
    
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.DrawWireDisc(transform.position, transform.up, maxDistance);
        UnityEditor.Handles.DrawWireDisc(transform.position, transform.up, maxDistance + escapeDistance);
    }
    #endif
}
