 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class ArtistDecal : MonoBehaviour
{
    [SerializeField] Material waterMat;
    [SerializeField] Material lavaMat;

    public ArtistDecalType type = ArtistDecalType.WATER;

    DecalProjector projector;
    private float lifespawn = 5;

    private void Start()
    {
        projector = GetComponent<DecalProjector>();
        if (type == ArtistDecalType.WATER) projector.material = waterMat;
        else projector.material = lavaMat;
        StartCoroutine(FadeOutOfExistence(5));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (type == ArtistDecalType.WATER)
        {
            Debug.Log("WALKING IN WATER!");
        }
        else if (type == ArtistDecalType.LAVA)
        {
            Debug.Log("WALKING IN LAVA");
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
public enum ArtistDecalType { WATER, LAVA }