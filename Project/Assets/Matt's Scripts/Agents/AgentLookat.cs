using System;
using UnityEngine;
using UnityEngine.AI;

[AddComponentMenu("Matt's Scripts/Agents/Agent Look At")]
[RequireComponent(typeof(NavMeshAgent))]
public class AgentLookat : MonoBehaviour
{
    public Transform target;
    private NavMeshAgent agent;
    
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        agent.updateRotation = false;
    }

    private void OnDisable()
    {
        agent.updateRotation = true;
    }

    private void LateUpdate()
    {
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                Quaternion.LookRotation(target.position - transform.position, Vector3.up),
                agent.angularSpeed * Time.deltaTime);
    }
}
