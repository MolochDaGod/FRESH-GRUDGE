using UnityEngine;

// TARGET CLEANSE ITEM

[CreateAssetMenu(menuName = "ADDON/Item/Item Target Cleanse", order = 999)]
public class Item_TargetCleanse : UsableItem
{
    [Header("[-=-[ Target Cleanse Item ]-=-]")]
    [Tooltip("% Chance to remove each Nerf on the target (Nerf = Buff set to 'disadvantegous')")]
    [Range(0, 1)] public float successChance;

    public float maxDistance = 10;
    public string successMessage = "You cleansed: ";

    [Header("[-=-[ Buff on Target ]-=-]")]
    public BuffSkill applyBuff;

    public int buffLevel;
    [Range(0, 1)] public float buffChance;
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

                player.target.Tools_CleanupStatusNerfs(successChance);
                player.target.Tools_ApplyBuff(applyBuff, buffLevel, buffChance);
                player.Tools_TargetAddMessage(successMessage + player.target.name);

                // decrease amount
                slot.DecreaseAmount(decreaseAmount);
                player.inventory.slots[inventoryIndex] = slot;
            }
        }
    }
#endif

    // tooltip
    /*public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{USAGEHEALTH}", usageHealth.ToString());
        tip.Replace("{USAGEMANA}", usageMana.ToString());
        tip.Replace("{USAGEEXPERIENCE}", usageExperience.ToString());
        tip.Replace("{USAGEPETHEALTH}", usagePetHealth.ToString());
        return tip.ToString();
    }*/
    // -----------------------------------------------------------------------------------
}
