using Mirror;
using UnityEngine;

// INTERACTABLE LOTTERY
public partial class InteractableLottery : Tools_InteractableObject
{
    [Header("[-=-[ Lottery Object ]-=-]")]
    public Tools_InteractionRewards rewards;

    // -----------------------------------------------------------------------------------
    // OnInteractServer
    // @Server
    // -----------------------------------------------------------------------------------
    [ServerCallback]
    public override void OnInteractServer(Player player)
    {
        rewards.gainRewards(player);
    }
}
