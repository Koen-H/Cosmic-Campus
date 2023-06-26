using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class BossEnemy : PunchAttack
{
    [SerializeField] private GameObject mineLauncher;
    [SerializeField] private GameObject minePrefab;


    [Header("Mine launcher settings")]
    [SerializeField] private float launchStrength = 750;
    [SerializeField] private Range minesPerLaunch = new Range(2, 8);
    [SerializeField] private Range launchInterval = new Range(16, 30);


    protected void Awake()
    {
        base.Awake();
        StartCoroutine(ShootMines());
        BackgroundMusicManager.Instance.PlayBossMusic();
    }


    void LaunchMine()
    {
        if (!IsOwner) return;
        Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        // Get a random tilt angle within the specified range
        float upwardAngle = Random.Range(0f, 45);

        // Apply the random tilt angle to the rotation
        Quaternion tiltRotation = Quaternion.Euler(upwardAngle, 0, 0f);

        // Set the rotation of the GameObject
        mineLauncher.transform.rotation = randomRotation * tiltRotation;

        GameObject item = Instantiate(minePrefab, mineLauncher.transform.position, mineLauncher.transform.rotation);
        item.GetComponent<NetworkObject>().Spawn(true);
        item.GetComponent<Rigidbody>().AddForce(mineLauncher.transform.forward * launchStrength);
    }

    IEnumerator ShootMines()
    {
        if (!IsOwner) yield return null;
        while(true)
        {
            yield return new WaitForSeconds(launchInterval.GetRandomValue());
            int mines = (int)minesPerLaunch.GetRandomValue();
            for (int i = 0; i < mines; i++) LaunchMine();
        }
    }

    private void OnDestroy()
    {
        BackgroundMusicManager.Instance.LoadDefault();
    }


}
