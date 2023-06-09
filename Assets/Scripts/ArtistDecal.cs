using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class ArtistDecal : MonoBehaviour
{
    private DecalProjector projector;
    [SerializeField, Tooltip("How long should this decal stay?")]
    private float lifespan = 5;
    private Effect effect;

    private void Start()
    {
        effect = GetComponent<Effect>();
        projector = GetComponent<DecalProjector>();
        StartCoroutine(FadeOutOfExistence(lifespan));

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Player"))
        {
            other.GetComponentInParent<EffectManager>().AddEffect(effect);
        }
    }

    //Slowly fade the decal out of existence
    IEnumerator FadeOutOfExistence(float duration)
    {
        float elapsedTime = 0f;
        float fadeDuration = duration * 0.9f; // Adjust the fade-out duration to leave some time for the initial wait

        while (elapsedTime < fadeDuration)
        {
            float fadeAmount = 1f - (elapsedTime / fadeDuration);
            projector.fadeFactor = fadeAmount;
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        projector.fadeFactor = 0f; // Ensure the fade factor reaches 0 at the end
        yield return new WaitForSeconds(0.1f);

        Destroy(gameObject);
    }
}
