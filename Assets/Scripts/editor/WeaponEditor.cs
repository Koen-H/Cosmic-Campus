using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WeaponData))]
public class WeaponEditor : Editor
{
    private WeaponData weapon;
    private SerializedProperty weaponTypeProperty;
    private SerializedProperty nickNameProperty;
    private SerializedProperty weaponPrefabProperty;
    private SerializedProperty weaponObjOffsetProperty;
    private SerializedProperty damageProperty;
    private SerializedProperty damageHealProperty;
    private SerializedProperty speedProperty;
    private SerializedProperty cooldownProperty;
    private SerializedProperty rangeProperty;
    private SerializedProperty projectilePrefabProperty;
    private SerializedProperty maxChargeTimeProperty;
    private SerializedProperty chargeProjectileSpeedProperty;
    private SerializedProperty chargeProjectileDamageProperty;
    private SerializedProperty accuracyProperty;
    private SerializedProperty beamVfxProperty;

    private void OnEnable()
    {
        weapon = (WeaponData)target;
        weaponTypeProperty = serializedObject.FindProperty("weaponType");
        nickNameProperty = serializedObject.FindProperty("nickName");
        weaponPrefabProperty = serializedObject.FindProperty("weaponPrefab");
        weaponObjOffsetProperty = serializedObject.FindProperty("weaponObjOffset");
        damageProperty = serializedObject.FindProperty("damage");
        damageHealProperty = serializedObject.FindProperty("damageHeal");
        speedProperty = serializedObject.FindProperty("speed");
        cooldownProperty = serializedObject.FindProperty("cooldown");
        rangeProperty = serializedObject.FindProperty("range");
        projectilePrefabProperty = serializedObject.FindProperty("projectilePrefab");
        maxChargeTimeProperty = serializedObject.FindProperty("maxChargeTime");
        chargeProjectileSpeedProperty = serializedObject.FindProperty("chargeProjectileSpeed");
        chargeProjectileDamageProperty = serializedObject.FindProperty("chargeProjectileDamage");
        accuracyProperty = serializedObject.FindProperty("accuracy");
        beamVfxProperty = serializedObject.FindProperty("beamVfx");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(weaponTypeProperty);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        switch (weapon.weaponType)
        {
            case WeaponType.SWORD:
                EditorGUILayout.PropertyField(nickNameProperty);
                EditorGUILayout.PropertyField(weaponPrefabProperty);
                EditorGUILayout.PropertyField(weaponObjOffsetProperty);
                EditorGUILayout.PropertyField(damageProperty);

                EditorGUILayout.PropertyField(damageHealProperty);


                break;
            case WeaponType.BOW:
                EditorGUILayout.PropertyField(nickNameProperty);
                EditorGUILayout.PropertyField(weaponPrefabProperty);
                EditorGUILayout.PropertyField(weaponObjOffsetProperty);
                EditorGUILayout.PropertyField(damageProperty);

               // EditorGUILayout.PropertyField(speedProperty);
                EditorGUILayout.PropertyField(cooldownProperty);

                EditorGUILayout.PropertyField(projectilePrefabProperty);
                EditorGUILayout.PropertyField(maxChargeTimeProperty);
                EditorGUILayout.PropertyField(chargeProjectileSpeedProperty);
                EditorGUILayout.PropertyField(chargeProjectileDamageProperty);

                break;
            case WeaponType.STAFF:
                EditorGUILayout.PropertyField(nickNameProperty);
                EditorGUILayout.PropertyField(weaponPrefabProperty);
                EditorGUILayout.PropertyField(weaponObjOffsetProperty);
                EditorGUILayout.PropertyField(damageProperty);
                EditorGUILayout.PropertyField(cooldownProperty);

                EditorGUILayout.PropertyField(rangeProperty);
                EditorGUILayout.PropertyField(accuracyProperty);
                EditorGUILayout.PropertyField(beamVfxProperty);


                break;
            default:
                HideAllFields();
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void HideAllFields()
    {
        EditorGUI.BeginChangeCheck();

        weaponTypeProperty.enumValueIndex = EditorGUILayout.Popup("Weapon type", weaponTypeProperty.enumValueIndex, weaponTypeProperty.enumDisplayNames);

        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();

        EditorGUI.BeginChangeCheck();

        var serializedProperties = serializedObject.GetIterator();
        while (serializedProperties.NextVisible(true))
        {
            if (serializedProperties.name != "m_Script" && serializedProperties.name != "weaponType")
            {
                EditorGUILayout.PropertyField(serializedProperties, true);
            }
        }

        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }
}
