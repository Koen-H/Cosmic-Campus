using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SideClickerManager : MonoBehaviour
{
    //[SerializeField] ReadyUpManager readyUpManager;
    [SerializeField] protected List<SideClickerValue> sideClickerValues;
    protected int currentSideClick;
    [SerializeField] private TextMeshProUGUI displayText;
    public event System.Action<SideClickerValue, int> OnValueChangedEvent;
    [SerializeField]protected Button selectButton;
    [SerializeField] protected TextMeshProUGUI selectButtonText;
    [SerializeField] protected Button deselectButton;

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

    protected void UpdateText()
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


    public virtual void CheckTaken()
    {

    }


}

[System.Serializable]
public class SideClickerValue
{
    [SerializeField]public string display;
    [SerializeField]public string value;
    [SerializeField] public Sprite rolePlate;
    [SerializeField] public ReadyOption option;
    [SerializeField] public GameObject relatedObj;
}
