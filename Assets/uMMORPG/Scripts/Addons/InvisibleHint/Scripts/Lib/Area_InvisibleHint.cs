using UnityEngine;
using Mirror;

// ===================================================================================
// INVISIBLE HINT AREA
// ===================================================================================
#if _iMMO2D
[RequireComponent(typeof(BoxCollider2D))]
#else
[RequireComponent(typeof(BoxCollider))]
#endif
public class Area_InvisibleHint : Tools_InteractableAreaBox
{
    [Tooltip("[Optional] One click deactivation")]
    public bool isActive = true;

    [Tooltip("[Required] Text to display while in the area"), TextArea(1, 30)]
    public string textToDisplay = "";

    [Tooltip("[Optional] Automatically hide after x seconds?")]
    public float hideAfter;

    // -----------------------------------------------------------------------------------
    // OnInteractClient
    // -----------------------------------------------------------------------------------
    //[ClientCallback]
    public override void OnInteractClient(Player player)
    {
        if (isActive && textToDisplay != "")
            player.InvisibleHint_Show(textToDisplay, hideAfter);
    }

    // -------------------------------------------------------------------------------
    // OnTriggerExit
    // -------------------------------------------------------------------------------
    [ClientCallback]
#if _iMMO2D
    private void OnTriggerExit2D(Collider2D co)
#else
    private void OnTriggerExit(Collider co)
#endif
    {
        Player player = co.GetComponentInParent<Player>();

        if (player && player == Player.localPlayer && textToDisplay != "")
            player.InvisibleHint_Hide();
    }

    // -------------------------------------------------------------------------------
}