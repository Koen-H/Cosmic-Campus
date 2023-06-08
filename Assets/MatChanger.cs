using System.Collections;
using UnityEngine;

public class MatChanger : MonoBehaviour
{
    public Material originalMaterial;
    public Material changedMaterial;

    private bool isChanged = false;
    private Coroutine coroutine;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) ToggleMaterialChange(1);
    }

    /// <summary>
    /// Tell the MatChanger to change the material to the other.
    /// </summary>
    public void ToggleMaterialChange(float time = 1.0f)
    {
        if (coroutine != null) StopCoroutine(coroutine);
        if (isChanged) coroutine = StartCoroutine(ChangeMaterialOverTime(changedMaterial, originalMaterial, time));
        else coroutine = StartCoroutine(ChangeMaterialOverTime(originalMaterial, changedMaterial, time));
        isChanged = !isChanged;
    }

    IEnumerator ChangeMaterialOverTime(Material startMaterial, Material endMaterial, float duration)
    {
        float time = 0;
        Material mat = GetComponent<Renderer>().material;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            mat.Lerp(startMaterial, endMaterial, t);
            yield return null;
        }
    }
}