// Simple character selection list (V2 - slot-based).
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections.Generic;

public partial class UI_CharacterSelectionV2 : MonoBehaviour
{
    [Header("Assign this variable")]
    [Space(20)]
    public UI_CharacterCreation uiCharacterCreation;
    public UIConfirmation uiConfirmation;
    public NetworkManagerMMO manager;
    public Transform mycamera;

    [Header("[-=-[ Default Configuration ]-=-]")]
    [SerializeField] bool showConfiguration;
    [BoolShowConditional(conditionFieldName: "showConfiguration", conditionValue: true)]
    public UI_CharacterSlotV2 slotCharacter;
    [BoolShowConditional(conditionFieldName: "showConfiguration", conditionValue: true)]
    public Transform content;
    [BoolShowConditional(conditionFieldName: "showConfiguration", conditionValue: true)]
    public GameObject panel;
    [BoolShowConditional(conditionFieldName: "showConfiguration", conditionValue: true)]
    public Button startButton;
    [BoolShowConditional(conditionFieldName: "showConfiguration", conditionValue: true)]
    public Button deleteButton;
    [BoolShowConditional(conditionFieldName: "showConfiguration", conditionValue: true)]
    public Button createButton;
    [BoolShowConditional(conditionFieldName: "showConfiguration", conditionValue: true)]
    public Button quitButton;

    [BoolShowConditional(conditionFieldName: "showConfiguration", conditionValue: true)]
    public bool useSelectionAnimation = false;
    [BoolShowConditional(conditionFieldName: "showConfiguration", conditionValue: true)]
    public string boolNameAnimation;
    [HideInInspector] public int curid = 0;

    private NetworkManagerMMO.CharactersAvailableMsg.CharacterPreview[] characters;
    private bool listenersRegistered = false;

    void OnEnable()
    {
        if (listenersRegistered) return;
        listenersRegistered = true;

        if (startButton  != null) startButton.onClick.AddListener(OnStartClick);
        if (deleteButton != null) deleteButton.onClick.AddListener(OnDeleteClick);
        if (createButton != null) createButton.onClick.AddListener(OnCreateClick);
        if (quitButton   != null) quitButton.onClick.AddListener(() => NetworkManagerMMO.Quit());
    }

    void Update()
    {
        if (manager.state == NetworkState.Lobby && !uiCharacterCreation.IsVisible())
        {
            panel.SetActive(true);

            if (manager.charactersAvailableMsg.characters != null)
            {
                characters = manager.charactersAvailableMsg.characters;

                UIUtils.BalancePrefabs(slotCharacter.gameObject, characters.Length, content);
                manager.selection = (manager.selection >= 0 && characters.Length >= 1) ? manager.selection : 0;

                for (int c = 0; c < characters.Length; c++)
                {
                    int currentID = c;
                    UI_CharacterSlotV2 slot = content.GetChild(currentID).GetComponent<UI_CharacterSlotV2>();

                    slot.characterName.text  = characters[currentID].name;
                    slot.characterLevel.text = characters[currentID].level.ToString();
                    slot.characterClasse.text = characters[currentID].className;

                    slot.button.interactable = (currentID != manager.selection);
                    slot.button.onClick.SetListener(() => { manager.selection = currentID; });

                    manager.selectionLocations[currentID].gameObject.SetActive(manager.selection == currentID);

                    Player plyer = manager.selectionLocations[currentID].gameObject.GetComponentInChildren<Player>();
                    if (useSelectionAnimation && boolNameAnimation != "")
                        if (plyer != null) plyer.animator.SetBool(boolNameAnimation, true);

                    if (plyer != null) plyer.nameOverlay.gameObject.SetActive(false);
                    if (plyer && plyer.portraitIcon != null) slot.characterImage.sprite = plyer.portraitIcon;

                    curid = manager.selection;
                }

                bool hasSelection = manager.selection != -1;
                if (startButton  != null) startButton.gameObject.SetActive(hasSelection);
                if (deleteButton != null) deleteButton.gameObject.SetActive(hasSelection);
                if (createButton != null) createButton.interactable = characters.Length < manager.characterLimit;
            }

#if _iMMOSCENELOADER
            if (SceneLoaderAsync.Instance.LoadingProgress == 100 && SceneLoaderAsync.Instance.LoadedScene)
            {
                manager.ClearPreviews();
                panel.SetActive(false);
            }
#endif
        }
        else panel.SetActive(false);
    }

    // ── Button Handlers ───────────────────────────────────────────────────

    private void OnStartClick()
    {
        NetworkClient.Ready();
#if _iMMOLOBBY
        Tools_UI_Tools.FadeOutScreen(false);
#endif
        if (characters != null && manager.selection >= 0 && manager.selection < characters.Length)
            NetworkClient.Send(new CharacterSelectMsg { index = manager.selection });

#if _iMMOSCENELOADER
        SceneLoaderAsync.Instance.LoadScene(characters[manager.selection].startingScene);
#else
        manager.ClearPreviews();
        panel.SetActive(false);
#endif
    }

    private void OnDeleteClick()
    {
        if (characters == null || manager.selection < 0 || manager.selection >= characters.Length) return;
        string charName = characters[manager.selection].name;
        uiConfirmation.Show(
            "Do you really want to delete <b>" + charName + "</b>?",
            () => NetworkClient.Send(new CharacterDeleteMsg { index = manager.selection })
        );
    }

    private void OnCreateClick()
    {
        if (uiCharacterCreation == null)
        {
            Debug.LogWarning("[UI_CharacterSelectionV2] uiCharacterCreation is not assigned in the Inspector!");
            return;
        }
        panel.SetActive(false);
        uiCharacterCreation.Show();
    }

    public void MoveCameraToPlayer(Vector3 playerpos)
    {
        mycamera.position = playerpos - new Vector3(-2, -2, 0);
    }
}

