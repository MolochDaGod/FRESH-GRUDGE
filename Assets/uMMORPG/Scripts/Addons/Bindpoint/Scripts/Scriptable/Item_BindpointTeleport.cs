using Mirror;
using System.Text;
using UnityEngine;

// PORTABLE TELEPORT - ITEM
[CreateAssetMenu(menuName = "ADDON/Bindpoint/Bindpoint Teleport", order = 998)]
public class Item_BindpointTeleport : UsableItem
{
    [Header("Last Combat end TimeLogout min")]
    public int combatEnTimer = 10;
    [Header("[-=-[ Teleport to current Bindpoint ]-=-]")]
    [Tooltip("Decrease amount by how many each use (can be 0)?")]
    public int decreaseAmount = 1;
    private double allowedTeleportTime;
    private double remainingTeleportTime;
    // -----------------------------------------------------------------------------------
    // Use
    // @Server
    // -----------------------------------------------------------------------------------
#if _SERVER
    public override void Use(Player player, int inventoryIndex)
    {
        ItemSlot slot = player.inventory.slots[inventoryIndex];

        allowedTeleportTime = player.lastCombatTime + combatEnTimer;
        remainingTeleportTime = NetworkTime.time < allowedTeleportTime ? (allowedTeleportTime - NetworkTime.time) : 0;
        if (remainingTeleportTime <= 0)
        {
            // -- Only activate if enough charges left + a bindpoint has been set
            if (player.playerAddonsConfigurator.MyBindpoint.Valid && (decreaseAmount == 0 || slot.amount >= decreaseAmount))
            {
                // always call base function too
                base.Use(player, inventoryIndex);

                // -- Decrease Amount
                if (decreaseAmount != 0)
                {
                    slot.DecreaseAmount(decreaseAmount);
                    player.inventory.slots[inventoryIndex] = slot;
                }

                // -- Activate Teleport
                player.playerAddonsConfigurator.RespawnToLocalBindpoint();
            }
        }
        else
        {
            player.Tools_AddMessage("you were in combat very recently, you have to wait again " + ((int)remainingTeleportTime).ToString() + " Seconds" );
        }
    }
#endif

    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{COOLDOWN}", cooldown.ToString());
        return tip.ToString();
    }
    // -----------------------------------------------------------------------------------
}
