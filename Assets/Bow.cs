using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : MonoBehaviour
{
    public float maxChargeTime = 2f; // Maximum time to charge the bow
    public GameObject arrowPrefab; // Prefab of the arrow

    private bool isCharging;
    private float chargeStartTime;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartCharge();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            ShootArrow();
        }
        if (Input.GetMouseButton(0))
        {
            Aim();
        }
    }

    void Aim()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 clickPoint = hit.point;

            transform.LookAt(new Vector3(transform.position.x + clickPoint.x, transform.position.y, transform.position.z + clickPoint.z));
            //transform.forward = new Vector3(transform.position.x + clickPoint.x, transform.position.y, transform.position.z + clickPoint.z);
        }
    }

    private void StartCharge()
    {
        isCharging = true;
        chargeStartTime = Time.time;
    }

    private void ShootArrow()
    {
        if (!isCharging)
            return;

        isCharging = false;

        float chargeLevel = Mathf.Clamp01((Time.time - chargeStartTime) / maxChargeTime) * 1000;
        GameObject arrow = Instantiate(arrowPrefab, transform.position, transform.rotation);
        arrow.GetComponent<Rigidbody>().AddForce(transform.forward * chargeLevel);
        //ArrowController arrowController = arrow.GetComponent<ArrowController>();

        // Set the charge level of the arrow
        //arrowController.SetChargeLevel(chargeLevel);

        // Apply any other logic to the arrow (e.g., add force, apply damage, etc.)
        // arrowController.ApplyForce(...);
        // arrowController.ApplyDamage(...);
    }
}
