using UnityEngine;

// TARGET CLEANSE ITEM

[CreateAssetMenu(menuName = "ADDON/Item/Item Target Buff", order = 999)]
public class Item_TargetBuff : UsableItem
{
    [Header("[-=-[ Target Buff Item ]-=-]")]
    public BuffSkill applyBuff;

    public int buffLevel;
    [Range(0, 1)] public float buffChance;
    public float distance;
    public string successMessage = "You buffed {0} with {1}";

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
    public bool affectMonsters;
    public bool affectNpc;
    public bool affectPets;

    public bool defaultSelfTarget;

    [Tooltip("Decrease amount by how many each use (can be 0)?")]
    public int decreaseAmount = 1;

    private Entity _saveoldTarget;


    // -----------------------------------------------------------------------------------
    // CheckTarget
    // -----------------------------------------------------------------------------------
    public bool CheckTarget(Entity caster)
    {
        return caster.target != null && (caster.CanAttack(caster.target) || caster.target == caster);
    }

    // -----------------------------------------------------------------------------------
    // CheckDistance
    // -----------------------------------------------------------------------------------
    public bool CheckDistance(Entity caster, int skillLevel, out Vector3 destination)
    {
        if (caster.target != null)
        {
            destination = caster.target.collider.ClosestPointOnBounds(caster.transform.position);
            return Utils.ClosestDistance(caster, caster.target) <= distance;
        }
        caster.target = caster;
        destination = caster.transform.position;
        return false;
    }

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
            // check target is good
            if (player.target != null && player.Tools_GetCorrectedTargets(player.target, false, affectSelf, affectOwnParty, affectOwnGuild, affectOwnRealm, reverseTargeting, affectPlayers, affectNpc, affectMonsters, affectPets)
                && Utils.ClosestDistance(player, player.target) <= distance)
            {
                // always call base function too
                base.Use(player, inventoryIndex);

                player.target.Tools_ApplyBuff(applyBuff, buffLevel, buffChance);

                player.Tools_TargetAddMessage(string.Format(successMessage, player.target.name, applyBuff.name));

                // decrease amount
                slot.DecreaseAmount(decreaseAmount);
                player.inventory.slots[inventoryIndex] = slot;

            }
            // if is not goot and select defaultSelfTarget
            else if (defaultSelfTarget)
            {
                //Save old target for restaure
                _saveoldTarget = player.target;
                player.target = player;

                // always call base function too
                base.Use(player, inventoryIndex);

                player.target.Tools_ApplyBuff(applyBuff, buffLevel, buffChance);

                player.Tools_TargetAddMessage(string.Format(successMessage, player.target.name, applyBuff.name));

                // decrease amount
                slot.DecreaseAmount(decreaseAmount);
                player.inventory.slots[inventoryIndex] = slot;

                // Restore old target
                player.target = _saveoldTarget;
            }

        }
    }
#endif
    // -----------------------------------------------------------------------------------
}
