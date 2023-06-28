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

    /// <summary>
    /// Set the background volume
    /// </summary>
    /// <param name="newValue"></param>
    public void SetVolume(float newValue)
    {
        BackgroundMusicManager.Instance.SetVolume(newValue);
    }

    /// <summary>
    /// Disconnect the player and load back to the main menu.
    /// </summary>
    public void Disconnect()
    {
        CanvasManager.Instance.DisableAll();
        NetworkManager.Singleton.Shutdown();
        CanvasManager.Instance.ToggleLoadingScreen(true);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu Scene");
        CanvasManager.Instance.ToggleLoadingScreen(false);
    }
}
