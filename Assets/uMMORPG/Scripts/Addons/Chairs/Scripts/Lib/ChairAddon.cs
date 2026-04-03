using UnityEngine;
using Mirror;

public partial class ChairAddon : Tools_InteractableObject
{
    [SyncVar]
    public bool inUse = false;
    public Collider sitCollider = null;

    [ClientCallback]
    public override void OnInteractClient(Player player)
    {
        //Debug.Log(" requierement :" + interactionRequirements.checkRequirements(player));
        if (!Check(player) && !interactionRequirements.checkRequirements(player)) return;
        sitCollider.enabled = false;
        player.playerAddonsConfigurator.currentChair = this;
        player.StartCoroutine(player.playerAddonsConfigurator.startSitting());
    }

    [ServerCallback]
    public override void OnInteractServer(Player player)
    {
        if (!Check(player)) return;
        sitCollider.enabled = false;
        player.playerAddonsConfigurator.currentChair = this;
        inUse = true;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Used to check if Seat is taken, or player is already seated; just incase. //
    ///////////////////////////////////////////////////////////////////////////////
    private bool Check(Player player)
    {
        if (inUse || player.playerAddonsConfigurator.isSeated == true && !interactionRequirements.checkRequirements(player))
        {
            return false;
        }
        return true;
    }
}
