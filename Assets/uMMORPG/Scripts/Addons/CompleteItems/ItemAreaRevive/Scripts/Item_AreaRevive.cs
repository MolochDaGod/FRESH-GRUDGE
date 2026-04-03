using UnityEngine;
using System.Collections.Generic;

// AREA REVIVE ITEM
[CreateAssetMenu(menuName = "ADDON/Item/Item Area Revive", order = 999)]
public class Item_AreaRevive : UsableItem
{
    [Header("[-=-[ Area Revive Item ]-=-]")]
    [Range(0, 1)] public float successChance;

    public float range;
    public int healsHealth;
    public int healsMana;

    [Header("[-=-[ Buff on Target ]-=-]")]
    public BuffSkill applyBuff;

    public int buffLevel;
    [Range(0, 1)] public float buffChance;

    public string successMessage = "You revived: ";
    public string failedMessage = "You failed to revive: ";

    public bool reverseTargeting;

    public bool affectOwnParty;
    public bool affectOwnGuild;
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

            targets = player.Tools_GetCorrectedTargetsInSphere(player.transform, range, true, affectOwnParty, affectOwnGuild, affectOwnRealm, reverseTargeting, affectPlayers, affectNpcs, affectMonsters, affectPets);

            foreach (Entity target in targets)
            {
                if (UnityEngine.Random.value <= successChance)
                {
                    target.health.current += healsHealth;
                    target.mana.current += healsMana;
                    target.Tools_ApplyBuff(applyBuff, buffLevel, buffChance);
                    target.Tools_OverrideState("IDLE");
                    player.Tools_TargetAddMessage(successMessage + target.name);
                }
                else
                {
                    player.Tools_TargetAddMessage(failedMessage + target.name);
                }
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
