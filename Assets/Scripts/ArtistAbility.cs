using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Rendering.VolumeComponent;
using static UnityEngine.UI.Image;

public class ArtistAbility : Ability
{
    //The colors stored in order as collected
    private List<ArtistPaintColor> paintBucket;
    private int paintBucketCapacity = 5;
    private ServerSpawner serverSpawner;

    private GameObject suckFVX;

    private void Awake()
    {
        suckFVX = Resources.Load<GameObject>("Crystal/CrystalGrab");
        paintBucket = new();
        base.Awake();
        if(player.IsOwner) serverSpawner = ServerSpawner.Instance;
    }

    protected override void Update()
    {
        base.Update();
        if (!player.IsOwner) return;
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwapColor(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwapColor(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwapColor(3);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SwapColor(4);

    }

    void SwapColor(int index)
    {
        //Check if something is inside the given index
        if (paintBucket.Count > index)
        {
        Debug.Log("colorswap");
            ArtistPaintColor temp = paintBucket[index];
            paintBucket[index] = paintBucket[0];
            paintBucket[0] = temp;
            CanvasManager.Instance.UpdateArtistUI(paintBucket);
        }


    }



    public override void Activate(Vector3 origin, Vector3 direction)
    {
        Ray ray = new Ray(origin, direction);
        LayerMask layerMask = ~(LayerMask.GetMask("Decal") | LayerMask.GetMask("Enemy") | LayerMask.GetMask("Area") | LayerMask.GetMask("Player") | LayerMask.GetMask("UI"));
        RaycastHit hit;
        GameObject target = null;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)) target = hit.collider.gameObject;
        if (target == null) return;
        if (target.CompareTag("Crystal"))
        {
            if (paintBucketCapacity == paintBucket.Count) return;//Bucket full!
            CrystalManagerWithShader crystalManager = target.GetComponent<CrystalManagerWithShader>();
            ArtistPaintColor color = crystalManager.GetColor();
            if (color == ArtistPaintColor.NONE) return;
            paintBucket.Add(color);

            ///Spawn a object at the place that floats towards the player! Obj should change based on color.
            GameObject suckFVXInstance = Instantiate(suckFVX,hit.point, Quaternion.identity);
            crystalGrabManager crystalGrab = suckFVXInstance.GetComponent<crystalGrabManager>();
            crystalGrab.trail.GetComponent<Renderer>().material = crystalManager.GetComponent<Renderer>().material;
            crystalGrab.staffObject = player.centerPoint;
        }
        else if(paintBucket.Count > 0)
        {
            ArtistPaintColor firstColor = paintBucket[0];
            if (player.IsOwner) serverSpawner.SpawnArtistDecalPrefabServerRpc(hit.point, firstColor);//Spawn the decal!
            paintBucket.RemoveAt(0);
        }
        if (player.IsOwner) CanvasManager.Instance.UpdateArtistUI(paintBucket);
    }
}
public enum ArtistPaintColor { NONE, WHITE, BLUE, YELLOW, ORANGE, RED, GREEN, PURPLE, PINK }