using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemySO))]
public class EnemyEditor : Editor
{


        private EnemySO enemy;

        private void OnEnable()
        {
            enemy = (EnemySO)target;
        
        }


        public override void OnInspectorGUI()
        {

            base.OnInspectorGUI();

            serializedObject.Update();

            switch (enemy.enemyType)
            {
                case EnemySO.EnemyType.Melee:
                enemy.meleeRange = EditorGUILayout.FloatField(nameof(enemy.meleeRange), enemy.meleeRange);

                break;

                case EnemySO.EnemyType.Range:
                enemy.projectileType = (EnemySO.ProjectileType)EditorGUILayout.EnumPopup("Projectile Type", enemy.projectileType);


                enemy.projectileSpeed = EditorGUILayout.FloatField(nameof(enemy.projectileSpeed), enemy.projectileSpeed);
                    break;
                default:
                    break;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
