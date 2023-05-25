using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    private Transform target; 

    private NavMeshAgent agent;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void SetTarget(Transform Target) { if (Target != null) target = Target; }
    public void RemoveTarget(){target = null;}
    public void SetSpeed(float speed) { if (speed >= 0) agent.speed = speed;}



    private void Update()
    {
       if(target != null)agent.destination = target.position;
    }


}
