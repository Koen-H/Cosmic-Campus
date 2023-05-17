using System.Collections;
using Unity.Burst.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.UI.Image;

public class Ability : NetworkBehaviour
{
    private string interactableTag = "Interactable";  // Set this to whatever tag you're using
    public float interactionRange = 5f;
    [SerializeField] protected float cooldown;
    protected bool canUse = true;

    public void Update()
    { 
        if (Input.GetMouseButtonUp(1) && canUse)  // 1 is the right mouse button
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.CompareTag(interactableTag))
                {
                    if ((hit.transform.position - transform.position).magnitude <= interactionRange)
                    {
                        Debug.Log("You right-clicked on " + hit.collider.gameObject.name);

                        //When we click on something, tell the server we clicked something!
                        ActivateServerRpc(ray.origin,ray.direction);
                    }
                    else Debug.Log("You are out of range");
                }
                else Debug.Log("You clicked on a different taged object");
            }
            else Debug.Log("Nothing was clicked");
        }
    }

    [ServerRpc]
    void ActivateServerRpc(Vector3 origin, Vector3 direction)
    {
        //According to the client, we hit something...
        Ray ray = new Ray(origin,direction);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.CompareTag(interactableTag))
            {
                if ((hit.transform.position - transform.position).magnitude <= interactionRange)
                {
                    Debug.Log("You right-clicked on " + hit.collider.gameObject.name);

                    //Client is telling the truth! Do the same functionality for each client!
                    ActivateClientRpc(ray.origin, ray.direction);
                }
                else Debug.Log("You are out of range");
            }
            else Debug.Log("You clicked on a different taged object");
        }
        else Debug.Log("Nothing was clicked");
    }


    [ClientRpc]
    void ActivateClientRpc(Vector3 origin, Vector3 direction)
    {
        Ray ray = new Ray(origin, direction);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.CompareTag(interactableTag))
            {
                if ((hit.transform.position - transform.position).magnitude <= interactionRange)
                {
                    Debug.Log("You right-clicked on " + hit.collider.gameObject.name);

                    //Client is telling the truth! Do the same functionality for each client!
                    Activate(hit.collider.gameObject);
                }
                else Debug.Log("You are out of range");
            }
            else Debug.Log("You clicked on a different taged object");
        }
        else Debug.Log("Nothing was clicked");
    }


    public virtual void Activate(GameObject target)
    {
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
