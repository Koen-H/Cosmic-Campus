using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiDirectionIndicatorManager : MonoBehaviour
{
    [SerializeField] UiDirectionIndicator deathPrefab;
    [SerializeField] Camera cam;
    [SerializeField] Canvas canvas;

    [SerializeField] Sprite engineerSprite;
    [SerializeField] Sprite artistSprite;
    [SerializeField] Sprite designerSprite;
    
    [SerializeField] Sprite engineerArrow;
    [SerializeField] Sprite artistArrow;
    [SerializeField] Sprite designerArrow;

    private List<PlayerCharacterController> characterControllers = new List<PlayerCharacterController>();

    private void Start()
    {
        CanvasManager.Instance.camera = Camera.main;
    }


    public void AddToCharacterToTrack(PlayerCharacterController character) => characterControllers.Add(character); 

    public void InitiateDirections()
    {
        foreach (var player in characterControllers)
        {
            UiDirectionIndicator indicator = Instantiate(deathPrefab, this.transform);
            switch (player.GetPlayerData().playerRole.Value)
            {
                case PlayerRole.ARTIST:
                    indicator.SetData(artistSprite, artistArrow, player);
                    break;
                case PlayerRole.ENGINEER:
                    indicator.SetData(engineerSprite, engineerArrow, player);
                    break;
                case PlayerRole.DESIGNER:
                    indicator.SetData(designerSprite, designerArrow, player);
                    break;
            }
        }
    }




}
