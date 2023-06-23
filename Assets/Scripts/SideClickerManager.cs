using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class SideClickerManager : MonoBehaviour
{
    //[SerializeField] ReadyUpManager readyUpManager;
    [SerializeField] protected List<SideClickerValue> sideClickerValues;
    protected int currentSideClick;
    [SerializeField] private TextMeshProUGUI displayText;
    public event System.Action<SideClickerValue, int> OnValueChangedEvent;


    protected void OnEnable()
    {
        UpdateValue(0);
    }

    public void NextValue()
    {
        if (currentSideClick == sideClickerValues.Count -1) currentSideClick = 0;
        else currentSideClick++;
        SelfChanged();
        OnValueChanged();
    }

    public void PrevValue() 
    {
        if (currentSideClick == 0) currentSideClick = sideClickerValues.Count - 1;
        else currentSideClick--;
        SelfChanged();
        OnValueChanged();
    }
    protected virtual void OnValueChanged()
    {
        UpdateText();
        if(OnValueChangedEvent !=null) OnValueChangedEvent.Invoke(sideClickerValues[currentSideClick], currentSideClick);
    }

    public void UpdateValue(int newValue)
    {
        currentSideClick = newValue;
        OnValueChanged();
    }

    void UpdateText()
    {
        displayText.text = sideClickerValues[currentSideClick].display;
    }

    public string GetValue()
    {
        return sideClickerValues[currentSideClick].value;
    }
    
    /// <summary>
    /// When the variable is changed by the user themself.
    /// </summary>
    protected virtual void SelfChanged() { }
}

[System.Serializable]
public class SideClickerValue
{
    [SerializeField]public string display;
    [SerializeField]public string value;
    [SerializeField] public Sprite rolePlate;
}
