using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPotion : Potion
{
    protected override void UsePlayerPotion(GameObject other)
    {
        PlayerCharacterController player = other.GetComponentInParent<PlayerCharacterController>();
        player.Heal(100);//Heal the player by 100%!

    }



}
