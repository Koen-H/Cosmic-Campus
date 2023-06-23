using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSideClickerManager : SideClickerManager
{
    [SerializeField] ReadyUpManager readyUpManager;
    [SerializeField] Image bannerPlateImg;

    private void Start()
    {
        selectButton.onClick.AddListener(() => TakeOption(true));
        deselectButton.onClick.AddListener(() => TakeOption(false));
    }

    void TakeOption(bool take)
    {
        readyUpManager.TakeOptionServerRpc(sideClickerValues[currentSideClick].option, take);
    }


    protected override void SelfChanged()
    {
        readyUpManager.HandleRoleValueChange(sideClickerValues[currentSideClick]);
        readyUpManager.ValueChangedServerRpc(currentSideClick);
    }

    protected override void OnValueChanged()
    {
        base.OnValueChanged();
        UpdateBackgroundImage();
        CheckTaken();
    }
    void UpdateBackgroundImage()
    {
        bannerPlateImg.sprite = sideClickerValues[currentSideClick].rolePlate;
    }

    public override void CheckTaken()
    {
        //if (readyUpManager.optionsTaken.Contains(sideClickerValues[currentSideClick].option))
        bool isTaken = readyUpManager.optionsTaken.Contains(sideClickerValues[currentSideClick].option);
        if (isTaken)
        {
            selectButton.interactable = false;
            selectButtonText.text = "Taken";
        }
        else
        {
            selectButton.interactable = true;
            selectButtonText.text = "Select";
        }
        Debug.Log(sideClickerValues[currentSideClick].option);
    }
}
