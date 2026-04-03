using System;
using UnityEngine;

#if _iMMOATTRIBUTES
// EQUIPMENT ITEM
public partial class EquipmentItem
{
    [Header("[-=-[ TrugordAttribute Modifiers ]-=-]")]
    public Tools_AttributeModifier[] AttributeModifiers = { };

    [Header("[-=-[ Extra Stats ]-=-]")]
    public bool showExtraStat = false;
    [BoolShowConditional("showExtraStat", true)]
    public ExtraStats extraStats;
   
    /*
		Tooltip
		Tooltips on partial classes not possible by uMMORPG design at the moment, will
		require another core modification that sets a hook
	*/
}

[Serializable]
public partial struct ExtraStats
{
    [Tooltip("[Optional] Accuracy - increases chance to inflict a (negative) buff on a target (0.01=1%, 0.5=50%, 1=100%)")]
    [Range(-1f, 1f)] public float accuracy;

    [Tooltip("[Optional] Resistances - increases chance to resist a (negative) buff being inflicted on self (0.01=1%, 0.5=50%, 1=100%)")]
    [Range(-1f, 1f)] public float resistance;

    [Tooltip("[Optional] Block Factor - increases the power of blocking an attack (damage reduction) (0.01=1%, 0.5=50%, 1=100%)")]
    [Range(-1f, 1f)] public float blockFactor;

    [Tooltip("[Optional] Critical Factor - increases the damage of a critical hit (0.01=1%, 0.5=50%, 1=100%)")]
    [Range(-1f, 1f)] public float criticalFactor;

    [Tooltip("[Optional] Drain Health Factor - drains a percentage of health from the target with every hit (0.01=1%, 0.5=50%, 1=100%)")]
    [Range(-1f, 1f)] public float drainHealthFactor;

    [Tooltip("[Optional] Drain Mana Factor - drains a percentage of mana from the target with every hit (0.01=1%, 0.5=50%, 1=100%)")]
    [Range(-1f, 1f)] public float drainManaFactor;

    [Tooltip("[Optional] Reflect Damage Factor - reflects a percentage of damage dealt to self back onto the attacker (0.01=1%, 0.5=50%, 1=100%)")]
    [Range(-1f, 1f)] public float reflectDamageFactor;

    [Tooltip("[Optional] Defense Break Factor - reduces the targets defense (0.01=1%, 0.5=50%, 1=100%)")]
    [Range(-1f, 1f)] public float defenseBreakFactor;

    [Tooltip("[Optional] Block Break Factor - reduces the targets block chance (0.01=1%, 0.5=50%, 1=100%)")]
    [Range(-1f, 1f)] public float blockBreakFactor;

    [Tooltip("[Optional] Critical Evasion - reduces the chance of receiving a critical hit (0.01=1%, 0.5=50%, 1=100%)")]
    [Range(-1f, 1f)] public float criticalEvasion;

    [Tooltip("[Optional] Absorb Health Factor - regenerates a percentage of health on every received hit (0.01=1%, 0.5=50%, 1=100%)")]
    [Range(-1f, 1f)] public float absorbHealthFactor;

    [Tooltip("[Optional] Absorb Mana Factor - regenerates a percentage of health on every received hit (0.01=1%, 0.5=50%, 1=100%)")]
    [Range(-1f, 1f)] public float absorbManaFactor;

}
[Serializable]
public partial struct BaseExtraStats
{
    [Tooltip("[Optional] Accuracy - increases chance to inflict a (negative) buff on a target (0.01=1%, 0.5=50%, 1=100%)")]
    public LinearFloat accuracy;

    [Tooltip("[Optional] Resistances - increases chance to resist a (negative) buff being inflicted on self (0.01=1%, 0.5=50%, 1=100%)")]
    public LinearFloat resistance;

    [Tooltip("[Optional] Block Factor - increases the power of blocking an attack (damage reduction) (0.01=1%, 0.5=50%, 1=100%)")]
    public LinearFloat blockFactor;

    [Tooltip("[Optional] Critical Factor - increases the damage of a critical hit (0.01=1%, 0.5=50%, 1=100%)")]
    public LinearFloat criticalFactor;

    [Tooltip("[Optional] Drain Health Factor - drains a percentage of health from the target with every hit (0.01=1%, 0.5=50%, 1=100%)")]
    public LinearFloat drainHealthFactor;

    [Tooltip("[Optional] Drain Mana Factor - drains a percentage of mana from the target with every hit (0.01=1%, 0.5=50%, 1=100%)")]
    public LinearFloat drainManaFactor;

    [Tooltip("[Optional] Reflect Damage Factor - reflects a percentage of damage dealt to self back onto the attacker (0.01=1%, 0.5=50%, 1=100%)")]
    public LinearFloat reflectDamageFactor;

    [Tooltip("[Optional] Defense Break Factor - reduces the targets defense (0.01=1%, 0.5=50%, 1=100%)")]
    public LinearFloat defenseBreakFactor;

    [Tooltip("[Optional] Block Break Factor - reduces the targets block chance (0.01=1%, 0.5=50%, 1=100%)")]
    public LinearFloat blockBreakFactor;

    [Tooltip("[Optional] Critical Evasion - reduces the chance of receiving a critical hit (0.01=1%, 0.5=50%, 1=100%)")]
    public LinearFloat criticalEvasion;

    [Tooltip("[Optional] Absorb Health Factor - regenerates a percentage of health on every received hit (0.01=1%, 0.5=50%, 1=100%)")]
    public LinearFloat absorbHealthFactor;

    [Tooltip("[Optional] Absorb Mana Factor - regenerates a percentage of health on every received hit (0.01=1%, 0.5=50%, 1=100%)")]
    public LinearFloat absorbManaFactor;

}
#endif