using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{

    [SerializeField] Slider volumeSlider;

    private void Awake()
    {
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float newValue)
    {
        BackgroundMusicManager.Instance.SetVolume(newValue);
    }
}
