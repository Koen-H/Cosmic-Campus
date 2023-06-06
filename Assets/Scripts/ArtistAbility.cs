using System.Collections;
using Unity.Burst.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Image;

public class ArtistAbility : Ability
{
    [SerializeField] GameObject artistDecalPrefab;
    ArtistDecalType type;
    float ammo = 0;
    float maxAmmo = 2;

    private void Awake()
    {
        base.Awake();
        artistDecalPrefab = Resources.Load<GameObject>("Artist Decal");
    }

    public override void Activate(Vector3 origin, Vector3 direction)
    {
        Ray ray = new Ray(origin, direction);
        LayerMask layerMask = ~LayerMask.GetMask("Decal");
        RaycastHit hit;
        GameObject target = null;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)) target = hit.collider.gameObject;
        if (target == null) return;
        if (target.CompareTag("Water"))
        {
            type = ArtistDecalType.WATER;
            ammo++;
        }
        else if (target.CompareTag("Lava"))
        {
            type = ArtistDecalType.LAVA;
            ammo++;
        }
        else if(ammo > 0)
        {
            ArtistDecal decal = Instantiate(artistDecalPrefab, hit.point, Quaternion.LookRotation(-hit.normal)).GetComponent<ArtistDecal>();
            decal.transform.parent = hit.collider.transform;
            decal.type = type;
            ammo--;
        }
        if(ammo > maxAmmo) ammo = maxAmmo;
    }
}
