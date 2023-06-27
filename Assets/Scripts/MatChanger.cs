using UnityEngine;
using System.Collections;

public class MatChanger : MonoBehaviour
{
    private Material[] currentMaterials;

    [SerializeField] private float time = 2f;
    [SerializeField] private Material[] defaultMat;
    [SerializeField] private Material[] artistMat;
    [SerializeField] private Material[] designerMat;
    [SerializeField] private Material[] engineerMat;


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

        //currentMaterials = new Material[renderer.sharedMaterials.Length];
        //for (int i = 0; i < currentMaterials.Length; i++)
        //{
        //    currentMaterials[i] = renderer.sharedMaterials[i];
        //}
        //    myMaterial = currentMaterials[0];
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

        //coroutine = StartCoroutine(ChangeMaterialOverTime(currentMaterials, SelectTargetMat(enemyType), time));
    }


    //private Material[] SelectTargetMat(EnemyType enemyType)
    //{
    //    switch (enemyType)
    //    {
    //        case EnemyType.ARTIST:
    //            return artistMat;
    //        case EnemyType.DESIGNER:
    //            return designerMat;
    //        case EnemyType.ENGINEER:
    //            return engineerMat;
    //        default:
    //            return defaultMat;
    //    }
    //}

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

    //private IEnumerator ChangeMaterialOverTime(Material[] startMaterials, Material[] targetMaterials, float duration)
    //{
    //    float time = 0;
    //    Renderer renderer = GetComponent<Renderer>();
    //    int numMaterials = Mathf.Min(startMaterials.Length, targetMaterials.Length);

    //    for (int i = 0; i < numMaterials; i++)
    //    {
    //        Material startMat = startMaterials[i];
    //        Material targetMat = targetMaterials[i];

    //        while (time < duration)
    //        {
    //            time += Time.deltaTime;
    //            float t = time / duration;
    //            renderer.sharedMaterials[i].Lerp(startMat, targetMat, t);
    //            yield return null;
    //        }

    //        time = 0;
    //    }
    //}

    private IEnumerator ChangeEmissionColor(Color newColor)
    {
        //myMaterial = GetComponent<Renderer>().material;
        Color[] startColor = new Color[myMaterials.Length];
        for (int i = 0; i < myMaterials.Length; i++)
        {
            startColor[i] = myMaterials[i].GetColor("_EmissiveColor");
        }

        float elapsedTime = 0f;
        float duration = 4f;

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
