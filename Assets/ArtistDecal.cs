using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class ArtistDecal : MonoBehaviour
{
    [SerializeField] Material waterMat;
    [SerializeField] Material lavaMat;

    public ArtistDecalType type = ArtistDecalType.WATER;

    private void Start()
    {
        if(type == ArtistDecalType.WATER) GetComponent<DecalProjector>().material = waterMat;
        else GetComponent<DecalProjector>().material = lavaMat;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (type == ArtistDecalType.WATER)
        {
            Debug.Log("WALKING IN WATER!");
        }
        else if (type == ArtistDecalType.LAVA)
        {
            Debug.Log("WALKING IN LAVA");
        }
    }
}
public enum ArtistDecalType { WATER, LAVA }