using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walker : MonoBehaviour
{
    [SerializeField] float moveSpeed = 0.5f; 

    private void FixedUpdate()
    {
        transform.Translate(transform.forward * moveSpeed); 
    }
}
