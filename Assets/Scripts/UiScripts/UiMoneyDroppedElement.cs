using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UiMoneyDroppedElement : MonoBehaviour
{
    [SerializeField] private RectTransform self;
    [SerializeField] private TMP_Text moneyText; 
    [SerializeField] private float fadeDuration = 1f;

    public void SetText(string text)
    {
        moneyText.text = text;
        FadeOutAndMoveUp();
    }
    public void SetPosition(Vector3 worldPosition, Camera cam)
    {
        cam = CameraManager.MyCamera.GetCamera();
        CanvasScaler canvasScaler = GetComponentInParent<CanvasScaler>();

        float scaleFactor = canvasScaler.scaleFactor;
        Vector3 screenPosition = cam.WorldToScreenPoint(worldPosition);
        screenPosition.x /= scaleFactor;
        screenPosition.y /= scaleFactor;
        GetComponent<RectTransform>().anchoredPosition = screenPosition;
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
