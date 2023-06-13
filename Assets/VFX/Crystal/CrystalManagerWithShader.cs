using System.Collections;
using UnityEngine;

public class CrystalManagerWithShader: MonoBehaviour
{
    [SerializeField, Tooltip("What color is this crystal? And what effect will it bring?")]
    private ArtistPaintColor color = ArtistPaintColor.WHITE;
    [SerializeField, Tooltip("How much essence is there in this crystal and how many times can its power be drained?")]
    private float essence = 2;
    private float startEssence;

    private Material Topmaterial, Botmaterial;
    private float drainSpeed = 1f;

    private Color initialEmissionColor;
    private float currentEmissionIntensity;
    private float targetEmissionIntensity;

    private Range emissionRange = new Range(0f, 1);

    private void Start()
    {
        startEssence = essence;
        Topmaterial = GetComponent<Renderer>().material;
        currentEmissionIntensity = Topmaterial.GetFloat("_TopLine");
    }

    public ArtistPaintColor GetColor()
    {
        if (essence == 0) return ArtistPaintColor.NONE;
        essence--;
        StartCoroutine(DrainMaterialCoroutine());
        return color;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            essence--;
            StartCoroutine(DrainMaterialCoroutine());
        }
    }

    private void CalculateTargetEmission()
    {
        float essencePercentage = Mathf.Clamp01(1f - (essence / startEssence));
        targetEmissionIntensity = Mathf.Lerp(emissionRange.max, emissionRange.min, essencePercentage);
    }


    private IEnumerator DrainMaterialCoroutine()
    {
        CalculateTargetEmission();
        while (currentEmissionIntensity > targetEmissionIntensity)
        {
            currentEmissionIntensity -= drainSpeed * Time.deltaTime;

            currentEmissionIntensity = Mathf.Clamp01(currentEmissionIntensity);

            Color newEmissionColor = initialEmissionColor * currentEmissionIntensity;
            Topmaterial.SetFloat("_TopLine", currentEmissionIntensity);
            

            yield return null;
        }
    }
}
