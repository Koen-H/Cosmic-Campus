using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiDirectionIndicatorManager : MonoBehaviour
{
    [SerializeField] private UiDirectionIndicator deathPrefab;
    [SerializeField] private Camera cam;
    [SerializeField] private Canvas canvas;

    [SerializeField] private Sprite engineerSprite;
    [SerializeField] private Sprite artistSprite;
    [SerializeField] private Sprite designerSprite;
    
    [SerializeField] private Sprite engineerArrow;
    [SerializeField] private Sprite artistArrow;
    [SerializeField] private Sprite designerArrow;

    private List<PlayerCharacterController> characterControllers = new List<PlayerCharacterController>();

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
