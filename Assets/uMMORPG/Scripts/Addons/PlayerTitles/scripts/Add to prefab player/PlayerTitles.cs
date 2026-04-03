using TMPro;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public  class PlayerTitles : NetworkBehaviour
{
    [Header("Components")]
    public Player player;

    public TMP_Text titleOverlay;
    public Tmpl_TitlesConfiguration titlesConfiguration;


    //[Header("[Events]")]
    [HideInInspector] public UnityEvent onTitleLearnedChanged;

    [SyncVar(hook = nameof(ChangeCurrentTitle)), HideInInspector] public string currentTitle;
    [SyncVar(hook = nameof(ChangeDysplayTitle)), HideInInspector] public string displayTitle;

    [HideInInspector] public bool noTitle = true;
    // a dictionary to store all earned titles
    public readonly SyncList<Titles> EarnedTitles = new SyncList<Titles>();

    void OnTitleLearnedChanged(SyncList<Titles>.Operation op, int index, Titles oldData, Titles newData)
    {
        //Debug.Log("EarnedTitles Changed update required");
        titlesConfiguration.ui_titleUpdate.TriggerEvent();
        onTitleLearnedChanged.Invoke();
    }
    public void Start()
    {
        if (!isServer && !isClient) return;
        EarnedTitles.Callback += OnTitleLearnedChanged;
    }


    [Client]
    private void ChangeCurrentTitle(string oldValue, string newValue)
    {
        CmdUseTitle(currentTitle);
    }

    //[ClientRpc]
    private void ChangeDysplayTitle(string oldValue, string newValue)
    {
        //Debug.Log("currentTitle : " + currentTitle);
        //Debug.Log("displayTitle : " + displayTitle);
        if (noTitle)
        {
            titleOverlay.gameObject.SetActive(false);
            titleOverlay.text = "";
        }
        if (displayTitle != null)
        {
            titleOverlay.gameObject.SetActive(true);
            titleOverlay.color = titlesConfiguration.color;
            titleOverlay.text = !string.IsNullOrWhiteSpace(displayTitle) ? titlesConfiguration.overlayPrefix + displayTitle + titlesConfiguration.overlaySuffix : "";
        }
    }

    public override void OnStartLocalPlayer()
    {
        // load the players chosen title on start up
        Cmd_LoadCurrentTite();
    }


    /**
     * 
     */
    [Command]
    public void Cmd_LoadCurrentTite()
    {
#if _SERVER
        Database.singleton.LoadChosenTitle(player);
#endif
    }


    /**
     * Disable Title
     */

    [Command]
    public void CmdDisableTitle()
    {
#if _SERVER
        RpcDisableTitle();
#endif
    }

    [ClientRpc]
    public void RpcDisableTitle()
    {
        noTitle = true;
        currentTitle = null;
        displayTitle = null;
        titleOverlay.gameObject.SetActive(false);
        titleOverlay.text = "";
    }

    /**
     * Use title
     */
    [Command]
    public void CmdUseTitle(string titleIndex)
    {
#if _SERVER
        RpcUseTitle(titleIndex);
#endif
    }

    [ClientRpc]
    public void RpcUseTitle(string titleIndex)
    {
        // set no title to false (for entity update)
        noTitle = false;
        currentTitle = titleIndex;

        foreach (Tmpl_Titles title in titlesConfiguration.titles)
        {
            if (title.name == titleIndex)
            {
                //player.playerTitles.EarnedTitles[titleIndex].isDisplay = true;
                displayTitle = title.displayName;
                //Debug.Log("lol..." + displayTitle);
                break;
            }
        }
    }



#if _SERVER


    [Server]
    public void ChangeTitle(string titleIndex)
    {
        RpcUseTitle(titleIndex);
    }

    [Server]
    public void EarnTitle(Tmpl_Titles Title)
    {
        //bool saveRequired = false;
        foreach (Tmpl_Titles title in titlesConfiguration.titles)
        {
            if (IsEarned(Title) && title == Title)
            {
                // create a new title to add to players title list
                Titles newTitle = new Titles()
                {
                    // set the new titles info from titles list
                    title = title.name,
                    displayTitle = title.displayName,
                    isLearned = true
                };
                //newTitle.isPrefix = title.isPrefix;
                // add the title and its info into players title dictionary
                player.playerTitles.EarnedTitles.Add(newTitle);

                // if we have set the notify prefab then show, otherwise skip
                player.Target_Tools_ShowPopup(connectionToClient, titlesConfiguration.earnTitleUpNotice.message + title.displayName + titlesConfiguration.earnTitleUpNotice.suffix);
                //saveRequired = true;
            }
        }
    }

    [Server]
    public bool IsEarned(Tmpl_Titles Title)
    {
        int idx = player.playerTitles.EarnedTitles.FindIndex(x => x.title == Title.name);
        return (idx < 0);
    }
#endif

}