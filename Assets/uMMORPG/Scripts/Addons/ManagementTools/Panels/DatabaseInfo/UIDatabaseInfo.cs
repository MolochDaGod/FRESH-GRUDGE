using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDatabaseInfo : MonoBehaviour
{
    public static UIDatabaseInfo singleton;
    public Canvas canvas;

    public Button deleteAccountsButton;

    public TMP_Text totalAccountsText;
    public TMP_Text bannedAccountsText;
    public TMP_Text playerAccountsText;
    public TMP_Text staffAccountsText;

    public TMP_Text playersText;
    public TMP_Text maxGoldText;
    public TMP_Text spawnersText;
    public TMP_Text checkpointsText;
    public TMP_Text verifiedBugsText;
    public TMP_Text fixedBugsText;
    public TMP_Text helpRequestsText;
    public TMP_Text monstersText;
    public TMP_Text npcsText;
    public TMP_Text environmentsText;
    public TMP_Text itemsText;

    [HideInInspector] public string[] info = new string[15];

    public GameObject itemsRow;
    //public bool ItemDrop;

    void Awake()
    {
        // initialize singleton
        if (singleton == null) singleton = this;
    }
#if ITEM_DROP_C || ITEM_DROP_R
    void Start()
    {
        if (MToolsSettings.Settings.itemDrop.isActive)
        {
            itemsRow.SetActive(true);
        }
    }
#endif
    // triggered by clicking the info button on the toolbar
    public void PanelActivation()
    {
        if (!canvas.enabled)
        {
            // only the owner can delete accounts
            if (UIManagement.singleton.player.OwnerAccess())
            {
                if (!deleteAccountsButton.interactable)
                {
                    deleteAccountsButton.interactable = true;
                }
            }

            // show info on the basis of data loaded from the database
            UIManagement.singleton.player.CmdShowDatabaseInfo();
        }
        UIManagement.singleton.PanelActivation(canvas);
        AddonManagement.ShowCanvasContent(canvas);
    }

    public void Refresh()
    {
        // refresh info
        UIManagement.singleton.player.CmdShowDatabaseInfo();
    }

    public void SaveWorldState()
    {
        // save the world state
        UIManagement.singleton.player.CmdMsgSave();

        // refresh info
        UIManagement.singleton.player.CmdShowDatabaseInfo();
    }

    public void DeleteAccounts()
    {
        UICommandConfirmation.singleton.Show("Delete all accounts except for access level 3?", () => 
        {
            // delete all accounts with characters and other related tables from the database except for access level 3
            UIManagement.singleton.player.CmdMsgDeleteAccounts();

            // refresh info
            UIManagement.singleton.player.CmdShowDatabaseInfo();
        });
    }

    public void DeleteMonsters()
    {
        UICommandConfirmation.singleton.Show("Delete all monsters?", () =>
        {
            UIManagement.singleton.player.CmdMsgDeleteMonsters();

            // refresh info
            UIManagement.singleton.player.CmdShowDatabaseInfo();
        });
    }

    public void DeleteNpcs()
    {
        UICommandConfirmation.singleton.Show("Delete all npcs?", () =>
        {
            UIManagement.singleton.player.CmdMsgDeleteNpcs();

            // refresh info
            UIManagement.singleton.player.CmdShowDatabaseInfo();
        });
    }

    public void DeleteEnvironments()
    {
        UICommandConfirmation.singleton.Show("Delete all environments?", () =>
        {
            UIManagement.singleton.player.CmdMsgDeleteEnvironments();

            // refresh info
            UIManagement.singleton.player.CmdShowDatabaseInfo();
        });
    }

    public void DeleteSpawners()
    {
        UICommandConfirmation.singleton.Show("Delete all spawners?", () =>
        {
            UIManagement.singleton.player.CmdMsgDeleteSpawners();

            // refresh info
            UIManagement.singleton.player.CmdShowDatabaseInfo();
        });
    }

    public void DeleteCheckpoints()
    {
        UICommandConfirmation.singleton.Show("Delete all checkpoints?", () =>
        {
            UIManagement.singleton.player.CmdMsgDeleteCheckpoints();

            // refresh info
            UIManagement.singleton.player.CmdShowDatabaseInfo();
        });
    }

    public void DeleteReportedBugs()
    {
        UICommandConfirmation.singleton.Show("Delete all reported bugs?", () =>
        {
            UIManagement.singleton.player.CmdMsgDeleteBugReports(0);

            // refresh info
            UIManagement.singleton.player.CmdShowDatabaseInfo();
        });
    }

    public void DeleteVerifiedBugs()
    {
        UICommandConfirmation.singleton.Show("Delete all verified bugs?", () =>
        {
            UIManagement.singleton.player.CmdMsgDeleteBugReports(1);

            // refresh info
            UIManagement.singleton.player.CmdShowDatabaseInfo();
        });
    }

    public void DeleteFixedBugs()
    {
        UICommandConfirmation.singleton.Show("Delete all fixed bugs?", () =>
        {
            UIManagement.singleton.player.CmdMsgDeleteBugReports(2);

            // refresh info
            UIManagement.singleton.player.CmdShowDatabaseInfo();
        });
    }

    public void DeleteHelpRequests()
    {
        UICommandConfirmation.singleton.Show("Delete all help requests?", () =>
        {
            UIManagement.singleton.player.CmdMsgDeleteHelpRequests();

            // refresh info
            UIManagement.singleton.player.CmdShowDatabaseInfo();
        });
    }

    // Item Drop
    public void DeleteItems()
    {
#if ITEM_DROP_C || ITEM_DROP_R
        UICommandConfirmation.singleton.Show("Delete all items?", () =>
        {
            UIManagement.singleton.player.CmdMsgDeleteItems(); 

            // refresh info
            UIManagement.singleton.player.CmdShowDatabaseInfo();
        });
#endif
    }

    public void RefreshInfo()
    {
        totalAccountsText.text = info[0];
        bannedAccountsText.text = info[1];
        playerAccountsText.text = info[2];
        staffAccountsText.text = info[3];

        playersText.text = info[4];
        maxGoldText.text = info[5];
        spawnersText.text = info[6];
        checkpointsText.text = info[7];
        verifiedBugsText.text = info[8];
        fixedBugsText.text = info[9];
        helpRequestsText.text = info[10];
        monstersText.text = info[11];
        npcsText.text = info[12];
        environmentsText.text = info[13];
        // Item Drop
#if ITEM_DROP_C || ITEM_DROP_R
        itemsText.text = info[14];
#endif
    }
}
