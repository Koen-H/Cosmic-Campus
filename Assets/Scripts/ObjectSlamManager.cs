using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ObjectSlamManager : NetworkBehaviour
{
    public PlayerCharacterController playerController;
    Rigidbody rb;
    private bool hasFallen = false;
    float slamDistance = 3;
    float damage = 40;
    float nockback = 20;
    GameObject slamObjVFX, slamExtraVFX;
    List<Enemy> directHits = new List<Enemy>();

    float dropSpeed = 0.1f;
    
    float sinkSpeed = 0.1f;
    float sinkSpeedIncrement = 0.05f;
    bool isSinking = false;

    [SerializeField] GameObject collider;

    private void Awake()
    {
        collider.SetActive(false);
        if (!TryGetComponent(out rb)) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        //raycastDistance *= transform.lossyScale.z;
        //TODO: Make damage better/correct?
        damage += transform.lossyScale.z;
        slamObjVFX = Resources.Load<GameObject>("SlamEffect/Slam");
        slamExtraVFX = Resources.Load<GameObject>("SlamEffect/Slam2");
    }
    private void Update()
    {
        if(!hasFallen) rb.velocity += Vector3.down * dropSpeed;
        if (isSinking)
        {
            SinkObject();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        //if (collision.gameObject.CompareTag("Enemy"))
        //{
        //    collision.gameObject.GetComponentInParent<Enemy>().TakeDamage(damage, playerController.damageType);
        //    return;
        //}
        if (!hasFallen)
        {
            hasFallen = true;
            GroundSlam();
        }

    }
    private void GroundSlam()
    {
        rb.isKinematic = true;
        collider.SetActive(true);
        CameraManager.MyCamera.ShakeCamera(2,0.5f);
        StartCoroutine(SinkCountdown(1));
        //Do fancy particle stuff
        //GameObject slamObjVFXinstance = Instantiate(slamObjVFX, transform.position, Quaternion.identity);
        //slamObjVFXinstance.GetComponent<ParticleSystem>().startSpeed = transform.lossyScale.x * transform.lossyScale.y * transform.lossyScale.z;
        // GameObject slamExtraVFXinstance = Instantiate(slamExtraVFX, transform.position, Quaternion.identity);
        //slamExtraVFXinstance.GetComponent<ParticleSystem>().startSpeed = transform.lossyScale.x * transform.lossyScale.y * transform.lossyScale.z;
        //If the current client is the owner, we deal the damage
        if (IsOwner)
        {

            Collider[] colliders = Physics.OverlapSphere(transform.position, slamDistance);
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Enemy"))
                {
                    Enemy enemy = collider.GetComponentInParent<Enemy>();
                    enemy.TakeDamage(damage, EnemyType.DESIGNER);//Hardcoded engineer, because it's a engineer ability
                    Vector3 knockbackDirection = enemy.transform.position - transform.position;
                    float knockbackForce = nockback;
                    float knockbackDuration = 0.5f;

                    EnemyMovement enemyMovement = enemy.GetComponentInParent<EnemyMovement>();
                    enemyMovement.ApplyKnockback(knockbackDirection, knockbackForce, knockbackDuration);
                }

                if (collider.CompareTag("Player"))
                {
                    PlayerCharacterController player = collider.GetComponent<PlayerCharacterController>();

                    Vector3 knockbackDirection = player.transform.position - transform.position;
                    Debug.Log(knockbackDirection.magnitude);
                    //if (knockbackDirection.magnitude > slamDistance / 2) return;
                    knockbackDirection = new Vector3(knockbackDirection.x, 0, knockbackDirection.z);
                    float knockbackForce = 25;
                    float knockbackDuration = 0.5f;
                    player.ApplyKnockback(knockbackDirection.normalized,knockbackForce,knockbackDuration);
                }
            }

            //Do damage things

            //List<GameObject> enemiesHit = new List<GameObject>();
            //for (int i = 0; i < 360; i += 10)
            //{
            //    Vector3 direction = Quaternion.Euler(0f, i, 0f) * transform.forward;
            //    Ray ray = new Ray(transform.position, direction);
            //    RaycastHit hit;
            //    Debug.DrawRay(transform.position, direction * raycastDistance, Color.red, 5f);

            //    if (Physics.Raycast(ray, out hit, raycastDistance))
            //    {
            //        if (hit.collider.CompareTag("Enemy"))
            //        {
            //            if (!enemiesHit.Contains(hit.collider.gameObject)) enemiesHit.Add(hit.collider.gameObject);
            //        }
            //    }
            //}
            //foreach (GameObject enemy in enemiesHit)
            //{
            //    Vector3 knockbackDirection = enemy.transform.position - transform.position;
            //    float knockbackForce = nockback; // Adjust the force to your desired value
            //    float knockbackDuration = 0.5f; // Adjust the duration to your desired value

            //    EnemyMovement enemyMovement = enemy.GetComponentInParent<EnemyMovement>();
            //    enemyMovement.ApplyKnockback(knockbackDirection, knockbackForce, knockbackDuration);
            //}

        }
        //Destroy(this);
    }

    IEnumerator SinkCountdown(float sinkCountdown)
    {
        yield return new WaitForSeconds(sinkCountdown);
        isSinking = true;
    }


    void SinkObject()
    {
        transform.position = new Vector3(transform.position.x,transform.position.y - (sinkSpeed * Time.deltaTime),transform.position.z);
        sinkSpeed += sinkSpeedIncrement;
    }

}
