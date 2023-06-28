using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class crystalGrabManager : MonoBehaviour
{
    public Transform staffObject;

    public Transform trail;
    [SerializeField] private float speed = 0.3f;

    [SerializeField] private float rotationSpeed = 5f;

    private void Update()
    {
        transform.LookAt(staffObject.position);
        transform.position = transform.position +( transform.forward * (speed * Time.deltaTime));

        // Check if the object is close to the staffObject and destroy it if necessary
        if ((transform.position - staffObject.position).magnitude < 0.3f)
        {
            Destroy(gameObject);
        }
    }
}
