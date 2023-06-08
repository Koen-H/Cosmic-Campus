using System.Collections;
using UnityEngine;
using static PlayerSO;

public class MatChanger : MonoBehaviour
{
    Material currentMaterial;

    [SerializeField] private float time = 1.0f;
    [SerializeField] private Material defaultMat;
    [SerializeField] private Material artistMat;
    [SerializeField] private Material designerMat;
    [SerializeField] private Material engineerMat;
    private Coroutine coroutine;


    private void Awake()
    {
        currentMaterial = GetComponent<Material>();
    }

    /// <summary>
    /// Tell the MatChanger to change the material to the other.
    /// </summary>
    public void ChangeMaterial(EnemyType enemyType)
    {
        if (coroutine != null) StopCoroutine(coroutine);
        coroutine = StartCoroutine(ChangeMaterialOverTime(currentMaterial, SelectTargetMat(enemyType), time));
    }

    private Material SelectTargetMat(EnemyType enemyType)
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

    private IEnumerator ChangeMaterialOverTime(Material startMaterial, Material targetMaterial, float duration)
    {
        float time = 0;
        Material mat = GetComponent<Renderer>().material;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            mat.Lerp(startMaterial, targetMaterial, t);
            yield return null;
        }
        currentMaterial = targetMaterial;
    }
}