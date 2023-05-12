using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Contains all the data of a player.
/// </summary>
public class PlayerData : NetworkBehaviour
{
    //Networkvariables
    public NetworkVariable<PlayerRole> playerRole = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);//What role is the player?
    public NetworkVariable<int> avatarId = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);//What avatar from that role is the player using?
    public NetworkVariable<int> weaponId = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);//What weapon from that role is the player using?

    /// <summary>
    /// Set the player Data
    /// </summary>
    public void SetData(PlayerRole newPlayerRole, int newAvatarId)
    {
        playerRole.Value = newPlayerRole;
        avatarId.Value = newAvatarId;
        Debug.Log("Player data set!");
    }

}
public enum PlayerRole { ARTIST, DESIGNER, ENGINEER}