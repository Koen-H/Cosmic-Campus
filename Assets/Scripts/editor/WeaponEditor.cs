using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(WeaponData))]

public class WeaponEditor : Editor
{

        private WeaponData weapon;

        private void OnEnable()
        {
            weapon = (WeaponData)target;
        }


        public override void OnInspectorGUI()
        {

            base.OnInspectorGUI();

            serializedObject.Update();

            switch (weapon.weaponType)
            {
            case WeaponType.SWORD:
                   // enemy.meleeRange = EditorGUILayout.FloatField(nameof(enemy.meleeRange), enemy.meleeRange);
                    break;

                case WeaponType.BOW:
                    //enemy.projectileType = (EnemySO.ProjectileType)EditorGUILayout.EnumPopup("Projectile Type", enemy.projectileType);
                    //enemy.projectileSpeed = EditorGUILayout.FloatField(nameof(enemy.projectileSpeed), enemy.projectileSpeed);
                    break;
            case WeaponType.STAFF:
                //enemy.projectileType = (EnemySO.ProjectileType)EditorGUILayout.EnumPopup("Projectile Type", enemy.projectileType);
                //enemy.projectileSpeed = EditorGUILayout.FloatField(nameof(enemy.projectileSpeed), enemy.projectileSpeed);
                break;
            default:
                //hide all
                    break;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }