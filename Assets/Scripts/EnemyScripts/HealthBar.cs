using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private GameObject bar;

    [SerializeField] private Image bgSprite; 
    [SerializeField] private Image barSprite;
    [SerializeField] private float fadeDuration = 1f; 
 
    private float maxValue;
    private float barMult;

    private Coroutine coBg;
    private Coroutine coBar;

    [SerializeField] private bool dontFade = false;

    public void ResetBar()
    {
        bar.transform.localScale = new Vector3(1, bar.transform.localScale.y, bar.transform.localScale.z);
    }
    public void SetMaxValue(float value)
    {
        if (value <= 0) return;
        maxValue = value;
        barMult = 1 / maxValue;
        ResetBar();
    }
    /// <summary>
    /// Scales bar on X Axis accourding to the maximum value and given value
    /// </summary>
    /// <param name="value"></param>
    public void UpdateBar(float value)
    {
        if(coBg != null)StopCoroutine(coBg);
        if(coBar != null) StopCoroutine(coBar);

        float xValue = value * barMult;
        if (value < 0) xValue = 0;
        bar.transform.localScale = new Vector3(xValue, bar.transform.localScale.y, bar.transform.localScale.z);

        coBg = StartCoroutine(FadeInOut(bgSprite, fadeDuration));
        coBar = StartCoroutine(FadeInOut(barSprite, fadeDuration));
    }

    IEnumerator FadeInOut(Image image, float duration, bool fadeIn = false)
    {
            for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration)
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, Mathf.Lerp(image.color.r, 1, t));
                yield return null;
            }
            yield return new WaitForSeconds(5); // Wait 5 seconds before starting the fade out
            for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration)
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, Mathf.Lerp(1, 0, t));
                yield return null;
            }
    }
}
