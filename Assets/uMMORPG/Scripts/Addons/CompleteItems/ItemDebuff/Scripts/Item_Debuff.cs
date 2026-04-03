using UnityEngine;

// ITEM DEBUFF
[CreateAssetMenu(menuName = "ADDON/Item/Item Debuff", order = 999)]
public class Item_Debuff : UsableItem
{
    [Header("[-=-[ Item Debuff ]-=-]")]
    [Tooltip("% Chance to remove the Buff from the target")]
    [Range(0, 1)] public float successChance;

    public BuffSkill[] removeBuffs;
    public float maxDistance = 10;
    public string successMessage = "You removed {0} from: {1} ";

    [Header("[-=-[ New Buff on Target? ]-=-]")]
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

                foreach (BuffSkill removeBuff in removeBuffs)
                {
                    player.target.Tools_RemoveBuff(removeBuff, successChance);
                    player.Tools_TargetAddMessage(string.Format(successMessage, removeBuff.name, player.target.name));
                }

                player.target.Tools_ApplyBuff(applyBuff, buffLevel, buffChance);

                // decrease amount
                slot.DecreaseAmount(decreaseAmount);
                player.inventory.slots[inventoryIndex] = slot;
            }
        }
    }
#endif

    // tooltip
    // is serait bien d'afficher la liste des buffs qu'il suprimme 
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
