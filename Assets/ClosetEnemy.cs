using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosetEnemy : EnemyTargettingBehaviour
{
    public override void FindTarget()
    {

        GetClosestPlayer();
        return;
    }

}