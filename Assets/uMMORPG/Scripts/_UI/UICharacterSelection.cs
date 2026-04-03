// Simple character selection list. The character prefabs are known, so we could
// easily show 3D models, stats, etc. too.
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public partial class UICharacterSelection : MonoBehaviour
{
    public UICharacterCreation uiCharacterCreation;
    public UIConfirmation uiConfirmation;
    public NetworkManagerMMO manager;
    public GameObject panel;
    public Button startButton;
    public Button deleteButton;
    public Button createButton;
    public Button quitButton;

    // ── Friend / Direct Connect ────────────────────────────────────────────
    [Header("Join Friend / Custom Server")]
    [Tooltip("Input field for a friend's server IP or code. Optional — leave unassigned to hide.")]
    public InputField friendServerInput;
    [Tooltip("Button to connect to the address in friendServerInput.")]
    public Button joinFriendButton;
    [Tooltip("Label shown above the join-friend section.")]
    public Text joinFriendLabel;

    private bool listenersRegistered = false;

    // Returns true if either creation panel (standard or extended addon) is open
    private bool IsAnyCreationVisible()
    {
        if (uiCharacterCreation != null && uiCharacterCreation.IsVisible()) return true;
        // uiCharacterCreationExtented is declared in the CharacterCreation addon partial class
        if (uiCharacterCreationExtented != null && uiCharacterCreationExtented.IsVisible()) return true;
        return false;
    }

    void OnEnable()
    {
        if (listenersRegistered) return;
        listenersRegistered = true;

        if (startButton  != null) startButton.onClick.AddListener(OnStartClick);
        if (deleteButton != null) deleteButton.onClick.AddListener(OnDeleteClick);
        if (createButton != null) createButton.onClick.AddListener(OnCreateClick);
        if (quitButton   != null) quitButton.onClick.AddListener(() => NetworkManagerMMO.Quit());

        // Extended create button (added by CharacterCreation addon partial class)
        if (createButtonExtend != null)
            createButtonExtend.onClick.AddListener(OnCreateClick);

        if (joinFriendButton != null)
            joinFriendButton.onClick.AddListener(OnJoinFriendClick);
    }

    void Update()
    {
        // Show while in lobby and not creating a character
        if (manager.state == NetworkState.Lobby && !IsAnyCreationVisible())
        {
            panel.SetActive(true);

            if (manager.charactersAvailableMsg.characters != null)
            {
                NetworkManagerMMO.CharactersAvailableMsg.CharacterPreview[] characters =
                    manager.charactersAvailableMsg.characters;

                bool hasSelection = manager.selection != -1;

                if (startButton  != null) startButton.gameObject.SetActive(hasSelection);
                if (deleteButton != null) deleteButton.gameObject.SetActive(hasSelection);
                if (createButton != null) createButton.interactable = characters.Length < manager.characterLimit;
                if (createButtonExtend != null) createButtonExtend.interactable = characters.Length < manager.characterLimit;

                // Friend join section visibility
                if (friendServerInput != null)
                {
                    bool hasText = friendServerInput.text.Length > 0;
                    if (joinFriendButton != null) joinFriendButton.interactable = hasText;
                    if (joinFriendLabel  != null) joinFriendLabel.gameObject.SetActive(true);
                }
            }
        }
        else panel.SetActive(false);
    }

    // ── Button Handlers ───────────────────────────────────────────────────

    private void OnStartClick()
    {
        NetworkClient.Ready();
        NetworkClient.Send(new CharacterSelectMsg { index = manager.selection });
        manager.ClearPreviews();
        panel.SetActive(false);
    }

    private void OnDeleteClick()
    {
        if (manager.charactersAvailableMsg.characters == null) return;
        if (manager.selection < 0 || manager.selection >= manager.charactersAvailableMsg.characters.Length) return;
        string charName = manager.charactersAvailableMsg.characters[manager.selection].name;
        uiConfirmation.Show(
            "Do you really want to delete <b>" + charName + "</b>?",
            () => NetworkClient.Send(new CharacterDeleteMsg { index = manager.selection })
        );
    }

    private void OnCreateClick()
    {
        panel.SetActive(false);

        // Prefer the extended CharacterCreation addon panel if assigned
        if (uiCharacterCreationExtented != null)
            uiCharacterCreationExtented.Show();
        else if (uiCharacterCreation != null)
            uiCharacterCreation.Show();
        else
            Debug.LogWarning("[UICharacterSelection] No character creation panel assigned!");
    }

    private void OnJoinFriendClick()
    {
        if (friendServerInput == null || string.IsNullOrWhiteSpace(friendServerInput.text)) return;
        manager.networkAddress = friendServerInput.text.Trim();
        WebGLWssBootstrap.ApplyRuntimeOverrides(manager);
        manager.StartClient();
    }
}


