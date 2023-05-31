using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SideClickerManager : MonoBehaviour
{
    [SerializeField] List<SideClickerValue> sideClickerValues;
    int currentSideClick;
    [SerializeField] TextMeshProUGUI displayText;


    public void NextValue()
    {
        if (currentSideClick == sideClickerValues.Count -1) currentSideClick = 0;
        else currentSideClick++;
        UpdateText();
    }

    public void PrevValue() 
    {
        if (currentSideClick == 0) currentSideClick = sideClickerValues.Count - 1;
        else currentSideClick--;
        UpdateText();
    }


    void UpdateText()
    {
        displayText.text = sideClickerValues[currentSideClick].display;
    }

    public string GetValue()
    {
        return sideClickerValues[currentSideClick].value;
    }

}

[System.Serializable]
public class SideClickerValue
{
    [SerializeField]public string display;
    [SerializeField]public string value;
}
