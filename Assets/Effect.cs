using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    [System.Flags]
    public enum EffectType {NONE = 0, ATTACK = 1, MOVEMENT = 2, RESISTANCE = 4,}
   
    float duration = 1.0f;
    [SerializeField] EffectType type;

    public void ResetEffect()
    {

    }

}
