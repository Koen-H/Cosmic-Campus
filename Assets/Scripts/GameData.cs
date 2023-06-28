using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class GameData : MonoBehaviour
{
    public PlayerRoleData artistData;
    public PlayerRoleData designerData;
    public PlayerRoleData engineerData;

    private static GameData _instance;
    public static GameData Instance
    {
        get
        {
            if (_instance == null) Debug.LogError("GameData is null");
            return _instance;
        }
    }


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            _instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }
}
