using Mirror;
using UnityEngine;


// SIMPLE TELEPORT AREA

[RequireComponent(typeof(Collider))]
public class AreaSphereTeleportBindpoint : NetworkBehaviour
{
    [Header("[-=-=-=-[TELEPORTER]-=-=-=-]")]
    [Tooltip("[Optional] One click deactivation")]
    public bool isActive = true;

    [Header("-=-=- Editor -=-=-")]
    public Color gizmoColor = new Color(0, 1, 1, 0.25f);

    public Color gizmoWireColor = new Color(1, 1, 1, 0.8f);

    // -----------------------------------------------------------------------------------
    // OnDrawGizmos
    // @Editor
    // -----------------------------------------------------------------------------------
    private void OnDrawGizmos()
    {
        Collider collider = GetComponent<Collider>();
    }

    // -----------------------------------------------------------------------------------
    // OnTriggerEnter
    // -----------------------------------------------------------------------------------
    [ServerCallback]
    private void OnTriggerEnter(Collider co)
    {
        Player player = co.GetComponentInParent<Player>();
        if (player && isActive)
                        // -- Activate Teleport
		if (player.playerAddonsConfigurator.MyBindpoint.Valid)
			player.movement.Warp(player.playerAddonsConfigurator.MyBindpoint.position);
    }

    // -----------------------------------------------------------------------------------
}
