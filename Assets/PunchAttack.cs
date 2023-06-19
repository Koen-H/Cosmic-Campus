using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchAttack : EnemyAttackBehaviour
{

    [SerializeField, Tooltip("From what distance should we try the attack?")] 
    protected float attackRange = 2f;
    Animator attackAnim;


    [SerializeField] List<AttackCollider> attackColliders;
    
    List<Transform> hits = new List<Transform>();

    private void Awake()
    {
        base.Awake();
        foreach(AttackCollider atCol in attackColliders)
        {
            atCol.OnTriggerEnterEvent += OnAttackColliderEnter;
        }
        ToggleColliders(false);
    }
    private void OnDisable()
    {
        foreach (AttackCollider atCol in attackColliders)
        {
            atCol.OnTriggerEnterEvent -= OnAttackColliderEnter;
        }
    }

    public override void TryAttack()
    {
        Transform currentTarget = enemy.CurrentTarget;
        if (currentTarget == null) return;
        if ((currentTarget.position - transform.position).magnitude < attackRange) Attack();
        return;
    }

    protected override void Attack()
    {
        Attacked();
        //
        ToggleColliders(true);

        //Play the punch attack animation

        //For now...
        if (enemy.IsOwner)
        {
            enemy.enemyAnimationState.Value = EnemyAnimationState.SWORDSLASH;
        }

      /*  attackAnim = GetComponentInChildren<Animator>();
        attackAnim.SetTrigger("Animate");*/

        StartCoroutine(AfterAttackAnim(enemy.animator.GetCurrentAnimatorStateInfo(0).length));
    }

    void OnAttackColliderEnter(Transform enteredTransform)
    {
        if (!IsOwner) return;
        if (hits.Contains(enteredTransform)) return;
        hits.Add(enteredTransform);
        if (enteredTransform.TryGetComponent(out PlayerCharacterController player))
        {
            player.TakeDamage(damage);
        }
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
            foreach(Collider collider in colliders) collider.enabled = enabled;
        }
    }
}
