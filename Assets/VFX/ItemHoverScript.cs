using UnityEngine;

public class ItemHoverScript : MonoBehaviour
{
    public float hoverHeight = 1f;        // Maximum height of the hover
    public float hoverSpeed = 1f;         // Speed of the hover

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        float newY = Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
        transform.position = startPos + new Vector3(0f, newY, 0f);
    }
}
