// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections;

public partial class UI_Login : MonoBehaviour
{
    public UIPopup uiPopup;
    public NetworkManagerMMO manager; // singleton=null in Start/Awake
    public NetworkAuthenticatorMMO auth;
    public GameObject panel;
    public Text statusText;
    public InputField accountInput;
    public InputField passwordInput;
    public Dropdown serverDropdown;
    public Button loginButton;
    public Button registerButton;
    [TextArea(1, 30)] public string registerMessage = "First TimeLogout? Just log in and we will\ncreate an account automatically.";
    public GameObject panelServerOption;
    public Button hostButton;
    public Button dedicatedButton;
    public Button cancelButton;
    public Button quitButton;

    public Toggle remember;
    public bool savePassword = false;


    public void Start()
    {
        // load last server by name in case order changes some day.
        if (PlayerPrefs.HasKey("LastServer"))
        {
            string last = PlayerPrefs.GetString("LastServer", "");
            if (manager.serverList.Count > 1)
                serverDropdown.value = manager.serverList.FindIndex(s => s.name == last);
            else
                serverDropdown.gameObject.SetActive(false);
        }

        if (remember && remember.isOn && PlayerPrefs.HasKey("Account"))
            accountInput.text = PlayerPrefs.GetString("Account");

        if (remember && savePassword && remember.isOn && PlayerPrefs.HasKey("Password"))
            passwordInput.text = PlayerPrefs.GetString("Password");
    }

    void OnDestroy()
    {
        // save last server by name in case order changes some day
        PlayerPrefs.SetString("LastServer", serverDropdown.captionText.text);
    }

    void Update()
    {
        // only show while offline
        // AND while in handshake since we don't want to show nothing while
        // trying to login and waiting for the server's response
#if _iMMOMAINMENU
        if (!manager.changingCharacters)
#endif
        if (manager.state == NetworkState.Offline || manager.state == NetworkState.Handshake)
        {
            panel.SetActive(true);

            // status
            if (NetworkClient.isConnecting)
                statusText.text = "Connecting...";
            else if (manager.state == NetworkState.Handshake)
                statusText.text = "Handshake...";
            else
                statusText.text = "";

            // buttons. interactable while network is not active
            // (using IsConnecting is slightly delayed and would allow multiple clicks)
            registerButton.interactable = !manager.isNetworkActive;
            registerButton.onClick.SetListener(() => { uiPopup.Show(registerMessage); });
            
            loginButton.interactable = !manager.isNetworkActive && auth.IsAllowedAccountName(accountInput.text);
            loginButton.onClick.SetListener(() => { SaveChooseAccount(); WebGLWssBootstrap.ApplyRuntimeOverrides(manager); manager.StartClient(); });

#if _SERVER
            hostButton.interactable = Application.platform != RuntimePlatform.WebGLPlayer && !manager.isNetworkActive && auth.IsAllowedAccountName(accountInput.text);
            hostButton.onClick.SetListener(() => {  SaveChooseAccount(); manager.StartHost(); });
                
            dedicatedButton.interactable = Application.platform != RuntimePlatform.WebGLPlayer && !manager.isNetworkActive;
            dedicatedButton.onClick.SetListener(() => { manager.StartServer(); });
#else
                panelServerOption.SetActive(false);
            hostButton.gameObject.SetActive(false);
            dedicatedButton.gameObject.SetActive(false);
#endif

            cancelButton.gameObject.SetActive(NetworkClient.isConnecting);
            cancelButton.onClick.SetListener(() => { manager.StopClient(); });
            quitButton.onClick.SetListener(() => { NetworkManagerMMO.Quit(); });

            // inputs
            auth.loginAccount = accountInput.text;
            auth.loginPassword = passwordInput.text;

                // copy servers to dropdown; copy selected one to networkmanager ip/port.
                if (manager.serverList.Count > 1)
                {
                    serverDropdown.interactable = !manager.isNetworkActive;
                    serverDropdown.options = manager.serverList.Select(
                        sv => new Dropdown.OptionData(sv.name)
                    ).ToList();

                    manager.networkAddress = manager.serverList[serverDropdown.value].ip;
                }
                else
                {
                    manager.networkAddress = manager.serverList[0].ip;
                }
        }
        else panel.SetActive(false);
    }


    // -----------------------------------------------------------------------------------
    // Save Account
    // Save our account name when login is clicked.
    // -----------------------------------------------------------------------------------
    public void SaveChooseAccount()
    {
        PlayerPrefs.SetString("Account", accountInput.text);
        if (savePassword)
            PlayerPrefs.SetString("Password", passwordInput.text);
    }
}
