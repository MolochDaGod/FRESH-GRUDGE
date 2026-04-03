using UnityEngine;

// TAUNT ITEM
[CreateAssetMenu(menuName = "ADDON/Item/Item Taunt", order = 999)]
public class Item_Taunt : UsableItem
{
    [Header("[-=-[ Taunt Item ]-=-]")]
    [Range(0, 1)] public float successChance;

    public float maxDistance = 10;
    public string tauntMessage = "You taunted: ";
    public string failedMessage = "You failed to taunt: ";
    [Header("[-=-[ Targeting ]-=-]")]
    [Tooltip("[Optional] Changes 'affect' affect into 'not affect' and vice-versa")]
    public bool reverseTargeting;

    [Tooltip("[Optional] Does affect members of the own party")]
    public bool affectOwnParty;

    [Tooltip("[Optional] Does affect members of the own guild")]
    public bool affectOwnGuild;

    [Tooltip("[Optional] Does affect members of the own realm (requires PVP ZONE AddOn")]
    public bool affectOwnRealm;

    public bool affectPlayers;
    public bool affectMonsters;
    public bool affectNpc;
    public bool affectPets;
    [Tooltip("Decrease amount by how many each use (can be 0)?")]
    public int decreaseAmount = 1;

#if _SERVER
    // -----------------------------------------------------------------------------------
    // Use
    // -----------------------------------------------------------------------------------
    public override void Use(Player player, int inventoryIndex)
    {
        ItemSlot slot = player.inventory.slots[inventoryIndex];

        // -- Only activate if enough charges left
        if (decreaseAmount == 0 || slot.amount >= decreaseAmount)
        {
            if (player.target != null && player.target is Entity && player.target.isAlive && player.Tools_GetCorrectedTargets(player.target, false, false, affectOwnParty, affectOwnGuild, affectOwnRealm, reverseTargeting, affectPlayers, affectNpc, affectMonsters, affectPets) && Utils.ClosestDistance(player, player.target) <= maxDistance)
            {
                // always call base function too
                base.Use(player, inventoryIndex);

                //Monster monster = player.target.GetComponent<Monster>();

                if (UnityEngine.Random.value <= successChance)
                {
                    player.target.Tools_OnAggro(player, 1);
                    player.Tools_TargetAddMessage(tauntMessage + player.target.name);
                }
                else
                {
                    player.Tools_TargetAddMessage(failedMessage + player.target.name);
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
