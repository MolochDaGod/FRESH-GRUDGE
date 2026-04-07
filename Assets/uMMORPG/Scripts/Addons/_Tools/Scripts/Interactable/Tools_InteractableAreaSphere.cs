#if _iMMOTOOLS
using Mirror;
using UnityEngine;

// INTERACTABLE AREA (SPHERE) CLASS
#if !_iMMO2D
[RequireComponent(typeof(SphereCollider))]
#else
[RequireComponent(typeof(CircleCollider2D))]
#endif
public partial class Tools_InteractableAreaSphere : Tools_Interactable
{
    [Header("Draw Gizmo")]
    public Color gizmoColor = new Color(0, 1, 1, 0.25f);
    public Color gizmoWireColor = new Color(1, 1, 1, 0.8f);

    // -----------------------------------------------------------------------------------
    // OnDrawGizmos
    // @Editor
    // -----------------------------------------------------------------------------------
    private void OnDrawGizmos()
    {
#if !_iMMO2D
        SphereCollider collider = GetComponent<SphereCollider>();
#else
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
#endif

#if !_iMMO2D
        // we need to set the gizmo matrix for proper scale & rotation
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(collider.center, collider.radius);
        Gizmos.color = gizmoWireColor;
        Gizmos.DrawWireSphere(collider.center, collider.radius);
        Gizmos.matrix = Matrix4x4.identity;
#endif
    }

    // -----------------------------------------------------------------------------------
    // OnTriggerEnter
    // @Client
    // -----------------------------------------------------------------------------------
    [ClientCallback]
#if !_iMMO2D
    private void OnTriggerEnter(Collider co)
#else
    private void OnTriggerEnter2D(Collider2D co)
#endif
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
#if !_iMMO2D
    private void OnTriggerExit(Collider co)
#else
    private void OnTriggerExit2D(Collider2D co)
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