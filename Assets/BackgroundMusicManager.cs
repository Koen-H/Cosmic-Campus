using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    [SerializeField] AudioSource calmMusic;
    [SerializeField] AudioSource battleMusic;
    [SerializeField] AudioSource bossMusic;

    [SerializeField] private AudioSource currentMusic;

    [SerializeField] float maxVolume;
    [SerializeField] float fadeDuration;

    private Coroutine fade;
    [SerializeField] bool groundBasedMusic = true;


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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) PlayCalmMusic();
        else if (Input.GetKeyDown(KeyCode.J)) PlayBattleMusic();
        else if (Input.GetKeyDown(KeyCode.K)) PlayBossMusic();
    }

    public void SetVolume(float newVolume)
    {
        maxVolume = newVolume;
        currentMusic.volume = maxVolume;
    }

    string lastTag = "";
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
    void PlayBossMusic()
    {
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
