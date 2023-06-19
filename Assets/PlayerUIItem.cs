using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class PlayerUIItem : MonoBehaviour
{
    private ClientManager relatedClient;


    [SerializeField] private List<GameObject> artistUI;
    [SerializeField] private List<GameObject> designerUI;
    [SerializeField] private List<GameObject> engineerUI;

    [SerializeField] private GameObject bowUI;
    [SerializeField] private GameObject swordUI;
    [SerializeField] private GameObject staffUI;

    [SerializeField] private Image artistHealthUI;
    [SerializeField] private Image designerHealthUI;
    [SerializeField] private Image engineerHealthUI;

    [SerializeField] private Image artistCooldownhUI;
    [SerializeField] private Image designerCooldownUI;
    [SerializeField] private Image engineerCooldownUI;

    [SerializeField] private Image relatedHealthUI;
    [SerializeField] private Image relatedCooldownUI;
    [SerializeField] private TextMeshProUGUI relatedText;

    [SerializeField] private RawImage playerImage;

    float maxHealth;
    float barMult; 

    public void SetClient(ClientManager client)
    {
        relatedClient = client;

    }

    public void LoadCorrectUI()
    {
        List<GameObject> uiItems = new();
        switch (relatedClient.playerData.playerRole.Value)
        {
            case PlayerRole.ARTIST:
            {
                    relatedHealthUI = artistHealthUI;
                    relatedCooldownUI = artistHealthUI;
                    uiItems = artistUI;
                    break;
            }
            case PlayerRole.DESIGNER:
            {
                    relatedHealthUI = designerHealthUI;
                    relatedCooldownUI = designerCooldownUI;
                    uiItems = designerUI;
                    break;
                }
            case PlayerRole.ENGINEER:
                {
                    relatedHealthUI = engineerHealthUI;
                    relatedCooldownUI = engineerCooldownUI;
                    uiItems = engineerUI;
                    break;
                }
        }
        switch (relatedClient.playerData.weaponId.Value)
        {
            case 0:
                {
                    bowUI.SetActive(true);
                    break;
                }
            case 1:
                {
                    swordUI.SetActive(true);
                    break;
                }
            case 2:
                {
                    staffUI.SetActive(true);
                    break;
                }
        }
        foreach (GameObject item in uiItems) item.SetActive(true);
        relatedClient.playerCharacter.health.OnValueChanged += UpdateHealthBar;
        maxHealth = relatedClient.playerCharacter.maxHealth.Value;
        barMult = 1 / maxHealth;
        UpdateHealthBar(0, relatedClient.playerCharacter.health.Value);
        if (relatedText != null) relatedText.text = relatedClient.playerName.Value.ToString();
        LoadSteamAvatar();
    }

    async void LoadSteamAvatar(){
        if(playerImage == null) return;
        if(!SteamClient.IsLoggedOn) return;
        if(!relatedClient.usingSteam.Value) return;
        var avatarTask = SteamAvatarTest.GetAvatar(relatedClient.steamId.Value);

        await Task.WhenAll(avatarTask);
        var avatar  = await avatarTask;

        if(avatar != null){
            Texture2D avatarTexture = SteamAvatarTest.Convert(avatar);
            playerImage.texture = avatarTexture;
        }

    }


    void UpdateHealthBar(float prevHealth, float newHealth)
    {
        float xValue = newHealth * barMult;
        if (newHealth < 0) xValue = 0;
        relatedHealthUI.fillAmount = xValue;

    }

    public void SetCooldown(float value)
    {
        relatedCooldownUI.fillAmount = value;
    }

}
