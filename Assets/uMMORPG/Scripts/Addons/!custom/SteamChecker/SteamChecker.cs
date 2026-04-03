using UnityEngine;
#if !DISABLESTEAMWORKS
using Steamworks;

public class SteamChecker : MonoBehaviour
{
    [SerializeField]
    private uint appId; // Use uint for App ID and expose it to the Inspector

    private float checkInterval = 10f; // Time interval for checking ownership in seconds
    private float nextCheckTime = 0f;   // Tracks when to perform the next check

    private void Start()
    {
        // Ensure the app is launched via Steam
        if (SteamAPI.RestartAppIfNecessary((AppId_t)this.appId))
        {
            Application.Quit();
            return;
        }

        // Initial Check for Steam
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam not initialized.");
            return;
        }

        // Schedule the first ownership check
        nextCheckTime = Time.time + checkInterval;
    }

    private void Update()
    {
        // Call Steam API callbacks every frame
        if (SteamManager.Initialized)
        {
            SteamAPI.RunCallbacks();
        }

        // Check if it's time to check game ownership
        if (Time.time >= nextCheckTime)
        {
            CheckGameOwnership();
            nextCheckTime = Time.time + checkInterval; // Schedule the next check
        }
    }

    private void CheckGameOwnership()
    {
        // Check if the current app is owned
        bool ownsGame = SteamApps.BIsSubscribedApp((AppId_t)this.appId);

        if (ownsGame)
        {
            Debug.Log("Player owns the game.");
            // Add additional game logic for owners if needed
        }
        else
        {
            Debug.LogWarning("Player does not own the game. Exiting the application...");
            Application.Quit(); // Close the application if the game is not owned
        }
    }
}
#endif