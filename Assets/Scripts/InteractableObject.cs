using UnityEngine;
using UnityEngine.EventSystems;

public class InteractableObject : MonoBehaviour, IPointerClickHandler
{
    public delegate void ClickAction(GameObject interactedObject);
    public static event ClickAction OnRightClicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClicked?.Invoke(gameObject);
        }
    }
}