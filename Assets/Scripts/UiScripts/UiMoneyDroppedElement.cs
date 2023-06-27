using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UiMoneyDroppedElement : MonoBehaviour
{
    [SerializeField] RectTransform self;
    [SerializeField] TMP_Text moneyText; 
    [SerializeField] float fadeDuration = 1f;
    [SerializeField] float moveUpDistance = 1f;

    public void SetText(string text)
    {
        moneyText.text = text;
        FadeOutAndMoveUp();
    }
    public void SetPosition(Vector3 worldPosition, Camera cam)
    {
        cam = CameraManager.MyCamera.GetCamera();
        Vector3 pointOnPlane = cam.transform.position;
        Vector3 point = worldPosition;
        Vector3 normal = Vector3.Cross(cam.transform.right, cam.transform.up);
        Vector3 planeToPoint = point - pointOnPlane;
        float dotProduct = Vector3.Dot(planeToPoint, normal);

        Vector3 screenPosition = cam.WorldToScreenPoint(worldPosition);

        if (dotProduct < 0) screenPosition *= -1;

        self.anchoredPosition = screenPosition;
    }

    private void FadeOutAndMoveUp()
    {
        StartCoroutine(FadeOutAndMoveUpCoroutine());
    }

    private IEnumerator FadeOutAndMoveUpCoroutine()
    {
        Color initialColor = moneyText.color;
        Vector3 initialPosition = self.anchoredPosition;

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            // Calculate how far along the duration we are (between 0 and 1)
            float t = elapsedTime / fadeDuration;

            // Fade the color
            moneyText.color = new Color(initialColor.r, initialColor.g, initialColor.b, Mathf.Lerp(initialColor.a, 0f, t));

            elapsedTime += Time.deltaTime;

            // Yield control back to Unity until next frame
            yield return null;
        }

        // At the end, reset the text color and position
        moneyText.color = initialColor;
        moneyText.transform.position = initialPosition;

        //this.gameObject.SetActive(false);
        Destroy(this.gameObject);
    }

}
