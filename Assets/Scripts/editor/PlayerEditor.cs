using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerSO))]
public class PlayerEditor : Editor
{

    private PlayerSO player;
    private void OnEnable()
    {
        player = (PlayerSO)target;

    }

    public override void OnInspectorGUI()
    {

        base.OnInspectorGUI();

        serializedObject.Update();

        switch (player.weaponType)
        {
            case PlayerSO.WeaponType.Bow:
                player.projectileSpeed = EditorGUILayout.FloatField(nameof(player.projectileSpeed), player.projectileSpeed);
                break;
            default:
                break;
        }
        serializedObject.ApplyModifiedProperties();
    }


}
