using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class RemoteEngineerAbility : NetworkBehaviour
{
    NetworkVariable<bool> isBuilding = new(true, default, NetworkVariableWritePermission.Owner);

    [SerializeField] Transform objCollector;

    [SerializeField] private float collectRadius = 1f;
    [SerializeField] private float collectRadiusIncreaseIncrement = 0.1f;

    [SerializeField] private float sphereRadius = 0.1f;
    [SerializeField] private float sphereRadiusIncreaseIncrement= 0;

    [SerializeField] private float accelerationTime = 0.5f;
    [SerializeField] private float decelerationTime = 0.5f;
    [SerializeField] private float maxSpeed = 10f;
    private float currentSpeed = 5f;
    private Rigidbody rigidbody;

    private Vector3 currentDirection = Vector3.zero;

    public override void OnNetworkSpawn()
    {

        //if (IsOwner)
        //{
        //}
    }

    private void Start()
    {
        CollectBuildingPieces();
        rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        CollectBuildingPieces();

        if (isBuilding.Value) CollectBuildingPieces();
        else if(IsOwner) Move();
        Move();
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
            Vector2 lerpDir = Vector2.Lerp(movementDir, currentDir, Time.deltaTime * accelerationTime);
            Debug.Log(lerpDir);
            currentDirection = new Vector3(lerpDir.x, 0, lerpDir.y);
        }
        rigidbody.velocity = currentDirection * currentSpeed;
    }


    public void CollectBuildingPieces()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, collectRadius);

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Debris"))
            {
                AttachObject(collider.gameObject);
                sphereRadius += sphereRadiusIncreaseIncrement;
            }
        }
        collectRadius += collectRadiusIncreaseIncrement * Time.deltaTime;
    }

    public void AttachObject(GameObject debrisObject)
    {
        debrisObject.transform.parent = objCollector;
        Vector3 randomPosition = Random.onUnitSphere * sphereRadius;
        Quaternion targetRotation = Random.rotation;

        TransformLerper lerper =  debrisObject.AddComponent<TransformLerper>();
        lerper.targetVector = randomPosition;
        lerper.targetRotation = targetRotation;

        Rigidbody debrisRigidbody = debrisObject.GetComponent<Rigidbody>();
        if (debrisRigidbody != null) Destroy(debrisRigidbody);
        Collider debrisCollider = debrisObject.GetComponent<Collider>();
        if (debrisCollider != null) Destroy(debrisCollider);
    }
}
