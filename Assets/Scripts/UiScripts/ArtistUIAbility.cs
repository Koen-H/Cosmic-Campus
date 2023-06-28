using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ArtistUIAbility : MonoBehaviour
{

    [SerializeField] List<GameObject> artistAbilityImages = new();

    private void Start()
    {
        DisableAll();
    }

    void DisableAll()
    {
        foreach (GameObject obj in artistAbilityImages) obj.SetActive(false);

    }

    public void UpdateImage(ArtistPaintColor color)
    {
        DisableAll();
        switch (color)
        {
            case (ArtistPaintColor.BLUE):
                artistAbilityImages[0].SetActive(true);
                break;
            case (ArtistPaintColor.YELLOW):
                artistAbilityImages[1].SetActive(true);
                break;
            case (ArtistPaintColor.ORANGE):
                artistAbilityImages[2].SetActive(true);
                break;
            case (ArtistPaintColor.GREEN):
                artistAbilityImages[3].SetActive(true);
                break;
            case (ArtistPaintColor.PINK):
                artistAbilityImages[4].SetActive(true);
                break;
            case (ArtistPaintColor.PURPLE):
                artistAbilityImages[5].SetActive(true);
                break;
            case (ArtistPaintColor.RED):
                artistAbilityImages[6].SetActive(true);
                break;
            case (ArtistPaintColor.WHITE):

                break;
            default:
               // Debug.LogWarning("Undefined color?");
                break;
        }
    }
}
