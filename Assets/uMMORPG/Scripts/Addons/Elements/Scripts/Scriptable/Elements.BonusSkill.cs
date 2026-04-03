using System.Linq;
using UnityEngine;

public abstract partial class BonusSkill : ScriptableSkill
{
    [Header("[-=-=-[ ELEMENTAL RESISTANCES ]-=-=-]")]
    public LevelBasedElement[] elementalResistances;

    // GetResistance

    public float GetResistance(ElementTemplate element, int level)
    {
        return elementalResistances.FirstOrDefault(x => x.template == element).Get(level);
    }
}