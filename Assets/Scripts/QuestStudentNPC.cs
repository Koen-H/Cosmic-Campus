using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class QuestStudentNPC : QuestNPC
{
    [SerializeField] NavMeshAgent agent;

    [SerializeField] Animator studnetAnimator;
    bool running;


    private void Update()
    {
        bool temp = running; 
        if (agent.desiredVelocity.magnitude > 0) running = true;
        else running = false;

        if (temp != running) studnetAnimator.SetBool("Running", running);
    }
}
