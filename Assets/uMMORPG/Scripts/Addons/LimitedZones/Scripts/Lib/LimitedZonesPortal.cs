using UnityEngine;
using Mirror;

// SHARED INSTANCE PORTAL
#if _iMMO2D
[RequireComponent(typeof(BoxCollider2D))]
#else
[RequireComponent(typeof(BoxCollider))]
#endif
[RequireComponent(typeof(NetworkIdentity))]
public class LimitedZonesPortal : NetworkBehaviour
{
    [Header("[INSTANCE PORTAL]")]
    public UI_LimitedZones ui;

    public int instanceCategory;

    [Header("[Editor]")]
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

    // -----------------------------------------------------------------------------------
    // OnTriggerExit
    // -----------------------------------------------------------------------------------
    [ClientCallback]
#if _iMMO2D
    private void OnTriggerEnter2D(Collider2D co)
#else
    private void OnTriggerEnter(Collider co)
#endif
    {
        Player player = co.GetComponentInParent<Player>();

        if (!player || !player.isAlive || player != Player.localPlayer) return;

        if (!ui)
            ui = FindObjectOfType<UI_LimitedZones>();

        ui.Show(instanceCategory);
    }

    // -----------------------------------------------------------------------------------
    // OnTriggerExit
    // -----------------------------------------------------------------------------------
    [ClientCallback]
#if _iMMO2D
    private void OnTriggerExit2D(Collider2D co)
#else
    private void OnTriggerExit(Collider co)
#endif
    {
        Player player = co.GetComponentInParent<Player>();

        if (!player || !player.isAlive || player != Player.localPlayer) return;

        if (!ui)
            ui = FindObjectOfType<UI_LimitedZones>();

        ui.Hide();
    }

    // -----------------------------------------------------------------------------------
}