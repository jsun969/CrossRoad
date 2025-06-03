using System;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Random = UnityEngine.Random;

[AddComponentMenu("Matt's Scripts/Agents/Agent Wander")]
[RequireComponent(typeof(NavMeshAgent))]
public class AgentWander : MonoBehaviour
{
    [Header("AI Navigation")]
    public NavMeshSurface navigationFloor;
    public AgentZone spawnZone;

    [Header("Settings")] 
    public float waitSeconds;
    
    public UnityEvent OnWanderTargetReached;
    public UnityEvent OnStartWander;
    public UnityEvent OnStopWander;

    private NavMeshAgent agent;
    private float waitTimer;
    private bool isMoving;
    private Vector3 targetPosition;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        waitTimer = Random.value * waitSeconds;
    }

    private void OnDisable()
    {
        if (isMoving) StopMoving();
    }

    void Update()
    {
        if (isMoving)
        {
            if (Distance2D(transform.position, targetPosition) < 0.05f)
            {
                OnWanderTargetReached.Invoke();
                StopMoving();
            }
        }
        else if (waitTimer >= waitSeconds) // can start moving
        {
            RaycastHit raycastHit;
            for (int i = 0; i < 10; i++)
            {
                Vector3 samplePosition = spawnZone.GetRandom();
                if( Physics.Raycast(samplePosition, Vector3.down, out raycastHit))
                {
                    if (NavMesh.SamplePosition(raycastHit.point, out var hit, 1f, NavMesh.AllAreas))
                    {
                        StartMoving(hit.position);
                        return;
                    }
                }
            }
            Debug.LogWarning("Failed to find a position to move to");
        }
        else // stand still
        {
            waitTimer += Time.deltaTime;
        }
    }

    private void StartMoving(Vector3 newTarget)
    {
        targetPosition = newTarget;
        agent.SetDestination(targetPosition);
        agent.isStopped = false;
        waitTimer = 0;
        isMoving = true;
        
        OnStartWander.Invoke();
    }

    private void StopMoving()
    {
        isMoving = false;
        
        if (agent.isOnNavMesh)
            agent.isStopped = true;
        
        OnStopWander.Invoke();
    }

    private void OnDrawGizmos()
    {
        if (isMoving)
        {
            Gizmos.DrawSphere(targetPosition, 0.2f);
        }
    }

    private float Distance2D(Vector3 a, Vector3 b)
    {
        Vector2 offset = Convert(a) - Convert(b);
        return Mathf.Sqrt(offset.x * offset.x + offset.y * offset.y);
    }
    
    private Vector2 Convert(Vector3 v) => new Vector2(v.x, v.z);
    private Vector3 Convert(Vector2 v) => new Vector3(v.x, 0, v.y);
}
