using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Changes the color of the platform when someone is ready
/// </summary>
public class ReadyUpPlatform : MonoBehaviour
{
    [SerializeField] private Color unreadyColor = new Color(1, 0, 0, 1f);
    [SerializeField] private Color readyColor = new Color(0, 1, 0, 1f);

    private Material[] myMaterials;
    private float[] myEmissionIntensity;
    private Coroutine coroutine;

    [SerializeField] private List<Renderer> renderers;

    private void Awake()
    {
        myMaterials = new Material[renderers.Count];
        myEmissionIntensity = new float[renderers.Count];

        for (int i = 0; i < renderers.Count; i++)
        {
            Renderer renderer = renderers[i];
            myMaterials[i] = renderer.material;
            myEmissionIntensity[i] = renderer.material.GetFloat("_EmissiveIntensity");
        }
    }


    public void ChangeColor(bool ready)
    {
        if (coroutine != null) StopCoroutine(coroutine);
        if (ready) coroutine = StartCoroutine(ChangeEmissionColor(readyColor));
        else coroutine = StartCoroutine(ChangeEmissionColor(unreadyColor));
    }

    private IEnumerator ChangeEmissionColor(Color newColor)
    {
        //myMaterial = GetComponent<Renderer>().material;
        Color[] startColor = new Color[myMaterials.Length];
        for (int i = 0; i < myMaterials.Length; i++)
        {
            startColor[i] = myMaterials[i].GetColor("_EmissiveColor");
        }

        float elapsedTime = 0f;
        float duration = 1f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / duration);

            for (int i = 0; i < myMaterials.Length; i++)
            {
                Color tempColor = newColor * myEmissionIntensity[i];
                Color currentColor = Color.Lerp(startColor[i], tempColor, t);
                myMaterials[i].SetColor("_EmissiveColor", currentColor);
            }
            yield return null;
        }
        for (int i = 0; i < myMaterials.Length; i++)
        {
            myMaterials[i].SetColor("_EmissiveColor", newColor * myEmissionIntensity[i]);
        }
    }
}
