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
    private SphereCollider sphereCollider;

    private Vector3 currentDirection = Vector3.zero;

    float t = 0;
    int attachedObjects = 0;
    [SerializeField] float timeTillExplosion = 15f;
    [SerializeField] float explosionDamage = 1f;
    [SerializeField] float damageIncreasePerObj = 1f;
    [SerializeField] float explosionRange = 2f;
    [SerializeField] float rangeIncreasePerObj = 0.2f;
    [SerializeField] float maxExplosionRange = 20f;

    [SerializeField] GameObject explosionVFX, electricityVFX, chargingVFX, boilingVFX;

    ParticleSystem.ShapeModule shape;
    private bool exploded = false;

    Vector3 startPos;

    private PlayerCharacterController playerController;

    public override void OnNetworkSpawn()
    {
        isBuilding.OnValueChanged += BuildingStopped;
        if (IsOwner)
        {
            //Get the camera to focus on this object!
            CameraManager.MyCamera.SetFollowTarg(transform);
            CameraManager.MyCamera.SetLookTarg(transform);
            CanvasManager.Instance.SetEngineerPrompt("Hold Right click to charge!");
            playerController = ClientManager.MyClient.playerCharacter;
        }
    }

    public void BuildingStopped(bool oldValue, bool newValue)
    {
        if (!newValue)//IF Stopped building
        {
            Destroy(chargingVFX);
            boilingVFX.GetComponent<ParticleSystem>().Play();
            rigidbody.isKinematic = false;
            if(IsOwner)StartCoroutine(ExplosionCountdown());
        }
    }

    public void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        chargingVFX.GetComponent<ParticleSystem>();
        shape = chargingVFX.GetComponent<ParticleSystem>().shape;
        sphereCollider = GetComponent<SphereCollider>();
        startPos = transform.position;
    }

    public void Update()
    {
        if (isBuilding.Value)
        {
            
            CollectBuildingPieces();
            if (!IsOwner) return;
            if (Input.GetMouseButtonUp(1))
            {
                isBuilding.Value = false;
            }
            return;
        }
        else if (IsOwner) Control();
    }

    public void Control()
    {
        Move();
        if ((Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(0)) && !exploded && isBuilding.Value == false)
        {
            exploded = true;
            ExplodeServerRpc();
        }
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
        rigidbody.velocity = currentDirection * playerController.effectManager.ApplyMovementEffect(maxSpeed);
    }


    public void CollectBuildingPieces()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, collectRadius);

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider.CompareTag("Debris"))
            {

                AttachObject(collider.gameObject);

                float d = 2 * Mathf.Pow((Mathf.Pow(sphereRadius, 3) + Mathf.Pow(sphereRadiusIncreaseIncrement, 3)), (1 / 3f));

                sphereRadius = d / 2;
                sphereCollider.radius = sphereRadius;
                transform.position = new Vector3(transform.position.x, startPos.y + sphereRadius, transform.position.z);
            }
        }
/*        foreach (Collider collider in colliders)
        {

        }*/
        
        collectRadius += collectRadiusIncreaseIncrement * Time.deltaTime;
        shape.radius = collectRadius;

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
        exploded = true;
        ExplodeClientRpc();
    }
    
    [ClientRpc]
    public void ExplodeClientRpc()
    {
        exploded = true;
        GameObject explosionVFXinstance = Instantiate(explosionVFX, transform.position, Quaternion.identity);
        explosionVFXinstance.GetComponent<ParticleSystem>().startSize *= attachedObjects * 10 + 1000;
        GameObject electricityVFXinstance = Instantiate(electricityVFX, transform.position, Quaternion.identity);
        electricityVFXinstance.GetComponent<ParticleSystem>().startSpeed *= attachedObjects / 5;
        objCollector.gameObject.SetActive(false);
        if (!IsOwner) return;
        CanvasManager.Instance.SetEngineerPrompt(" ", false);
        Collider[] colliders = Physics.OverlapSphere(transform.position, (explosionRange + rangeIncreasePerObj * attachedObjects) > maxExplosionRange ? maxExplosionRange : (explosionRange + rangeIncreasePerObj * attachedObjects));
        float damage = explosionDamage + damageIncreasePerObj * attachedObjects;
        damage = playerController.effectManager.ApplyAttackEffect(damage);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                collider.GetComponentInParent<Enemy>().TakeDamage(damage,EnemyType.ENGINEER);//Hardcoded engineer, because it's a engineer ability
            }
        }
        
        StartCoroutine(LateCameraMoveBack(1));

        IEnumerator LateCameraMoveBack(float duration)
        {
            rigidbody.isKinematic = true;

            yield return new WaitForSeconds(duration);
            CameraManager.MyCamera.TargetPlayer();
            ClientManager.MyClient.playerCharacter.LockPlayer(false);
            ClientManager.MyClient.playerCharacter.engineering = false;
            CanvasManager.Instance.SetEngineerPrompt(" ",false);
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
            if (exploded) yield return null;
            CanvasManager.Instance.SetEngineerPrompt($"{timeTillExplosion}");
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
