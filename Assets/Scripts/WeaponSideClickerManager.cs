using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSideClickerManager : SideClickerManager
{

    [SerializeField] private ReadyUpManager readyUpManager;
    [SerializeField] private GameObject current;

    protected override void OnValueChanged()
    {
        current.SetActive(false);
        current = sideClickerValues[currentSideClick].relatedObj;
        current.SetActive(true);
        CheckTaken();
    }


    public override void CheckTaken()
    {
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
    }

    public void SelectWeapon()
    {
        readyUpManager.SelectWeapon(int.Parse(sideClickerValues[currentSideClick].value));
        
    }

    public void TakeOption(bool take)
    {
        readyUpManager.TakeOptionServerRpc(sideClickerValues[currentSideClick].option, take);
        readyUpManager.ReadyUpServerRpc(take);

    }
}
