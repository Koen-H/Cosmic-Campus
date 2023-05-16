using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraFollow : MonoBehaviour
{
    [SerializeField] public Transform target;

    [SerializeField] private float offset;

    private void Update()
    {
        if (target == null) return;

        transform.position = target.position + new Vector3(offset,offset*2,-offset);
        transform.LookAt(target);




    }




}
