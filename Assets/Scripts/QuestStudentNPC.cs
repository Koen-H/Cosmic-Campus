using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class QuestStudentNPC : QuestNPC
{
    [SerializeField] NavMeshAgent agent;

    [SerializeField] Animator studnetAnimator;
    bool running;
    [ClientRpc]
    private void ToggleRunningClientRpc(bool isRunning)
    {
        running = isRunning;
        studnetAnimator.SetBool("Running", running);
    }


    private void Update()
    {
        if (!IsServer) return;
        bool temp = running; 
        if (agent.desiredVelocity.magnitude > 0) running = true;
        else running = false;

        if (temp != running) {
            if(IsServer) ToggleRunningClientRpc(running);
            studnetAnimator.SetBool("Running", running);

        }
    }
}
