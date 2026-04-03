// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public partial class UILogin : MonoBehaviour
{
    public UIPopup uiPopup;
    public NetworkManagerMMO manager;
    public NetworkAuthenticatorMMO auth;
    public GameObject panel;
    public Text statusText;
    public InputField accountInput;
    public InputField passwordInput;
    public Dropdown serverDropdown;
    public Button loginButton;
    public Button registerButton;
    [TextArea(1, 30)] public string registerMessage = "First time? Just log in and we will\ncreate an account automatically.";
    public Button hostButton;
    public Button dedicatedButton;
    public Button cancelButton;
    public Button quitButton;

    // ── Friend / Direct Connect ────────────────────────────────────────────
    [Header("Direct Connect (Friend / Custom Server)")]
    [Tooltip("Optional input field for typing a custom server IP or friend code.\nLeave unassigned to hide this feature.")]
    public InputField directConnectInput;
    [Tooltip("Button to connect to the address in directConnectInput.")]
    public Button directConnectButton;

    // Track last known server list to avoid rebuilding dropdown every frame
    private int lastServerListCount = -1;
    private bool listenersRegistered = false;

    void Start()
    {
        // Restore last used server from PlayerPrefs
        if (PlayerPrefs.HasKey("LastServer"))
        {
            string last = PlayerPrefs.GetString("LastServer", "");
            int idx = manager.serverList.FindIndex(s => s.name == last);
            if (idx >= 0) serverDropdown.value = idx;
        }

        // Restore last saved credentials for convenience (never save password)
        if (PlayerPrefs.HasKey("LastAccount"))
            accountInput.text = PlayerPrefs.GetString("LastAccount", "");
    }

    void OnEnable()
    {
        if (listenersRegistered) return;
        listenersRegistered = true;

        registerButton.onClick.AddListener(OnRegisterClick);
        loginButton.onClick.AddListener(OnLoginClick);
        hostButton.onClick.AddListener(OnHostClick);
        cancelButton.onClick.AddListener(OnCancelClick);
        dedicatedButton.onClick.AddListener(OnDedicatedClick);
        quitButton.onClick.AddListener(() => NetworkManagerMMO.Quit());
        serverDropdown.onValueChanged.AddListener(OnServerDropdownChanged);

        if (directConnectButton != null)
            directConnectButton.onClick.AddListener(OnDirectConnectClick);

        // Sync credentials once on enable
        SyncCredentials();
    }

    void OnDisable()
    {
        // Save on hide so we don't lose the account name
        PlayerPrefs.SetString("LastServer", serverDropdown.captionText.text);
        PlayerPrefs.SetString("LastAccount", accountInput.text);
    }

    void OnDestroy()
    {
        PlayerPrefs.SetString("LastServer", serverDropdown.captionText.text);
        PlayerPrefs.SetString("LastAccount", accountInput.text);
    }

    void Update()
    {
#if _iMMOMAINMENU
        if (manager.changingCharacters) { panel.SetActive(false); return; }
#endif
        bool show = manager.state == NetworkState.Offline || manager.state == NetworkState.Handshake;
        panel.SetActive(show);
        if (!show) return;

        // Status text — only update when it changes
        string newStatus = NetworkClient.isConnecting ? "Connecting..."
                         : manager.state == NetworkState.Handshake ? "Handshake..."
                         : "";
        if (statusText.text != newStatus) statusText.text = newStatus;

        bool networkIdle = !manager.isNetworkActive;
        bool validAccount = auth.IsAllowedAccountName(accountInput.text);

        loginButton.interactable    = networkIdle && validAccount;
        hostButton.interactable     = networkIdle && validAccount && Application.platform != RuntimePlatform.WebGLPlayer;
        dedicatedButton.interactable = networkIdle && Application.platform != RuntimePlatform.WebGLPlayer;
        registerButton.interactable = networkIdle;
        cancelButton.gameObject.SetActive(NetworkClient.isConnecting);

        if (directConnectButton != null)
            directConnectButton.interactable = networkIdle && directConnectInput != null
                                               && directConnectInput.text.Length > 0;

        // Rebuild server dropdown only when list changes
        if (manager.serverList.Count != lastServerListCount)
        {
            lastServerListCount = manager.serverList.Count;
            serverDropdown.options = manager.serverList
                .Select(sv => new Dropdown.OptionData(sv.name)).ToList();
            serverDropdown.RefreshShownValue();
        }

        serverDropdown.interactable = networkIdle;

        // Keep networkAddress in sync with current selection (safe bounds check)
        int sel = Mathf.Clamp(serverDropdown.value, 0, manager.serverList.Count - 1);
        if (manager.serverList.Count > 0)
            manager.networkAddress = manager.serverList[sel].ip;

        // Keep credentials in sync (only when input fields change, via listeners, but
        // also ensure they are set before connecting in case Update runs first)
        SyncCredentials();
    }

    // ── Button Handlers ────────────────────────────────────────────────────

    private void OnRegisterClick() => uiPopup.Show(registerMessage);

    private void OnLoginClick()
    {
        SyncCredentials();
        WebGLWssBootstrap.ApplyRuntimeOverrides(manager);
        manager.StartClient();
    }

    private void OnHostClick()
    {
        SyncCredentials();
        manager.StartHost();
    }

    private void OnCancelClick() => manager.StopClient();

    private void OnDedicatedClick() => manager.StartServer();

    private void OnServerDropdownChanged(int index)
    {
        if (manager.serverList.Count > 0)
        {
            int safe = Mathf.Clamp(index, 0, manager.serverList.Count - 1);
            manager.networkAddress = manager.serverList[safe].ip;
        }
    }

    private void OnDirectConnectClick()
    {
        if (directConnectInput == null || string.IsNullOrWhiteSpace(directConnectInput.text)) return;
        SyncCredentials();
        manager.networkAddress = directConnectInput.text.Trim();
        WebGLWssBootstrap.ApplyRuntimeOverrides(manager);
        manager.StartClient();
    }

    private void SyncCredentials()
    {
        auth.loginAccount  = accountInput.text;
        auth.loginPassword = passwordInput.text;
    }
}

