using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Effect : MonoBehaviour
{
    [SerializeField] public string effectName = "";
    public float duration = 1f;
    public float strength = 1f;

    public GameObject vfx;
    protected GameObject vfxInstance;

    [SerializeField, Tooltip("If it's a reference it will be used for applying and not doing anything by iteself.")] 
    bool isReference = false;

    [SerializeField] public EffectType effectTypes;
    [HideInInspector] public EffectManager manager;


    /// <summary>
    /// Simple countdown for when the effect should wear off.
    /// The effects duration can change mid-effect.
    /// </summary>
    /// <returns></returns>
    private IEnumerator EffectCountdown()
    {
        float oneSec = 0;
        while (duration > 0)
        {
            duration -= 0.1f;
            oneSec += 0.1f;
            yield return new WaitForSeconds(0.1f);
            if (oneSec >= 1)
            {
                oneSec = 0;
                EverySecond();
            }

        }
        manager.RemoveEffect(this);
    }

    /// <summary>
    /// Everysecond, this will be run. Usefull for burning or healing over time!
    /// </summary>
    public virtual void EverySecond() { }

    /// <summary>
    /// Reset the effect if the duration is longer
    /// Also increases the strength of the ffect if thew new effect is stronger.
    /// </summary>
    /// <param name="effect"></param>
    public void ResetEffect(Effect effect)
    {
        if (effect.duration > duration) duration = effect.duration;
        if (effect.strength > strength)
        {
            strength = effect.strength;
        }
    }

    /// <summary>
    /// At the start of the effect, could add some particles around the player
    /// </summary>
    public virtual void ApplyEffect()
    {
        if (vfx != null)
        {
            vfxInstance = Instantiate(vfx, transform);
            vfxInstance.transform.localPosition = new Vector3(0, 2, 0);//Todo: Temporary
        }
        StartCoroutine(EffectCountdown());
    }

    /// <summary>
    /// At the end of the effect, could remove some particles 
    /// </summary>
    public virtual void RemoveEffect()
    {
        if (vfxInstance != null) Destroy(vfxInstance);
    }

    public virtual void CopyFrom(Effect original)
    {
        duration = original.duration;
        strength = original.strength;
        vfx = original.vfx;
    }

    /// <summary>
    /// Applies the buff to the damage
    /// </summary>
    /// <param name="damage">The damage</param>
    /// <returns>The effect applied damage</returns>
    public virtual float ApplyAttackEffect(float damage)
    {
        Debug.LogWarning("Apply Damage Effect called but not implemented!");
        return damage;
    }

    /// <summary>
    /// Applies the buff to the movementSpeed
    /// </summary>
    /// <param name="movementSpeed">returns the buffed movementSpeed</param>
    /// <returns>The effect applied movement</returns>
    public virtual float ApplyMovementEffect(float movementSpeed)
    {
        Debug.LogWarning("Apply Movement Effect called but not implemented!");
        return movementSpeed;
    }

    /// <summary>
    /// Applies the resistance effect to the incoming damage
    /// </summary>
    /// <param name="incDamage">the damage</param>
    /// <returns>the effect applied incoming damage</returns>
    public virtual float ApplyResistanceEffect(float incDamage)
    {
        Debug.LogWarning("Apply Resistance Effect called but not implemented!");
        return incDamage;
    }

}
[System.Flags]
public enum EffectType { NONE = 0, ATTACK = 1, MOVEMENT = 2, RESISTANCE = 4, }