using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesignerAbility : Ability
{
    public float offset = 5;
    public float distFromGround = 3;
    public float maxScale = 30;
    public float minScale = 0.2f;

    private GameObject target;
    private Vector3 initialScale;
    private Quaternion initialRotation;
    public LayerMask groundLayer;
    private float scaleAmount = 1; 

    public override void Activate(GameObject Target)
    {
        if (!canUse) return;

        canUse = false;
        target = Target;
        initialScale = target.transform.localScale;
        initialRotation = target.transform.rotation;
    }

    private void Update()
    {
        bool noTarget = target;
        base.Update();

        if (target != noTarget) return; 
        if (target == null) return;

        if (Input.GetMouseButtonUp(1))
        {
            Debug.Log("Placed Down! ");
            PutDown(target);
            target = null;
            scaleAmount = 1;
            return;
        }


        PickedUp(target);
    }

    void PickedUp(GameObject target)
    {
        // Calculate the direction towards the mouse cursor on the ground plane
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer)) // Added groundLayer here
        {
            Vector3 directionToMouse = (new Vector3(hit.point.x, transform.position.y, hit.point.z) - transform.position).normalized;

            // Calculate the target position by offsetting from the player/camera position
            // in the direction towards the mouse
            Vector3 targetPosition = transform.position + directionToMouse * offset + new Vector3(0, distFromGround, 0);
            target.transform.position = targetPosition;

            // Calculate rotation towards the mouse and add it to the initial rotation
            Vector3 directionToMouseFlat = new Vector3(directionToMouse.x, 0, directionToMouse.z);
            Quaternion rotationTowardsMouse = Quaternion.LookRotation(directionToMouseFlat);
            target.transform.rotation = initialRotation * rotationTowardsMouse;
        }

        // Handle scale changes based on mouse scroll
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        scaleAmount += scroll * 0.1f;
        Vector3 targetScale = initialScale * scaleAmount;

        if (targetScale.x > maxScale || targetScale.x < minScale
            || targetScale.y > maxScale || targetScale.y < minScale
            || targetScale.z > maxScale || targetScale.z < minScale)
        {
            scaleAmount -= scroll * 0.1f;
            return;
        }

        // Ensure each component is within the min and max scale
        targetScale.x = Mathf.Clamp(targetScale.x, minScale, maxScale);
        targetScale.y = Mathf.Clamp(targetScale.y, minScale, maxScale);
        targetScale.z = Mathf.Clamp(targetScale.z, minScale, maxScale);

            target.transform.localScale = targetScale;
/*        // If any scale component has hit the limit, prevent further scaling
        if (targetScale.x == maxScale || targetScale.y == maxScale || targetScale.z == maxScale ||
            targetScale.x == minScale || targetScale.y == minScale || targetScale.z == minScale)
        {
            scaleAmount -= scroll * 0.1f;  // Undo the last scale change
        }
        else
        {
        }*/
    }










    void PutDown(GameObject target)
    {
        // Place target on the ground
        target.transform.position = new Vector3(
            target.transform.position.x,
            target.transform.localScale.y / 2,
            target.transform.position.z
        );
        canUse = true;
    }

    IEnumerator Cooldown(float time)
    {
        yield return new WaitForSeconds(time);
        canUse = true;
    }
}
