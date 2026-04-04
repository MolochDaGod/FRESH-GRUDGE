#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// ─────────────────────────────────────────────────────────────────────────────
// GrudgeMenuDeployer
//
// Open via:  Grudge Studio ▶ Deploy Grudge Menu
//
// ONE BUTTON builds the entire Grudge hub UI into whatever scene is currently
// open, creates all required slot prefabs, wires script references, attaches
// GrudgeHotbarLayout to the existing Skillbar, and marks the scene dirty so
// you can Ctrl+S to save.
//
// Safe to run multiple times — it checks for existing "GrudgeMenu" root first.
// ─────────────────────────────────────────────────────────────────────────────
public class GrudgeMenuDeployer : EditorWindow
{
    // ── Colours ───────────────────────────────────────────────────────────────
    static readonly Color GRUDGE_DARK   = new Color(0.08f, 0.08f, 0.10f, 0.96f);
    static readonly Color GRUDGE_PANEL  = new Color(0.12f, 0.12f, 0.15f, 0.98f);
    static readonly Color GRUDGE_GOLD   = new Color(0.85f, 0.60f, 0.15f, 1.00f);
    static readonly Color GRUDGE_DIM    = new Color(0.20f, 0.20f, 0.22f, 0.90f);
    static readonly Color GRUDGE_ACCENT = new Color(0.30f, 0.55f, 1.00f, 1.00f);

    static readonly string[] TAB_NAMES = { "Skills", "Class Tree", "Professions", "Character", "Onboarding", "Tools" };

    // ── Prefab paths we'll create ─────────────────────────────────────────────
    const string PREFAB_DIR      = "Assets/uMMORPG/Scripts/Addons/!custom/GrudgeMenu/Prefabs";
    const string PREFAB_SKILLSLOT  = PREFAB_DIR + "/GrudgeSkillSlot.prefab";
    const string PREFAB_PROFROW    = PREFAB_DIR + "/GrudgeProfessionRow.prefab";
    const string PREFAB_PROFBADGE  = PREFAB_DIR + "/GrudgeProficiencyBadge.prefab";

    // ── Window ────────────────────────────────────────────────────────────────
    Vector2 _scroll;
    string  _log = "";

    [MenuItem("Grudge Studio/Deploy Grudge Menu", priority = 1)]
    static void Open() => GetWindow<GrudgeMenuDeployer>("Grudge Menu Deployer").minSize = new Vector2(420, 600);

    void OnGUI()
    {
        EditorGUILayout.Space(8);
        GUI.color = GRUDGE_GOLD;
        EditorGUILayout.LabelField("⚔  GRUDGE MENU DEPLOYER", EditorStyles.boldLabel);
        GUI.color = Color.white;
        EditorGUILayout.Space(4);
        EditorGUILayout.HelpBox(
            "Deploys the complete Grudge Hub UI into the currently active scene.\n" +
            "• Creates GrudgeMenu root on the HUD Canvas\n" +
            "• Builds 6 tabs: Skills, Class Tree, Professions, Character, Onboarding, Tools\n" +
            "• Creates slot prefabs in Assets/uMMORPG/.../GrudgeMenu/Prefabs/\n" +
            "• Attaches GrudgeHotbarLayout to existing Skillbar\n" +
            "• Adds PlayerGrudgeProfessions to Player prefabs found in scene\n" +
            "Safe to re-run — will skip if already deployed.",
            MessageType.Info);

        EditorGUILayout.Space(8);
        GUI.backgroundColor = GRUDGE_GOLD;
        GUI.color = Color.black;
        if (GUILayout.Button("▶  DEPLOY GRUDGE MENU INTO SCENE", GUILayout.Height(54)))
        {
            _log = "";
            Deploy();
        }
        GUI.color = Color.white;
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Log", EditorStyles.boldLabel);
        _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));
        EditorGUILayout.TextArea(_log, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  DEPLOY
    // ══════════════════════════════════════════════════════════════════════════
    void Deploy()
    {
        Log("=== Grudge Menu Deploy Started ===");

        // ── 0. Prefab directory ────────────────────────────────────────────
        Directory.CreateDirectory(PREFAB_DIR);
        AssetDatabase.Refresh();

        // ── 1. Find / create canvas ────────────────────────────────────────
        Canvas hudCanvas = FindHUDCanvas();
        if (hudCanvas == null) { Log("ERROR: No Canvas found in scene. Open a game scene first."); return; }
        Log($"Canvas: {hudCanvas.gameObject.name}");

        // ── 2. Guard: already deployed? ────────────────────────────────────
        Transform existing = hudCanvas.transform.Find("GrudgeMenu");
        if (existing != null) { Log("GrudgeMenu already exists — skipping root creation."); PatchExisting(existing.gameObject, hudCanvas); return; }

        // ── 3. Build slot prefabs (needed before building the UI) ──────────
        GameObject skillSlotPrefab  = EnsureSkillSlotPrefab();
        GameObject profRowPrefab    = EnsureProfessionRowPrefab();
        GameObject profBadgePrefab  = EnsureProficiencyBadgePrefab();
        Log("Prefabs ready.");

        // ── 4. Root panel (full-screen, darkened overlay) ──────────────────
        GameObject root = MakePanel("GrudgeMenu", hudCanvas.transform,
            anchorMin: Vector2.zero, anchorMax: Vector2.one,
            offsetMin: Vector2.zero, offsetMax: Vector2.zero,
            color: GRUDGE_DARK);
        root.SetActive(false); // starts hidden; G key opens it

        // ── 5. Left tab sidebar ────────────────────────────────────────────
        GameObject sidebar = MakePanel("Sidebar", root.transform,
            anchorMin: new Vector2(0, 0), anchorMax: new Vector2(0, 1),
            offsetMin: new Vector2(0, 0), offsetMax: new Vector2(180, 0),
            color: GRUDGE_PANEL);

        AddVerticalLayoutGroup(sidebar, 0, new RectOffset(8, 8, 8, 8));

        // ── 6. Content area ────────────────────────────────────────────────
        GameObject contentRoot = MakePanel("ContentRoot", root.transform,
            anchorMin: new Vector2(0, 0), anchorMax: new Vector2(1, 1),
            offsetMin: new Vector2(184, 0), offsetMax: new Vector2(0, 0),
            color: new Color(0, 0, 0, 0));

        // ── 7. Title bar ───────────────────────────────────────────────────
        GameObject titleBar = MakePanel("TitleBar", root.transform,
            anchorMin: new Vector2(0, 1), anchorMax: new Vector2(1, 1),
            offsetMin: new Vector2(0, -40), offsetMax: new Vector2(0, 0),
            color: GRUDGE_GOLD);
        AddText(titleBar, "TitleText", "⚔ GRUDGE WARLORDS", 20, Color.black,
            TextAnchor.MiddleCenter, FontStyle.Bold);

        Button closeBtn = MakeButton(titleBar, "CloseButton",
            "✕", 16, new Color(0.7f, 0, 0),
            new Vector2(1, 0.5f), new Vector2(1, 0.5f),
            new Vector2(-36, -14), new Vector2(-4, 14));

        // ── 8. Tab buttons + content panels ───────────────────────────────
        var tabButtons   = new Button[TAB_NAMES.Length];
        var tabPanels    = new GameObject[TAB_NAMES.Length];
        var tabHighlights= new Image[TAB_NAMES.Length];
        var badges       = new GameObject[TAB_NAMES.Length];

        for (int i = 0; i < TAB_NAMES.Length; i++)
        {
            // Tab button row
            GameObject btnGO  = MakePanel($"Tab_{TAB_NAMES[i]}", sidebar.transform,
                anchorMin: new Vector2(0, 1), anchorMax: new Vector2(1, 1),
                offsetMin: new Vector2(0, -(i + 1) * 44), offsetMax: new Vector2(0, -i * 44),
                color: GRUDGE_DIM);
            Image  highlight  = btnGO.GetComponent<Image>();
            tabHighlights[i]  = highlight;

            Button btn        = btnGO.AddComponent<Button>();
            AddText(btnGO, "Label", TAB_NAMES[i], 14, Color.white, TextAnchor.MiddleLeft);
            tabButtons[i]     = btn;

            // Notification badge (small red dot)
            GameObject badge  = MakePanel("Badge", btnGO.transform,
                anchorMin: new Vector2(1, 0.5f), anchorMax: new Vector2(1, 0.5f),
                offsetMin: new Vector2(-20, -6), offsetMax: new Vector2(-4, 6),
                color: new Color(1, 0.2f, 0.2f));
            badge.SetActive(false);
            badges[i]         = badge;

            // Content panel
            GameObject contentPanel = MakePanel($"Panel_{TAB_NAMES[i]}", contentRoot.transform,
                anchorMin: Vector2.zero, anchorMax: Vector2.one,
                offsetMin: Vector2.zero, offsetMax: Vector2.zero,
                color: new Color(0, 0, 0, 0));
            contentPanel.SetActive(i == 0);
            tabPanels[i] = contentPanel;

            Log($"  Tab created: {TAB_NAMES[i]}");
        }

        // ── 9. Skills tab content ──────────────────────────────────────────
        BuildSkillsPanel(tabPanels[0], skillSlotPrefab);

        // ── 10. Class tree tab (placeholder + link label) ─────────────────
        BuildClassTreePanel(tabPanels[1]);

        // ── 11. Professions tab ────────────────────────────────────────────
        BuildProfessionsPanel(tabPanels[2], profRowPrefab);

        // ── 12. Character tab ──────────────────────────────────────────────
        BuildCharacterPanel(tabPanels[3], profBadgePrefab);

        // ── 13. Onboarding tab ─────────────────────────────────────────────
        BuildOnboardingPanel(tabPanels[4]);

        // ── 14. Tools tab ─────────────────────────────────────────────────
        BuildToolsPanel(tabPanels[5]);

        // ── 15. Attach UI_GrudgeMenu controller ────────────────────────────
        UI_GrudgeMenu hubCtrl = root.AddComponent<UI_GrudgeMenu>();
        hubCtrl.hotKey        = KeyCode.G;
        hubCtrl.panel         = root;
        hubCtrl.tabButtons    = tabButtons;
        hubCtrl.tabPanels     = tabPanelsToGO(tabPanels);
        hubCtrl.tabHighlightImages = tabHighlights;
        hubCtrl.activeTabColor   = GRUDGE_GOLD;
        hubCtrl.inactiveTabColor  = GRUDGE_DIM;
        hubCtrl.notificationBadges = badges;

        // Close button wires to hub
        closeBtn.onClick.AddListener(() => hubCtrl.Close());

        // Attach tab sub-scripts
        UI_GrudgeSkillsTab skillsTab   = tabPanels[0].AddComponent<UI_GrudgeSkillsTab>();
        UI_GrudgeProfessionsTab profsTab = tabPanels[2].AddComponent<UI_GrudgeProfessionsTab>();
        UI_GrudgeCharacterTab charTab   = tabPanels[3].AddComponent<UI_GrudgeCharacterTab>();
        UI_GrudgeOnboarding onboarding  = tabPanels[4].AddComponent<UI_GrudgeOnboarding>();

        // Wire slot prefabs
        skillsTab.slotPrefab = skillSlotPrefab.GetComponent<GrudgeSkillSlot>();
        skillsTab.content    = tabPanels[0].transform.Find("ScrollRect/Viewport/Content");
        skillsTab.skillXpText = FindTextInChildren(tabPanels[0], "SkillXpText");

        profsTab.professionRows = FindProfessionRows(tabPanels[2]);
        profsTab.totalProgressText = FindTextInChildren(tabPanels[2], "TotalProgressText");

        charTab.proficiencyBadgePrefab = profBadgePrefab.GetComponent<GrudgeProficiencyBadge>();
        charTab.proficiencyContent     = tabPanels[3].transform.Find("ProficiencyContent");
        charTab.treeSummaryText        = FindTextInChildren(tabPanels[3], "TreeSummaryText");
        charTab.grudgeMenu             = hubCtrl;

        hubCtrl.skillsTab     = skillsTab;
        hubCtrl.professionsTab = profsTab;
        hubCtrl.characterTab  = charTab;
        hubCtrl.onboarding    = onboarding;

        onboarding.grudgeMenu = hubCtrl;

        // ── 16. Wire existing ClassTreePanel if present ────────────────────
        UI_ClassTreePanel existingTree = Object.FindObjectOfType<UI_ClassTreePanel>();
        if (existingTree != null)
        {
            hubCtrl.classTreePanel = existingTree;
            Log("  Linked existing UI_ClassTreePanel.");
        }

        // ── 17. Attach GrudgeHotbarLayout to Skillbar ─────────────────────
        PatchSkillbar();

        // ── 18. Patch player prefabs with PlayerGrudgeProfessions ──────────
        PatchPlayerPrefabs();

        // ── 19. Mark scene dirty ───────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Log("=== DEPLOY COMPLETE. Press Ctrl+S to save. ===");
        Repaint();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Tab Content Builders
    // ══════════════════════════════════════════════════════════════════════════

    void BuildSkillsPanel(GameObject panel, GameObject slotPrefab)
    {
        // Header
        GameObject header = MakePanel("Header", panel.transform,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -28), new Vector2(0, 0),
            GRUDGE_GOLD);
        AddText(header, "Title", "⚔ Skills", 16, Color.black, TextAnchor.MiddleLeft, FontStyle.Bold);
        AddText(panel, "SkillXpText", "Skill XP: 0", 12, new Color(0.8f, 0.8f, 0.8f), TextAnchor.UpperRight);

        // Scroll view
        ScrollRect scroll = MakeScrollRect(panel, "ScrollRect",
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 28), new Vector2(0, -32));
        scroll.gameObject.GetOrAddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        AddVerticalLayoutGroup(scroll.content.gameObject, 4, new RectOffset(6, 6, 4, 4));

        // Upgrade-all button
        MakeButton(panel, "UpgradeAllButton", "Upgrade All ✓", 13, GRUDGE_GOLD,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-130, -28), new Vector2(-4, -4));
    }

    void BuildClassTreePanel(GameObject panel)
    {
        AddText(panel, "Info", "Class Tree panel is embedded below.\nHotkey: T", 14,
            new Color(0.8f, 0.8f, 0.8f), TextAnchor.MiddleCenter);
        AddText(panel, "Note",
            "The existing UI_ClassTreePanel will show here.\n" +
            "If no tree panel is in your scene yet, create one from\n" +
            "the !custom/ClassLevelingTree prefab and assign it in\n" +
            "the UI_GrudgeMenu inspector under 'classTreePanel'.",
            12, new Color(0.6f, 0.6f, 0.6f), TextAnchor.MiddleCenter);
    }

    void BuildProfessionsPanel(GameObject panel, GameObject rowPrefab)
    {
        GameObject header = MakePanel("Header", panel.transform,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -28), new Vector2(0, 0),
            GRUDGE_GOLD);
        AddText(header, "Title", "⛏ Professions", 16, Color.black, TextAnchor.MiddleLeft, FontStyle.Bold);

        AddText(panel, "TotalProgressText", "Max-tier professions: 0 / 5", 12,
            new Color(0.7f, 0.7f, 0.7f), TextAnchor.UpperRight);

        // 5 profession rows stacked vertically
        string[] profNames = { "Harvesting", "Mining", "Woodcutting", "Fishing", "Crafting" };
        Color[]  profColors = {
            new Color(0.3f, 0.8f, 0.3f),
            new Color(0.8f, 0.6f, 0.3f),
            new Color(0.5f, 0.8f, 0.5f),
            new Color(0.3f, 0.5f, 1.0f),
            new Color(0.9f, 0.4f, 0.8f)
        };

        float rowH = 68f;
        for (int i = 0; i < profNames.Length; i++)
        {
            float yMin = 1f - (i + 1) * rowH / 400f;
            float yMax = 1f - i * rowH / 400f;

            GameObject row = MakePanel($"ProfRow_{profNames[i]}", panel.transform,
                new Vector2(0.01f, 0), new Vector2(0.99f, 1),
                new Vector2(0, -(i + 1) * (rowH + 4) + 28),
                new Vector2(0, -(i) * (rowH + 4) + 28 - rowH + 28),
                GRUDGE_PANEL);

            GrudgeProfessionRow rowComp = row.AddComponent<GrudgeProfessionRow>();

            // Name label
            GameObject nameGO = MakeTextGO(row, "NameText", profNames[i], 14,
                profColors[i], TextAnchor.UpperLeft, FontStyle.Bold);
            rowComp.nameText = nameGO.GetComponent<Text>();

            // Tier label
            GameObject tierGO = MakeTextGO(row, "TierText", "Tier 1", 12,
                Color.white, TextAnchor.UpperRight);
            rowComp.tierText = tierGO.GetComponent<Text>();

            // XP bar (filled image)
            GameObject barBg = MakePanel("XPBarBg", row.transform,
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(4, 6), new Vector2(-4, 18),
                new Color(0.1f, 0.1f, 0.1f));
            GameObject barFill = MakePanel("XPBarFill", barBg.transform,
                Vector2.zero, new Vector2(0.5f, 1),
                Vector2.zero, Vector2.zero,
                profColors[i]);
            Image barImg = barFill.GetComponent<Image>();
            barImg.type = Image.Type.Filled;
            barImg.fillMethod = Image.FillMethod.Horizontal;
            barImg.fillAmount = 0f;
            rowComp.xpBar = barImg;

            // XP text
            GameObject xpTextGO = MakeTextGO(row, "XPText", "0 / 100 XP", 11,
                new Color(0.7f, 0.7f, 0.7f), TextAnchor.LowerLeft);
            rowComp.xpText = xpTextGO.GetComponent<Text>();

            // Milestone
            GameObject milGO = MakeTextGO(row, "MilestoneText", "Unlock Tier 2 resources", 10,
                GRUDGE_GOLD, TextAnchor.LowerRight);
            rowComp.milestoneText = milGO.GetComponent<Text>();
        }
    }

    void BuildCharacterPanel(GameObject panel, GameObject badgePrefab)
    {
        GameObject header = MakePanel("Header", panel.transform,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -28), new Vector2(0, 0),
            GRUDGE_GOLD);
        AddText(header, "Title", "📋 Character", 16, Color.black, TextAnchor.MiddleLeft, FontStyle.Bold);

        // Identity block
        AddText(panel, "CharacterNameText", "Player Name", 18, Color.white,
            TextAnchor.UpperLeft, FontStyle.Bold);
        AddText(panel, "ClassNameText",    "Class: —",    14, GRUDGE_GOLD, TextAnchor.UpperLeft);
        AddText(panel, "LevelText",        "Level 1",      13, Color.white, TextAnchor.UpperRight);

        // Stats
        AddText(panel, "HealthText",  "HP: —",      12, new Color(0.6f, 1f, 0.6f), TextAnchor.UpperLeft);
        AddText(panel, "ManaText",    "MP: —",      12, new Color(0.6f, 0.7f, 1f), TextAnchor.UpperLeft);
        AddText(panel, "DamageText",  "Damage: —",  12, new Color(1f,   0.7f, 0.5f), TextAnchor.UpperLeft);
        AddText(panel, "DefenseText", "Defense: —", 12, new Color(0.7f, 0.7f, 1f), TextAnchor.UpperLeft);
        AddText(panel, "SkillXpText", "Skill XP: —",12, Color.white, TextAnchor.UpperLeft);

        // Tree summary (scrollable)
        ScrollRect treeSR = MakeScrollRect(panel, "TreeSummaryScroll",
            new Vector2(0, 0.4f), new Vector2(1, 1),
            new Vector2(0, 28), new Vector2(0, -28));
        AddText(treeSR.content.gameObject, "TreeSummaryText",
            "Class tree summary will appear here.", 12, Color.white, TextAnchor.UpperLeft);

        // Proficiency badges
        GameObject profContent = MakePanel("ProficiencyContent", panel.transform,
            new Vector2(0, 0), new Vector2(1, 0.38f),
            new Vector2(4, 0), new Vector2(-4, 0), new Color(0, 0, 0, 0));
        AddHorizontalLayoutGroup(profContent, 4, new RectOffset(2, 2, 2, 2));

        // Open class tree button
        MakeButton(panel, "OpenClassTreeButton", "Open Class Tree →", 12, GRUDGE_ACCENT,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-160, -30), new Vector2(-4, -4));
    }

    void BuildOnboardingPanel(GameObject panel)
    {
        GameObject header = MakePanel("Header", panel.transform,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -28), new Vector2(0, 0),
            GRUDGE_GOLD);
        AddText(header, "Title", "📖 Getting Started", 16, Color.black,
            TextAnchor.MiddleLeft, FontStyle.Bold);

        AddText(panel, "StepCounterText", "Step 1 of 6", 12, GRUDGE_GOLD, TextAnchor.UpperRight);

        // Progress bar
        GameObject barBg   = MakePanel("ProgressBg", panel.transform,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(4, -48), new Vector2(-4, -34),
            new Color(0.1f, 0.1f, 0.1f));
        GameObject barFill = MakePanel("ProgressFill", barBg.transform,
            Vector2.zero, new Vector2(0.16f, 1), Vector2.zero, Vector2.zero, GRUDGE_GOLD);
        Image barImg = barFill.GetComponent<Image>();
        barImg.type = Image.Type.Filled;
        barImg.fillMethod = Image.FillMethod.Horizontal;
        barImg.fillAmount = 1f / 6f;

        AddText(panel, "StepTitleText", "Welcome to Grudge Warlords", 16,
            Color.white, TextAnchor.UpperLeft, FontStyle.Bold);

        ScrollRect bodySR = MakeScrollRect(panel, "BodyScroll",
            new Vector2(0, 0.25f), new Vector2(1, 1),
            new Vector2(0, 50), new Vector2(0, -56));
        AddText(bodySR.content.gameObject, "StepBodyText",
            "Step body text goes here. The UI_GrudgeOnboarding script will fill this in.",
            13, new Color(0.85f, 0.85f, 0.85f), TextAnchor.UpperLeft);

        // Navigation buttons
        MakeButton(panel, "PrevButton", "◀ Prev", 13, GRUDGE_DIM,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(4, 4), new Vector2(90, 34));
        Button nextBtn = MakeButton(panel, "NextButton", "Next ▶", 13, GRUDGE_GOLD,
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(-94, 4), new Vector2(-4, 34));
        MakeButton(panel, "DismissButton", "Skip Guide", 11, new Color(0.4f, 0.4f, 0.4f),
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-60, 4), new Vector2(60, 24));
        MakeButton(panel, "OpenTabButton", "Open Tab →", 12, GRUDGE_ACCENT,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-70, 30), new Vector2(70, 52));
    }

    void BuildToolsPanel(GameObject panel)
    {
        GameObject header = MakePanel("Header", panel.transform,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -28), new Vector2(0, 0),
            GRUDGE_GOLD);
        AddText(header, "Title", "🔧 Tools & Shortcuts", 16, Color.black,
            TextAnchor.MiddleLeft, FontStyle.Bold);

        string[] toolLabels = {
            "⚔  Open Class Tree Panel (T)",
            "🎒  Open Inventory (I)",
            "💬  Open Chat",
            "⚙   Open Settings",
            "🗺   Open Map (M)",
            "👥  Open Party Panel",
            "📦  Open Crafting",
            "🏆  Open Quests",
        };
        float rowH = 36f;
        for (int i = 0; i < toolLabels.Length; i++)
        {
            GameObject row = MakePanel($"Tool_{i}", panel.transform,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(4, -(i + 1) * (rowH + 4) + 28),
                new Vector2(-4, -(i) * (rowH + 4) + 28 - rowH + 28),
                GRUDGE_PANEL);
            row.AddComponent<Button>();
            AddText(row, "Label", toolLabels[i], 13, Color.white, TextAnchor.MiddleLeft);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Patch helpers
    // ══════════════════════════════════════════════════════════════════════════

    void PatchSkillbar()
    {
        UI_Skillbar bar = Object.FindObjectOfType<UI_Skillbar>();
        if (bar == null) { Log("  Skillbar not found in scene — skipping hotbar patch."); return; }
        if (bar.GetComponent<GrudgeHotbarLayout>() == null)
        {
            bar.gameObject.AddComponent<GrudgeHotbarLayout>();
            Log($"  GrudgeHotbarLayout added to {bar.gameObject.name}.");
        }
        else Log("  GrudgeHotbarLayout already on Skillbar.");
    }

    void PatchPlayerPrefabs()
    {
        // Find player GameObjects in scene that have a Player component
        Player[] players = Object.FindObjectsOfType<Player>();
        foreach (Player p in players)
        {
            if (p.GetComponent<PlayerGrudgeProfessions>() == null)
            {
                p.gameObject.AddComponent<PlayerGrudgeProfessions>();
                Log($"  PlayerGrudgeProfessions added to {p.gameObject.name}.");
            }
        }
        if (players.Length == 0)
        {
            Log("  No Player instances in scene — remember to add PlayerGrudgeProfessions to your Player prefab.");
        }
    }

    void PatchExisting(GameObject root, Canvas canvas)
    {
        PatchSkillbar();
        PatchPlayerPrefabs();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Log("=== Patch complete. ===");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Prefab creators
    // ══════════════════════════════════════════════════════════════════════════

    GameObject EnsureSkillSlotPrefab()
    {
        if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), PREFAB_SKILLSLOT)))
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_SKILLSLOT);
            if (existing != null) return existing;
        }

        GameObject slot = new GameObject("GrudgeSkillSlot");
        slot.AddComponent<RectTransform>();
        slot.AddComponent<CanvasRenderer>();
        Image bg = slot.AddComponent<Image>();
        bg.color = GRUDGE_PANEL;

        GrudgeSkillSlot comp = slot.AddComponent<GrudgeSkillSlot>();

        // Icon
        GameObject iconGO = MakeImageChild(slot, "IconImage", Color.white);
        comp.iconImage = iconGO.GetComponent<Image>();

        // Name
        GameObject nameGO = MakeTextChild(slot, "NameText", "Skill Name", 13, Color.white, TextAnchor.UpperLeft, FontStyle.Bold);
        comp.nameText = nameGO.GetComponent<Text>();

        // Level
        GameObject lvlGO = MakeTextChild(slot, "LevelText", "Lv. 0 / 0", 11, GRUDGE_GOLD, TextAnchor.UpperRight);
        comp.levelText = lvlGO.GetComponent<Text>();

        // Category
        GameObject catGO = MakeTextChild(slot, "CategoryTag", "", 10, GRUDGE_ACCENT, TextAnchor.LowerLeft);
        comp.categoryTag = catGO.GetComponent<Text>();

        // Description
        GameObject descGO = MakeTextChild(slot, "DescText", "", 11, new Color(0.75f, 0.75f, 0.75f), TextAnchor.UpperLeft);
        comp.descText = descGO.GetComponent<Text>();

        // Locked overlay
        GameObject locked = MakeImageChild(slot, "LockedOverlay", new Color(0.8f, 0, 0, 0.5f));
        AddText(locked, "LockIcon", "🔒 Locked", 12, Color.white, TextAnchor.MiddleCenter);
        locked.SetActive(false);
        comp.lockedOverlay = locked;

        // Glow
        GameObject glow = MakeImageChild(slot, "UpgradableGlow", new Color(1, 0.9f, 0, 0.15f));
        glow.SetActive(false);
        comp.upgradableGlow = glow;

        // Upgrade button
        GameObject btnGO = new GameObject("UpgradeButton");
        btnGO.transform.SetParent(slot.transform, false);
        btnGO.AddComponent<RectTransform>();
        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = GRUDGE_GOLD;
        Button btn = btnGO.AddComponent<Button>();
        AddText(btnGO, "Text", "Upgrade", 12, Color.black, TextAnchor.MiddleCenter, FontStyle.Bold);
        comp.upgradeButton = btn;

        GameObject prefabGO = PrefabUtility.SaveAsPrefabAsset(slot, PREFAB_SKILLSLOT);
        Object.DestroyImmediate(slot);
        Log($"  Created prefab: {PREFAB_SKILLSLOT}");
        return prefabGO;
    }

    GameObject EnsureProfessionRowPrefab()
    {
        if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), PREFAB_PROFROW)))
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PROFROW);
            if (existing != null) return existing;
        }
        GameObject row = new GameObject("GrudgeProfessionRow");
        row.AddComponent<RectTransform>();
        row.AddComponent<CanvasRenderer>();
        row.AddComponent<Image>().color = GRUDGE_PANEL;
        GrudgeProfessionRow comp = row.AddComponent<GrudgeProfessionRow>();

        comp.nameText      = MakeTextChild(row, "NameText",      "Profession", 14, Color.white, TextAnchor.UpperLeft, FontStyle.Bold).GetComponent<Text>();
        comp.tierText      = MakeTextChild(row, "TierText",      "Tier 1",     12, GRUDGE_GOLD,  TextAnchor.UpperRight).GetComponent<Text>();
        comp.xpText        = MakeTextChild(row, "XPText",        "0/100 XP",   11, Color.gray,   TextAnchor.LowerLeft).GetComponent<Text>();
        comp.milestoneText = MakeTextChild(row, "MilestoneText", "Next unlock",10, GRUDGE_ACCENT,TextAnchor.LowerRight).GetComponent<Text>();

        // XP bar
        GameObject barBg   = MakeImageChild(row, "XPBarBg",   new Color(0.1f, 0.1f, 0.1f));
        GameObject barFill = MakeImageChild(barBg, "XPBarFill", GRUDGE_GOLD);
        Image fill = barFill.GetComponent<Image>();
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillAmount = 0f;
        comp.xpBar = fill;

        GameObject prefabGO = PrefabUtility.SaveAsPrefabAsset(row, PREFAB_PROFROW);
        Object.DestroyImmediate(row);
        Log($"  Created prefab: {PREFAB_PROFROW}");
        return prefabGO;
    }

    GameObject EnsureProficiencyBadgePrefab()
    {
        if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), PREFAB_PROFBADGE)))
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PROFBADGE);
            if (existing != null) return existing;
        }
        GameObject badge = new GameObject("GrudgeProficiencyBadge");
        badge.AddComponent<RectTransform>();
        badge.AddComponent<CanvasRenderer>();
        badge.AddComponent<Image>().color = GRUDGE_DIM;
        GrudgeProficiencyBadge comp = badge.AddComponent<GrudgeProficiencyBadge>();
        comp.weaponTypeText = MakeTextChild(badge, "WeaponTypeText", "Sword", 12, Color.white, TextAnchor.UpperCenter, FontStyle.Bold).GetComponent<Text>();
        comp.bonusText      = MakeTextChild(badge, "BonusText",      "+15%",  11, GRUDGE_GOLD,  TextAnchor.LowerCenter).GetComponent<Text>();

        GameObject prefabGO = PrefabUtility.SaveAsPrefabAsset(badge, PREFAB_PROFBADGE);
        Object.DestroyImmediate(badge);
        Log($"  Created prefab: {PREFAB_PROFBADGE}");
        return prefabGO;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Low-level UI factory helpers
    // ══════════════════════════════════════════════════════════════════════════

    static Canvas FindHUDCanvas()
    {
        Canvas[] all = Object.FindObjectsOfType<Canvas>();
        // Prefer canvas named "HUD", "UI", "Canvas" or a screen-space overlay
        foreach (Canvas c in all)
        {
            string n = c.gameObject.name.ToLower();
            if (n.Contains("hud") || n.Contains("ui") || n == "canvas")
                if (c.renderMode == RenderMode.ScreenSpaceOverlay) return c;
        }
        // Fallback: any screen-space overlay
        foreach (Canvas c in all)
            if (c.renderMode == RenderMode.ScreenSpaceOverlay) return c;
        // Fallback: any canvas
        return all.Length > 0 ? all[0] : null;
    }

    static GameObject MakePanel(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin  = anchorMin;
        rt.anchorMax  = anchorMax;
        rt.offsetMin  = offsetMin;
        rt.offsetMax  = offsetMax;
        go.AddComponent<CanvasRenderer>();
        go.AddComponent<Image>().color = color;
        return go;
    }

    static ScrollRect MakeScrollRect(GameObject parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offMin, Vector2 offMax)
    {
        GameObject srGO = new GameObject(name);
        srGO.transform.SetParent(parent.transform, false);
        RectTransform rt = srGO.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = offMin;   rt.offsetMax = offMax;
        srGO.AddComponent<Image>().color = new Color(0, 0, 0, 0);
        ScrollRect sr = srGO.AddComponent<ScrollRect>();
        sr.horizontal = false;

        // Viewport
        GameObject vpGO = new GameObject("Viewport");
        vpGO.transform.SetParent(srGO.transform, false);
        RectTransform vpRT = vpGO.AddComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero; vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = Vector2.zero; vpRT.offsetMax = Vector2.zero;
        vpGO.AddComponent<Image>().color = new Color(0, 0, 0, 0);
        vpGO.AddComponent<Mask>().showMaskGraphic = false;
        sr.viewport = vpRT;

        // Content
        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(vpGO.transform, false);
        RectTransform cRT = contentGO.AddComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0, 1); cRT.anchorMax = new Vector2(1, 1);
        cRT.offsetMin = Vector2.zero;      cRT.offsetMax = Vector2.zero;
        cRT.pivot     = new Vector2(0.5f, 1);
        ContentSizeFitter csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sr.content = cRT;

        return sr;
    }

    static Button MakeButton(GameObject parent, string name, string label, int fontSize,
        Color bgColor, Vector2 pivotMin, Vector2 pivotMax, Vector2 offMin, Vector2 offMax)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = pivotMin; rt.anchorMax = pivotMax;
        rt.offsetMin = offMin;   rt.offsetMax = offMax;
        go.AddComponent<CanvasRenderer>();
        go.AddComponent<Image>().color = bgColor;
        Button btn = go.AddComponent<Button>();
        AddText(go, "Text", label, fontSize, Color.white, TextAnchor.MiddleCenter);
        return btn;
    }

    static void AddText(GameObject parent, string name, string text, int size,
        Color color, TextAnchor align, FontStyle style = FontStyle.Normal)
    {
        GameObject go = MakeTextGO(parent, name, text, size, color, align, style);
        _ = go;
    }

    static GameObject MakeTextGO(GameObject parent, string name, string text, int size,
        Color color, TextAnchor align, FontStyle style = FontStyle.Normal)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(4, 2); rt.offsetMax = new Vector2(-4, -2);
        go.AddComponent<CanvasRenderer>();
        Text t = go.AddComponent<Text>();
        t.text      = text;
        t.fontSize  = size;
        t.color     = color;
        t.alignment = align;
        t.fontStyle = style;
        t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ??
                      Resources.GetBuiltinResource<Font>("Arial.ttf");
        return go;
    }

    static GameObject MakeTextChild(GameObject parent, string name, string text, int size,
        Color color, TextAnchor align, FontStyle style = FontStyle.Normal) =>
        MakeTextGO(parent, name, text, size, color, align, style);

    static GameObject MakeImageChild(GameObject parent, string name, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        go.AddComponent<CanvasRenderer>();
        go.AddComponent<Image>().color = color;
        return go;
    }

    static void AddVerticalLayoutGroup(GameObject go, int spacing, RectOffset padding)
    {
        VerticalLayoutGroup vlg = go.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = spacing;
        vlg.padding = padding;
        vlg.childControlWidth = true;
        vlg.childForceExpandWidth = true;
        ContentSizeFitter csf = go.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    static void AddHorizontalLayoutGroup(GameObject go, int spacing, RectOffset padding)
    {
        HorizontalLayoutGroup hlg = go.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = spacing;
        hlg.padding = padding;
        hlg.childControlHeight = true;
        hlg.childForceExpandHeight = true;
    }

    // ── Script wiring helpers ─────────────────────────────────────────────────

    static GrudgeProfessionRow[] FindProfessionRows(GameObject panel)
    {
        var rows = new System.Collections.Generic.List<GrudgeProfessionRow>();
        foreach (GrudgeProfessionRow r in panel.GetComponentsInChildren<GrudgeProfessionRow>(true))
            rows.Add(r);
        return rows.ToArray();
    }

    static Text FindTextInChildren(GameObject root, string objectName)
    {
        Transform t = root.transform.Find(objectName);
        if (t != null) return t.GetComponent<Text>();
        // Deep search
        foreach (Text tx in root.GetComponentsInChildren<Text>(true))
            if (tx.gameObject.name == objectName) return tx;
        return null;
    }

    static GameObject[] tabPanelsToGO(GameObject[] panels) => panels;

    // ── Log ───────────────────────────────────────────────────────────────────
    void Log(string msg) { _log += msg + "\n"; Debug.Log("[GrudgeMenuDeployer] " + msg); }
}

static class GameObjectExtensions
{
    public static T GetOrAddComponent<T>(this GameObject go) where T : Component =>
        go.GetComponent<T>() ?? go.AddComponent<T>();
}
#endif
