using UnityEngine;

public abstract partial class UsableItem : ScriptableItem
{
    [Header("[-=-[ Usage Requirements ]-=-]")]
    public Tools_Requirements usageRequirements;

    [Tooltip("Can only be used while the player is inside a usage area of the same ID (0 = disabled)")]
    public int usageAreaId;

    // -----------------------------------------------------------------------------------
    // Item_CanUse
    // -----------------------------------------------------------------------------------
    public virtual bool Item_CanUse(Player player)
    {
        return (usageAreaId <= 0 || player.usageRequirementsAreaId == usageAreaId) && usageRequirements.checkRequirements(player);
    }

    public virtual bool Item_CanUse(Player player, Item item)
    {
        bool isValid = (usageAreaId <= 0 || player.usageRequirementsAreaId == usageAreaId);
#if _iMMOITEMLEVELUP
        if (isValid)
        {
            if (item.equipmentLevel > 0)
            {
                EquipmentItem info = (EquipmentItem)item.data;
                if(info.enableLevelUp)
                    isValid = info.LevelUpParameters[item.equipmentLevel - 1].usageRequirements.checkRequirements(player);
            }
        }
#endif
        return isValid;
        
    }
    // -----------------------------------------------------------------------------------
}
