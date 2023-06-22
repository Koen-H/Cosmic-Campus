using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class crystalGrabManager : MonoBehaviour
{
    public Transform staffObject;

    public Transform trail;
    [SerializeField] float speed = 0.3f;

    [SerializeField] private float rotationSpeed = 5f;
    float timer; 

    private void Start()
    {
        Vector3 targetDirection = transform.position - staffObject.position;
        transform.rotation = Quaternion.LookRotation(targetDirection.normalized, Vector3.up);
    }


    private void Update()
    {
        timer += Time.deltaTime; 
        // Calculate the direction to look at the staffObject
        Vector3 targetDirection = staffObject.position - transform.position;
        //Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized, Vector3.up);

        // Rotate the object towards the target rotation with the desired speed
        //transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, (rotationSpeed + (rotationSpeed/(1+timer))) * Time.deltaTime);

        // Move the object forward
        transform.Translate(transform.forward * (speed * Time.deltaTime));

        // Check if the object is close to the staffObject and destroy it if necessary
        if ((transform.position - staffObject.position).magnitude < 0.3f)
        {
            Destroy(gameObject);
        }
    }
}
