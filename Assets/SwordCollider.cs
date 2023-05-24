using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordCollider : MonoBehaviour
{
    public Sword swrod; 

    private void OnTriggerEnter(Collider other)
    {
        if (!swrod) return;
        Debug.Log(other.gameObject);
        swrod.DealDamage(other.gameObject); 
    }




}
