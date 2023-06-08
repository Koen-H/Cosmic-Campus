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
    private Coroutine coroutine;

    private void Awake()
    {
        Renderer renderer = GetComponent<Renderer>();
        currentMaterials = new Material[renderer.sharedMaterials.Length];
        for (int i = 0; i < currentMaterials.Length; i++)
        {
            currentMaterials[i] = renderer.sharedMaterials[i];
        }
    }

    /// <summary>
    /// Tell the MatChanger to change the material to the other.
    /// </summary>
    public void ChangeMaterial(EnemyType enemyType)
    {
        if (coroutine != null) StopCoroutine(coroutine);
        coroutine = StartCoroutine(ChangeMaterialOverTime(currentMaterials, SelectTargetMat(enemyType), time));
    }

    private Material[] SelectTargetMat(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.ARTIST:
                return artistMat;
            case EnemyType.DESIGNER:
                return designerMat;
            case EnemyType.ENGINEER:
                return engineerMat;
            default:
                return defaultMat;
        }
    }

    private IEnumerator ChangeMaterialOverTime(Material[] startMaterials, Material[] targetMaterials, float duration)
    {
        float time = 0;
        Renderer renderer = GetComponent<Renderer>();
        int numMaterials = Mathf.Min(startMaterials.Length, targetMaterials.Length);

        for (int i = 0; i < numMaterials; i++)
        {
            Material startMat = startMaterials[i];
            Material targetMat = targetMaterials[i];

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                renderer.sharedMaterials[i].Lerp(startMat, targetMat, t);
                yield return null;
            }

            time = 0;
        }
    }
}
