using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

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

    public void Disconnect()
    {
        CanvasManager.Instance.DisableAll();
       NetworkManager.Singleton.Shutdown();
        CanvasManager.Instance.ToggleLoadingScreen(true);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu Scene");
    }
}
