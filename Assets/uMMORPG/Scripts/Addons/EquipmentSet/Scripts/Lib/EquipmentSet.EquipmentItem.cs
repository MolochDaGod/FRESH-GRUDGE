using UnityEngine;

// EQUIPMENT ITEM TEMPLATE
public partial class EquipmentItem
{
    [Header("[-=-[ Equipment Set Bonus ]-=-]")]
    [Tooltip("Put any EquipmentSet here to make this item part of a larger set.")]
    public EquipmentSetTemplate equipmentSet;

    [Header("[-=-[ Individual Set Bonus ]-=-]")]
    [Tooltip("All items must be equipped in order for this individual set bonus to be effective.")]
    public EquipmentItem[] setItems;

    [Tooltip("This is the individual set bonus granted.")]
    public Tools_StatModifier individualStatModifiers;

    // -----------------------------------------------------------------------------------
    // HasIndividualSetBonus
    // -----------------------------------------------------------------------------------
    public bool HasIndividualSetBonus
    {
        get
        {
            return individualStatModifiers != null && individualStatModifiers.hasModifier;
        }
    }
}
