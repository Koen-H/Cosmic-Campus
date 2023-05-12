using Steamworks;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ClientManager : NetworkBehaviour
{
    [SerializeField] private ulong clientId;
    public NetworkVariable<string> playerName = null;
    public NetworkVariable<uint> steamAccountId = null;
    [SerializeField] public PlayerData playerData = null;

    GameObject playerCharacter = null;

    private void Start()
    {
        clientId = NetworkManager.Singleton.LocalClientId;
        if (SteamClient.IsLoggedOn == true)
        {
            playerName.Value = SteamClient.Name;
            steamAccountId.Value = SteamClient.SteamId.AccountId;
            this.gameObject.name = $"Client ({playerName.Value})";
        }
    }
}
