using UnityEngine;
using UnityEngine.Events;

public class AngleDetector : MonoBehaviour
{
    public Transform targetDirection;
    [Range(0, 180)] public float angle;
    [Range(0, 180)] public float escapeAngle;

    public UnityEvent OnEnterAngle;
    public UnityEvent OnExitAngle;

    private bool isInAngle;

    private void OnDisable()
    {
        if (isInAngle)
        {
            isInAngle = false;
            OnExitAngle.Invoke();
        }
    }

    void Update()
    {
        if (isInAngle)
        {
            if (Vector3.Angle(targetDirection.forward, transform.forward) > angle + escapeAngle)
            {
                isInAngle = false;
                OnExitAngle.Invoke();
            }
        }
        else
        {
            if (Vector3.Angle(targetDirection.forward, transform.forward) < angle)
            {
                isInAngle = true;
                OnEnterAngle.Invoke();
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
            Gizmos.color = isInAngle ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position, transform.forward);

        
        if (targetDirection)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, targetDirection.forward); 
        }
    }
}
