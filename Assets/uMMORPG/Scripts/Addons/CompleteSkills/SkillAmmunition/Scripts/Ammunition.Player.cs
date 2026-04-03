using System.Linq;

// PLAYER
public partial class Player
{
    // -----------------------------------------------------------------------------------
    // Ammunition_HasEquipmentOfCategory
    // -----------------------------------------------------------------------------------
    public bool Ammunition_HasEquipmentOfCategory(string categoryName)
    {
        return equipment.slots.FindIndex(slot => slot.amount > 0 &&
            ((EquipmentItem)slot.item.data).category.StartsWith(categoryName)
        ) != -1;
    }

    // -----------------------------------------------------------------------------------
    // Ammunition_checkHasSkill
    // -----------------------------------------------------------------------------------
    public bool Ammunition_checkHasSkill(ScriptableSkill skill, int level)
    {
        if (skill == null || level <= 0) return true;
        return skills.skills.Any(s => s.name == skill.name && s.level >= level);
    }

    // -----------------------------------------------------------------------------------
    // Ammunition_checkHasEquipment
    // -----------------------------------------------------------------------------------
    public bool Ammunition_checkHasEquipment(ScriptableItem item)
    {
        if (item == null) return true;

        foreach (ItemSlot slot in equipment.slots)
            if (slot.amount > 0 && slot.item.data == item) return true;

        return false;
    }
}
