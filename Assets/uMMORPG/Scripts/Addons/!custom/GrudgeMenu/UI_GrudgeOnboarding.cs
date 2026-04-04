using UnityEngine;
using UnityEngine.UI;

// ─────────────────────────────────────────────────────────────────────────────
// UI_GrudgeOnboarding
//
// Multi-step first-login guide shown inside the Grudge hub Onboarding tab.
// State is persisted in PlayerPrefs keyed to the character name so each
// character tracks its own onboarding progress independently.
//
// PREFAB SETUP:
//   stepPanels[]     — one GameObject per onboarding step (shown one at a time)
//   stepTitleText    — title text updated per step
//   stepBodyText     — body text updated per step
//   stepCounterText  — "Step 2 of 6"
//   progressBar      — Image (filled) showing progress through all steps
//   prevButton       — go to previous step
//   nextButton       — go to next step / finish
//   dismissButton    — skip onboarding entirely
//   grudgeMenu       — reference to UI_GrudgeMenu for deep-link buttons
// ─────────────────────────────────────────────────────────────────────────────
public class UI_GrudgeOnboarding : MonoBehaviour
{
    // ── Step data ─────────────────────────────────────────────────────────────
    static readonly OnboardingStep[] Steps = {
        new OnboardingStep {
            title = "Welcome to Grudge Warlords",
            body  = "You have entered a living world of factions, professions, and class-driven combat.\n\n" +
                    "Press <b>G</b> at any time to open this menu and track your progression.",
            tabHint = UI_GrudgeMenu.GrudgeTab.Skills
        },
        new OnboardingStep {
            title = "Your Class Tree",
            body  = "At Level 1 you choose your initial class path. Each milestone (Lv 5, 10, 15, 20) " +
                    "unlocks a new tier of class upgrades that shape your skills and playstyle.\n\n" +
                    "Open the <b>Class Tree</b> tab to make your first choice.",
            tabHint = UI_GrudgeMenu.GrudgeTab.ClassTree
        },
        new OnboardingStep {
            title = "Skills & Upgrades",
            body  = "Your class tree grants you skills. Spend <b>Skill XP</b> (earned from combat) to " +
                    "upgrade them. Some skills are locked behind class tree choices — check the Skills tab " +
                    "for the lock icon.\n\n" +
                    "Slots <b>1–4</b> on your hotbar are for skills. Drag from the Skills tab to equip.",
            tabHint = UI_GrudgeMenu.GrudgeTab.Skills
        },
        new OnboardingStep {
            title = "Professions",
            body  = "Grudge has 5 harvesting professions: <b>Harvesting, Mining, Woodcutting, Fishing, Crafting</b>.\n\n" +
                    "Gather resources in the world to gain profession XP and advance through 5 tiers. " +
                    "Higher tiers unlock rarer resources and crafting recipes.",
            tabHint = UI_GrudgeMenu.GrudgeTab.Professions
        },
        new OnboardingStep {
            title = "Combat Hotbar Layout",
            body  = "Your hotbar follows the Grudge layout:\n" +
                    "  <b>1–4</b>  Active skills\n" +
                    "  <b>5</b>    Empty (reserved)\n" +
                    "  <b>6–8</b>  Consumables (food, potions, relics)\n\n" +
                    "Drag items from your inventory directly onto slots 6–8.",
            tabHint = UI_GrudgeMenu.GrudgeTab.Skills
        },
        new OnboardingStep {
            title = "You're Ready!",
            body  = "Explore the world, build your class path, level your professions, and fight for " +
                    "your faction.\n\n" +
                    "Good luck, Warlord. The Grudge awaits.",
            tabHint = UI_GrudgeMenu.GrudgeTab.Character
        }
    };

    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Step Display")]
    public Text      stepTitleText;
    public Text      stepBodyText;
    public Text      stepCounterText;
    public Image     progressBar;         // fillAmount
    public Button    prevButton;
    public Button    nextButton;
    public Button    dismissButton;
    public Text      nextButtonText;      // changes "Next" → "Done" on last step

    [Header("Deep-link Buttons (optional)")]
    public Button    openTabButton;       // opens the hint tab for the current step
    public Text      openTabButtonLabel;

    [Header("References")]
    public UI_GrudgeMenu grudgeMenu;

    // ── State ─────────────────────────────────────────────────────────────────
    int    _step = 0;
    bool   _dismissed = false;
    string _prefsKey  = "";

    const string PREFS_PREFIX = "GrudgeOnboarding_dismissed_";

    // ── Unity ─────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (prevButton    != null) prevButton.onClick.AddListener(PrevStep);
        if (nextButton    != null) nextButton.onClick.AddListener(NextStep);
        if (dismissButton != null) dismissButton.onClick.AddListener(Dismiss);
        if (openTabButton != null) openTabButton.onClick.AddListener(OpenHintTab);
    }

    void OnEnable()
    {
        // Re-check dismissal state when the tab becomes visible
        Player player = Player.localPlayer;
        if (player != null) _prefsKey = PREFS_PREFIX + player.name;
        _dismissed = !string.IsNullOrEmpty(_prefsKey) && PlayerPrefs.GetInt(_prefsKey, 0) == 1;
        ShowStep(_step);
    }

    // ── Public ────────────────────────────────────────────────────────────────
    /// <summary>Returns true if the player has dismissed the onboarding guide.</summary>
    public bool IsDismissed(Player player)
    {
        if (player == null) return true;
        return PlayerPrefs.GetInt(PREFS_PREFIX + player.name, 0) == 1;
    }

    // ── Step navigation ───────────────────────────────────────────────────────
    void PrevStep()
    {
        if (_step > 0) ShowStep(_step - 1);
    }

    void NextStep()
    {
        if (_step < Steps.Length - 1)
            ShowStep(_step + 1);
        else
            Dismiss();
    }

    void Dismiss()
    {
        _dismissed = true;
        if (!string.IsNullOrEmpty(_prefsKey))
        {
            PlayerPrefs.SetInt(_prefsKey, 1);
            PlayerPrefs.Save();
        }
        // Close the onboarding tab content (parent panel hiding is handled by hub)
        gameObject.SetActive(false);
    }

    void OpenHintTab()
    {
        if (grudgeMenu != null && _step < Steps.Length)
            grudgeMenu.OpenTab(Steps[_step].tabHint);
    }

    void ShowStep(int index)
    {
        _step = Mathf.Clamp(index, 0, Steps.Length - 1);
        OnboardingStep s = Steps[_step];

        if (stepTitleText   != null) stepTitleText.text   = s.title;
        if (stepBodyText    != null) stepBodyText.text    = s.body;
        if (stepCounterText != null) stepCounterText.text = $"Step {_step + 1} of {Steps.Length}";
        if (progressBar     != null) progressBar.fillAmount = (_step + 1f) / Steps.Length;

        // Button states
        if (prevButton != null) prevButton.gameObject.SetActive(_step > 0);
        if (nextButtonText != null)
            nextButtonText.text = (_step == Steps.Length - 1) ? "Done" : "Next";

        // Deep-link button label
        if (openTabButton != null)
        {
            openTabButton.gameObject.SetActive(grudgeMenu != null);
            if (openTabButtonLabel != null)
                openTabButtonLabel.text = $"Open {s.tabHint} tab →";
        }
    }

    // ── Helper struct ─────────────────────────────────────────────────────────
    struct OnboardingStep
    {
        public string title;
        public string body;
        public UI_GrudgeMenu.GrudgeTab tabHint;
    }
}
