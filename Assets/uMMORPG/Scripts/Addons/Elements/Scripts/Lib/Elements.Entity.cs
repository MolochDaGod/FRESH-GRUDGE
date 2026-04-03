using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ENTITY
public partial class Entity
{
    //[Header("[-=-=-[ ELEMENTAL RESISTANCES ]-=-=-]")]
    //public LevelBasedElement[] elementalResistances;
    
    protected Dictionary<string, ElementCache> _elementsCache = new Dictionary<string, ElementCache>();

    // -----------------------------------------------------------------------------------
    // CalculateElementalResistance
    // -----------------------------------------------------------------------------------
    public virtual float CalculateElementalResistance(ElementTemplate element, bool bCache = true)
    {
        float fResistance = 1.0f;                                       // 1.0f = 100% damage by default
        float fValue = 0f;
        ElementCache elementCache = null;

        // -- Check Caching
        if (bCache && _elementsCache.TryGetValue(element.name, out elementCache) && Time.time < elementCache.timer)
            return elementCache.value;

        // ------------------------------- Calculation -----------------------------------

        // -- Bonus: Base Resistance
        float fBase = fResistance;                                  // 1.0f = 100% damage by default

        foreach (LevelBasedElement ele in combat.elementalResistances)
        {
            if (ele.template == element)
                fBase += ele.Get(level.current);
        }

        // -- Bonus: Passive Skills
        float fPassiveBonus = (from skill in skills.skills
                               where skill.level > 0 && skill.data is PassiveSkill
                               select ((PassiveSkill)skill.data).GetResistance(element, skill.level)).Sum();

        // -- Bonus: Buffs
        float fBuffBonus = skills.buffs.Sum(buff => buff.GetResistance(element));

        fValue = fResistance - (fBase + fPassiveBonus + fBuffBonus);

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