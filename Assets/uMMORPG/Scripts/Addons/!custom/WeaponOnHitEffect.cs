using System;
using UnityEngine;

// Defines a single on-hit proc that a weapon can apply to its target.
// Attach one or more of these to WeaponItem.onHitEffects.
// Use BuffSkill ScriptableObjects with NEGATIVE values for debuffs:
//   Poison      -> healthPercentPerSecondBonus = -0.02 (2% max HP per second)
//   Slow        -> speedBonus = -2.0 (units per second reduction)
//   Dmg Reduce  -> damageBonus = -10 (flat damage stat reduction)
[Serializable]
public struct WeaponOnHitEffect
{
    [Tooltip("BuffSkill to apply to the target on a successful hit. Use negative BonusSkill values for debuffs.")]
    public BuffSkill buff;

    [Tooltip("Level of the buff/debuff applied (must be <= buff.maxLevel).")]
    public int buffLevel;

    [Range(0f, 1f)]
    [Tooltip("Probability of the proc triggering each hit (0 = never, 1 = always).")]
    public float procChance;
}
