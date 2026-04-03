#if _iMMOUNLOCKABLECLASSES
using Mirror;

public partial class PlayerAddonsConfigurator
{
    public readonly SyncList<string> UnlockedClasses = new SyncList<string>();

    #region SERVER ONLY 
#if _SERVER
    // -----------------------------------------------------------------------------------
    // UnlockClass
    // -----------------------------------------------------------------------------------
    [Server]
    public bool UnlockClass(Player oPlayer, string message)
    {
        if (!HasUnlockedClass(oPlayer))
        {
            UnlockedClasses.Add(oPlayer.name);
            player.Tools_ShowPopup(message + oPlayer.name);
            return true;
        }

        return false;
    }

    // -----------------------------------------------------------------------------------
    // HasUnlockedClass
    // -----------------------------------------------------------------------------------
    [Server]
    public bool HasUnlockedClass(Player oPlayer)
    {
        if (oPlayer == null || oPlayer.name == "") return false;

        return UnlockedClasses.Contains(oPlayer.name);
    }
#endif
#endregion SERVER ONLY
    // -----------------------------------------------------------------------------------
}
#endif