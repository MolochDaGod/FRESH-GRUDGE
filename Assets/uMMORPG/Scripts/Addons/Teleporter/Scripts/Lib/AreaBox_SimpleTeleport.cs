using Mirror;
using UnityEngine;

// SIMPLE TELEPORT AREA
#if _iMMO2D
[RequireComponent(typeof(BoxCollider2D))]
#else
[RequireComponent(typeof(BoxCollider))]
#endif
public class AreaBox_SimpleTeleport : NetworkBehaviour
{
    [Header("[-=-[ Teleporter ]-=-]")]
    [Tooltip("[Optional] One click deactivation")]
    public bool isActive = true;

    [Tooltip("[Required] Any on scene Transform or GameObject OR off scene coordinates (requires Network Zones AddOn)")]
    public Tools_TeleportationTarget teleportationTarget;
    public string targerScene;


    [Header("[-=-[ Editor ]-=-]")]
    public Color gizmoColor = new Color(0, 1, 1, 0.25f);

    public Color gizmoWireColor = new Color(1, 1, 1, 0.8f);

    // -----------------------------------------------------------------------------------
    // OnDrawGizmos
    // @Editor
    // -----------------------------------------------------------------------------------
    private void OnDrawGizmos()
    {
#if _iMMO2D
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
#else
        BoxCollider collider = GetComponent<BoxCollider>();
#endif
        // we need to set the gizmo matrix for proper scale & rotation
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.color = gizmoColor;
#if _iMMO2D
        Gizmos.DrawCube(collider.offset, collider.size);
#else
        Gizmos.DrawCube(collider.center, collider.size);
#endif
        Gizmos.color = gizmoWireColor;
#if _iMMO2D
        Gizmos.DrawWireCube(collider.offset, collider.size);
#else
        Gizmos.DrawWireCube(collider.center, collider.size);
#endif

        Gizmos.matrix = Matrix4x4.identity;
    }

#if _SERVER
    // -----------------------------------------------------------------------------------
    // OnTriggerEnter
    // -----------------------------------------------------------------------------------
    [ServerCallback]
#if _iMMO2D
    private void OnTriggerEnter2D(Collider2D co)
#else
    private void OnTriggerEnter(Collider co)
#endif
    {
        Player player = co.GetComponentInParent<Player>();
        if (player && isActive)
            teleportationTarget.OnTeleport(player);
    }
#endif
    // -----------------------------------------------------------------------------------
}
