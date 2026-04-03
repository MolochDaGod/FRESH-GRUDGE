using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Individual node button inside the Class Tree panel.
///
/// PREFAB SETUP (create a UI button prefab):
///  icon             — Image for the node icon
///  nameText         — Text: node name
///  descriptionText  — Text: multi-line description + bonuses
///  button           — Button component (set onClick in Setup)
///  lockedOverlay    — GameObject: dimmed overlay when node is locked
///  chosenBorder     — GameObject: highlight border when this node is chosen
///  availableBorder  — GameObject: glow border when node is available to choose
///  tierLevelText    — Text: "Lv. 5" label
/// </summary>
public class UI_ClassTreeNodeButton : MonoBehaviour
{
    [Header("UI References")]
    public Image  icon;
    public Text   nameText;
    public Text   descriptionText;
    public Button button;

    [Header("State Visuals")]
    public GameObject lockedOverlay;    // shown when prerequisites not met or level too low
    public GameObject chosenBorder;     // shown when this node is the chosen one for its tier
    public GameObject availableBorder;  // shown when the player can pick this node now
    public GameObject unavailableDim;   // shown when tier is available but another node was picked

    [Header("Labels")]
    public Text tierLevelText;          // "Lv. 5"

    // ── Setup ─────────────────────────────────────────────────────────────

    public void Setup(ClassTreeNodeTemplate node, PlayerClassTree classTree,
                      bool tierLocked, bool tierAlreadyChosen)
    {
        bool chosen   = classTree.HasChosenNode(node);
        bool canPick  = !tierLocked && !tierAlreadyChosen && classTree.CanChooseNode(node);

        // Icon & name
        if (icon != null)
        {
            icon.sprite = node.icon;
            icon.enabled = node.icon != null;
        }
        if (nameText != null) nameText.text = node.nodeName;
        if (tierLevelText != null) tierLevelText.text = "Lv. " + node.GetRequiredLevel();

        // Build tooltip description
        if (descriptionText != null)
            descriptionText.text = BuildDescription(node);

        // State visuals
        if (chosenBorder    != null) chosenBorder.SetActive(chosen);
        if (availableBorder != null) availableBorder.SetActive(!chosen && canPick);
        if (unavailableDim  != null) unavailableDim.SetActive(!chosen && tierAlreadyChosen);
        if (lockedOverlay   != null) lockedOverlay.SetActive(tierLocked || (!chosen && !canPick && !tierAlreadyChosen));

        // Button
        if (button != null)
        {
            button.interactable = canPick;
            button.onClick.RemoveAllListeners();
            if (canPick)
            {
                int hash = node.name.GetStableHashCode();
                button.onClick.AddListener(() => classTree.CmdChooseNode(hash));
            }
        }
    }

    static string BuildDescription(ClassTreeNodeTemplate node)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(node.description);

        // Prerequisites
        if (node.prerequisites != null && node.prerequisites.Length > 0)
        {
            sb.AppendLine();
            sb.Append("<color=#aaaaaa>Requires: ");
            sb.Append(string.Join(", ", System.Array.ConvertAll(node.prerequisites, p => p.nodeName)));
            sb.AppendLine("</color>");
        }

        // Stat bonuses
        bool hasBonuses = node.damageBonus != 0 || node.defenseBonus != 0 ||
                          node.healthBonus != 0  || node.manaBonus    != 0;
        if (hasBonuses)
        {
            sb.AppendLine();
            if (node.damageBonus  != 0) sb.AppendLine($"<color=#ff8888>+{node.damageBonus} Damage</color>");
            if (node.defenseBonus != 0) sb.AppendLine($"<color=#8888ff>+{node.defenseBonus} Defense</color>");
            if (node.healthBonus  != 0) sb.AppendLine($"<color=#88ff88>+{node.healthBonus} HP</color>");
            if (node.manaBonus    != 0) sb.AppendLine($"<color=#88aaff>+{node.manaBonus} MP</color>");
        }

        // Weapon proficiencies
        if (node.weaponProficiencies != null && node.weaponProficiencies.Length > 0)
        {
            sb.AppendLine();
            sb.AppendLine("<b>Weapon Proficiency:</b>");
            foreach (var prof in node.weaponProficiencies)
            {
                string line = $"  <color=#ffcc44>{prof.weaponCategory}</color>: " +
                              $"+{Mathf.RoundToInt(prof.damageMultiplierBonus * 100)}% dmg";
                if (prof.flatDamageBonus != 0) line += $" +{prof.flatDamageBonus} flat";
                sb.AppendLine(line);
            }
        }

        // Unlocked skill category
        if (!string.IsNullOrEmpty(node.unlocksSkillCategory))
        {
            sb.AppendLine();
            sb.AppendLine($"<b>Unlocks Skill Tree:</b> <color=#ffdd88>{node.unlocksSkillCategory}</color>");
        }

        // Granted skills
        if (node.grantedSkills != null && node.grantedSkills.Length > 0)
        {
            sb.AppendLine();
            sb.AppendLine("<b>Grants Skills:</b>");
            foreach (var s in node.grantedSkills)
                if (s != null) sb.AppendLine($"  • {s.name}");
        }

        return sb.ToString().TrimEnd();
    }
}
