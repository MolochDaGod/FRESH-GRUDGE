using System.Linq;
using UnityEngine;

// PLAYER
public partial class Player
{
    // -----------------------------------------------------------------------------------
    // GetAttackElement
    // -----------------------------------------------------------------------------------
    public ElementTemplate GetAttackElement()
    {
        if (Tools_GetWeapon() != null && Tools_GetWeapon().elementalAttack != null) return Tools_GetWeapon().elementalAttack;

        foreach (ItemSlot slot in equipment.slots)
        {
            if (slot.amount > 0 && ((EquipmentItem)slot.item.data).elementalAttack != null)
                return ((EquipmentItem)slot.item.data).elementalAttack;
        }

        return null;
    }

    // -----------------------------------------------------------------------------------
    // CalculateElementalResistance
    // -----------------------------------------------------------------------------------
    public override float CalculateElementalResistance(ElementTemplate element, bool bCache = true)
    {
        float fValue = 0f;
        ElementCache elementCache = null;

        // -- Check Caching
        if (_elementsCache != null) _elementsCache.TryGetValue(element.name, out elementCache);
        if (bCache && elementCache != null && Time.time < elementCache.timer)
            return elementCache.value;

        // ------------------------------- Calculation -----------------------------------
        float fResistance = 0f;
        float fEquipmentBonus = 0f;
        float fSetBonus = 0f;
        float fTraitBonus = 0f;

        // -- Bonus: Base Resistance
        fResistance += base.CalculateElementalResistance(element, false);

        // -- Bonus: Equipment
        foreach (ItemSlot slot in equipment.slots)
        {
            if (slot.amount > 0)
            {
#if _iMMOTRAITS && _iMMOELEMENTS
                // -- Bonus: Equipment
                fEquipmentBonus += ((EquipmentItem)slot.item.data).GetResistance(element);
#if _iMMOITEMLEVELUP
                if(slot.item.equipmentLevel > 0)
                    fEquipmentBonus += ((EquipmentItem)slot.item.data).GetResistance(element, (slot.item.equipmentLevel-1));
#endif
#endif
#if _iMMOTRAITS && _iMMOEQUIPMENTSETS && _iMMOELEMENTS

                // -- Equipment Bonus (Set Bonus Individual)
                fSetBonus += slot.item.setBonusElementIndividual(slot, equipment.slots, element);

                // -- Equipment Bonus (Set Bonus Partial)
                fSetBonus += slot.item.setBonusElementPartial(slot, equipment.slots, element);

                // -- Equipment Bonus (Set Bonus Complete)
                fSetBonus += slot.item.setBonusElementComplete(slot, equipment.slots, element);
#endif


            }
        }

        // -- Bonus: Traits
#if _iMMOTRAITS && _iMMOELEMENTS
        fTraitBonus += playerTraits.Traits.Sum(trait => trait.GetResistance(element));
#endif


        fValue += fResistance - (fEquipmentBonus + fTraitBonus + fSetBonus);

        // ----------------------------- Calculation End ---------------------------------

        // -- Update Caching
        if (bCache && elementCache != null)
        {
            elementCache.timer = Time.time + cacheTimerInterval;
            elementCache.value = fValue;
            _elementsCache[element.name] = elementCache;
        }
        else if (bCache)
        {
            elementCache = new ElementCache
            {
                timer = Time.time + cacheTimerInterval,
                value = fValue
            };
            _elementsCache.Add(element.name, elementCache);
        }

        return fValue;
    }

    // -----------------------------------------------------------------------------------
}