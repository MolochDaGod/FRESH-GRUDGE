using UnityEngine;
using Mirror;

// ===================================================================================
// NETWORK PORTAL (BOX)
// ===================================================================================
public class NetworkZonePortalBox : Tools_InteractableAreaBox
{
    [Header("[-=-=-[ NETWORK ZONE PORTAL ]-=-=-]")]
    public SceneLocation targetScene;

    // -----------------------------------------------------------------------------------
    // OnInteractClient
    // @Client
    // -----------------------------------------------------------------------------------
    [ClientCallback]
    public override void OnInteractClient(Player player)
    {
        Tools_UI_Tools.FadeOutScreen(false, 0.25f);
    }

    // -----------------------------------------------------------------------------------
    // OnInteractServer
    // @Server
    // -----------------------------------------------------------------------------------
    [ServerCallback]
    public override void OnInteractServer(Player player)
    {
#if _SERVER
        player.playerNetworkLobby.OnPortal(targetScene);
#endif
    }

    // -----------------------------------------------------------------------------------
}
