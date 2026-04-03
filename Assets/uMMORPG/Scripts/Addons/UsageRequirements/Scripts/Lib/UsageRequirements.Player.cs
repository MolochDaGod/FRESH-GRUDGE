using UnityEngine;

// PLAYER
public partial class Player : Entity
{
    [HideInInspector] public int usageRequirementsAreaId;

    // -----------------------------------------------------------------------------------
    // UsageRequirementsAreaEnter
    // -----------------------------------------------------------------------------------
    public void UsageRequirementsAreaEnter(int id)
    {
        if (id <= 0) return;
        usageRequirementsAreaId = id;
    }

    // -----------------------------------------------------------------------------------
    // UsageRequirementsAreaExit
    // -----------------------------------------------------------------------------------
    public void UsageRequirementsAreaExit()
    {
        usageRequirementsAreaId = 0;
    }

    // -----------------------------------------------------------------------------------
    // UsageRequirementsGetEquipmentId
    // -----------------------------------------------------------------------------------
    public bool UsageRequirementsGetEquipmentId(int id)
    {
        return equipment.slots.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).usageEquipmentId == id) != -1;
    }
    // -----------------------------------------------------------------------------------
}
