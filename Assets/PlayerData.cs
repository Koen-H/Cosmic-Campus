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
    public NetworkVariable<PlayerRole> playerRole = null;
    public NetworkVariable<int> avatarId = null;


    
}
public enum PlayerRole { ARTIST, DESIGNER, ENGINEER}