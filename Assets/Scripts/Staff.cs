using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.Netcode;
using UnityEngine;

public class Staff : Weapon
{
    /// <summary>
    /// When the player starts with the input
    /// </summary>
    public override void OnAttackInputStart()
    {
        Aim();
        Attack();
        playerController.ToggleMovement(false);
    }
    /// <summary>
    /// While the player is holding the input
    /// </summary>
    public override void OnAttackInputHold()
    {
        Aim();
        playerController.AttackServerRpc();//Tell the server, that we are attacking!
        Attack();
    }
    /// <summary>
    /// When the player lets go of the input
    /// </summary>
    public override void OnAttackInputStop()
    {
        Aim();
        playerController.ToggleMovement(true);
    }

    /// <summary>
    /// For when the enemy stuns/cancels the player during the attack.
    /// </summary>
    public override void CancelAttack()
    {

    }

    public override void Attack()
    {
        FindClosestEnemy();
    }

    void FindClosestEnemy()
    {
        Vector3 playerPosition = playerController.playerObj.transform.position;
        Vector3 playerForward = playerController.playerObj.transform.forward;
        Collider[] colliders = Physics.OverlapSphere(transform.position + playerForward * weaponData.range, weaponData.range);
        List<Enemy> enemies = new List<Enemy>();
        List<Enemy> orderedEnemies = new List<Enemy>();

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                enemies.Add(collider.GetComponentInParent<Enemy>());
            }
        }
        if (enemies.Count == 0) return;
        List<Enemy> unorderedEnemies = new List<Enemy>(enemies);

        Enemy closestEnemy = GetClosestEnemy(playerPosition, unorderedEnemies);
        orderedEnemies.Add(closestEnemy);
        unorderedEnemies.Remove(closestEnemy);

        while (orderedEnemies.Count < enemies.Count)
        {
            closestEnemy = GetClosestEnemy(orderedEnemies.Last().transform.position, unorderedEnemies);
            orderedEnemies.Add(closestEnemy);
            unorderedEnemies.Remove(closestEnemy);
        }

        int i = 1;
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, weaponObj.transform.position);
        bool doDamage = false;
        if (playerController.IsOwner && weaponState == WeaponState.READY) doDamage = true;
        float damage = weaponData.damage.GetRandomValue(); 
        foreach (Enemy enemy in orderedEnemies)
        {
            lineRenderer.SetPosition(i, enemy.avatar.transform.position);
            if (doDamage) damage = DealDamage(enemy, damage);
            i++;
        }

        //orderedEnemies;











        //int numRaycasts = 5;
        //float maxDistance = weaponData.range;
        //float angle = weaponData.accuracy;
        //float startAngle = -angle * (numRaycasts - 1) * 0.5f;

        //float closestDistance = Mathf.Infinity;
        //GameObject closestEnemy = null;

        ////Shoot 5 rays forward
        //// Iterate over the number of raycasts
        //for (int i = 0; i < numRaycasts; i++)
        //{
        //    float currentAngle = startAngle + i * angle;

        //    Vector3 raycastDirection = Quaternion.Euler(0f, currentAngle, 0f) * playerForward;
        //    RaycastHit hit;
        //    Debug.DrawRay(playerPosition + Vector3.up, raycastDirection * maxDistance, Color.red, 0.01f);

        //    if (Physics.Raycast(playerPosition + Vector3.up, raycastDirection, out hit, maxDistance))
        //    {
        //        Debug.Log($"Hit {hit.collider.name}");
        //        if (hit.collider.CompareTag("Enemy"))
        //        {
        //            float distance = Vector3.Distance(playerPosition, hit.collider.transform.position);

        //            if (distance < closestDistance)
        //            {
        //                closestDistance = distance;
        //                closestEnemy = hit.collider.gameObject;
        //            }
        //        }
        //    }
        //}

        //if (closestEnemy != null)
        //{
        //    //draw fancy laser line
        //    Debug.DrawLine(playerPosition + Vector3.up, closestEnemy.transform.position + Vector3.up, Color.yellow, 1f);
        //    ShowStaffBeam(weaponObj.transform,closestEnemy.transform);
        //    if (playerController.IsOwner && weaponState == WeaponState.READY) DealDamage(closestEnemy);//Only on the client that owns the weapon, we do damage!
        //}

    }

    Enemy GetClosestEnemy(Vector3 position, List<Enemy> enemies)
    {
        if (enemies.Count == 1) return enemies.First();
        Enemy closestEnemy = enemies.First();
        float closestDistance = (position - closestEnemy.transform.position).magnitude;

        for (int i = 1; i < enemies.Count; i++)
        {
            Enemy enemy = enemies[i];
            float distance = (position - enemy.transform.position).magnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }

    void ShowStaffBeam(Transform p1, Transform p2)
    {
        //TEMPORARY AWFULL
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, p1.position);
        lineRenderer.SetPosition(1, p2.position);
    }

    float DealDamage(Enemy enemy,float prevDam)
    {
        enemy.TakeDamage(prevDam);
        float damage = prevDam * 0.7f; 
        base.Attack();//Handle cooldown?
        return damage;
    }
}

