using UnityEngine;

public class ObjectSpin : MonoBehaviour
{
    public float rotationSpeed = 100f;

    // Update is called once per frame
    void Update()
    {
        // Rotate the object around its Y-axis
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
}
