using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class crystalGrabManager : MonoBehaviour
{
    public Transform staffObject;

    public Transform trail;
    [SerializeField] float speed = 0.3f;

    private void Update()
    {
        transform.LookAt(staffObject);
        transform.Translate(transform.forward * (speed * Time.deltaTime));
        if ((this.transform.position - staffObject.position).magnitude < 0.3f) Destroy(gameObject);
    }
}
