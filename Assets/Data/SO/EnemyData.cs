using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "EnemyData")]

//Scriptable objects has references to the avatars an weapons.
public class EnemyData : ScriptableObject
{
    [SerializeField] private List<GameObject> avatars;
    [SerializeField] private List<GameObject> weapons;

    public int AvatarCount()
    {
        return avatars.Count;
    }
    public int WeaponsCount()
    {
        return weapons.Count;
    }

    public GameObject GetAvatar(int index) {
        index = Mathf.Clamp(index % avatars.Count, 0, avatars.Count);
        return avatars[index]; 
    }
    public GameObject GetWeapon(int index) {
        index = Mathf.Clamp(index % weapons.Count, 0, weapons.Count);
        return weapons[index]; 
    }
}
