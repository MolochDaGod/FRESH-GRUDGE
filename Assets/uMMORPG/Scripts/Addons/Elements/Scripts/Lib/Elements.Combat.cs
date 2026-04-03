using UnityEngine;

public partial class Combat
{
    [Header("[-=-[ ELEMENTAL RESISTANCES ]-=-]")]
    public LevelBasedElement[] elementalResistances;

    // -----------------------------------------------------------------------------------
    // OnDealDamage_Tools_Elements
    // -----------------------------------------------------------------------------------
    private void OnDealDamage_Elements(Entity target, ElementTemplate element, int damageDealt, MutableWrapper<int> damageBonus)
    {
        if (target == null || element == null || damageDealt <= 0) return;
        damageBonus.Value = (int)(damageDealt * target.CalculateElementalResistance(element));
    }
}
