using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class RemoteEngineerAbility : NetworkBehaviour
{
    NetworkVariable<bool> isBuilding = new(true, default, NetworkVariableWritePermission.Owner);

    [SerializeField] Transform objCollector;

    [SerializeField] private float collectRadius = 1f;
    [SerializeField] private float collectRadiusIncreaseIncrement = 0.1f;

    [SerializeField] private float sphereRadius = 0.1f;
    [SerializeField] private float sphereRadiusIncreaseIncrement = 0;

    [SerializeField] private float accelerationTime = 0.5f;
    [SerializeField] private float maxSpeed = 10f;
    private float currentSpeed = 5f;
    private Rigidbody rigidbody;

    private Vector3 currentDirection = Vector3.zero;

    float t = 0;
    int attachedObjects = 0;
    [SerializeField] float timeTillExplosion = 15f;
    [SerializeField] float explosionDamage = 1f;
    [SerializeField] float damageIncreasePerObj = 1f;
    [SerializeField] float explosionRange = 2f;
    [SerializeField] float rangeIncreasePerObj = 0.2f;

    [SerializeField] GameObject explosionVFX, electricityVFX;
    private bool exploded = false;


    public override void OnNetworkSpawn()
    {

        if (IsOwner)
        {
            //Get the camera to focus on this object!
            CameraManager.MyCamera.SetFollowTarg(transform);
            CameraManager.MyCamera.SetLookTarg(transform);
        }
    }
    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.isKinematic = true;

    }

    private void Update()
    {
        if (isBuilding.Value)
        {
            CollectBuildingPieces();
            if (Input.GetMouseButtonUp(1))
            {
                isBuilding.Value = false;
                rigidbody.isKinematic = false;
                StartCoroutine(ExplosionCountdown());
            }
            return;
        }
        else if (IsOwner) Control();
    }

    public void Control()
    {
        Move();
        if(Input.GetMouseButtonDown(0) && !exploded) ExplodeServerRpc();
    }

    private void Move()
    {
        int horizontalInput = 0;
        int verticalInput = 0;

        if (Input.GetKey(KeyCode.D)) horizontalInput = 1;
        if (Input.GetKey(KeyCode.A)) horizontalInput = -1;
        if (Input.GetKey(KeyCode.W)) verticalInput = 1;
        if (Input.GetKey(KeyCode.S)) verticalInput = -1;


        Vector3 movementDirection = new Vector3(horizontalInput, 0, verticalInput).normalized;
        Quaternion rotationQuaternion = Quaternion.Euler(0, -45, 0);
        movementDirection = rotationQuaternion * movementDirection;

        if (movementDirection != Vector3.zero)
        {
            Vector2 currentDir = new Vector2(currentDirection.x, currentDirection.z);
            Vector2 movementDir = new Vector2(movementDirection.x, movementDirection.z);
            t = Time.deltaTime;
            Vector2 lerpDir = Vector2.Lerp(currentDir, movementDir, t * accelerationTime);
            currentDirection = new Vector3(lerpDir.x, 0, lerpDir.y);
        }
        rigidbody.velocity = currentDirection * maxSpeed;
    }


    public void CollectBuildingPieces()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, collectRadius);

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Debris"))
            {

                Vector3 diff = (collider.transform.position - transform.position);
               // RaycastHit hit;
              //  Physics.Raycast(new Ray(transform.position, diff), out hit, diff.magnitude);

               // if (hit.transform == null) continue;
               // if (hit.transform.CompareTag("Debris"))
               // {
                    AttachObject(collider.gameObject);
                    sphereRadius += sphereRadiusIncreaseIncrement;
               // }


            }
        }
        collectRadius += collectRadiusIncreaseIncrement * Time.deltaTime;
    }

    public void AttachObject(GameObject debrisObject)
    {
        attachedObjects++;
        debrisObject.transform.parent = objCollector;
        Vector3 randomPosition = Random.onUnitSphere * sphereRadius;
        Quaternion targetRotation = Random.rotation;

        TransformLerper lerper = debrisObject.AddComponent<TransformLerper>();
        lerper.targetVector = randomPosition;
        lerper.targetRotation = targetRotation;

        Rigidbody debrisRigidbody = debrisObject.GetComponent<Rigidbody>();
        if (debrisRigidbody != null) Destroy(debrisRigidbody);
        Collider debrisCollider = debrisObject.GetComponent<Collider>();
        if (debrisCollider != null) Destroy(debrisCollider);
    }


    [ServerRpc]
    public void ExplodeServerRpc()
    {
        ExplodeClientRpc();
    }

    [ClientRpc]
    public void ExplodeClientRpc()
    {
        GameObject explosionVFXinstance = Instantiate(explosionVFX, transform.position, Quaternion.identity);
        explosionVFXinstance.GetComponent<ParticleSystem>().startSize *= attachedObjects * 10 + 1000;
        GameObject electricityVFXinstance = Instantiate(electricityVFX, transform.position, Quaternion.identity);
        electricityVFXinstance.GetComponent<ParticleSystem>().startSpeed *= attachedObjects / 5;
        if (!IsOwner) return;
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRange + rangeIncreasePerObj * attachedObjects);
        float damage = explosionDamage + damageIncreasePerObj * attachedObjects;
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                collider.GetComponentInParent<Enemy>().TakeDamage(damage);
            }
        }
        StartCoroutine(LateCameraMoveBack(1));

        IEnumerator LateCameraMoveBack(float duration)
        {
            rigidbody.isKinematic = true;

            yield return new WaitForSeconds(duration);
            CameraManager.MyCamera.TargetPlayer();
            ClientManager.MyClient.playerCharacter.LockPlayer(false);
            DestroyServerRpc();
        }
    }

    /// <summary>
    /// Tell the server that the object can be destroyed!
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void DestroyServerRpc()
    {
        Destroy(this.gameObject);

    }

    IEnumerator ExplosionCountdown()
    {
        while(timeTillExplosion > 0)
        {
            Debug.Log(timeTillExplosion);
            yield return new WaitForSeconds(1);
            timeTillExplosion--;
        }
        if(!exploded) ExplodeServerRpc();
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, collectRadius);
    }

}
