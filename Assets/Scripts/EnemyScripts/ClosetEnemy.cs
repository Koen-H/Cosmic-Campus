using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosetEnemy : EnemyTargettingBehaviour
{
    //Gets the closest enemy, based on the third eye
    public override void FindTarget()
    {

        GetClosestPlayer();
        return;
    }

}