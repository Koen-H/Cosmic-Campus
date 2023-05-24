using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vfxDestroyer : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        var main = GetComponent<ParticleSystem>().main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }
    void OnParticleSystemStopped()
    {
        Destroy(gameObject);
    }

}
