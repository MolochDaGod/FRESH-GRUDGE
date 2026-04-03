using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// UI QUESTS
public partial class UI_Quests : MonoBehaviour
{
    public GameObject panel;

    public Transform content;
    public UI_QuestSlot slotPrefab;

    public Button activeQuestsButton;
    public Button completedQuestsButton;
    public Button trackerButton;

    [Header("[-=-[ Cancel configuration ]-=-]")]
    public GameObject cancelQuestPanel;
    public Button cancelYes;
    public Button cancelNo;
    public Text cancelText;
    public string cancelTextValue = "Do you want to cancel: ";
    protected string questName;
    protected GameObject cancelslot;

    public GameObject trackerPanel;

    public string expandPrefix = "[+] ";
    public string hidePrefix = "[-] ";

    public Color fulfilledQuestColor;
    public Color inprogressQuestColor;

    public float cacheInterval = 2.0f;

    protected bool showActiveQuests = true;


    // -----------------------------------------------------------------------------------
    // OnEnable
    // -----------------------------------------------------------------------------------
    private void OnEnable()
    {

        activeQuestsButton.onClick.SetListener(() =>
        {
            showActiveQuests = true;
            UpdateEvent();
        });

        completedQuestsButton.onClick.SetListener(() =>
        {
            showActiveQuests = false;
            UpdateEvent();
        });

        trackerButton.onClick.SetListener(() =>
        {
            trackerPanel.SetActive(!trackerPanel.activeSelf);
        });

    }

    public void Show()
    {
        UpdateEvent();
    }

    public void UpdateEvent()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        List<ExtendedQuest> activeQuests = new List<ExtendedQuest>();

        if (showActiveQuests)
            activeQuests = player.playerExtendedQuest.extendedQuests.Where(q => !q.completed || (q.repeatable > 0 && !q.completedAgain)).ToList();
        else
            activeQuests = player.playerExtendedQuest.extendedQuests.Where(q => q.completed).ToList();


        // =======================================================================
        // Count Active quest 1 time
        // =======================================================================
        int activeQuestsCount = activeQuests.Count;

        UIUtils.BalancePrefabs(slotPrefab.gameObject, activeQuestsCount, content);

        // =======================================================================
        // -- refresh all
        // =======================================================================
        for (int i = 0; i < activeQuestsCount; ++i)
        {
            int index = i;
            UI_QuestSlot slot = content.GetChild(index).GetComponent<UI_QuestSlot>();
            ExtendedQuest quest = activeQuests[index];


            // =======================================================================
            // -- check gathered items
            // =======================================================================
            int[] gathered = player.playerExtendedQuest.CheckGatheredItems(quest);


            // =======================================================================
            // -- check explored areas
            // =======================================================================
            int explored = 0;
#if _iMMOEXPLORATION
            foreach (Exploration_Area area in quest.exploreTarget)
            {
                if (player.playerAddonsConfigurator.HasExploredArea(area))
                    explored++;
            }

#endif

            // =======================================================================
            // -- check faction requirement
            // =======================================================================
            bool factionRequirementsMet = true;
#if _iMMOFACTIONS
            factionRequirementsMet = player.playerFactions.CheckFactionRating(quest.factionRequirement);
#endif

            // =======================================================================
            // name button
            // =======================================================================
            GameObject descriptionPanel = slot.descriptionText.gameObject;
            string prefix = descriptionPanel.activeSelf ? hidePrefix : expandPrefix;
            slot.nameButton.GetComponentInChildren<Text>().text = prefix + quest.name;

            if (showActiveQuests)
            {
                if (quest.IsFulfilled(gathered, explored, factionRequirementsMet))
                {
                    slot.nameButton.GetComponent<Image>().color = fulfilledQuestColor;
                    //TODO on devrais ajouter ici la vérification pour validé une qête complete
                    if (quest.autoCompleteQuest)
                    {
                        player.playerExtendedQuest.Cmd_CompleteQuestAuto(quest.name);
                    }
                }
                else
                {
                    slot.nameButton.GetComponent<Image>().color = inprogressQuestColor;
                }
            }
            else
            {
                slot.nameButton.GetComponent<Image>().color = fulfilledQuestColor;
            }

            slot.nameButton.onClick.SetListener(() =>
            {
                descriptionPanel.SetActive(!descriptionPanel.activeSelf);
            });


            // =======================================================================
            // -- share button
            // =======================================================================
            if (showActiveQuests && player.party.InParty())
            {
                slot.shareButton.gameObject.SetActive(true);
                slot.shareButton.onClick.SetListener(() =>
                {
                    player.playerExtendedQuest.Cmd_ShareQuest(quest.name);
                    panel.SetActive(false);
                });
            }
            else
            {
                slot.shareButton.gameObject.SetActive(false);
            }

            // =======================================================================
            // -- cancel button
            // =======================================================================
            if (showActiveQuests)
            {
                slot.cancelButton.gameObject.SetActive(true);
                slot.cancelButton.onClick.SetListener(() =>
                {
                    Show(quest.name, slot.gameObject);
                });

            }
            else
            {
                slot.cancelButton.gameObject.SetActive(false);
            }

            // =======================================================================
            // -- update description
            // =======================================================================
            slot.descriptionText.text = quest.ToolTip(gathered, explored, factionRequirementsMet);
        }
    }

    // -----------------------------------------------------------------------------------





    // -----------------------------------------------------------------------------------
    // Show
    // -----------------------------------------------------------------------------------
    public void Show(string _questName, GameObject slot)
    {
        questName = _questName;
        cancelslot = slot;
        cancelText.text = cancelTextValue + "\r\n" + questName;
        cancelQuestPanel.SetActive(true);
    }

    // -----------------------------------------------------------------------------------
    // onClickYes
    // -----------------------------------------------------------------------------------

    public void onClickYes()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        player.playerExtendedQuest.Cmd_CancelQuest(questName);
        cancelQuestPanel.SetActive(false);
        cancelslot.SetActive(false);
        cancelslot = null;
    }

    // -----------------------------------------------------------------------------------
    // onClickNo
    // -----------------------------------------------------------------------------------
    public void onClickNo()
    {
        cancelQuestPanel.SetActive(false);
    }
}