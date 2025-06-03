using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Random = Unity.Mathematics.Random;

[AddComponentMenu("Matt's Scripts/Agents/Agent Move To Target")]
[RequireComponent(typeof(NavMeshAgent))]
public class AgentMoveToTarget : MonoBehaviour
{
    [Tooltip("Object the AI will try and move to")]
    public Transform target;
    [Tooltip("distance target must move to recalculate route")]
    public float deltaDistance = 0.05f;
    [Tooltip("Distance agent must be to count as reached target")]
    public float minTargetDistance = 0.5f;
    [Tooltip("How far the target needs to move to escape this agent")]
    [Min(0)] public float escapeDistance = 0.5f;

    public UnityEvent OnTargetReached;
    
    private bool isFollowing;
    private bool hasReachedTarget = false;
    private Vector3 targetPosition;
    private NavMeshAgent agent;
    private NavMeshPath path;
    
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
    }
    
    private void OnDisable()
    {
        if (isFollowing)
            StopFollowing();
    }

    void Update()
    {
        if (isFollowing)
        {
            if (hasReachedTarget)
            {
                if (Distance2D(targetPosition, agent.transform.position) >= minTargetDistance + escapeDistance)
                    hasReachedTarget = false;
            }
            else
            {
                if (Distance2D(targetPosition, agent.transform.position) < minTargetDistance)
                {
                    hasReachedTarget = true;
                    OnTargetReached.Invoke();
                }
            }
            
            // need to update path if target moves 
            if (Distance2D(targetPosition, target.position) > deltaDistance)
            {
                if (RecalculatePath())
                {
                    agent.SetPath(path);
                }
                else
                {
                    StopFollowing();
                }
            }
        }
        else
        {
            if (RecalculatePath())
                StartFollowing();
        }
    }

    bool RecalculatePath()
    {
        if (agent.CalculatePath(target.position, path))
        {
            targetPosition = path.corners[^1];
            return true;
        }

        return false;
    }

    void StopFollowing()
    {
        if (agent.hasPath)
            agent.isStopped = true;
        
        hasReachedTarget = false;
        isFollowing = false;
    }

    void StartFollowing()
    {
        isFollowing = true;
        
        agent.SetPath(path);
        agent.isStopped = false;
    }


    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (isFollowing)
        {
            UnityEditor.Handles.DrawWireDisc(targetPosition, Vector3.up, minTargetDistance);
            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.DrawWireDisc(targetPosition, Vector3.up, minTargetDistance + escapeDistance);
        }
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
