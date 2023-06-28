using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(ClientNetworkTransform),typeof(AudioSource))]
public class ObjectSlamManager : NetworkBehaviour
{
    public PlayerCharacterController playerController;
    private Rigidbody rb;
    private bool hasFallen = false;
    [SerializeField] private float slamDistance = 3;
    [SerializeField] private float damage = 40;
    [SerializeField] private float nockback = 20;

    private float dropSpeed = 0.1f;

    private float sinkSpeed = 0.1f;
    private float sinkSpeedIncrement = 0.05f;
    private bool isSinking = false;

    [FormerlySerializedAs("collider")]
    [SerializeField] private GameObject coll;
    [SerializeField] private ParticleSystem vfxPrefab;

    [SerializeField] private AudioSource impactSFX;

    private void Awake()
    {
        coll.SetActive(false);
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
        coll.SetActive(true);

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
