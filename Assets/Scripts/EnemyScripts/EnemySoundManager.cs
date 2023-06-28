using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySoundManager : MonoBehaviour
{
    public AudioSource enemyAttack;
    public AudioSource enemyShoot;
    [SerializeField] private AudioClip enemyDeath;
    [SerializeField] private SFXPlayer sfxPlayer;

    /// <summary>
    /// Spawns the sfx
    /// </summary>
    public void PlayDeathSFX()
    {
        Instantiate(sfxPlayer, transform.position, Quaternion.identity).audioClip = enemyDeath;
    }
}
