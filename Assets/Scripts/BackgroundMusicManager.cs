using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    [Header("Background Music")]
    [SerializeField] private AudioSource calmMusic;
    [SerializeField] private AudioSource battleMusic;
    [SerializeField] private AudioSource bossMusic;

    private AudioSource currentMusic;

    [SerializeField] private float maxVolume;
    [SerializeField] private float fadeDuration;

    private Coroutine fade;
    [SerializeField] private bool groundBasedMusic = true;

    private string lastTag = "";

    private static BackgroundMusicManager _instance;
    public static BackgroundMusicManager Instance
    {
        get
        {
            if (_instance == null) Debug.LogError("BackgroundMusicManager is null");
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null) { 
            Destroy(gameObject);
            return;
        }
        _instance= this;
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        currentMusic = calmMusic;
        calmMusic.volume = maxVolume;

    }

    public void LoadDefault()
    {
        groundBasedMusic = true;
        PlayCalmMusic();
    }

    public void SetVolume(float newVolume)
    {
        maxVolume = newVolume;
        currentMusic.volume = maxVolume;
    }

    public void HandleGroundMusic(string groundTag)
    {
        if (!groundBasedMusic) return;
        if (lastTag == groundTag) return;
        switch (groundTag)
        {
            case "RainbowRoad":
                PlayCalmMusic();
                lastTag = groundTag;
                break;
            case "Ground":
                PlayBattleMusic();
                lastTag = groundTag;
                break;
        }
    }

    public void PlayCalmMusic()
    {
        if (fade != null) { StopCoroutine(fade); }
        fade = StartCoroutine(FadeMusic(calmMusic));
    }
    public void PlayBattleMusic()
    {
        if (fade != null) { StopCoroutine(fade); }
        fade = StartCoroutine(FadeMusic(battleMusic));
    }
    public void PlayBossMusic()
    {
        groundBasedMusic = false;
        fade = StartCoroutine(FadeMusic(bossMusic));
    }

    IEnumerator FadeMusic(AudioSource fadeIn)
    {
        float currentTime = 0f;
        AudioSource oldMusic = currentMusic;
        float oldMusicVolume = oldMusic.volume;
        currentMusic = fadeIn;
        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            float t = currentTime / fadeDuration;
            fadeIn.volume = Mathf.Lerp(0f, maxVolume, t);
            oldMusic.volume = Mathf.Lerp(oldMusicVolume, 0, t);
            yield return null;
        }
        oldMusic.volume = 0;
        currentMusic.volume = maxVolume;
    }
}
