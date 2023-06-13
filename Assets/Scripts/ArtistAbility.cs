using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Image;

public class ArtistAbility : Ability
{
    //The colors stored in order as collected
    private List<ArtistPaintColor> paintBucket;
    private int paintBucketCapacity = 5;
    private ServerSpawner serverSpawner;


    private void Awake()
    {
        paintBucket = new();
        base.Awake();
        if(player.IsOwner) serverSpawner = ServerSpawner.Instance;
    }

    public override void Activate(Vector3 origin, Vector3 direction)
    {
        Ray ray = new Ray(origin, direction);
        LayerMask layerMask = ~LayerMask.GetMask("Decal");
        RaycastHit hit;
        GameObject target = null;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)) target = hit.collider.gameObject;
        if (target == null) return;
        if (target.CompareTag("Crystal"))
        {
            if (paintBucketCapacity == paintBucket.Count) return;//Bucket full!
            CrystalManagerWithShader crystalManager = target.GetComponent<CrystalManagerWithShader>();
            ArtistPaintColor color = crystalManager.GetColor();
            paintBucket.Add(color);

            ///Spawn a object at the place that floats towards the player! Obj should change based on color.

            if (color == ArtistPaintColor.NONE) return;
        }
        else if(paintBucket.Count > 0)
        {
            ArtistPaintColor firstColor = paintBucket[0];
            if (player.IsOwner) serverSpawner.SpawnArtistDecalPrefabServerRpc(hit.point, firstColor);//Spawn the decal!
            paintBucket.RemoveAt(0);
            Debug.Log(paintBucket.Count);
        }
    }
}
public enum ArtistPaintColor { NONE, WHITE, BLUE, YELLOW, ORANGE, RED, GREEN, PURPLE, PINK }