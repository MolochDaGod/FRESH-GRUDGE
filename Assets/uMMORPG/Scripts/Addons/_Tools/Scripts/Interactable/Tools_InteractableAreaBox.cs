#if _iMMOTOOLS
using Mirror;
using UnityEngine;

// INTERACTABLE AREA (BOX) CLASS
#if _iMMO2D
[RequireComponent(typeof(BoxCollider2D))]
#else
[RequireComponent(typeof(BoxCollider))]
#endif
public partial class Tools_InteractableAreaBox : Tools_Interactable
{
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
    // OnTriggerEnter
    // @Client
    // -----------------------------------------------------------------------------------
    [ClientCallback]
#if _iMMO2D
    private void OnTriggerEnter2D(Collider2D co)
#else
    private void OnTriggerEnter(Collider co)
#endif
    {
        Player player = co.GetComponentInParent<Player>();

        if (player != null && player == Player.localPlayer)
        {
            if ((!interactionRequirements.hasRequirements() && !interactionRequirements.requierementCost.HasCosts()) || automaticActivation)
            {
                ConfirmAccess();
            }
            else
            {
                ShowAccessRequirementsUI();
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // OnTriggerExit
    // @Client
    // -----------------------------------------------------------------------------------
    [ClientCallback]
#if _iMMO2D
    private void OnTriggerExit2D(Collider2D co)
#else
    private void OnTriggerExit(Collider co)
#endif
    {
        Player player = co.GetComponentInParent<Player>();

        if (player != null && player == Player.localPlayer)
            HideAccessRequirementsUI();
    }

    // -----------------------------------------------------------------------------------
    // Update
    // @Client
    // -----------------------------------------------------------------------------------
    /*[ClientCallback]
    private void Update()
    {
        player player = player.localPlayer;
        if (!player) return;

        // -- check for interaction Distance
        if (IsWorthUpdating())
            this.GetComponentInChildren<SpriteRenderer>().enabled = UMMO_Tools.Tools_CheckSelectionHandling(this.gameObject);
    }*/

    // -----------------------------------------------------------------------------------
}
#endif