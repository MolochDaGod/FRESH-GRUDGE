using Mirror;
using UnityEngine;

// AREA (BOX) INTERACTABLE TELEPORT
#if _iMMO2D
[RequireComponent(typeof(BoxCollider2D))]
#else
[RequireComponent(typeof(BoxCollider))]
#endif
public class AreaBox_InteractableTeleport : Tools_InteractableAreaBox
{
    [Header("[-=-[ Teleporter ]-=-]")]
    [Tooltip("[Required] Any on scene Transform or GameObject OR off scene coordinates (requires Network Zones AddOn)")]
    public Tools_TeleportationTarget teleportationTarget;
    public string sceneName;

#if _SERVER
    // -----------------------------------------------------------------------------------
    // OnInteractServer
    // -----------------------------------------------------------------------------------
    [ServerCallback]
    public override void OnInteractServer(Player player)
    {
        teleportationTarget.OnTeleport(player);
    }
#endif
    // -----------------------------------------------------------------------------------
}
