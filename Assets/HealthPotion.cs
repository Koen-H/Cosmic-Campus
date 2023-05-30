using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPotion : Potion
{
    [SerializeField] float healAmount = 100;
    protected override void UsePlayerPotion(GameObject other)
    {
        PlayerCharacterController player = other.GetComponentInParent<PlayerCharacterController>();
        player.Heal(healAmount);//Heal the player by 100%!

    }



}
