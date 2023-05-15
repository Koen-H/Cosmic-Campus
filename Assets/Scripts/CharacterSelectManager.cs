using Palmmedia.ReportGenerator.Core.Common;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class CharacterSelectManager : MonoBehaviour
{
    PlayerRoleData playerRoleData = null;
    [SerializeField] TMP_InputField avatarIdInput;
    [SerializeField] GameObject avatarShowcase;

    PlayerRole playerRole;
    int avatarId;


    public void AssignRole(int newRoleInt)
    {
        playerRole = (PlayerRole)newRoleInt;
        switch (playerRole)
        {
            case PlayerRole.ARTIST:
                playerRoleData = GameData.Instance.artistData;
                Debug.Log("Assigned role: Artist");
                break;

            case PlayerRole.DESIGNER:
                playerRoleData = GameData.Instance.designerData;
                Debug.Log("Assigned role: Designer");
                break;

            case PlayerRole.ENGINEER:
                playerRoleData = GameData.Instance.engineerData;
                Debug.Log("Assigned role: Engineer");
                break;

            default:
                Debug.LogWarning("Unknown role!");
                break;
        }
        UpdatePlayerCharacter();
    }

    void UpdatePlayerCharacter()
    {
        string newAvatarIdstr = avatarIdInput.text;
        if (newAvatarIdstr == "") return;
        avatarId = int.Parse(newAvatarIdstr);
        GameObject newAvatar = playerRoleData.GetAvatar(avatarId);

        foreach (Transform child in avatarShowcase.transform) Destroy(child.gameObject);

        Instantiate(newAvatar, avatarShowcase.transform);
    }

    public void ConfirmPlayerCharacter()
    {
        ClientManager.MyClient.playerData.SetData(playerRole, avatarId);
       //NetworkManager.LocalClient.PlayerObject
    }


}
