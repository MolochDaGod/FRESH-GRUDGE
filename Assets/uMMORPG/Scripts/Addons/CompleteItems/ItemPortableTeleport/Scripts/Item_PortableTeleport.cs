using Mirror;
using System.Text;
using UnityEngine;

// PORTABLE TELEPORT - ITEM

[CreateAssetMenu(menuName = "ADDON/Item/Item Portable Teleport", order = 998)]
public class Item_PortableTeleport : UsableItem
{
    [Header("[-=-[ Portable Teleport ]-=-]")]
    [Tooltip("[Required] GameObject prefab with coordinates OR off scene coordinates (requires Network Zones AddOn)")]
    public Tools_TeleportationTarget teleportationTarget;

    [Tooltip("This will ignore the teleport Location and choose the nearest spawn point instead")]
    public bool teleportToClosestSpawnpoint;

    [Header("Last Combat end time min")]
    public int combatEnTimer = 10;

    [Tooltip("Decrease amount by how many each use (can be 0)?")]
    public int decreaseAmount = 1;
    private double allowedTeleportTime;
    private double remainingTeleportTime;

#if _SERVER
    // -----------------------------------------------------------------------------------
    // Use
    // @Server
    // -----------------------------------------------------------------------------------
    public override void Use(Player player, int inventoryIndex)
    {
        ItemSlot slot = player.inventory.slots[inventoryIndex];
        allowedTeleportTime = player.lastCombatTime + combatEnTimer;
        remainingTeleportTime = NetworkTime.time < allowedTeleportTime ? (allowedTeleportTime - NetworkTime.time) : 0;
        if (remainingTeleportTime <= 0)
        {
            // -- Only activate if enough charges left
            if (decreaseAmount == 0 || slot.amount >= decreaseAmount)
            {
                // always call base function too
                base.Use(player, inventoryIndex);

                // -- Decrease Amount
                if (decreaseAmount != 0)
                {
                    slot.DecreaseAmount(decreaseAmount);
                    player.inventory.slots[inventoryIndex] = slot;
                }

                // -- Determine Teleportation Target
                if (teleportToClosestSpawnpoint)
                {
                    Transform target = NetworkManagerMMO.GetNearestStartPosition(player.transform.position);
                    player.Tools_Warp(target.position);
                }
                else
                {
                    teleportationTarget.OnTeleport(player);
                }
            }
        }
        else
        {
            player.Tools_TargetAddMessage("you were in combat very recently, you have to wait again " + ((int)remainingTeleportTime).ToString() + " Seconds");
        }
    }
#endif

    // -----------------------------------------------------------------------------------
    // Tooltip
    // @Client
    // -----------------------------------------------------------------------------------
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{COMBAT}", usageRequirements.cannotUseInCombat.ToString());
        tip.Replace("{DESTINATION}", teleportationTarget.name);
        tip.Replace("{COOLDOWN}", cooldown.ToString());
        return tip.ToString();
    }

    // -----------------------------------------------------------------------------------
}
