using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class CharacterSelectManager : MonoBehaviour
{
    private PlayerRoleData playerRoleData = null;
    [SerializeField] private TMP_InputField avatarIdInput;
    [SerializeField] private TMP_InputField weaponIdInput;
    [SerializeField] private GameObject avatarShowcase;
    [SerializeField] private GameObject weaponShowcase;


    private PlayerRole playerRole;
    private int avatarId;
    private int weaponId;


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
        string newWeaponIdstr = weaponIdInput.text;
        if (newWeaponIdstr == "") return;
        weaponId = int.Parse(newWeaponIdstr);
        
        GameObject newAvatar = playerRoleData.GetAvatar(avatarId);
        foreach (Transform child in avatarShowcase.transform) Destroy(child.gameObject);
        Instantiate(newAvatar, avatarShowcase.transform);
        GameObject newWeapon = playerRoleData.GetWeapon(weaponId).weaponPrefab;
        foreach (Transform child in weaponShowcase.transform) Destroy(child.gameObject);
        Instantiate(newWeapon, weaponShowcase.transform);
    }

    public void ConfirmPlayerCharacter()
    {
        ClientManager.MyClient.playerData.SetData(playerRole, avatarId, weaponId);
    }


}
