using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollider : MonoBehaviour
{
    public event System.Action<Transform> OnTriggerEnterEvent;


    private void OnTriggerEnter(Collider other)
    {
        if (OnTriggerEnterEvent != null)
        OnTriggerEnterEvent.Invoke(other.transform);
    }

}
