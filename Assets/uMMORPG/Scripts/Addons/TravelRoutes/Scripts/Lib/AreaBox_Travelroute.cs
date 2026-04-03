using Mirror;
using UnityEngine;

// TRAVELROUTE - BOX COLLIDER
#if _iMMO2D
[RequireComponent(typeof(BoxCollider2D))]
#else
[RequireComponent(typeof(BoxCollider))]
#endif
[RequireComponent(typeof(NetworkIdentity))]
public class AreaBox_Travelroute : NetworkBehaviour
{

    [Header("[-=-[ TRAVEL ROUTES ]-=-]")]
    [Tooltip("[Optional] One click deactivation")]
    public bool isActive = true;

    [Tooltip("[Required] Travelroutes available on enter")]
    public Travelroute[] Travelroutes;

    [Tooltip("[Optional] Travelroutes unlocked on enter")]
    public Unlockroute[] Unlockroutes;

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
        Gizmos.DrawWireCube(collider.offset, collider.size);
#else
        Gizmos.DrawCube(collider.center, collider.size);
        Gizmos.DrawWireCube(collider.center, collider.size);
#endif
        Gizmos.color = gizmoWireColor;
        Gizmos.matrix = Matrix4x4.identity;
    }

    // -----------------------------------------------------------------------------------
    // OnTriggerEnter
    // @Client
    // -----------------------------------------------------------------------------------
#if _iMMO2D
    private void OnTriggerEnter2D(Collider2D co)
#else
    private void OnTriggerEnter(Collider co)
#endif
    {
        Player player = co.GetComponentInParent<Player>();
        if (player != null && player.isAlive && isActive && (player == player.isLocalPlayer || isServer))
        {
            player.playerTravelroute.myTravelrouteArea = this;
            player.playerTravelroute.UnlockTravelroutes();
            player.playerTravelroute.ShowTravelRoute();
        }
    }

    // -----------------------------------------------------------------------------------
    // OnTriggerExit
    // @Client
    // -----------------------------------------------------------------------------------
#if _iMMO2D
    private void OnTriggerExit2D(Collider2D co)
#else
    private void OnTriggerExit(Collider co)
#endif
    {
        Player player = co.GetComponentInParent<Player>();
        if (player != null && player.isAlive && isActive && (player == player.isLocalPlayer || isServer))
        {
            player.playerTravelroute.myTravelrouteArea = null;
            player.playerTravelroute.HideTravelRoute();
        }
    }

    // -----------------------------------------------------------------------------------
}