using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Potion : NetworkBehaviour
{
    [SerializeField] GameObject potion;
    public void SpawnPotion()
    {
        if(!IsServer) Destroy(gameObject);
        else
        {
            GameObject instance = Instantiate(potion, this.transform.position, this.transform.rotation);
            instance.GetComponent<NetworkObject>().Spawn();
        }
    }
    private void Start()
    {
        if(!IsServer) Destroy(gameObject);
    }


    [Tooltip("Can a enemy use this potion?")]
    [SerializeField] bool allowEnemyPikcup;
    private void OnTriggerEnter(Collider other)
    {
        if(!IsOwner) return;
        if (other.CompareTag("Player"))
        {
            UsePlayerPotion(other.gameObject);
            OnPotionUsed();
        }
        else if (allowEnemyPikcup && other.CompareTag("Enemy"))
        {
            UseEnemyPotion(other.gameObject);
            OnPotionUsed();
        }
    }

    /// <summary>
    /// When the player picks up the potion, we do player related things
    /// </summary>
    /// <param name="other">The gameobject of the player.</param>
    protected virtual void UsePlayerPotion(GameObject other)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// When the enemy picks up the potion, we do enemy related things
    /// </summary>
    /// <param name="other">The gameobject of the enemy.</param>
    protected virtual void UseEnemyPotion(GameObject other)
    {
        throw new System.NotImplementedException();
    }

    private void OnPotionUsed()
    {
        //some cool thing happen here or we simply destroy it.
        Destroy(gameObject);
    }
}
