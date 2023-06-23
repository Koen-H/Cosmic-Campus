using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    [SerializeField] AudioSource calmMusic;
    [SerializeField] AudioSource battleMusic;
    [SerializeField] AudioSource bossMusic;

    private AudioSource currentMusic;

    [SerializeField] float maxVolume;
    [SerializeField] float fadeDuration;

    private Coroutine fade;



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

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            float t = currentTime / fadeDuration;
            fadeIn.volume = Mathf.Lerp(0f, maxVolume, t);
            currentMusic.volume = Mathf.Lerp(maxVolume, 0, t);
            yield return null;
        }
        currentMusic = fadeIn;
    }
}
