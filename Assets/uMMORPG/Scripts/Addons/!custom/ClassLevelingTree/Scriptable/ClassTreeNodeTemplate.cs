using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject defining one node in the Class Leveling Tree.
/// Create via: Assets → Create → GRUDGE / ClassTree / Node
///
/// TIER / MILESTONE MAPPING:
///   Tier 0 → Level  1  (Initial class path selection)
///   Tier 1 → Level  5  (1st class upgrade)
///   Tier 2 → Level 10  (2nd class upgrade)
///   Tier 3 → Level 15  (3rd class upgrade)
///   Tier 4 → Level 20  (4th / final class upgrade)
///
/// NAMING: Each node's asset filename is used as its unique key.
///   Example assets: "Warrior_Tier0", "Berserker_Tier1", "BloodKnight_Tier2"
/// </summary>
[CreateAssetMenu(fileName = "New Class Tree Node", menuName = "GRUDGE/ClassTree/Node")]
public class ClassTreeNodeTemplate : ScriptableObject
{
    // ── Identity ──────────────────────────────────────────────────────────
    [Header("Identity")]
    public string  nodeName;
    [TextArea(2, 4)]
    public string  description;
    public Sprite  icon;

    // ── Tree Position ─────────────────────────────────────────────────────
    [Header("Tree Position")]
    [Tooltip("0=Lv1 (Start), 1=Lv5, 2=Lv10, 3=Lv15, 4=Lv20")]
    [Range(0, 4)]
    public int tier;

    [Tooltip("All of these nodes must be chosen before this node becomes available. Leave empty for no requirement.")]
    public ClassTreeNodeTemplate[] prerequisites;

    // ── What this node unlocks ────────────────────────────────────────────
    [Header("Unlocks")]
    [Tooltip("Skill category (from SkillCategories addon) that this path unlocks. Leave empty = no category restriction.")]
    public string unlocksSkillCategory;

    [Tooltip("Weapon type proficiencies granted by choosing this node.")]
    public WeaponProficiency[] weaponProficiencies;

    [Tooltip("These skills are automatically set to level 1 when this node is chosen.")]
    public ScriptableSkill[] grantedSkills;

    // ── Stat Bonuses (passive, always active once chosen) ─────────────────
    [Header("Stat Bonuses")]
    public int damageBonus;
    public int defenseBonus;
    public int healthBonus;
    public int manaBonus;

    // ── Milestone Level Lookup ────────────────────────────────────────────
    /// <summary>Maps tier index → required character level.</summary>
    public static readonly int[] MilestoneLevels = { 1, 5, 10, 15, 20 };

    public int GetRequiredLevel() =>
        (tier >= 0 && tier < MilestoneLevels.Length) ? MilestoneLevels[tier] : 999;

    // ── Static Cache (loaded from all Resources/ folders) ─────────────────
    static Dictionary<int, ClassTreeNodeTemplate> _cache;

    public static Dictionary<int, ClassTreeNodeTemplate> All
    {
        get
        {
            if (_cache == null)
            {
                _cache = new Dictionary<int, ClassTreeNodeTemplate>();
                foreach (var t in Resources.LoadAll<ClassTreeNodeTemplate>(""))
                {
                    int hash = t.name.GetStableHashCode();
                    if (!_cache.ContainsKey(hash))
                        _cache[hash] = t;
                    else
                        Debug.LogError($"[ClassTree] Duplicate name hash for ClassTreeNodeTemplate '{t.name}'. Rename one of them.");
                }
            }
            return _cache;
        }
    }

    /// <summary>Call this if you change tree node assets at runtime (editor tooling).</summary>
    public static void ClearCache() => _cache = null;
}

/// <summary>Defines proficiency with a specific weapon category.</summary>
[Serializable]
public struct WeaponProficiency
{
    [Tooltip("Must match ScriptableSkill.requiredWeaponCategory / GetEquippedWeaponCategory() prefix.\n" +
             "Examples: 'Sword', 'Bow', 'Staff', 'Mace', 'Gun', 'Crossbow', 'Dagger'")]
    public string weaponCategory;

    [Range(0f, 1f)]
    [Tooltip("Additive damage multiplier when this weapon type is equipped. 0.15 = +15% damage.")]
    public float damageMultiplierBonus;

    [Tooltip("Flat damage bonus when this weapon type is equipped (added to GetDamageBonus).")]
    public int flatDamageBonus;
}
