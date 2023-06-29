using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Sword : Weapon
{
    [SerializeField] List<AttackCollider> attackColliders;

    List<Transform> hits = new List<Transform>();

    void OnAttackColliderEnter(Transform enteredTransform)
    {
        if (hits.Contains(enteredTransform)) return;
        hits.Add(enteredTransform);
        if (enteredTransform.CompareTag("Enemy"))
        {
            playerController.playerSounds.swordHit.Play();
            if (!playerController.IsOwner) return;
            //Temporary for testing
            float heal = weaponData.damageHeal.GetRandomValue();
            if (playerController.health.Value + heal > playerController.maxHealth.Value)
            {
                heal = playerController.maxHealth.Value - playerController.health.Value;
            }
            playerController.health.Value += heal;



            Enemy enemy = enteredTransform.GetComponentInParent<Enemy>();
            float damage = playerController.effectManager.ApplyAttackEffect(weaponData.damage.GetRandomValue());
            enemy.TakeDamage(damage, playerController.damageType);
            enemy.GetComponent<EnemyMovement>().ApplyKnockback((enemy.transform.position - playerController.transform.position).normalized ,2f,1f);
        }
    }


    private void Start()
    {
        weaponObj.transform.parent = FindDeepChild(this.transform, "Tool_bone");
        ResetChildTransforms(weaponObj.transform, 2);
        weaponAnimation = GetComponentInChildren<Animator>();
        attackColliders = GetComponentsInChildren<AttackCollider>().ToList();
        foreach (AttackCollider atCol in attackColliders)
        {
            atCol.OnTriggerEnterEvent += OnAttackColliderEnter;
        }
        ToggleColliders(false);
    }


    protected override void AfterAttack()
    {
        base.AfterAttack();
        ToggleColliders(false);
        hits.Clear();
    }

    void ToggleColliders(bool enabled)
    {
        foreach (AttackCollider atCol in attackColliders)
        {
            Collider[] colliders = atCol.GetComponents<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.gameObject.SetActive(enabled);//The vfx is on this object as a child!
                collider.enabled = enabled;
            }
        }
    }
    public void ResetChildTransforms(Transform parentTransform, int depth)
    {
        // Reset the local position, rotation, and scale of the parent first
        parentTransform.localPosition = Vector3.zero;
        //Vector3(10.699996, 169, 58.6)
        parentTransform.localRotation = Quaternion.Euler(169, -11, -121);// Set(169, -11, -121,0);//   new Vector3(10.699996f, 169f, 58.6f); Quaternion.EulerAngles(196 * Mathf.Deg2Rad, (-83 + 20) * Mathf.Deg2Rad, (-156 -90) * Mathf.Deg2Rad);
        //parentTransform.localScale = Vector3.one;

        // Log the reset operation with depth indication
        Debug.Log(new string('-', depth) + " Reset: " + parentTransform.name);

        // Then do the same for each of its children
        foreach (Transform childTransform in parentTransform)
        {
           // ResetChildTransforms(childTransform, depth + 1);
        }
    }

    /// <summary>
    /// When the player starts with the input
    /// </summary>
    public override void OnAttackInputStart()
    {
        if (weaponState != WeaponState.READY) return;
        Aim();
        playerController.AttackServerRpc();
        Attack();
        playerController.ToggleMovement(false);
    }
    /// <summary>
    /// While the player is holding the input
    /// </summary>
    public override void OnAttackInputHold()
    {
        if (weaponState != WeaponState.READY) return;
        playerController.AttackServerRpc();
        Attack();
        playerController.ToggleMovement(false);
    }
    /// <summary>
    /// When the player lets go of the input
    /// </summary>
    public override void OnAttackInputStop()
    {

    }

    /// <summary>
    /// For when the enemy stuns/cancels the player during the attack.
    /// </summary>
    public override void CancelAttack()
    {
        AfterAttack();
    }

    public override void Attack()
    {
        Aim();
        base.Attack();
        //
        playerController.playerSounds.swordSlash.Play();
        ToggleColliders(true);

        if(playerController.IsOwner) playerController.playerAnimationState.Value = PlayerAnimationState.SWORDSLASH;
       // AnimatorClipInfo[] clips = weaponAnimation.GetCurrentAnimatorClipInfo(0);
        //Debug.Log();//.GetCurrentAnimatorStateInfo(0)
        StartCoroutine(AfterAnim(1.083f/2));
    }
    IEnumerator AfterAnim(float duration)
    {
        yield return new WaitForSeconds(duration);
        AfterAttack();
    }

    //public void DealDamage(GameObject enemyObject)
    //{
    //    if (!weaponAnimation) return;
    //    if (weaponAnimation.GetCurrentAnimatorStateInfo(0).IsName("MainBones|SwordSlashAnimation"))
    //    {
    //        Enemy enemy = enemyObject.transform.parent.GetComponent<Enemy>();
    //        if (enemy == null) return;
    //        float damage = playerController.effectManager.ApplyAttackEffect(weaponData.damage.GetRandomValue());
    //        enemy.TakeDamage(damage, playerController.damageType);
    //    }
    //}

}
