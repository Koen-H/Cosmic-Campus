using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private GameObject bar;

    [SerializeField] Image bgSprite; 
    [SerializeField] Image barSprite;
    [SerializeField] float fadeDuration = 1f; 
 
    private float maxValue;
    private float barMult;

    Coroutine coBg; 
    Coroutine coBar;

    private void Start()
    {
        barMult = 1 / maxValue;
        
    }

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
    public void UpdateBar(int value)
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
        if (fadeIn)
        {
            for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration)
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, Mathf.Lerp(0, 1, t));
                yield return null;
            }
        }
        else
        {
            for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration)
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, Mathf.Lerp(1, 0, t));
                yield return null;
            }
        }
    }

    private void OnDestroy()
    {
    }
}
