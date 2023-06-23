using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSideClickerManager : SideClickerManager
{
    [SerializeField] ReadyUpManager readyUpManager;
    [SerializeField] Image bannerPlateImg;
    protected override void SelfChanged()
    {
        readyUpManager.HandleRoleValueChange(sideClickerValues[currentSideClick]);
        readyUpManager.ValueChangedServerRpc(currentSideClick);
    }

    protected override void OnValueChanged()
    {
        base.OnValueChanged();
        UpdateBackgroundImage();
    }
    void UpdateBackgroundImage()
    {

        bannerPlateImg.sprite = sideClickerValues[currentSideClick].rolePlate;
    }
}
