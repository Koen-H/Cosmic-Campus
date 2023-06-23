using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(ClientNetworkTransform),typeof(AudioSource))]
public class ObjectSlamManager : NetworkBehaviour
{
    public PlayerCharacterController playerController;
    Rigidbody rb;
    private bool hasFallen = false;
    float slamDistance = 3;
    float damage = 40;
    float nockback = 20;
    //GameObject slamObjVFX, slamExtraVFX;
    List<Enemy> directHits = new List<Enemy>();

    float dropSpeed = 0.1f;
    
    float sinkSpeed = 0.1f;
    float sinkSpeedIncrement = 0.05f;
    bool isSinking = false;

    [SerializeField] GameObject collider;
    [SerializeField] ParticleSystem vfxPrefab;

    [SerializeField] AudioSource impactSFX;

    private void Awake()
    {
        collider.SetActive(false);
        if (!TryGetComponent(out rb)) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = false;
    }
    public override void OnNetworkSpawn()
    {
        if (IsOwner) StartCoroutine(LateDestroyObject());
    }

    private void Update()
    {
        if(!hasFallen) rb.velocity += Vector3.down * dropSpeed;
        if (isSinking)
        {
            SinkObject();
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (!hasFallen)
    //    {
    //        if (other.gameObject.CompareTag("Ground") || other.gameObject.CompareTag("RainbowRoad"))
    //        {
    //            hasFallen = true;
    //            GroundSlam();

    //        }
    //    }
    //}

    private void OnCollisionEnter (Collision collision)
    {
        if (!hasFallen)
        {
            if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("RainbowRoad"))
            {
                impactSFX.Play();
                hasFallen = true;
                GroundSlam();
                Instantiate(vfxPrefab, collision.contacts[0].point, Quaternion.identity);
            }
        }
    }


    private void GroundSlam()
    {
        rb.isKinematic = true;
        collider.SetActive(true);

        CameraManager.MyCamera.ShakeCamera(2,0.5f);
        StartCoroutine(SinkCountdown(1));
        //If the current client is the owner, we deal the damage
        if (IsOwner)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, slamDistance);
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Enemy"))
                {
                    Enemy enemy = collider.GetComponentInParent<Enemy>();
                    enemy.TakeDamage(playerController.effectManager.ApplyAttackEffect(damage), EnemyType.DESIGNER);//Hardcoded engineer, because it's a engineer ability
                    Vector3 knockbackDirection = enemy.transform.position - transform.position;
                    float knockbackForce = nockback;
                    float knockbackDuration = 0.5f;
                    Debug.Log("DOING THINGS");

                    EnemyMovement enemyMovement = enemy.GetComponentInParent<EnemyMovement>();
                    knockbackDirection = new Vector3(knockbackDirection.x, 0, knockbackDirection.z);
                    enemyMovement.ApplyKnockback(knockbackDirection.normalized, knockbackForce, knockbackDuration);
                }
                else if (collider.CompareTag("Player"))
                {
                    PlayerCharacterController player = collider.GetComponent<PlayerCharacterController>();

                    Vector3 knockbackDirection = player.transform.position - transform.position;
                    //if (knockbackDirection.magnitude > slamDistance / 2) return;
                    knockbackDirection = new Vector3(knockbackDirection.x, 0, knockbackDirection.z);
                    float knockbackForce = 25;
                    float knockbackDuration = 0.5f;
                    player.ApplyKnockback(knockbackDirection.normalized,knockbackForce,knockbackDuration);
                }
            }
        }
    }

    IEnumerator SinkCountdown(float sinkCountdown)
    {
        yield return new WaitForSeconds(sinkCountdown);
        isSinking = true;
    }


    IEnumerator LateDestroyObject()
    {
        yield return new WaitForSeconds(30);
        Destroy(this.gameObject);
    }

    void SinkObject()
    {
        transform.position = new Vector3(transform.position.x,transform.position.y - (sinkSpeed * Time.deltaTime),transform.position.z);
        sinkSpeed += sinkSpeedIncrement;
    }

}
