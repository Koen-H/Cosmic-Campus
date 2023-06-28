using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMillRotation : MonoBehaviour
{
    [SerializeField] float rotationSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotationSpeed, 0, 0);
    }
}
