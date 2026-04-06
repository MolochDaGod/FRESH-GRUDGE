#if _iMMOTOOLS
using Mirror;
using UnityEngine;

// ===================================================================================
// INTERACTABLE CLASS
// ===================================================================================
[RequireComponent(typeof(NetworkIdentity))]
public abstract partial class Tools_Interactable : NetworkBehaviour
{


    [Header("[-=-=-[ INTERACTABLE ]-=-=-]")]
    public SpriteRenderer interactionSpriteRenderer;
    public string interactionText = "Interact with this Object";
    public Sprite interactionIcon;
    public bool automaticActivation;

    public Tools_InteractionRequirements interactionRequirements;

    protected Tools_UI_InteractableAccessRequirement instance;

    [SyncVar, HideInInspector] public bool unlocked = false;

    // -----------------------------------------------------------------------------------
    // Start
    // -----------------------------------------------------------------------------------
    public virtual void Start()
    {
        if (interactionIcon != null && interactionSpriteRenderer != null)
            interactionSpriteRenderer.sprite = interactionIcon;
    }

    // -----------------------------------------------------------------------------------
    // OnInteractClient
    // -----------------------------------------------------------------------------------
    //[ClientCallback]
    public virtual void OnInteractClient(Player player) { }

    // -----------------------------------------------------------------------------------
    // OnInteractServer
    // -----------------------------------------------------------------------------------
    //[ServerCallback]
    public virtual void OnInteractServer(Player player) { }

    // -----------------------------------------------------------------------------------
    // IsUnlocked
    // -----------------------------------------------------------------------------------
    public virtual bool IsUnlocked() { return false; }

    // -----------------------------------------------------------------------------------
    // ConfirmAccess
    // @Client
    // -----------------------------------------------------------------------------------
    public virtual void ConfirmAccess()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        if (interactionRequirements.checkRequirements(player) || IsUnlocked())
        {
            OnInteractClient(player);
            player.Cmd_Tools_OnInteractServer(this.gameObject);
        }
        else
        {
            if (automaticActivation)
                interactionRequirements.UpdateRequirementChat();
        }
    }

    // -----------------------------------------------------------------------------------
    // ShowAccessRequirementsUI
    // @Client
    // -----------------------------------------------------------------------------------
    protected virtual void ShowAccessRequirementsUI()
    {
        if (instance == null)
            instance = FindObjectOfType<Tools_UI_InteractableAccessRequirement>();

        instance.Show(this);
    }

    // -----------------------------------------------------------------------------------
    // HideAccessRequirementsUI
    // @Client
    // -----------------------------------------------------------------------------------
    protected void HideAccessRequirementsUI()
    {
        if (instance == null)
            instance = FindObjectOfType<Tools_UI_InteractableAccessRequirement>();

        instance.Hide();
    }

    // -----------------------------------------------------------------------------------
    // IsWorthUpdating
    // -----------------------------------------------------------------------------------
    public virtual bool IsWorthUpdating()
    {
        return netIdentity.observers == null ||
               netIdentity.observers.Count > 0;
    }

    // -----------------------------------------------------------------------------------
}
#endif