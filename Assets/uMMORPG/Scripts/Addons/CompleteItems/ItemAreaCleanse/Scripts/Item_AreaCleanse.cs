using System.Collections.Generic;
using UnityEngine;

// AREA CLEANSE ITEM

[CreateAssetMenu(menuName = "ADDON/Item/Item Area Cleanse", order = 999)]
public class Item_AreaCleanse : UsableItem
{
    [Header("[-=-[ Target Cleanse Item ]-=-]")]
    [Tooltip("% Chance to remove each Nerf on the target (Nerf = Buff set to 'disadvantegous')")]
    [Range(0, 1)] public float successChance;

    public float range;
    public string successMessage = "You cleansed: ";

    [Header("[-=-[ Buff on Target ]-=-]")]
    public BuffSkill applyBuff;

    public int buffLevel;
    [Range(0, 1)] public float buffChance;

    [Header("[-=-[ Targeting ]-=-]")]
    [Tooltip("[Optional] Changes 'affect' affect into 'not affect' and vice-versa")]
    public bool reverseTargeting;

    [Tooltip("[Optional] Does affect the caster")]
    public bool affectSelf;

    [Tooltip("[Optional] Does affect members of the own party")]
    public bool affectOwnParty;

    [Tooltip("[Optional] Does affect members of the own guild")]
    public bool affectOwnGuild;

    [Tooltip("[Optional] Does affect members of the own realm (requires PVP ZONE AddOn")]
    public bool affectOwnRealm;

    public bool affectPlayers;
    public bool affectNpcs;
    public bool affectMonsters;
    public bool affectPets;

    [Tooltip("Decrease amount by how many each use (can be 0)?")]
    public int decreaseAmount = 1;

    // -----------------------------------------------------------------------------------
    // CheckTarget
    // -----------------------------------------------------------------------------------
    public bool CheckTarget(Entity caster)
    {
        caster.target = caster;
        return true;
    }

    // -----------------------------------------------------------------------------------
    // CheckDistance
    // -----------------------------------------------------------------------------------
    public bool CheckDistance(Entity caster, int skillLevel, out Vector3 destination)
    {
        destination = caster.transform.position;
        return true;
    }

#if _SERVER
    // -----------------------------------------------------------------------------------
    // Use
    // -----------------------------------------------------------------------------------
    public override void Use(Player player, int inventoryIndex)
    {
        List<Entity> targets = new List<Entity>();
        ItemSlot slot = player.inventory.slots[inventoryIndex];

        // -- Only activate if enough charges left
        if (decreaseAmount == 0 || slot.amount >= decreaseAmount)
        {
            // always call base function too
            base.Use(player, inventoryIndex);

            targets = player.Tools_GetCorrectedTargetsInSphere(player.transform, range, false, affectSelf, affectOwnParty, affectOwnGuild, affectOwnRealm, reverseTargeting, affectPlayers, affectNpcs, affectMonsters, affectPets);

            foreach (Entity target in targets)
            {
                target.Tools_CleanupStatusNerfs(successChance);
                target.Tools_ApplyBuff(applyBuff, buffLevel, buffChance);
                player.Tools_TargetAddMessage(successMessage + target.name);
            }

            // decrease amount
            slot.DecreaseAmount(decreaseAmount);
            player.inventory.slots[inventoryIndex] = slot;
        }

        targets.Clear();
    }
#endif
    // -----------------------------------------------------------------------------------
}
