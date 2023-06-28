using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [SerializeField] private List<Effect> allEffects = new List<Effect>();
    [SerializeField] private List<Effect> attackEffects = new List<Effect>();
    [SerializeField] private List<Effect> movementEffects = new List<Effect>();
    [SerializeField] private List<Effect> resistanceEffects = new List<Effect>();

    public event System.Action OnEffectChange;

    public PlayerCharacterController player;
    public Enemy enemy;
    public bool isEnemy;
    public bool IsOwner;

    #region unity

    private void Awake()
    {
        player = GetComponent<PlayerCharacterController>();
        enemy = GetComponent<Enemy>();
    }

    private void Start()
    {
        if (enemy != null)
        {
            isEnemy = true;
            IsOwner = enemy.IsOwner;
        }
        else
        {
            IsOwner = player.IsOwner;
        }
    }

    #endregion

    #region effectsManagement
    public void AddEffect(Effect newEffect)
    {
        // Check if the effect type isn't in the list yet
        if (!allEffects.Exists(effect => effect.GetType() == newEffect.GetType()))
        {
            Effect instance = gameObject.AddComponent(newEffect.GetType()) as Effect;
            instance.CopyFrom(newEffect);
            instance.ApplyEffect();
            allEffects.Add(instance);
            instance.manager = this;
            if (instance.effectTypes == EffectType.ATTACK) attackEffects.Add(instance);
            if (instance.effectTypes == EffectType.MOVEMENT) movementEffects.Add(instance);
            if (instance.effectTypes == EffectType.RESISTANCE) resistanceEffects.Add(instance);
        }
        else
        {
            // If the effect is already in the list, reset it
            Effect existingEffect = allEffects.Find(effect => effect.GetType() == newEffect.GetType());
            existingEffect.ResetEffect(newEffect);
        }
        if (OnEffectChange != null) OnEffectChange.Invoke();
    }

    public void RemoveEffect(Effect oldEffect)
    {
        oldEffect.RemoveEffect();
        allEffects.Remove(oldEffect);
        if (oldEffect.effectTypes == EffectType.ATTACK) attackEffects.Remove(oldEffect);
        if (oldEffect.effectTypes == EffectType.MOVEMENT) movementEffects.Remove(oldEffect);
        if (oldEffect.effectTypes == EffectType.RESISTANCE) resistanceEffects.Remove(oldEffect);
        Destroy(oldEffect);
        if (OnEffectChange != null) OnEffectChange.Invoke();
    }
    #endregion

    #region effectsApplyment

    public float ApplyAttackEffect(float oldDamage)
    {
        float newDamage = oldDamage;
        foreach(Effect attackEffect in attackEffects)
        {
            newDamage = attackEffect.ApplyAttackEffect(newDamage);
        }
        return newDamage;
    }

    public float ApplyMovementEffect(float oldMovement)
    {
        float newMovement = oldMovement;
        foreach (Effect movementEffect in movementEffects)
        {
            newMovement = movementEffect.ApplyMovementEffect(newMovement);
        }
        return newMovement;
    }

    public float ApplyResistanceEffect(float oldIncDamage)
    {
        float newIncDamage = oldIncDamage;
        foreach (Effect resistanceEffect in resistanceEffects)
        {
            newIncDamage = resistanceEffect.ApplyResistanceEffect(newIncDamage);
        }
        return newIncDamage;
    }

    #endregion

}
