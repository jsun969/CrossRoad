using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[AddComponentMenu("Matt's Scripts/Agents/Agent Proximity")]
[RequireComponent(typeof(NavMeshAgent))]
public class AgentProximity : MonoBehaviour
{
    private static int s_Counter = 0;
    private int instanceID;
    
    [Min(0)] public float maxDistance;
    public Transform target;
    
    [Tooltip("Optionally require distance and a path to the player")]
    public AgentZone optionalZone;
    
    public UnityEvent OnEnterProximity;
    public UnityEvent OnLeaveProximity;

    private bool isWithinProximity = false;
    
    private NavMeshAgent agent;
    private NavMeshPath path;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        path = new();
        instanceID = s_Counter++;
    }

    private void Update()
    {
        // run once every 10 frames
        if (((Time.frameCount + instanceID) % 10) != 0)
            return;
        
        bool proximityCheck = Distance2D(target.position, transform.position) <= maxDistance;
        
        if (proximityCheck && optionalZone)
            proximityCheck = proximityCheck && optionalZone.Contains(target.position);

        if (proximityCheck)
            proximityCheck = proximityCheck && agent.CalculatePath(target.position, path);

        if (proximityCheck != isWithinProximity)
        {
            isWithinProximity = proximityCheck;
            
            if (isWithinProximity)
                OnEnterProximity.Invoke();
            else
                OnLeaveProximity.Invoke();
        }
    }
    
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, maxDistance);
#endif
    }
    
    private float Distance2D(Vector3 a, Vector3 b)
    {
        Vector2 offset = Convert(a) - Convert(b);
        return Mathf.Sqrt(offset.x * offset.x + offset.y * offset.y);
    }

    private Vector2 Convert(Vector3 v) => new Vector2(v.x, v.z);
    private Vector3 Convert(Vector2 v) => new Vector3(v.x, 0, v.y);
}
