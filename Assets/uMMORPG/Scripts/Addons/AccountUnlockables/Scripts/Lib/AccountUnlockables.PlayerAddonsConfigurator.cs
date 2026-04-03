using Mirror;
using UnityEngine;

// PLAYER
public partial class PlayerAddonsConfigurator
{

    public readonly SyncList<string> AccountUnlockables = new SyncList<string>();

#if _SERVER
    // -----------------------------------------------------------------------------------
    // AccountUnlock
    // Server
    // -----------------------------------------------------------------------------------
    public bool AccountUnlock(string unlockableName, string message)
    {
        if (!HasAccountUnlock(unlockableName))
        { 
            AccountUnlockables.Add(unlockableName.ToLower());
            player.Tools_ShowPopup(message + unlockableName);
            return true;
        }

        return false;
    }
#endif
    // -----------------------------------------------------------------------------------
    // HasAccountUnlock
    // Server & Client
    // -----------------------------------------------------------------------------------
    public bool HasAccountUnlock(string unlockableName)
    {
        if (string.IsNullOrWhiteSpace(unlockableName)) return true;

        return AccountUnlockables.Contains(unlockableName.ToLower());
    }
    // -----------------------------------------------------------------------------------
}