using Mirror;
using UnityEngine;
#if _iMMOCRAFTING
// INTERACTABLE WORKBENCH
public partial class InteractableWorkbench : Tools_InteractableObject
{
    [Header("[ANIMATION & SOUND]")]
    [Tooltip("[Optional] GameObject spawned as effect when successfully crafted (see ReadMe).")]
    public GameObject craftEffect;

    [Tooltip("[Optional] Sound played when successfully crafted.")]
    public AudioClip craftSound;

    [Header("[MESSAGES]")]
    public string levelUpMessage = "Craft level up: ";

    public string nothingMessage = "Nothing to craft!";

    // -----------------------------------------------------------------------------------
    // OnInteractClient
    // @Client
    // -----------------------------------------------------------------------------------
    [ClientCallback]
    public override void OnInteractClient(Player player)
    {
        player.playerCraftingExtended.OnSelect_InteractableWorkbench(this);
    }

    // -----------------------------------------------------------------------------------
    // OnCrafted
    // -----------------------------------------------------------------------------------
    public void OnCrafted()
    {
        SpawnEffect(craftEffect, craftSound);
    }
    // -----------------------------------------------------------------------------------
}
#endif