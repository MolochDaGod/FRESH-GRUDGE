using UnityEngine;

// SKILL PARTY LEADER BUFF
[CreateAssetMenu(menuName = "ADDON/Item/Item Party Leader Buff", order = 999)]
public class Item_PartyLeaderBuff : UsableItem
{
    [Header("[-=-[ Buff Party Members ]-=-]")]
    public BuffSkill applyBuff;

    public int skillLevel;

    [Tooltip("Decrease amount by how many each use (can be 0)?")]
    public int decreaseAmount = 1;

#if _SERVER
    // -----------------------------------------------------------------------------------
    // Apply
    // -----------------------------------------------------------------------------------
    public override void Use(Player player, int inventoryIndex)
    {
        ItemSlot slot = player.inventory.slots[inventoryIndex];

        // -- Only activate if enough charges left
        if (decreaseAmount == 0 || slot.amount >= decreaseAmount)
        {
            if (player.party.InParty())
            {
                // always call base function too
                base.Use(player, inventoryIndex);

                foreach (string member in (player.party.party.members))
                {
                    if (Player.onlinePlayers.ContainsKey(member))
                    {
                        Player plyr = Player.onlinePlayers[member];
                        if (plyr.isAlive)
                        {
                            plyr.Tools_ApplyBuff(applyBuff, skillLevel, 1);
                        }
                    }
                }

                // decrease amount
                slot.DecreaseAmount(decreaseAmount);
                player.inventory.slots[inventoryIndex] = slot;
            }
        }
    }
#endif
    // -----------------------------------------------------------------------------------
}
