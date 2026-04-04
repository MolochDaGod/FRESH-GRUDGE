using System.Text;
using UnityEngine;
using UnityEngine.UI;

// ─────────────────────────────────────────────────────────────────────────────
// UI_GrudgeCharacterTab
//
// Character summary tab for the Grudge hub.
// Shows class path, level, stat bonuses from the class tree, weapon
// proficiencies, and chosen milestone nodes.
//
// PREFAB SETUP:
//   characterNameText    — player name
//   classNameText        — player.className (set in PlayerCharacterCreation etc.)
//   levelText            — current level
//   healthText / manaText / damageText / defenseText — derived stats
//   treeSummaryText      — scrollable block listing chosen tree nodes + bonuses
//   proficiencyContent   — Transform for GrudgeProficiencyBadge prefabs
//   proficiencyBadgePrefab
//   openClassTreeButton  — shortcut to jump to ClassTree tab
// ─────────────────────────────────────────────────────────────────────────────
public class UI_GrudgeCharacterTab : MonoBehaviour
{
    [Header("Identity")]
    public Text characterNameText;
    public Text classNameText;
    public Text levelText;

    [Header("Stats")]
    public Text healthText;
    public Text manaText;
    public Text damageText;
    public Text defenseText;
    public Text skillXpText;

    [Header("Class Tree Summary")]
    public Text      treeSummaryText;           // Scrollable multi-line

    [Header("Weapon Proficiencies")]
    public Transform             proficiencyContent;
    public GrudgeProficiencyBadge proficiencyBadgePrefab;

    [Header("Actions")]
    public Button openClassTreeButton;          // Navigates to class tree tab
    public UI_GrudgeMenu grudgeMenu;

    void Awake()
    {
        if (openClassTreeButton != null && grudgeMenu != null)
            openClassTreeButton.onClick.AddListener(
                () => grudgeMenu.SelectTab(UI_GrudgeMenu.GrudgeTab.ClassTree));
    }

    // ── Called by UI_GrudgeMenu each frame while this tab is active ───────────
    public void Refresh(Player player)
    {
        if (player == null) return;

        // Identity
        if (characterNameText != null) characterNameText.text = player.name;
        if (levelText         != null) levelText.text         = $"Level {player.level.current}";

        // Class name — pulled from the class component or entity name
        if (classNameText != null)
            classNameText.text = player.className ?? player.gameObject.name;

        // Base stats
        if (healthText  != null) healthText.text  = $"HP: {player.health.current} / {player.health.max}";
        if (manaText    != null) manaText.text    = $"MP: {player.mana.current} / {player.mana.max}";
        if (damageText  != null) damageText.text  = $"Damage: {player.combat.damage}";
        if (defenseText != null) defenseText.text = $"Defense: {player.combat.defense}";

        PlayerSkills skills = player.skills as PlayerSkills;
        if (skillXpText != null && skills != null)
            skillXpText.text = $"Skill XP: {skills.skillExperience}";

#if _IMMOCLASSTREE
        RefreshClassTree(player);
#else
        if (treeSummaryText != null) treeSummaryText.text = "Class tree not enabled.";
#endif
    }

#if _IMMOCLASSTREE
    void RefreshClassTree(Player player)
    {
        PlayerClassTree tree = player.classTree;
        if (tree == null)
        {
            if (treeSummaryText != null) treeSummaryText.text = "No class tree data.";
            return;
        }

        // ── Tree text summary ──────────────────────────────────────────────
        var sb = new StringBuilder();
        int tier = tree.GetCurrentAvailableTier();
        sb.AppendLine($"<b>Available Tier:</b> {tier + 1} (requires Lv. {ClassTreeNodeTemplate.MilestoneLevels[tier]})");
        sb.AppendLine();
        sb.AppendLine("<b>Chosen Paths:</b>");

        bool anyChosen = false;
        for (int t = 0; t <= tier; t++)
        {
            ClassTreeNodeTemplate chosen = tree.GetChosenNodeForTier(t);
            if (chosen != null)
            {
                anyChosen = true;
                sb.AppendLine($"  Tier {t + 1}: <color=#ffcc44>{chosen.nodeName}</color>");
                if (!string.IsNullOrEmpty(chosen.unlocksSkillCategory))
                    sb.AppendLine($"    → Skill tree: {chosen.unlocksSkillCategory}");
            }
            else
            {
                sb.AppendLine($"  Tier {t + 1}: <color=#888888>(not yet chosen)</color>");
            }
        }

        if (!anyChosen) sb.AppendLine("  <color=#888888>No paths chosen yet.</color>");

        // Stat bonuses from tree
        sb.AppendLine();
        sb.AppendLine("<b>Tree Bonuses:</b>");
        int dmg = tree.GetDamageBonus();
        int def = tree.GetDefenseBonus();
        int hp  = tree.GetHealthBonus(player.level.current);
        int mp  = tree.GetManaBonus(player.level.current);
        if (dmg != 0) sb.AppendLine($"  <color=#ff8888>+{dmg} Damage</color>");
        if (def != 0) sb.AppendLine($"  <color=#8888ff>+{def} Defense</color>");
        if (hp  != 0) sb.AppendLine($"  <color=#88ff88>+{hp} HP</color>");
        if (mp  != 0) sb.AppendLine($"  <color=#88aaff>+{mp} MP</color>");
        if (dmg == 0 && def == 0 && hp == 0 && mp == 0)
            sb.AppendLine("  <color=#888888>No bonuses yet.</color>");

        if (treeSummaryText != null) treeSummaryText.text = sb.ToString();

        // ── Weapon proficiency badges ──────────────────────────────────────
        if (proficiencyContent == null || proficiencyBadgePrefab == null) return;

        // Gather all proficiencies from chosen nodes
        var profMap = new System.Collections.Generic.Dictionary<string, float>();
        foreach (int hash in tree.chosenNodes)
        {
            if (!ClassTreeNodeTemplate.All.TryGetValue(hash, out var node)) continue;
            foreach (var prof in node.weaponProficiencies)
            {
                if (!profMap.ContainsKey(prof.weaponCategory)) profMap[prof.weaponCategory] = 0f;
                profMap[prof.weaponCategory] += prof.damageMultiplierBonus;
            }
        }

        UIUtils.BalancePrefabs(proficiencyBadgePrefab.gameObject, profMap.Count, proficiencyContent);
        int idx = 0;
        foreach (var kvp in profMap)
        {
            proficiencyContent.GetChild(idx)
                .GetComponent<GrudgeProficiencyBadge>()
                .Refresh(kvp.Key, kvp.Value);
            idx++;
        }
    }
#endif
}

// ─────────────────────────────────────────────────────────────────────────────
// GrudgeProficiencyBadge — small badge showing one weapon type proficiency
// ─────────────────────────────────────────────────────────────────────────────
public class GrudgeProficiencyBadge : MonoBehaviour
{
    public Text weaponTypeText;
    public Text bonusText;
    public Image icon;   // optional weapon-type icon

    public void Refresh(string weaponType, float bonus)
    {
        if (weaponTypeText != null) weaponTypeText.text = weaponType;
        if (bonusText      != null) bonusText.text      = $"+{Mathf.RoundToInt(bonus * 100)}% dmg";
    }
}
