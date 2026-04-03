using System.Linq;
using UnityEngine;

// HEALING SERVICES
[System.Serializable]
public partial class HealingServices
{
    [Tooltip("One click deactivation")]
    public bool offersHealing;

    [Tooltip("Fixed gold Cost to heal or base cost for healing")]
    public float goldCost;

    [Tooltip("When checked, the cost above is the cost per point of healing")]
    public bool scaleCost;

    [Tooltip("Does it fully recover health?")]
    public bool healHealth;

    [Tooltip("Does it fully recover mana?")]
    public bool healMana;

    [Tooltip("Does it remove buffs (except Offender/Murderer etc.)?")]
    public bool removeBuffs;

    [Tooltip("Does it remove nerfs (buffs set to disadvantageous) (except Offender/Murderer etc.)?")]
    public bool removeNerfs;

#if _iMMOCURSEDEQUIPMENT

    [Tooltip("Does it force-unequip all cursed items up to this curse level from the player?")]
    [Range(0, 9)] public int unequipMaxCursedLevel;

#endif

    // -----------------------------------------------------------------------------------
    // Valid
    // -----------------------------------------------------------------------------------
    public bool Valid(Player player)
    {
        if (!offersHealing) return false;

        bool valid = (player != null && player.isAlive && getCost(player) <= player.gold);

        if (valid)
        {
            valid = false;

            if (healHealth)
                valid = (player.health.current < player.health.max) ? true : valid;

            if (healMana)
                valid = (player.mana.current < player.mana.max) ? true : valid;

            if (removeBuffs)
                valid = player.skills.buffs.Any(x => !x.data.disadvantageous) ? true : valid;

            if (removeNerfs)
                valid = player.skills.buffs.Any(x => x.data.disadvantageous) ? true : valid;

#if _iMMOCURSEDEQUIPMENT
            if (unequipMaxCursedLevel > 0)
                valid = player.equipment.slots.Any(x => x.amount > 0 && ((EquipmentItem)x.item.data).cursedLevel > 0) ? true : valid;
#endif
        }

        return valid;
    }

    // -----------------------------------------------------------------------------------
    // getCost
    // -----------------------------------------------------------------------------------
    public int getCost(Player player)
    {
        if (!scaleCost) return (int)goldCost;

        int price = 0;

        if (healHealth && scaleCost)
            price += (int)((player.health.max - player.health.current) * goldCost);

        if (healMana && scaleCost)
            price += (int)((player.mana.max - player.mana.max) * goldCost);

        if (removeBuffs && scaleCost)
            price += (int)(player.Tools_getBuffCount() * goldCost);

        if (removeNerfs && scaleCost)
            price += (int)(player.Tools_getNerfCount() * goldCost);

        return price;
    }
}