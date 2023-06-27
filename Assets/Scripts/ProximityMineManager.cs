using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ProximityMineManager : NetworkBehaviour
{
    [Tooltip("How long does the bomb stay?")]
    [SerializeField] private Range despawnTimer = new Range(10,11);//When they despawn, they don't need to explode all at once!
    [Tooltip("After something entered, how long till it will explode?")]
    [SerializeField] private float proximityTimer = 2;
    private bool exploding = false;
    [SerializeField] private float explosionRange = 2;

    [SerializeField] private float enemyDamage = 100;
    [SerializeField] private float playerDamage = 10;

    [SerializeField] private float playerKnockbackForce = 25;
    [SerializeField] private float playerKnockbackDuration = 0.5f;

    [SerializeField] private ParticleSystem explosionVfx;
    [SerializeField] private ParticleSystem mineVfx;
    [SerializeField] private GameObject mesh;

    public override void OnNetworkSpawn()
    {
        if (IsOwner) { StartCoroutine(DespawnTimer(despawnTimer.GetRandomValue())); }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6) return;//Ground layer? We don't explode by ground
        if (other.CompareTag("Enemy") || other.CompareTag("InvincibleEnemy")) return;
        if (IsOwner) ExplodeClientRpc();
    }

    IEnumerator ProximityTimer()
    {
        ///SOMEONE WALKED IN AND TRIGGERED IT! DO THINGS!
        bool toggle = false;
        while(proximityTimer > 0)
        {
            proximityTimer -= 0.1f;
            mesh.SetActive(toggle);
            yield return new WaitForSeconds(0.1f);
            toggle = !toggle;
        }
        if (IsOwner) Explode();

    }

    IEnumerator DespawnTimer(float despawnTimer)
    {
        yield return new WaitForSeconds(despawnTimer);
        if (exploding) yield return null;//Proximity is activated, wait for that to finish
        ExplodeClientRpc(); 
    }

    [ClientRpc]
    private void ExplodeClientRpc()
    {
        exploding = true;
        StartCoroutine(ProximityTimer());
    }

    

    private void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRange);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                Enemy enemy = collider.GetComponentInParent<Enemy>();
                enemy.TakeDamage(enemyDamage, EnemyType.NONE);
            }
            else if (collider.CompareTag("Player"))
            {
                PlayerCharacterController player = collider.GetComponent<PlayerCharacterController>();

                Vector3 knockbackDirection = player.transform.position - transform.position;
                knockbackDirection = new Vector3(knockbackDirection.x, 0, knockbackDirection.z);
                player.TakeDamageClientRpc(playerDamage,false);
                player.ApplyKnockback(knockbackDirection.normalized, playerKnockbackForce, playerKnockbackDuration);
            }
        }
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        //Spawn the explosion vfx!
        Instantiate(explosionVfx, this.transform.position, Quaternion.identity);
    }
}
