using UnityEngine;

// EQUIPMENT SET TEMPLATE
[CreateAssetMenu(fileName = "Equipment Set", menuName = "ADDON/Templates/Equipment Set template", order = 999)]
public class EquipmentSetTemplate : ScriptableObject
{
    [Header("[-=-[ EquipmentSet ]-=-]")]
    [Tooltip("A number of set items must be equipped for the partial bonus to be active. All set items must be equipped in order for the complete set bonus to be effective.")]
    public EquipmentItem[] setItems;

    [Header("[-=-[ Partial Set Bonus ]-=-]"), Tooltip("This number of set items must be equipped for the partial bonus to be active.")]
    [Range(0, 99)] public int partialSetItemsCount;

    public Tools_StatModifier partialStatModifiers;
    public Tools_StatModifier completeStatModifiers;

    // -----------------------------------------------------------------------------------
    // HasPartialSetBonus
    // -----------------------------------------------------------------------------------
    public bool HasPartialSetBonus
    {
        get
        {
            return partialStatModifiers.hasModifier;
        }
    }

    // -----------------------------------------------------------------------------------
    // HasCompleteSetBonus
    // -----------------------------------------------------------------------------------
    public bool HasCompleteSetBonus
    {
        get
        {
            return completeStatModifiers.hasModifier;
        }
    }

    // -----------------------------------------------------------------------------------
}