using System.Linq;
using UnityEngine;

// EQUIPMENT ITEM
public partial class EquipmentItem
{
    [Header("[-=-[ ELEMENTAL ATTACK ]-=-]")]
    public ElementTemplate elementalAttack;

    [Header("[-=-[ ELEMENTAL RESISTANCES ]-=-]")]
    public ElementModifier[] elementalResistances;

    public float GetResistance(ElementTemplate element)
    {
        if (elementalResistances.Any(x => x.template == element))
            return elementalResistances.FirstOrDefault(x => x.template == element).value;
        else
            return 0;
    }
#if _iMMOITEMLEVELUP
    public float GetResistance(ElementTemplate element, int equipmentLevel)
    {
        if (LevelUpParameters[equipmentLevel].equipmentLevelUpModifier.elementsResistenseBonus.Any(x => x.template == element))
            return LevelUpParameters[equipmentLevel].equipmentLevelUpModifier.elementsResistenseBonus.FirstOrDefault(x => x.template == element).value;
        else
            return 0;
    }
#endif
    // -----------------------------------------------------------------------------------
}