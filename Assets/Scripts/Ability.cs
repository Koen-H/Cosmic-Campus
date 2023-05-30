using System.Collections;
using Unity.Burst.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.UI.Image;

public class Ability : MonoBehaviour
{
    private string interactableTag = "Interactable";  // Set this to whatever tag you're using
    public float interactionRange = 5f;
    [SerializeField] protected float cooldown;
    protected bool canUse = true;

    protected PlayerCharacterController player;

    protected void Awake()
    {
        player = GetComponent<PlayerCharacterController>();
    }

    public void AbilityInput()
    {
        if (Input.GetMouseButtonUp(1) && canUse)  // 1 is the right mouse button
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (GetTarget(ray.origin, ray.direction) != null)
            {
                player.ActivateServerRpc(ray.origin, ray.direction);
                Activate(ray.origin, ray.direction);
                return;
            }
        }
        if (Input.GetMouseButtonUp(1) && !canUse)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (GetTarget(ray.origin, ray.direction) != null)
            {
                player.ActivateServerRpc(ray.origin, ray.direction);
                return;
            }
        }
        if (Input.GetMouseButtonUp(1) && !canUse)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(ray, out hit);
            if (hit.point != null) player.DeactivateServerRpc(hit.point);
        }
    }
    protected GameObject GetTarget(Vector3 origin, Vector3 direction)
    {
        Ray ray = new Ray(origin, direction);
        RaycastHit hit;
        GameObject target = null;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.CompareTag(interactableTag))
            {
                if ((hit.transform.position - transform.position).magnitude <= interactionRange)
                {
                    Debug.Log("You right-clicked on " + hit.collider.gameObject.name);

                    target = hit.collider.gameObject;
                }
            }
        }

        return target;

    }

    public virtual void Activate(Vector3 origin, Vector3 direction)
    {
        GameObject target = GetTarget(origin, direction);
        Debug.Log("TAREGT :  " + target.name); 
        if (target == null) return;


        canUse = false;
        Debug.Log("Activated ability on " + target.name);
        StartCoroutine(Cooldown(cooldown));
    }

    IEnumerator Cooldown(float time)
    {
        yield return new WaitForSeconds(time);
        canUse = true;
    }
}
