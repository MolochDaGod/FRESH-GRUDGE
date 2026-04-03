using Mirror;
using UnityEngine;

// sAREA (SPHERE) INTERACTABLE TELEPORT
#if _iMMO2D
[RequireComponent(typeof(CircleCollider2D))]
#else
[RequireComponent(typeof(SphereCollider))]
#endif
public class AreaSphere_InteractableTeleport : Tools_InteractableAreaSphere
{
    [Header("[-=-[ Teleporter ]-=-]")]
    [Tooltip("[Required] Any on scene Transform or GameObject OR off scene coordinates (requires Network Zones AddOn)")]
    public Tools_TeleportationTarget teleportationTarget;

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
