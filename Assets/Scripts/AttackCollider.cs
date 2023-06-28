using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The attack collider is a collider that can be subscribed to. Usefull to put on a sword when the behaviour of the sword is somewhere else
/// </summary>
public class AttackCollider : MonoBehaviour
{
    public event System.Action<Transform> OnTriggerEnterEvent;


    private void OnTriggerEnter(Collider other)
    {
        if (OnTriggerEnterEvent != null)
        OnTriggerEnterEvent.Invoke(other.transform);
    }

}
