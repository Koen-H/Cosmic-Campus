using UnityEngine;

public class Room : MonoBehaviour
{
    public Vector3 origin;
    private Vector3 scale;
    [SerializeField]
    public Collider roomCollider;

    [SerializeField]
    private MeshRenderer renderer;

    [SerializeField]
    public BoxCollider boxCollider;

    public float roomDepth; // added this new field


    private void Awake()
    {
        // roomCollider = GetComponent<Collider>();
        scale = transform.lossyScale; // get the scale of the object
        roomDepth = boxCollider.size.y * scale.y; // calculate the actual depth of the room
    }

    public Collider GetCollider()
    {
        return roomCollider;
    }

    public void SetColor(Color color)
    {
        renderer.material.color = color;
    }
}
