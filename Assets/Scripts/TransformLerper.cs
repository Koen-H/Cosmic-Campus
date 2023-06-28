using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformLerper : MonoBehaviour
{

    public Vector3 targetVector;
    public Quaternion targetRotation;

    public float lerpSpeed = 10f;

    private float closeDistance = 0.2f;

    void Update()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetVector, lerpSpeed * Time.deltaTime);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, lerpSpeed * Time.deltaTime);
        if ((transform.localPosition - targetVector).magnitude < closeDistance) Destroy(this);
    }
}
