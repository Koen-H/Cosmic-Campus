using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OnTestSceneEnter : MonoBehaviour
{
    private void Start()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        LobbyManager.Instance.CreateCharacters(Vector3.one);

    }
}
