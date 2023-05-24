using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walker : MonoBehaviour
{
    [SerializeField] float moveSpeed = 0.5f; 
    public GameObject eyePrefab; // Reference to the leg prefab you want to spawn


    private void ApplyEyes()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            Mesh mesh = meshFilter.sharedMesh;
            if (mesh != null)
            {
                // Calculate the center of the mesh
                Vector3 center = mesh.bounds.center;

                // Create the left eye
                GameObject leftEye = Instantiate(eyePrefab, center, Quaternion.identity);
                leftEye.transform.parent = transform;
                leftEye.name = "LeftEye";

                // Create the right eye
                GameObject rightEye = Instantiate(eyePrefab, center, Quaternion.identity);
                rightEye.transform.parent = transform;
                rightEye.name = "RightEye";

                // Offset the eyes based on the mesh bounds
                Vector3 extents = mesh.bounds.extents;
                float eyeOffset = extents.x * 0.3f; // Adjust this value to control the eye position
                leftEye.transform.localPosition = new Vector3(-eyeOffset, 0f, extents.z);
                rightEye.transform.localPosition = new Vector3(eyeOffset, 0f, extents.z);
            }
        }
    }
}
