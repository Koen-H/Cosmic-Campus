using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private Transform target; 

    private NavMeshAgent agent;
    [SerializeField] private float range; 
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }


    private void Update()
    {
        if((target.position-transform.position).magnitude < range)
        {
            agent.destination = target.position;
        }
    }


}
