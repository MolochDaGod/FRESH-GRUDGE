using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// UI NPC QUESTS
public partial class UINpcExtendedQuests : MonoBehaviour
{
    //public static UINpcExtendedQuests singleton;
    public GameObject panel;
    public UI_NpcQuestSlot slotPrefab;
    public Transform content;

    public string expandPrefix = "[+] ";
    public string hidePrefix = "[-] ";
    public string notEnoughSpace = "Not enough inventory space!";
    public string acceptButton = "Accept";
    public string completeButton = "Complete";

    /*public UINpcExtendedQuests()
    {
        // assign singleton only once (to work with DontDestroyOnLoad when
        // using Zones / switching scenes)
        if (singleton == null) singleton = this;
    }*/
    private void OnEnable()
    {
        UpdateEvent();
    }

    // -----------------------------------------------------------------------------------
    // Update (need remove Update)
    // -----------------------------------------------------------------------------------
    public void UpdateEvent()
    {
        Player player = Player.localPlayer;

        // use collider point(s) to also work with big entities
        if (player != null && player.target != null && player.target is Npc npc && Utils.ClosestDistance(player, player.target) <= 5)
        {

            List<Scriptable_Quest> questsAvailable = npc.extendedQuests.QuestsVisibleFor(player);

            UIUtils.BalancePrefabs(slotPrefab.gameObject, questsAvailable.Count, content);

            // refresh all
            for (int i = 0; i < questsAvailable.Count; ++i)
            {
                Scriptable_Quest npcQuest = questsAvailable[i];
                UI_NpcQuestSlot slot = content.GetChild(i).GetComponent<UI_NpcQuestSlot>();

                // find quest index in original npc quest list (unfiltered)
                int npcIndex = npc.extendedQuests.GetIndexByName(questsAvailable[i].name);
                player.playerExtendedQuest.IncreaseQuestNpcCounterFor(npc);
                // find quest index in player quest list
                int questIndex = player.playerExtendedQuest.GetQuestIndexByName(npcQuest.name);

                if (questIndex != -1)
                {
                    ExtendedQuest quest = player.playerExtendedQuest.extendedQuests[questIndex];

                    // -- quest must be acceptable or complete-able to show
                    if (player.playerExtendedQuest.CanRestartQuest(quest.data) || player.playerExtendedQuest.CanAcceptQuest(quest.data) || player.playerExtendedQuest.CanCompleteQuest(quest.name))
                    {
                            ScriptableItem reward = null;
                        int amount = 0;
                        if (npcQuest.questRewards.Length > 0 &&
                            npcQuest.questRewards[0].rewardItem.Length > 0)
                        {
                            reward = npcQuest.questRewards[0].rewardItem[0].item;
                            amount = npcQuest.questRewards[0].rewardItem[0].amount;
                        }

                        int gathered = 0;
                        foreach (GatherTarget gatherTarget in npcQuest.gatherTarget)
                        {
                            gathered += player.inventory.Count(new Item(gatherTarget.target));
                        }

                        // -- check explored areas
                        int explored = 0;
#if _iMMOEXPLORATION
                        foreach (Exploration_Area area in quest.exploreTarget)
                        {
                            if (player.playerAddonsConfigurator.HasExploredArea(area))
                                explored++;
                        }
#endif

                        // -- check faction requirement
                        bool factionRequirementsMet = true;
#if _iMMOFACTIONS
                        factionRequirementsMet = player.playerFactions.CheckFactionRating(quest.factionRequirement);
#endif
                        // -- check has space
                        bool hasSpace = player.playerExtendedQuest.GetHasEnoughSpace(quest);

                        // -- set gameobject active
                        slot.gameObject.SetActive(true);

                        // -- name button
                        GameObject descriptionText = slot.descriptionText.gameObject;
                        string prefix = descriptionText.activeSelf ? hidePrefix : expandPrefix;

                        slot.nameButton.GetComponentInChildren<Text>().text = prefix + quest.name;
                        slot.nameButton.onClick.SetListener(() =>
                        {
                            descriptionText.SetActive(!descriptionText.activeSelf);
                            //UpdateEvent();
                        });

                        // description + not enough space warning (if needed)
                        slot.descriptionText.text = quest.ToolTip(player.playerExtendedQuest.CheckGatheredItems(quest), explored, factionRequirementsMet);
                        if (!hasSpace)
                            slot.descriptionText.text += "\n<color=red>" + notEnoughSpace + "</color>";

                        if (player.playerExtendedQuest.CanAcceptQuest(quest.data))
                        {
                            // repeatable quest
                            slot.actionButton.interactable = true;
                            slot.actionButton.GetComponentInChildren<Text>().text = acceptButton;
                            slot.actionButton.onClick.SetListener(() =>
                            {
                                player.playerExtendedQuest.Cmd_AcceptQuest(npcIndex);
                                slot.gameObject.SetActive(false);

                            });
                        }
                        else
                        {
                            slot.actionButton.interactable = player.playerExtendedQuest.CanCompleteQuest(quest.name) && hasSpace;
                            slot.actionButton.GetComponentInChildren<Text>().text = completeButton;
                            slot.actionButton.onClick.SetListener(() =>
                            {
                                player.playerExtendedQuest.Cmd_CompleteQuest(npcIndex);
                                slot.gameObject.SetActive(false);
                            });
                        }
                    }
                    else
                    {
                        // -- deactivate slot
                        slot.gameObject.SetActive(false);
                    }
                }
                else
                {
                    ExtendedQuest quest = new ExtendedQuest(npcQuest);

                    // -- set gameobject active
                    slot.gameObject.SetActive(true);

                    // -- name button
                    GameObject descriptionText = slot.descriptionText.gameObject;
                    string prefix = descriptionText.activeSelf ? hidePrefix : expandPrefix;
                    slot.nameButton.GetComponentInChildren<Text>().text = prefix + quest.name;
                    slot.nameButton.onClick.SetListener(() =>
                    {
                        descriptionText.SetActive(!descriptionText.activeSelf);
                    });

                    // -- new quest
                    slot.descriptionText.text = quest.ToolTip(player.playerExtendedQuest.CheckGatheredItems(quest));
                    slot.actionButton.interactable = true;
                    slot.actionButton.GetComponentInChildren<Text>().text = acceptButton;
                    slot.actionButton.onClick.SetListener(() =>
                    {
                        player.playerExtendedQuest.Cmd_AcceptQuest(npcIndex);
                        slot.gameObject.SetActive(false);
                    });
                }
            }
            panel.SetActive(true);
        }
        else panel.SetActive(false);
    }

    // -----------------------------------------------------------------------------------
}