using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "ListSO")]
public class ListSO : ScriptableObject
{
   [SerializeField] private List<GameObject> gameObjects= new List<GameObject>();


    public GameObject GetGameObject(int index)
    {
        return gameObjects[index];
    }

    public int GetCount()
    {
        return gameObjects.Count;
    }
}
