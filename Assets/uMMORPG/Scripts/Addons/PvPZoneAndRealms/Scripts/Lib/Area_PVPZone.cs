using Mirror;
using UnityEngine;
#if _iMMO2D
[RequireComponent(typeof(CircleCollider2D))]
#else
[RequireComponent(typeof(SphereCollider))]
#endif
public partial class Area_PVPZone : NetworkBehaviour
{
    [Header("[SETTINGS (Set ProxChecker VisRange to 2x Radius of Area or bigger)]")]
    public PVPZONE_Settings pvpZone_settings;

#if !_iMMO2D
#if UNITY_EDITOR
    [Header("[EDITOR]")]
    public Color gizmoColor = new Color(0, 1, 1, 0.25f);

    public Color gizmoWireColor = new Color(1, 1, 1, 0.8f);

    // -----------------------------------------------------------------------------------
    // OnDrawGizmos
    // @Editor
    // -----------------------------------------------------------------------------------
    private void OnDrawGizmos()
    {
        SphereCollider collider = GetComponent<SphereCollider>();

        // we need to set the gizmo matrix for proper scale & rotationPlaceableObject
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(collider.center, collider.radius);
        Gizmos.color = gizmoWireColor;
        Gizmos.DrawWireSphere(collider.center, collider.radius);
        Gizmos.matrix = Matrix4x4.identity;
    }
#endif
#endif
    #region SERVER ONLY
#if _SERVER
    // -----------------------------------------------------------------------------------
    // OnTriggerEnter
    // @Server
    // -----------------------------------------------------------------------------------
#if _iMMO2D
    private void OnTriggerEnter2D(Collider2D co)
#else
    private void OnTriggerEnter(Collider co)
#endif
    {
        if (isServer)
        {
            Player player = co.GetComponentInParent<Player>();
            if (player)
            {

                if (player.playerPVPZone) // Check player is online for 
                {
                    player.playerPVPZone.PvpRegionEnter(pvpZone_settings);
                }
                else
                {
                    Debug.LogWarning("Player prefab " + player.className + " not have PlayerPVPZone linked !");
                }
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // OnTriggerExit
    // @Server
    // -----------------------------------------------------------------------------------
#if _iMMO2D
    private void OnTriggerExit2D(Collider2D co)
#else
    private void OnTriggerExit(Collider co)
#endif
    {
        if (isServer)
        {
            Player player = co.GetComponentInParent<Player>();
            if (player)
            {
                if (player.playerPVPZone)
                {
                    player.playerPVPZone.PvpRegionLeave(pvpZone_settings);
                    if (pvpZone_settings.requirements.checkRequirements(player))
                    {
                        // Intentionally no-op; keep conditional explicit for future hooks.
                    }
                }
            }
        }
    }
#endif
    #endregion SERVER ONLY
    // -----------------------------------------------------------------------------------
}
