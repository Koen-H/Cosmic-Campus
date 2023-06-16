using Discord;
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

    private static bool instanceExists;
    public Discord.Discord discord;

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
        if (!instanceExists)
        {
            instanceExists = true;
            DontDestroyOnLoad(gameObject);
        }
        else if (FindObjectsOfType(GetType()).Length > 1)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Log in with the Application ID
        discord = new Discord.Discord(applicationID, (System.UInt64)Discord.CreateFlags.NoRequireDiscord);
        UpdateStatus();
    }

    void Update()
    {
        // Destroy the GameObject if Discord isn't running
        try
        {
            discord.RunCallbacks();
        }
        catch
        {
            Destroy(gameObject);
        }
    }

    void LateUpdate()
    {
        UpdateStatus();
    }

    void UpdateStatus()
    {
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
}
