using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ─────────────────────────────────────────────────────────────────────────────
// UI_GrudgeMenu
//
// The central hub panel for all Grudge progression systems.
// Replaces the old MainPanel (skill list) as the primary player menu.
//
// PREFAB SETUP:
//   panel                   — root CanvasGroup / GameObject (toggled by G key)
//   tabButtons[]            — one Button per GrudgeTab in order
//   tabPanels[]             — one GameObject per GrudgeTab in order
//   tabHighlightImages[]    — optional Image per tab for active highlight color
//   notificationBadges[]    — optional small GameObject badges for new-content dots
//
// Attach this to the HUD canvas root (alongside other UI managers).
// ─────────────────────────────────────────────────────────────────────────────
public class UI_GrudgeMenu : MonoBehaviour
{
    // ── Tabs ──────────────────────────────────────────────────────────────────
    public enum GrudgeTab
    {
        Skills      = 0,
        ClassTree   = 1,
        Professions = 2,
        Character   = 3,
        Onboarding  = 4,
        Tools       = 5
    }

    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Root")]
    public KeyCode   hotKey = KeyCode.G;
    public GameObject panel;

    [Header("Tab Buttons (order must match GrudgeTab enum)")]
    public Button[]    tabButtons;
    public GameObject[] tabPanels;

    [Header("Tab Visuals (optional)")]
    public Image[]     tabHighlightImages;
    public Color       activeTabColor   = new Color(0.85f, 0.60f, 0.15f, 1f);   // Grudge gold
    public Color       inactiveTabColor = new Color(0.20f, 0.20f, 0.20f, 0.80f);

    [Header("Notification Badges (optional, one per tab)")]
    public GameObject[] notificationBadges;

    [Header("Embedded Panels (drag existing scripts here)")]
    public UI_ClassTreePanel classTreePanel;
    public UI_GrudgeSkillsTab    skillsTab;
    public UI_GrudgeProfessionsTab professionsTab;
    public UI_GrudgeCharacterTab   characterTab;
    public UI_GrudgeOnboarding     onboarding;

    // ── State ─────────────────────────────────────────────────────────────────
    GrudgeTab _currentTab = GrudgeTab.Skills;
    bool      _isOpen     = false;

    // ── Unity ─────────────────────────────────────────────────────────────────
    void Awake()
    {
        // Wire tab buttons at runtime so the prefab doesn't need event hookup
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int captured = i;
            if (tabButtons[i] != null)
                tabButtons[i].onClick.AddListener(() => SelectTab((GrudgeTab)captured));
        }
    }

    void Start()
    {
        SetOpen(false);
    }

    void Update()
    {
        Player player = Player.localPlayer;
        if (player == null) return;

        // Hotkey toggle (G) — skip if typing in any input field
        if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
            SetOpen(!_isOpen);

        // Refresh the active tab each frame while the menu is visible
        if (_isOpen)
            RefreshActiveTab(player);

        // Notification badges — check even when closed
        UpdateBadges(player);
    }

    // ── Open / Close ──────────────────────────────────────────────────────────
    public void SetOpen(bool open)
    {
        _isOpen = open;
        if (panel != null) panel.SetActive(open);
        if (open) SelectTab(_currentTab);
    }

    public void Toggle() => SetOpen(!_isOpen);
    public void Close()  => SetOpen(false);

    // ── Tab Selection ─────────────────────────────────────────────────────────
    public void SelectTab(GrudgeTab tab)
    {
        _currentTab = tab;

        for (int i = 0; i < tabPanels.Length; i++)
        {
            bool active = i == (int)tab;
            if (tabPanels[i] != null) tabPanels[i].SetActive(active);
        }

        // Highlight active tab button
        if (tabHighlightImages != null)
        {
            for (int i = 0; i < tabHighlightImages.Length; i++)
            {
                if (tabHighlightImages[i] != null)
                    tabHighlightImages[i].color = (i == (int)tab) ? activeTabColor : inactiveTabColor;
            }
        }

        // When switching to the ClassTree tab, delegate to the existing panel
        if (tab == GrudgeTab.ClassTree && classTreePanel != null)
        {
            // The existing UI_ClassTreePanel manages its own active state;
            // ensure its inner panel is shown
            if (classTreePanel.panel != null)
                classTreePanel.panel.SetActive(true);
        }
    }

    /// <summary>Open the menu and jump directly to a specific tab.</summary>
    public void OpenTab(GrudgeTab tab)
    {
        SetOpen(true);
        SelectTab(tab);
    }

    // ── Per-frame refresh ─────────────────────────────────────────────────────
    void RefreshActiveTab(Player player)
    {
        switch (_currentTab)
        {
            case GrudgeTab.Skills:
                if (skillsTab != null) skillsTab.Refresh(player);
                break;
            case GrudgeTab.Professions:
                if (professionsTab != null) professionsTab.Refresh(player);
                break;
            case GrudgeTab.Character:
                if (characterTab != null) characterTab.Refresh(player);
                break;
        }
    }

    // ── Notification badges ───────────────────────────────────────────────────
    void UpdateBadges(Player player)
    {
        if (notificationBadges == null || notificationBadges.Length == 0) return;

        // Skills tab badge: any upgradable skill?
        SetBadge(GrudgeTab.Skills, player != null && HasUpgradableSkill(player));

        // Class tree badge: new tier available and not yet chosen?
#if _IMMOCLASSTREE
        bool newTier = player != null && player.classTree != null &&
                       !player.classTree.HasChosenTier(player.classTree.GetCurrentAvailableTier());
        SetBadge(GrudgeTab.ClassTree, newTier);
#endif

        // Onboarding badge: guide not dismissed yet
        SetBadge(GrudgeTab.Onboarding, onboarding != null && !onboarding.IsDismissed(player));
    }

    bool HasUpgradableSkill(Player player)
    {
        if (player.skills == null) return false;
        foreach (Skill s in player.skills.skills)
            if (((PlayerSkills)player.skills).CanUpgrade(s)) return true;
        return false;
    }

    void SetBadge(GrudgeTab tab, bool visible)
    {
        int i = (int)tab;
        if (notificationBadges != null && i < notificationBadges.Length && notificationBadges[i] != null)
            notificationBadges[i].SetActive(visible);
    }
}
