using UnityEngine;
#if !DISABLESTEAMWORKS
using Steamworks;

public class SteamCheck : MonoBehaviour
{
    private float checkInterval = 10f; // Time interval to check for Steam (in seconds)
    private float nextCheckTime = 0f;   // Keeps track of the next check time

    private void Start()
    {
        // Run the first check immediately
        nextCheckTime = Time.time + checkInterval;
    }

    private void Update()
    {
        // Check if it's time to check Steam
        if (Time.time >= nextCheckTime)
        {
            CheckSteamStatus();
            nextCheckTime = Time.time + checkInterval; // Schedule the next check
        }

        // Run Steam API callbacks to keep it responsive
        if (SteamManager.Initialized)
        {
            SteamAPI.RunCallbacks();
        }
    }

    private void CheckSteamStatus()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("Steam is not running or initialized. Exiting the game...");
            Application.Quit(); // Close the application if Steam is not running
        }
    }
}
#endif