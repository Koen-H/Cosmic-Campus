using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class RemoteEngineerAbility : NetworkBehaviour
{
    NetworkVariable<bool> isBuilding = new(true, default, NetworkVariableWritePermission.Owner);

    [SerializeField] private float collectRadius = 1f;
    [SerializeField] private float collectRadiusIncreaseIncrement = 0.1f;

    [SerializeField] float sphereRadius = 0.1f;
    [SerializeField] float sphereRadiusIncreaseIncrement= 0;

    public override void OnNetworkSpawn()
    {

        //if (IsOwner)
        //{
        //}
    }

    private void Start()
    {
        CollectBuildingPieces();
    }

    private void Update()
    {
        CollectBuildingPieces();

        if (isBuilding.Value) CollectBuildingPieces();
    }

    //Slowly increases the area it can reach. 
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
        debrisObject.transform.SetParent(transform);
        Vector3 randomPosition = transform.position + Random.onUnitSphere * sphereRadius;
        debrisObject.transform.position = randomPosition;
        Quaternion targetRotation = Quaternion.LookRotation(transform.position - randomPosition);
        debrisObject.transform.rotation = targetRotation;

        // Enable Rigidbody component on the debrisObject
        Rigidbody debrisRigidbody = debrisObject.GetComponent<Rigidbody>();
        if (debrisRigidbody != null) Destroy(debrisRigidbody);
        Collider debrisCollider = debrisObject.GetComponent<Collider>();
        Destroy(debrisCollider);
    }
}
