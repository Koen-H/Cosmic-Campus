using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    [SerializeField] GameObject loadingScreen;
    [SerializeField] TextMeshProUGUI loadingHint;
    private static CanvasManager _instance;
    public static CanvasManager Instance
    {
        get
        {
            if (_instance == null) Debug.LogError("CanvasManager is null");
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }


    public void ToggleLoadingScreen(bool toggle)
    {
        loadingScreen.SetActive(toggle);
        if (toggle) loadingHint.text = "Please do not jump off the map";
    }

}
