using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXPlayer : MonoBehaviour
{
    [HideInInspector]public AudioClip audioClip;
    [SerializeField] private AudioSource audioSource;
    private void Start()
    {
        audioSource.clip = audioClip;
        audioSource.Play();
        StartCoroutine(DestroyAfterClip(audioClip.length));
    }

    private IEnumerator DestroyAfterClip(float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(this.gameObject);
    }
}
