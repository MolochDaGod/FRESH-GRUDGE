#if _iMMOTOOLS
using Mirror;
using UnityEngine;

// INTERACTABLE AREA (CAPSULE) CLASS

[RequireComponent(typeof(CapsuleCollider))]
public partial class Tools_InteractableAreaCapsule : Tools_Interactable
{
    public Color gizmoColor = new Color(0, 1, 1, 0.25f);
    public Color gizmoWireColor = new Color(1, 1, 1, 0.8f);

    // -----------------------------------------------------------------------------------
    // OnDrawGizmos
    // @Editor
    // -----------------------------------------------------------------------------------
    private void OnDrawGizmos()
    {
        CapsuleCollider collider = GetComponent<CapsuleCollider>();

        // we need to set the gizmo matrix for proper scale & rotation
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.color = new Color(0, 1, 1, 0.25f);
        Gizmos.DrawSphere(collider.center, collider.radius);
        Gizmos.color = new Color(1, 1, 1, 0.8f);
        Gizmos.DrawWireSphere(collider.center, collider.radius);
        Gizmos.matrix = Matrix4x4.identity;
    }

    // -----------------------------------------------------------------------------------
    // OnTriggerEnter
    // @Client
    // -----------------------------------------------------------------------------------
    [ClientCallback]
    private void OnTriggerEnter(Collider co)
    {
        Player player = co.GetComponentInParent<Player>();

        if (player != null && player == Player.localPlayer)
        {
            if ((!interactionRequirements.hasRequirements() && !interactionRequirements.hasCosts()) || automaticActivation)
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
    private void OnTriggerExit(Collider co)
    {
        Player player = co.GetComponentInParent<Player>();

        if (player != null && player == Player.localPlayer)
            HideAccessRequirementsUI();
    }

    // -----------------------------------------------------------------------------------
    // Update
    // @Client
    // -----------------------------------------------------------------------------------
    [ClientCallback]
    private void Update()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        // -- check for interaction Distance
        if (IsWorthUpdating())
            this.GetComponentInChildren<SpriteRenderer>().enabled = UMMO_Tools.Tools_CheckSelectionHandling(this.gameObject);
    }

    // -----------------------------------------------------------------------------------
}
#endif