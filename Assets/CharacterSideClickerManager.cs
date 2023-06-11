using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterSideClickerManager : SideClickerManager
{
    [SerializeField] ReadyUpManager readyUpManager;

    protected override void SelfChanged()
    {
        readyUpManager.HandleRoleValueChange(sideClickerValues[currentSideClick]);
        readyUpManager.ValueChangedServerRpc(currentSideClick);
    }

    protected override void OnValueChanged()
    {
        base.OnValueChanged();

    }
}
