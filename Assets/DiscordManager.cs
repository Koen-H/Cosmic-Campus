using Discord;
using System.Collections;
using UnityEngine;

public class DiscordManager : MonoBehaviour
{
    public long applicationID;
    [Space]
    public string details = "Fixing bugs Thomas made";
    public string state = "STATE ";
    [Space]
    public string largeImage = "game_logo";
    public string largeText = "Fixing bugs Thomas made";

    private long time;

    public Discord.Discord discord;

    public float randomUpdatesInterval = 20;
    public bool randomUpdates = false;
    bool usingDiscord = true;

    private static DiscordManager instance;
    public static DiscordManager Instance
    {
        get
        {
            if (instance == null) Debug.LogError("DiscordManager is null!");
            return instance;
        }
    }


    void Awake()
    {
        // Transition the GameObject between scenes, destroy any duplicates
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Log in with the Application ID
        discord = new Discord.Discord(applicationID, (System.UInt64)Discord.CreateFlags.NoRequireDiscord);
        UpdateStatus(details,state, largeText,largeImage);
    }

    void Update()
    {
        // Destroy the GameObject if Discord isn't running
        try
        {
            if(usingDiscord) discord.RunCallbacks();
        }
        catch
        {
            usingDiscord = false;
            //Destroy(gameObject);
        }
    }

    void OnApplicationQuit()
    {
        if (discord == null) return;
        discord.Dispose();
    }

    public void UpdateStatus(string newDetails = "", string newState = "", string newLargeText = "", string newLargeImage = "game_logo")
    {
        if (!usingDiscord) return;
        this.details = newDetails;
        this.state = newState;
        this.largeText = newLargeText;
        this.largeImage = newLargeImage;


        // Update Status every frame
        try
        {
            var activityManager = discord.GetActivityManager();
            var activity = new Discord.Activity
            {
                Details = details,
                State = state,
                Assets =
                {
                    LargeImage = largeImage,
                    LargeText = largeText
                },
                Timestamps =
                {
                    Start = time
                }
            };

            activityManager.UpdateActivity(activity, (res) =>
            {
                if (res != Discord.Result.Ok) Debug.LogWarning("Failed connecting to Discord!");
            });
        }
        catch
        {
            // If updating the status fails, Destroy the GameObject
            Destroy(gameObject);
        }
    }

    public void ToggleRandomUpdates(bool toggle)
    {
        randomUpdates= toggle;
        if(randomUpdates) StartCoroutine(RandomUpdates());
    }


    IEnumerator RandomUpdates()
    {
        if (!usingDiscord) yield return null;
        int options = 3;
        int r = Random.Range(0, options);
        switch (r)
        {
            case 0://Golems killed
                UpdateStatus("Fighting golems", $"Golems killed: {ClientManager.MyClient.golemsKilled.Value}");
                break;
            case 1://Show current health
                UpdateStatus("Chuckles in danger", $"Current Health: {ClientManager.MyClient.playerCharacter.health.Value}");
                break;
            case 2://Mark mom jokes
                UpdateStatus("Oh hi Mark!", $"Your Mom Jokes: Too many");
                break;
            default:
                UpdateStatus("Jumping in blackholes", $"Times died: {ClientManager.MyClient.timesDied.Value}");
                break;

        }


        yield return new WaitForSeconds(randomUpdatesInterval);
        if (randomUpdates) StartCoroutine(RandomUpdates());

    }
}
