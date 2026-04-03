using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class UICompleteChat : MonoBehaviour
{
    public static UICompleteChat singleton;
    public GameObject panel;
    public TMP_InputField messageInput;
    public Button sendButton;
    public Transform content;
    public ScrollRect scrollRect;
    public KeyCode[] activationKeys = { KeyCode.Return, KeyCode.KeypadEnter };
    public int keepHistory = 300; // only keep 'n' messages
   
    [Tooltip("If this option is actived chat is alway open, not close chat option")]
    public bool chatAlwayOpen;

    public GameObject showButtonObject;
    public Button closeButton;
    public Text showpanelContent;
    private Button showButton;
    private bool panelVisible;

    [Header("[-=-[ Tabs Toogle Group ]-=-]")]
    public ToggleGroup TabsToggleGroup;
    private string currentTabs = "World";

    [Header("[-=-[ Panel Macro ]-=-]")]
    public GameObject panelMacro;
    private bool panelMacroVisible;

    [Header("[-=-[ Panel Tab Config ]-=-]")]
    public GameObject panelTabConfig;
    private bool panelTabConfigVisible;

    [Header("[-=-[ Panel Comand ]-=-]")]
    public GameObject panelComand;
    private bool panelComandVisible;

    [Header("[-=-[ Panel Emote ]-=-]")]
    public GameObject panelEmote;
    private bool panelEmoteVisible;

    bool eatActivation;
#if _iMMOCOMPLETECHAT
    public UICompleteChat()
    {
        // assign singleton only once (to work with DontDestroyOnLoad when
        // using Zones / switching scenes)
        if (singleton == null) singleton = this;
    }

    private void Start()
    {
        //For first init
        if (!PlayerPrefs.HasKey("SaveSlot_ShowWorld"))
        {
            TabControl a = new TabControl();
            a.SaveTabConfig(true);
        }
        
        if (showButtonObject)
        {
            showButtonObject.SetActive(false);
            showButton = showButtonObject.GetComponent<Button>();
        }
    }

    void Update()
    {
        Player player = Player.localPlayer;
#if _iMMOSETTINGS
        if (player && (!PlayerPrefs.HasKey("ShowChat") || PlayerPrefs.GetInt("ShowChat") ==1))
#else
        if (player)
#endif
        {
            // character limit
            messageInput.characterLimit = player.chat.chatConfiguration.maxLengthMessage;

            // activation (ignored once after deselecting, so it doesn't immediately activate again)
            if (Utils.AnyKeyDown(activationKeys) && !eatActivation)
                messageInput.Select();

            eatActivation = true;

            // end edit listener
            //messageInput.onEndEdit.SetListener((value) =>
            messageInput.onEndEdit.SetListener((value) =>
            {
                // submit key pressed? then submit and set new input text
                if (Utils.AnyKeyDown(activationKeys))
                {
                    eventSendMessage(player);
                    eatActivation = true;
                }
            });

            // send button
            sendButton.onClick.SetListener(() =>
            {
                eventSendMessage(player);
            });


            if (!player || !showButtonObject || !closeButton)
                return;

            //panel.SetActive(panelVisible);
            if (!chatAlwayOpen)
                showButtonObject.SetActive(!panelVisible);
            if (chatAlwayOpen)
            {
                closeButton.gameObject.SetActive(false);
                panel.SetActive(true);
            }

            if (!panel.activeSelf && !showButtonObject.activeSelf)
                panel.SetActive(true);
            update_out();
        }
        else
        {
#if _iMMOSETTINGS
            if (PlayerPrefs.HasKey("ShowChat") && PlayerPrefs.GetInt("ShowChat") == 0)
                showButtonObject.SetActive(false);
            else if(player)
                showButtonObject.SetActive(true);
#endif
            if(!chatAlwayOpen)
                panel.SetActive(false);
        }
        //tabShowOptions();
    }
    
#endif
    /**
     * Add message in chat
     */
    private void eventSendMessage(Player player)
    {
        string prefix = "";
        foreach (Toggle toggle in TabsToggleGroup.ActiveToggles())
        {
            if (toggle.name == "ToggleWorld") prefix = "";
            else if (toggle.name == "ToggleGuild") prefix = "/g ";
            else if (toggle.name == "ToggleGroup") prefix = "/p ";
        }
        // submit and set new input text
        string newinput = player.chat.OnSubmit(((!messageInput.text.StartsWith("/")) ? prefix : "") + messageInput.text);
        messageInput.text = newinput;
        messageInput.MoveTextEnd(false);

        // unfocus the whole chat in any case. otherwise we would scroll or activate the chat window when doing wsad movement afterwards
        // UIUtils.DeselectCarefully();
        messageInput.Select();
    }

    private void showChat()
    {
        if (chatAlwayOpen)
        {
            panel.SetActive(true);
        }
        else
        {
            showButtonObject.SetActive(!panelVisible);
        }
    }

    public void AutoScroll()
    {
        // update first so we don't ignore recently added messages, then scroll
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0;
    }

    public void AddMessage(ChatMessage message)
    {


        // delete old messages so the UI doesn't eat too much performance.
        // => every Destroy call causes a lag because of a UI rebuild
        // => it's best to destroy a lot of messages at once so we don't
        //    experience that lag after every new chat message
        if (content.childCount >= keepHistory)
        {
            for (int i = 0; i < content.childCount / 2; ++i)
                Destroy(content.GetChild(i).gameObject);
        }

        // instantiate and initialize text prefab with parent
        GameObject go = Instantiate(message.textPrefab, content.transform, false);
        //go.GetComponent<TMP_Text>().text = message.Construct();
        //go.GetComponent<UICompleteChatEntry>().message = message;


        TMP_Text chatMessage = go.GetComponent<TMP_Text>(); chatMessage.text = message.Construct();

        UICompleteChatEntry chatEntry = go.GetComponent<UICompleteChatEntry>(); chatEntry.message = message; 
 
        string stringToCheck = message.Construct();
        int start = stringToCheck.IndexOf("[");
        int stop = stringToCheck.IndexOf("]");

        if (stop > -1)
        {
            string output = stringToCheck.Substring(start + 1, stop - start - 1);
            int key = output.GetStableHashCode();

            if (ScriptableItem.All.TryGetValue(key, out var data))
            {
#if _iMMOITEMRARITY
                chatMessage.text = chatMessage.text.Replace(output, "<color=#" + RarityColor.SetratityColorHtml(data.rarity.ToString()) + ">" + data.name + "</color>");
#else
                chatMessage.text = chatMessage.text.Replace(output,  data.name);
#endif
                chatEntry.toolTip.enabled = true;
            }

        }


            AutoScroll();
    }

    private void update_out()
    {
        //GameObject contentShow;
        int lastInsert = 0;
        if (content.childCount > 1)
        {
            lastInsert = content.childCount - 1;
            showpanelContent.text = content.GetChild(lastInsert).GetComponent<TMP_Text>().text;
        }
    }

    // called by chat entries when clicked
    public void OnEntryClicked(UICompleteChatEntry entry)
    {
        // any reply prefix?
        if (!string.IsNullOrWhiteSpace(entry.message.replyPrefix))
        {
            // set text to reply prefix
            messageInput.text = entry.message.replyPrefix;

            // activate
            messageInput.Select();

            // move cursor to end (doesn't work in here, needs small delay)
            Invoke(nameof(MoveTextEnd), 0.1f);
        }
    }

    void MoveTextEnd()
    {
        messageInput.MoveTextEnd(false);
    }


    public void tabShowOptions()
    {
        string loadPref = "ToggleWorld";
        switch (currentTabs)
        {
            case "ToggleWorld": loadPref = "SaveSlot_ShowWorld"; break;
            case "ToggleGuild": loadPref = "SaveSlot_ShowGuild"; break;
            case "ToggleGroup": loadPref = "SaveSlot_ShowGroup"; break;
            case "TogglePM":    loadPref = "SaveSlot_ShowPm"; break;
            case "ToggleInfo":  loadPref = "SaveSlot_ShowInfo"; break;
            case "ToggleCombat":loadPref = "SaveSlot_ShowCombat"; break;
            default: Debug.Log("oh !!");
                break;
        }
       
        string[] option = PlayerPrefs.GetString(loadPref).Split(":"[0]);
        for (int i = 0; i < content.childCount; i++)
        {
            content.GetChild(i).gameObject.SetActive(true);
            if (option[0] == "0" && content.GetChild(i).GetComponent<UICompleteChatEntry>().ToString().StartsWith("SlotChatMessageLocalTMP"))
                content.GetChild(i).gameObject.SetActive(false);
            if (option[1] == "0" && content.GetChild(i).GetComponent<UICompleteChatEntry>().ToString().StartsWith("SlotChatMessageGuildTMP"))
                content.GetChild(i).gameObject.SetActive(false);
            if (option[2] == "0" && content.GetChild(i).GetComponent<UICompleteChatEntry>().ToString().StartsWith("SlotChatMessagePartyTMP"))
                content.GetChild(i).gameObject.SetActive(false);
            if (option[3] == "0" && content.GetChild(i).GetComponent<UICompleteChatEntry>().ToString().StartsWith("SlotChatMessageWhisperTMP"))
                content.GetChild(i).gameObject.SetActive(false);
            if (option[4] == "0" && content.GetChild(i).GetComponent<UICompleteChatEntry>().ToString().StartsWith("SlotChatMessageInfoTMP"))
                content.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void addEmote(string emote)
    {
        string constructEmote = " <sprite=" + emote + "> ";

        messageInput.MoveTextEnd(true);
        messageInput.text = messageInput.text + constructEmote;
        panelEmote.SetActive(false);
    }

    public void shareMyLocation()
    {
        Player player = Player.localPlayer;
        float dirX = player.transform.position.x;
        float dirZ = player.transform.position.z;
        messageInput.text = "[Position X:" + dirX.ToString() + " * Y:" + dirZ.ToString() + "]";
    }


    /**
     * button send message
     */
    public void btnSendMacro(string text)
    {
        // any reply prefix?
        if (!string.IsNullOrWhiteSpace(text))
        {
            Player player = Player.localPlayer;
            // set text to reply prefix
            messageInput.text = text;

            // send message
            eventSendMessage(player); 
            btnShowMacro();
        }
    }

    /**
     * Events button TabOption
     */
    public void btnOpenWorldChatTab()
    {
        currentTabs = "ToggleWorld";
        tabShowOptions();
        messageInput.enabled = true;
        sendButton.enabled = true;
        AutoScroll();
    }
    public void btnOpenGuildChatTab()
    {
        currentTabs = "ToggleGuild";
        Player player = Player.localPlayer;
        tabShowOptions();
        messageInput.enabled = true;
        sendButton.enabled = true;
        messageInput.enabled = (((player.guild.InGuild()) ? true : false));
        sendButton.enabled = (((player.guild.InGuild()) ? true : false));
        AutoScroll();
    }
    public void btnOpenPartyChatTab()
    {
        currentTabs = "ToggleGroup";
        Player player = Player.localPlayer;
        tabShowOptions();
        messageInput.enabled = true;
        sendButton.enabled = true;
        messageInput.enabled = (((player.party.InParty()) ? true : false));
        sendButton.enabled = (((player.party.InParty()) ? true : false));
        AutoScroll();
    }
    public void btnOpenPmChatTab()
    {
        currentTabs = "TogglePM";
        tabShowOptions();
        messageInput.enabled = true;
        sendButton.enabled = true;
        AutoScroll();
    }
    public void btnOpenInfoChatTab()
    {
        currentTabs = "ToggleInfo";
        tabShowOptions();
        messageInput.enabled = false;
        sendButton.enabled = false;
        AutoScroll();
    }
    public void btnOpenCombatChatTab()
    {
        currentTabs = "ToggleCombat";
        tabShowOptions();
        messageInput.enabled = false;
        sendButton.enabled = false;
        AutoScroll();
    }

    /**
     * Events button chat option
     */
    public void btnOpenCloseChat()
    {
        panelVisible = !panelVisible;
        panel.SetActive(panelVisible);
        panelMacro.SetActive(false);
        panelTabConfig.SetActive(false);
        panelComand.SetActive(false);
        panelEmote.SetActive(false);
    }
    public void btnShowEmote()
    {
        panelEmoteVisible = !panelEmoteVisible;
        panelEmote.SetActive(panelEmoteVisible);
        panelMacro.SetActive(false);
        panelTabConfig.SetActive(false);
        panelComand.SetActive(false);
    }
    public void btnShowOption()
    {
        panelTabConfigVisible = ((panelTabConfigVisible) ? false : true);
        panelTabConfig.SetActive(panelTabConfigVisible);
        panelComand.SetActive(false);
        panelEmote.SetActive(false);
        panelMacro.SetActive(false);
    }
    public void btnShowMacro()
    {
        panelMacroVisible = !panelMacroVisible;
        panelMacro.SetActive(panelMacroVisible);
        panelComand.SetActive(false);
        panelEmote.SetActive(false);
        panelTabConfig.SetActive(false);
    }
    public void btnShowComand()
    {
        panelComandVisible = !panelComandVisible;
        panelComand.SetActive(panelComandVisible);
        panelMacro.SetActive(false);
        panelEmote.SetActive(false);
        panelTabConfig.SetActive(false);
    }
}