using UnityEngine;
using UnityEngine.UI;

// ─────────────────────────────────────────────────────────────────────────────
// UI_GrudgeProfessionsTab
//
// Professions tab for the Grudge hub. Shows XP bars and tier info for all 5
// Grudge harvesting professions from PlayerGrudgeProfessions.
//
// PREFAB SETUP:
//   professionRows[] — exactly 5 GrudgeProfessionRow components in order:
//                      0 Harvesting, 1 Mining, 2 Woodcutting, 3 Fishing, 4 Crafting
//   totalProgressText — optional Text showing overall progression summary
// ─────────────────────────────────────────────────────────────────────────────
public class UI_GrudgeProfessionsTab : MonoBehaviour
{
    [Header("Profession Rows (0=Harvesting 1=Mining 2=Woodcutting 3=Fishing 4=Crafting)")]
    public GrudgeProfessionRow[] professionRows;   // length = 5

    [Header("Summary")]
    public Text totalProgressText;

    // ── Called by UI_GrudgeMenu each frame while this tab is active ───────────
    public void Refresh(Player player)
    {
        PlayerGrudgeProfessions profs = player != null
            ? player.GetComponent<PlayerGrudgeProfessions>()
            : null;

        int maxed = 0;
        for (int i = 0; i < PlayerGrudgeProfessions.ProfessionCount; i++)
        {
            if (professionRows == null || i >= professionRows.Length) break;
            if (professionRows[i] == null) continue;

            if (profs != null)
            {
                int  tier     = profs.GetTier(i);
                long xp       = profs.GetXp(i);
                long needed   = profs.XpToNextTier(i);
                float progress = profs.TierProgress(i);

                professionRows[i].Refresh(
                    PlayerGrudgeProfessions.ProfessionNames[i],
                    tier,
                    xp,
                    needed,
                    progress);

                if (tier >= PlayerGrudgeProfessions.MaxTier) maxed++;
            }
            else
            {
                // No component yet — show default state
                professionRows[i].Refresh(PlayerGrudgeProfessions.ProfessionNames[i], 1, 0, 100, 0f);
            }
        }

        if (totalProgressText != null)
        {
            if (profs != null)
                totalProgressText.text = $"Max-tier professions: {maxed} / {PlayerGrudgeProfessions.ProfessionCount}";
            else
                totalProgressText.text = "Profession data not found on player.";
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// GrudgeProfessionRow — one row in the professions tab
// ─────────────────────────────────────────────────────────────────────────────
public class GrudgeProfessionRow : MonoBehaviour
{
    [Header("UI")]
    public Text     nameText;
    public Text     tierText;
    public Text     xpText;
    public Text     milestoneText;
    public Image    xpBar;           // fill amount 0..1
    public Image    tierIcon;        // optional icon per tier (swap sprite via tierSprites)
    public Sprite[] tierSprites;     // optional array of 5 sprites, one per tier

    static readonly string[] MilestoneDescriptions =
    {
        "Unlock Tier 2 resources",
        "Unlock Tier 3 resources + bonus speed",
        "Unlock Tier 4 resources + quality crafts",
        "Unlock Tier 5 resources + rare drops",
        "MASTERED — full access"
    };

    public void Refresh(string profName, int tier, long xp, long needed, float progress)
    {
        if (nameText != null) nameText.text = profName;
        if (tierText != null) tierText.text = $"Tier {tier}";
        if (xpBar    != null) xpBar.fillAmount = progress;

        if (xpText != null)
        {
            xpText.text = tier >= PlayerGrudgeProfessions.MaxTier
                ? "MASTERED"
                : $"{xp} / {needed} XP";
        }

        // Milestone label: what advancing to the next tier unlocks
        if (milestoneText != null)
        {
            int nextTier = tier; // 0-indexed into MilestoneDescriptions
            milestoneText.text = nextTier <= MilestoneDescriptions.Length
                ? MilestoneDescriptions[Mathf.Clamp(nextTier, 0, MilestoneDescriptions.Length - 1)]
                : "";
        }

        // Optional tier icon swap
        if (tierIcon != null && tierSprites != null && tier > 0 && tier <= tierSprites.Length)
        {
            tierIcon.sprite  = tierSprites[tier - 1];
            tierIcon.enabled = tierSprites[tier - 1] != null;
        }
    }
}
