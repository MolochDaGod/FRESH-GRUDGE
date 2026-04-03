using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections.Generic;

public partial class UI_CharacterSelection : MonoBehaviour
{
    public UI_CharacterCreation uiCharacterCreation;
    public UIConfirmation uiConfirmation;
    public NetworkManagerMMO manager;
    public GameObject panel;
    public Button startButton;
    public Button deleteButton;
    public Button createButton;
    public Button quitButton;
    public new string name;

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
        if (manager.state == NetworkState.Lobby && (uiCharacterCreation == null || !uiCharacterCreation.IsVisible()))
        {
            panel.SetActive(true);

            if (manager.charactersAvailableMsg.characters != null)
            {
                characters = manager.charactersAvailableMsg.characters;

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
            Debug.LogWarning("[UI_CharacterSelection] uiCharacterCreation is not assigned in the Inspector!");
            return;
        }
        panel.SetActive(false);
        uiCharacterCreation.Show();
    }
}


