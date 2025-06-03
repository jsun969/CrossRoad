using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[AddComponentMenu("Matt's Scripts/Agents/Agent Patrol")]
[RequireComponent(typeof(NavMeshAgent))]
public class AgentPatrol : MonoBehaviour
{
    public Transform[] waypointList;

    public float triggerDistance = 0.05f;
    public float waitTimeBetweenWaypoints = 0f;
    
    public UnityEvent OnWaypointReached;
    
    private NavMeshAgent agent;
    private int nextWaypointIndex;
    private bool isMoving;
    
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        isMoving = false;
        nextWaypointIndex = CalculateNearestWaypointIndex(agent.transform.position);

        StartCoroutine(RunAgent());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (agent.hasPath)
            agent.isStopped = true;
    }

    private IEnumerator RunAgent()
    {
        while (true)
        {
            if (isMoving)
            {
                // wait for agent to reach waypoint
                while (Distance2D(agent.transform.position, agent.path.corners[^1]) > triggerDistance)
                {
                    yield return null;
                }
                
                isMoving = false;
                agent.isStopped = true;
                OnWaypointReached.Invoke();

                yield return new WaitForSeconds(waitTimeBetweenWaypoints);
            }
            else
            {
                // each frame try to path to the next waypoint (will skip over unreachable waypoints)
                while (!agent.SetDestination(waypointList[nextWaypointIndex].position))
                {
                    nextWaypointIndex = (nextWaypointIndex + 1) % waypointList.Length;
                    yield return null;
                }

                nextWaypointIndex = (nextWaypointIndex + 1) % waypointList.Length;
                isMoving = true;
                agent.isStopped = false;
                
                yield return null;
            }
        }
    }

    private int IncrementWaypoint()
    {
        int ret = nextWaypointIndex;
        nextWaypointIndex = (nextWaypointIndex + 1) % waypointList.Length;
        return ret;
    }

    private int CalculateNearestWaypointIndex(in Vector3 point)
    {
        Vector2 p = Convert(point);

        int bestIndex = -1;
        float bestDistance = Single.PositiveInfinity;
        
        for (int i = 0; i < waypointList.Length; i++)
        {
            var distance = Vector2.Distance(p, Convert(waypointList[i].position));
            if (distance < bestDistance)
            {
                bestIndex = i;
                bestDistance = distance;
            }
        }

        return bestIndex;
    }

    private void OnDrawGizmos()
    {
        if (agent && agent.hasPath)
            Gizmos.DrawSphere(agent.pathEndPosition, 0.25f);
        
        if (waypointList.Length >= 2)
            Gizmos.DrawLine(waypointList[0].position, waypointList[^1].position);
            
        for (int i=1; i<waypointList.Length; i++)
        {
            Gizmos.DrawLine(waypointList[i-1].position, waypointList[i].position);
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
