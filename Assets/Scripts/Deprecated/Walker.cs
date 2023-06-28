using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script that walks to in to enemies and deals damage.
/// </summary>
public class Walker : MonoBehaviour
{
    [SerializeField] float moveSpeed = 0.2f; 
    public GameObject eyePrefab; // Reference to the leg prefab you want to spawn
    public PlayerCharacterController owner;
    Rigidbody rb;
    float raycastDistance = 3;
    float damage = 2;
    bool hasExploded = false;

    GameObject explodeVFX;

    private void Awake()
    {
        if (!TryGetComponent(out rb)) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        //TODO: Make damage better/correct?
        damage += transform.lossyScale.z;
        explodeVFX = Resources.Load<GameObject>("SlamEffect/Slam");
    }
    private void Update()
    {
        transform.Translate(transform.forward * moveSpeed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasExploded)
        {
            hasExploded = true;
            Explode();
        }
    }

    private void Explode()
    {
        rb.isKinematic = true;
        //Do fancy particle stuff
        explodeVFX = Instantiate(explodeVFX, transform.position, Quaternion.identity);
        explodeVFX.GetComponent<ParticleSystem>().startSpeed = transform.lossyScale.x * transform.lossyScale.y * transform.lossyScale.z;
        
        
        //If the current client is the owner, we deal the damage
        if (owner.IsOwner)
        {
            List<GameObject> enemiesHit = new List<GameObject>();
            for (int i = 0; i < 360; i += 10)
            {
                Vector3 direction = Quaternion.Euler(0f, i, 0f) * transform.forward;
                Ray ray = new Ray(transform.position, direction);
                RaycastHit hit;
                Debug.DrawRay(transform.position, direction * raycastDistance, Color.red, 5f);

                if (Physics.Raycast(ray, out hit, raycastDistance))
                {
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        if (!enemiesHit.Contains(hit.collider.gameObject)) enemiesHit.Add(hit.collider.gameObject);
                    }
                }
            }

            foreach (GameObject enemyObj in enemiesHit)
            {
                enemyObj.GetComponentInParent<Enemy>().TakeDamage(damage, EnemyType.ENGINEER);
                Debug.Log($"Did {damage} damage!");
            }
        }
        Destroy(this.gameObject);
    }

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
