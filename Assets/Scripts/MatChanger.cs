using UnityEngine;
using System.Collections;

public class MatChanger : MonoBehaviour
{
    [SerializeField] private float duration = 4f;

    private Material[] myMaterials;
    private float[] myEmissionIntensity;
    [SerializeField] private Color defaultCol2 = new Color(1f, 1f, 1f,1f);
    [SerializeField] private Color artistCol2 = new Color(1f, 0.121f, 0f,1f);
    [SerializeField] private Color designerCol2 = new Color(0.0f, 0.878f, 0.278f, 1f);
    [SerializeField] private Color engineerCol2 = new Color(0.0f, 0.647f, 1.0f, 1f);

    private Coroutine coroutine;

    private void Awake()
    {
        Renderer renderer = GetComponent<Renderer>();
        myMaterials = new Material[renderer.materials.Length];
        myEmissionIntensity = new float[renderer.materials.Length];
        for (int i = 0; i < myMaterials.Length; i++)
        {
            myMaterials[i] = renderer.materials[i];
            myEmissionIntensity[i] = renderer.materials[i].GetFloat("_EmissiveIntensity");
        }
    }

    /// <summary>
    /// Tell the MatChanger to change the material to the other.
    /// </summary>
    public void ChangeMaterial(EnemyType enemyType, bool instant = false)
    {
        if (coroutine != null) StopCoroutine(coroutine);
        if(!instant) coroutine = StartCoroutine(ChangeEmissionColor(SelectTargetColor(enemyType)));
        else
        {
            Color newColor = SelectTargetColor(enemyType);
            for (int i = 0; i < myMaterials.Length; i++)
            {
                myMaterials[i].SetColor("_EmissiveColor", newColor * myEmissionIntensity[i]);
            }
        }

    }

    private Color SelectTargetColor(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.ARTIST:
                return artistCol2;
            case EnemyType.DESIGNER:
                return designerCol2;
            case EnemyType.ENGINEER:
                return engineerCol2;
            default:
                return defaultCol2;
        }
    }

    private IEnumerator ChangeEmissionColor(Color newColor)
    {
        Color[] startColor = new Color[myMaterials.Length];
        for (int i = 0; i < myMaterials.Length; i++)
        {
            startColor[i] = myMaterials[i].GetColor("_EmissiveColor");
        }

        float elapsedTime = 0f;

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
