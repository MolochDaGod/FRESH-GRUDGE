using System.Collections;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UI_SettingsKeybind : MonoBehaviour
{
    public UI_Settings ui_Settings;
    public Button[] skillBarsButtons;

    public Button party;
    public Button skill;
    public Button quest;
    public Button guild;
    public Button characterInfo;
    public Button inventory;
    public Button itemMall;
    public Button crafting;
    public Button equipment;

    [HideInInspector] public bool waitingForKey = false;
    [HideInInspector] public GameObject currentButton;
    [HideInInspector] public KeyCode currentKey = KeyCode.W;
#if _CLIENT
    private void Start()
    {
        if (ui_Settings != null)
        {
            party.onClick.AddListener(() => StartKeybinding(UIType.Party));
            skill.onClick.AddListener(() => StartKeybinding(UIType.Skills));
            quest.onClick.AddListener(() => StartKeybinding(UIType.Quest));
            guild.onClick.AddListener(() => StartKeybinding(UIType.Guild));
            characterInfo.onClick.AddListener(() => StartKeybinding(UIType.CharacterInfo));
            inventory.onClick.AddListener(() => StartKeybinding(UIType.Inventory));
            itemMall.onClick.AddListener(() => StartKeybinding(UIType.ItemMall));
            crafting.onClick.AddListener(() => StartKeybinding(UIType.Crafting));
            equipment.onClick.AddListener(() => StartKeybinding(UIType.Equipment));

            for (int i = 0; i < skillBarsButtons.Length; i++)
            {
                int index = i; // Local copy for the closure
                skillBarsButtons[i].onClick.AddListener(() => StartSkillBarButton(index));
            }

            LoadKeybindings();
        }
    }

    private void StartSkillBarButton(int index)
    {
        if (!waitingForKey)
            StartCoroutine(AssignKeyToSkillBar(index));
    }

    private void OnEnable()
    {
        if (ui_Settings != null)
        {
            party.GetComponentInChildren<TMP_Text>().text = ui_Settings.settingsBinding.GetKeyCodeForUIType(UIType.Party).ToString();
            skill.GetComponentInChildren<TMP_Text>().text = ui_Settings.settingsBinding.GetKeyCodeForUIType(UIType.Skills).ToString();
            quest.GetComponentInChildren<TMP_Text>().text = ui_Settings.settingsBinding.GetKeyCodeForUIType(UIType.Quest).ToString();
            guild.GetComponentInChildren<TMP_Text>().text = ui_Settings.settingsBinding.GetKeyCodeForUIType(UIType.Guild).ToString();
            characterInfo.GetComponentInChildren<TMP_Text>().text = ui_Settings.settingsBinding.GetKeyCodeForUIType(UIType.CharacterInfo).ToString();
            inventory.GetComponentInChildren<TMP_Text>().text = ui_Settings.settingsBinding.GetKeyCodeForUIType(UIType.Inventory).ToString();
            itemMall.GetComponentInChildren<TMP_Text>().text = ui_Settings.settingsBinding.GetKeyCodeForUIType(UIType.ItemMall).ToString();
            crafting.GetComponentInChildren<TMP_Text>().text = ui_Settings.settingsBinding.GetKeyCodeForUIType(UIType.Crafting).ToString();
            equipment.GetComponentInChildren<TMP_Text>().text = ui_Settings.settingsBinding.GetKeyCodeForUIType(UIType.Equipment).ToString();

            // Load skillBar keybindings
            for (int i = 0; i < skillBarsButtons.Length; i++)
            {
                KeyCode key = LoadKeyCodeForSkillBar(i);
                skillBarsButtons[i].GetComponentInChildren<TMP_Text>().text = key.ToString();
            }
        }
    }

    public void StartKeybinding(UIType keyIndex)
    {
        if (!waitingForKey)
            StartCoroutine(AssignKey(keyIndex));
    }

    public void LoadKeybindings()
    {
        if (ui_Settings != null)
        {
            if (ui_Settings.uiParty != null) ui_Settings.uiParty.SetHotkey(ui_Settings.settingsBinding.GetKeyCodeForUIType(UIType.Party));
            if (ui_Settings.uiSkills != null) ui_Settings.uiSkills.SetHotkey(ui_Settings.settingsBinding.GetKeyCodeForUIType(UIType.Skills));
            if (ui_Settings.uiQuests != null) ui_Settings.uiQuests.SetHotkey(ui_Settings.settingsBinding.GetKeyCodeForUIType(UIType.Quest));
            if (ui_Settings.uiGuild != null) ui_Settings.uiGuild.SetHotkey(ui_Settings.settingsBinding.GetKeyCodeForUIType(UIType.Guild));
            if (ui_Settings.uiCharacterInfo != null) ui_Settings.uiCharacterInfo.SetHotkey(ui_Settings.settingsBinding.GetKeyCodeForUIType(UIType.CharacterInfo));
            if (ui_Settings.uiInventory != null) ui_Settings.uiInventory.SetHotkey(ui_Settings.settingsBinding.GetKeyCodeForUIType(UIType.Inventory));
            if (ui_Settings.uiItemMall != null) ui_Settings.uiItemMall.SetHotkey(ui_Settings.settingsBinding.GetKeyCodeForUIType(UIType.ItemMall));
            if (ui_Settings.uiCrafting != null) ui_Settings.uiCrafting.SetHotkey(ui_Settings.settingsBinding.GetKeyCodeForUIType(UIType.Crafting));
            if (ui_Settings.uiEquipment != null) ui_Settings.uiEquipment.SetHotkey(ui_Settings.settingsBinding.GetKeyCodeForUIType(UIType.Equipment));

            // Load skillBar keybindings
            for (int i = 0; i < skillBarsButtons.Length; i++)
            {
                KeyCode key = LoadKeyCodeForSkillBar(i);
                skillBarsButtons[i].GetComponentInChildren<TMP_Text>().text = key.ToString();
            }
        }
    }

    // Waits for a keybinding to be hit then assigns it.
    public IEnumerator AssignKey(UIType keyIndex)
    {
        waitingForKey = true;
        currentButton = EventSystem.current.currentSelectedGameObject;

        yield return WaitForKey(); // Executes endlessly until user presses a key

        foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
            if (Input.GetKey(kcode))
                currentKey = kcode;

        ui_Settings.settingsBinding.SaveKeyCodeForUIType(keyIndex, currentKey);

        currentButton.GetComponentInChildren<TMP_Text>().text = currentKey.ToString();

        waitingForKey = false;
        LoadKeybindings();
        yield return null;
    }

    // Assign key to skill bar
    public IEnumerator AssignKeyToSkillBar(int index)
    {
        Player player = Player.localPlayer;                         //Grab the player from utils.
        if (player == null) yield return null;                      //Don't continue if there is no player found.

        waitingForKey = true;
        currentButton = EventSystem.current.currentSelectedGameObject;

        yield return WaitForKey(); // Executes endlessly until user presses a key

        foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
            if (Input.GetKey(kcode))
                currentKey = kcode;

        PlayerPrefs.SetInt($"SkillBar_{index}", (int)currentKey);
        skillBarsButtons[index].GetComponentInChildren<TMP_Text>().text = currentKey.ToString();

        waitingForKey = false;
        LoadKeybindings();
        player.playerAddonsConfigurator.SetSkillbar_Hotkeys();
        yield return null;
    }

    // Waits for a key to be pressed.
    private IEnumerator WaitForKey()
    {
        while (!Input.anyKeyDown)
            yield return null;
    }

    // Load the keycode for a skill bar button
    private KeyCode LoadKeyCodeForSkillBar(int index)
    {
        string key = $"SkillBar_{index}";
        if (PlayerPrefs.HasKey(key))
        {
            return (KeyCode)PlayerPrefs.GetInt(key);
        }
        else
        {
            // Default keycodes (e.g., Alpha1, Alpha2, etc.)
            return ui_Settings.settingsBinding.skillBarBinding[index].keyCode;
        }
    }
#endif
}
