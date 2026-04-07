using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

/// <summary>
/// Attach to every Player prefab alongside Player, PlayerSkills, PlayerEquipment.
///
/// Implements ICombatBonus so damage/defense bonuses from chosen nodes are
/// automatically picked up by Combat.damage / Combat.defense via GetComponents.
///
/// Implements IHealthBonus / IManaBonus so HP/MP bonuses work the same way.
/// </summary>
[DefaultExecutionOrder(-280)]
[DisallowMultipleComponent]
public partial class PlayerClassTree : NetworkBehaviour
{
    // ── Inspector References ──────────────────────────────────────────────
    [Header("Components (assign in Inspector)")]
    public Player          player;
    public PlayerEquipment equipment;
    public PlayerSkills    skills;

    // ── Network State ─────────────────────────────────────────────────────
    /// <summary>Hashes of all chosen ClassTreeNodeTemplate names.</summary>
    public readonly SyncList<int> chosenNodes = new SyncList<int>();

    // ── Client-side notification tracking ────────────────────────────────
    int _lastNotifiedTier = -1;

    // ══════════════════════════════════════════════════════════════════════
    //  Public Query Helpers
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Has the player chosen this specific node?</summary>
    public bool HasChosenNode(ClassTreeNodeTemplate node) =>
        node != null && chosenNodes.Contains(node.name.GetStableHashCode());

    /// <summary>Has the player made ANY choice at this tier?</summary>
    public bool HasChosenTier(int tier) =>
        chosenNodes.Any(h => ClassTreeNodeTemplate.All.TryGetValue(h, out var n) && n.tier == tier);

    /// <summary>Which node did the player choose for this tier? (null if none yet)</summary>
    public ClassTreeNodeTemplate GetChosenNodeForTier(int tier) =>
        chosenNodes
            .Where(h => ClassTreeNodeTemplate.All.TryGetValue(h, out var n) && n.tier == tier)
            .Select(h => ClassTreeNodeTemplate.All[h])
            .FirstOrDefault();

    /// <summary>Highest tier the player's level unlocks (0-4).</summary>
    public int GetCurrentAvailableTier()
    {
        int[] ml = ClassTreeNodeTemplate.MilestoneLevels;
        for (int i = ml.Length - 1; i >= 0; i--)
            if (player.level.current >= ml[i]) return i;
        return 0;
    }

    /// <summary>Can the player choose this node right now?</summary>
    public bool CanChooseNode(ClassTreeNodeTemplate node)
    {
        if (node == null) return false;
        // Already chose something at this tier
        if (HasChosenTier(node.tier)) return false;
        // Level gate
        if (player.level.current < node.GetRequiredLevel()) return false;
        // All prerequisites must be chosen
        foreach (var prereq in node.prerequisites)
            if (!HasChosenNode(prereq)) return false;
        return true;
    }

    /// <summary>
    /// Does the player have access to this skill category via their tree choices?
    /// Returns true if: no node in the entire tree unlocks that category (open access),
    /// OR the player has chosen a node that unlocks it.
    /// </summary>
    public bool CanAccessSkillCategory(string category)
    {
        if (string.IsNullOrEmpty(category)) return true;

        bool anyNodeUnlocksIt = ClassTreeNodeTemplate.All.Values
            .Any(n => n.unlocksSkillCategory == category);

        if (!anyNodeUnlocksIt) return true; // no one gates this category

        // Player must have a chosen node that unlocks it
        return chosenNodes.Any(h =>
            ClassTreeNodeTemplate.All.TryGetValue(h, out var node) &&
            node.unlocksSkillCategory == category);
    }

    /// <summary>Sum of weapon proficiency damage multipliers for the given category prefix.</summary>
    public float GetWeaponProficiencyBonus(string weaponCategory)
    {
        if (string.IsNullOrEmpty(weaponCategory)) return 0f;
        float total = 0f;
        foreach (int h in chosenNodes)
        {
            if (!ClassTreeNodeTemplate.All.TryGetValue(h, out var node)) continue;
            foreach (var prof in node.weaponProficiencies)
                if (weaponCategory.StartsWith(prof.weaponCategory))
                    total += prof.damageMultiplierBonus;
        }
        return total;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Commands (client → server)
    // ══════════════════════════════════════════════════════════════════════

    [Command]
    public void CmdChooseNode(int nodeHash)
    {
        if (!ClassTreeNodeTemplate.All.TryGetValue(nodeHash, out var node)) return;
        if (!CanChooseNode(node)) return;

        chosenNodes.Add(nodeHash);

        // Auto-learn granted skills
        if (skills != null)
        {
            foreach (var scriptableSkill in node.grantedSkills)
            {
                int idx = skills.skills.FindIndex(s => s.name == scriptableSkill.name);
                if (idx != -1)
                {
                    Skill s = skills.skills[idx];
                    if (s.level == 0) { s.level = 1; skills.skills[idx] = s; }
                }
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  ICombatBonus
    // ══════════════════════════════════════════════════════════════════════

    public int GetDamageBonus()
    {
        int bonus = 0;
        string weaponCat = equipment != null ? equipment.GetEquippedWeaponCategory() : "";

        foreach (int h in chosenNodes)
        {
            if (!ClassTreeNodeTemplate.All.TryGetValue(h, out var node)) continue;

            bonus += node.damageBonus;

            // Weapon proficiency flat bonus
            if (!string.IsNullOrEmpty(weaponCat))
                foreach (var prof in node.weaponProficiencies)
                    if (weaponCat.StartsWith(prof.weaponCategory))
                        bonus += prof.flatDamageBonus;
        }
        return bonus;
    }

    public int GetDefenseBonus()
    {
        int bonus = 0;
        foreach (int h in chosenNodes)
            if (ClassTreeNodeTemplate.All.TryGetValue(h, out var node))
                bonus += node.defenseBonus;
        return bonus;
    }

    public float GetCriticalChanceBonus() => 0f;
    public float GetBlockChanceBonus()    => 0f;

    // ══════════════════════════════════════════════════════════════════════
    //  IHealthBonus / IManaBonus
    // ══════════════════════════════════════════════════════════════════════

    public int GetHealthBonus(int level)
    {
        int bonus = 0;
        foreach (int h in chosenNodes)
            if (ClassTreeNodeTemplate.All.TryGetValue(h, out var node))
                bonus += node.healthBonus;
        return bonus;
    }

    public int GetHealthRecoveryBonus() => 0;

    public int GetManaBonus(int level)
    {
        int bonus = 0;
        foreach (int h in chosenNodes)
            if (ClassTreeNodeTemplate.All.TryGetValue(h, out var node))
                bonus += node.manaBonus;
        return bonus;
    }

    public int GetManaRecoveryBonus() => 0;

    // ══════════════════════════════════════════════════════════════════════
    //  Milestone notification (local player only)
    // ══════════════════════════════════════════════════════════════════════

    void Update()
    {
        if (!isLocalPlayer || player == null) return;

        int available = GetCurrentAvailableTier();

        // Open the tree panel when a new tier is available and not yet chosen
        if (available > _lastNotifiedTier && !HasChosenTier(available))
        {
            _lastNotifiedTier = available;
            UI_ClassTreePanel panel = FindObjectOfType<UI_ClassTreePanel>();
            if (panel != null) panel.OpenForTier(available);
        }
    }
}
