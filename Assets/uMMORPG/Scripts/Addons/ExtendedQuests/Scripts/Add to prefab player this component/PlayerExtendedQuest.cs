using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PlayerInventory))]
[DisallowMultipleComponent]
public class PlayerExtendedQuest : NetworkBehaviour
{

    [Header("Components")]
    public Player player;
    public PlayerInventory inventory;
    public Combat combat;

    [Header("Game Event")]
    public GameEvent UINpcExtendedQuests;
    public GameEvent UIQuestTracker;
    public GameEvent UIQuests;

    [Header("[-=-[ EXTENDED QUESTS ]-=-]")]
    [Tooltip("[Required] Contains active and completed quests (=all)")]
    public int activeQuestLimit = 20;

    public string shareQuestMessage = "Quest shared: ";

    public Tools_PopupClass questCompletePopup;

    public readonly SyncList<ExtendedQuest> extendedQuests = new SyncList<ExtendedQuest>();


#if _SERVER
    public override void OnStartServer()
    {
        player.combat.onKilledEnemy.AddListener(QuestsOnKilled);
    }
#endif

#if _CLIENT
    public override void OnStartClient()
    {
#if MIRROR_90_OR_NEWER
        extendedQuests.OnChange += OnExtendedQuestUpdated;
#else
#pragma warning disable CS0618
        extendedQuests.Callback += OnExtendedQuestUpdated;
#pragma warning restore
#endif
#if _iMMOFACTIONS
        player.playerFactions.onFactionsChanged.AddListener(AutoQuest);
#endif
    }
#endif

#if _CLIENT
    // -----------------------------------------------------------------------------------
    // OnHarvestingProfessionUpdated
    // @Client
    // -----------------------------------------------------------------------------------
#if MIRROR_90_OR_NEWER
    void OnExtendedQuestUpdated(SyncList<ExtendedQuest>.Operation op, int index, ExtendedQuest oldIvalue)
#else
    void OnExtendedQuestUpdated(SyncList<ExtendedQuest>.Operation op, int index, ExtendedQuest oldIvalue, ExtendedQuest newValue)
#endif
    {
        AutoQuest();
        UIQuests.TriggerEvent();
        UIQuestTracker.TriggerEvent();
    }
#endif
    // =============================== CORE SCRIPT REWRITES ==============================


    // ==================================== FUNCTIONS ====================================

    private void AutoQuest()
    {
        if (extendedQuests.Count > 0)
        {
            int i = 0;
            foreach (ExtendedQuest quest in extendedQuests)
            {
                if (quest.data.autoCompleteQuest)
                {
                    Cmd_CheckQestCompletion(i);
                }
                ++i;
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // GetQuestIndexByName
    // -----------------------------------------------------------------------------------
    public int GetQuestIndexByName(string questName)
    {
        return extendedQuests.FindIndex(quest => quest.name == questName);
    }

    // -----------------------------------------------------------------------------------
    // HasCompletedQuest
    // -----------------------------------------------------------------------------------
    public bool HasCompletedQuest(string questName)
    {
        return extendedQuests.Any(q => q.name == questName && q.completed);
    }

    // -----------------------------------------------------------------------------------
    // CanRestartQuest
    // -----------------------------------------------------------------------------------
    public bool CanRestartQuest(Scriptable_Quest quest)
    {
        int idx = GetQuestIndexByName(quest.name);

        if (idx == -1) return true;

        ExtendedQuest tmp_quest = extendedQuests[idx];

        if (CanCompleteQuest(quest.name) ||
            HasActiveQuest(quest.name) ||
            quest.repeatable <= 0 ||
            (quest.repeatable > 0 && tmp_quest.getLastCompleted() < tmp_quest.repeatable && tmp_quest.completedAgain)
        )
            return false;

        return true;
    }

    // -----------------------------------------------------------------------------------
    // HasActiveQuest
    // -----------------------------------------------------------------------------------
    public bool HasActiveQuest(string questName)
    {
        //return extendedQuests.Any(q => q.name == questName && !q.completed || (q.name == questName && !q.completed && !q.completedAgain) || (q.name == questName && q.completed && !q.completedAgain));
        return extendedQuests.Any(q => q.name == questName && !q.completed || (q.name == questName && !q.completed && !q.completedAgain) || (q.name == questName && q.completed && q.repeatable > 0 && !q.completedAgain));
    }

    // -----------------------------------------------------------------------------------
    // IncreaseQuestNpcCounterFor
    // @Client
    // -----------------------------------------------------------------------------------
    [Client]
    public void IncreaseQuestNpcCounterFor(Npc npc)
    {
        for (int i = 0; i < extendedQuests.Count; ++i)
        {
            // active quest and not completed yet?
            if ((!extendedQuests[i].completed || !extendedQuests[i].completedAgain) &&
                extendedQuests[i].visitTarget.Length > 0 &&
                extendedQuests[i].visitTarget.Any(x => x.name.GetStableHashCode() == npc.name.GetStableHashCode()) &&
                !extendedQuests[i].visitedTarget.Any(x => x == npc.name.GetStableHashCode())
                )
            {
                int index = i;
                Cmd_IncreaseQuestNpcCounterFor(index, npc.name.GetStableHashCode());
            }
        }
    }



    #region COMMAND
    // -----------------------------------------------------------------------------------
    // Cmd_IncreaseQuestNpcCounterFor
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_CheckQestCompletion(int index)
    {
#if _SERVER
        CheckQuestCompletion(index);
#endif
    }

    private void CheckCompletionQuest()
    {
        Debug.Log("lol CheckCompletionQuest");
        UIQuests.TriggerEvent();
        UIQuestTracker.TriggerEvent();
        //extendedQuests.Callback += OnExtendedQuestUpdated;
        //Debug.Log("Oh OH OH");
        /*if (extendedQuests.Count > 0)
        {
            int i = 0;
            foreach (ExtendedQuest quest in extendedQuests)
            {
                if (quest.autoCompleteQuest)
                {
                    Cmd_CheckQestCompletion(i);
                }
                ++i;
            }
        }*/
    }
    // -----------------------------------------------------------------------------------
    // Cmd_IncreaseQuestNpcCounterFor
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_IncreaseQuestNpcCounterFor(int index, int hash)
    {
#if _SERVER
        ExtendedQuest quest = extendedQuests[index];
        //bool bChanged = false;
        for (int j = 0; j < extendedQuests[index].visitTarget.Length; ++j)
        {
            if (extendedQuests[index].visitTarget[j].name.GetStableHashCode() == hash && quest.visitedTarget[j] != hash)
            {
                quest.visitedTarget[j] = hash;
                quest.visitedCount++;
                extendedQuests[index] = quest;
                //bChanged = true;
                break;
            }
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // Cmd_AcceptQuest
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_AcceptQuest(int npcQuestIndex)
    {
#if _SERVER
        if (player.target != null &&
            player.isAlive &&
            player.target.isAlive &&
            player.target is Npc npc &&
            0 <= npcQuestIndex && npcQuestIndex < npc.extendedQuests.quests.Length &&
            Utils.ClosestDistance(player, player.target) <= player.interactionRange
             && CanAcceptQuest(npc.extendedQuests.quests[npcQuestIndex].extendedQuest) // TODO à verifier
            )
        {
            ScriptableExtendedQuestOffer quest = npc.extendedQuests.quests[npcQuestIndex];
            int idx = GetQuestIndexByName(quest.extendedQuest.name);

            if (idx == -1)
            {
                //ScriptableExtendedQuestOffer quest = npc.extendedQuests.quests[npcQuestIndex];
                extendedQuests.Add(new ExtendedQuest(quest.extendedQuest));

                // -- accept items
                if (quest.extendedQuest.acceptItems != null && quest.extendedQuest.acceptItems.Length > 0)
                {
                    foreach (RewardItem rewardItem in quest.extendedQuest.acceptItems)
                        inventory.Add(new Item(rewardItem.item), rewardItem.amount);
                }
            }
            else
            {
                ExtendedQuest questAdd = extendedQuests[idx];
                questAdd.resetQuest();
                questAdd.completedAgain = false;
                questAdd.lastCompleted = "";
                extendedQuests[idx] = questAdd;

                // -- accept items
                if (questAdd.acceptItems != null && questAdd.acceptItems.Length > 0)
                {
                    foreach (RewardItem rewardItem in questAdd.acceptItems)
                        inventory.Add(new Item(rewardItem.item), rewardItem.amount);
                }
            }
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // Cmd_CancelQuest
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_CancelQuest(string questName)
    {
#if _SERVER
        int index = GetQuestIndexByName(questName);

        ExtendedQuest quest = extendedQuests[index];

        if (!HasCompletedQuest(questName))
        {
            // -- remove accept items
            if (quest.acceptItems.Length > 0)
            {
                foreach (RewardItem rewardItem in quest.acceptItems)
                    inventory.Remove(new Item(rewardItem.item), rewardItem.amount);
            }

            extendedQuests.RemoveAt(index);
        }
#endif
    }

    //[Server]

    // -----------------------------------------------------------------------------------
    // Cmd_ShareQuest
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_ShareQuest(string questName)
    {
#if _SERVER
        if (!player.party.InParty()) return;

        if (!HasCompletedQuest(questName))
            ShareQuest(questName);
#endif
    }

    // -----------------------------------------------------------------------------------
    // Cmd_CompleteQuest
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_CompleteQuest(int npcQuestIndex)
    {
#if _SERVER
        if (player.isAlive &&
            player.target != null &&
            player.target.isAlive &&
            player.target is Npc npc &&
            0 <= npcQuestIndex && npcQuestIndex < npc.extendedQuests.quests.Length &&
            Utils.ClosestDistance(player, player.target) <= player.interactionRange)
        {
            ScriptableExtendedQuestOffer npcQuest = npc.extendedQuests.quests[npcQuestIndex];
            //Scriptable_Quest npcQuest = ((Npc)player.target).extendedQuests[npcQuestIndex];
            int index = GetQuestIndexByName(npcQuest.extendedQuest.name);

            FinishQuest(index);
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // Cmd_CompleteQuest
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_CompleteQuestAuto(String Quest)
    {
#if _SERVER
        if (player.isAlive)
        {
            /*
            int index = GetQuestIndexByName(Quest);
            if (extendedQuests[index].autoCompleteQuest && CanCompleteQuest(Quest))
            {
                Debug.Log(" if (extendedQuests[index].autoCompleteQuest && !extendedQuests[index].completed)");
                FinishQuest(index);
            }
            else
            {
                Debug.Log("tentative de chead certainement");
            }*/
            Debug.Log("Problem for autocomplete witch gather");
        }
#endif
    }


    #endregion COMMAND










    // -----------------------------------------------------------------------------------
    // CanAcceptQuest
    // -----------------------------------------------------------------------------------
    public bool CanAcceptQuest(Scriptable_Quest quest)
    {
        return extendedQuests.Count(q => !q.completed) < player.playerExtendedQuest.activeQuestLimit &&
               quest.questRequirements.checkRequirements(player) &&
               CanRestartQuest(quest);
    }

    // -----------------------------------------------------------------------------------
    // AcceptQuest
    // -----------------------------------------------------------------------------------
    public void AcceptQuest(string questName)
    {

        int idx = GetQuestIndexByName(questName);

        // -- only if we don't have the quest already
        if (idx == -1)
        {
            if (Scriptable_Quest.All.TryGetValue(questName.GetStableHashCode(), out Scriptable_Quest newQuest))
            {
                if (CanAcceptQuest(newQuest))
                {
                    extendedQuests.Add(new ExtendedQuest(newQuest));

                    // -- accept items
                    if (newQuest.acceptItems != null && newQuest.acceptItems.Length > 0)
                    {
                        foreach (RewardItem rewardItem in newQuest.acceptItems)
                            inventory.Add(new Item(rewardItem.item), rewardItem.amount);
                    }
                    player.Tools_ShowPopup(shareQuestMessage + newQuest.name);
                }
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // CanCompleteQuest
    // -----------------------------------------------------------------------------------
    public bool CanCompleteQuest(string questName)
    {
        int index = GetQuestIndexByName(questName);

        if (index != -1 && (!extendedQuests[index].completed || extendedQuests[index].repeatable > 0 && !extendedQuests[index].completedAgain))
        {
            ExtendedQuest quest = extendedQuests[index];

            // -- check explored areas
            int explored = 0;
#if _iMMOEXPLORATION
            foreach (Exploration_Area area in quest.exploreTarget)
            {
                if (player.playerAddonsConfigurator.HasExploredArea(area))
                {
                    explored++;
                }
            }
#endif

            // -- check faction requirement
            bool factionRequirementsMet = true;
#if _iMMOFACTIONS
            factionRequirementsMet = player.playerFactions.CheckFactionRating(quest.factionRequirement);
#endif

            // -- validate the rest
            if (quest.IsFulfilled(CheckGatheredItems(quest), explored, factionRequirementsMet))
            {
                return true;
            }

        }
        return false;
    }


    // -----------------------------------------------------------------------------------
    // GetHasEnoughSpace
    // -----------------------------------------------------------------------------------
    public bool GetHasEnoughSpace(ExtendedQuest quest)
    {
        if (quest.questRewards.Length > 0)
        {
            foreach (QuestReward questReward in quest.questRewards)
            {
                if (inventory.SlotsFree() < questReward.rewardItem.Length)
                    return false;
            }
        }
        return true;
    }

    // -----------------------------------------------------------------------------------
    // CheckGatheredItems
    // -----------------------------------------------------------------------------------
    public int[] CheckGatheredItems(ExtendedQuest quest)
    {
        int[] gathered = new int[10];
        for (int i = 0; i < quest.gatherTarget.Length; i++)
        {
            gathered[i] = Mathf.Min(inventory.Count(new Item(quest.gatherTarget[i].target)), quest.gatherTarget[i].amount);
        }
        return gathered;
    }


















    // -----------------------------------------------------------------------------------
    // Server Only
    // -----------------------------------------------------------------------------------
    #region SERVER ONLY
#if _SERVER


    // -----------------------------------------------------------------------------------
    // CheckQuestCompletion
    // @Server
    // -----------------------------------------------------------------------------------
    public void CheckQuestCompletion(int index)
    {
        if (HasActiveQuest(extendedQuests[index].name) && CanCompleteQuest(extendedQuests[index].name))
        {
            player.Tools_ShowPopup(questCompletePopup.message + extendedQuests[index].name);

            if (extendedQuests[index].autoCompleteQuest)
            {
                FinishQuest(index);
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // QuestsOnKilled
    // @Server
    // -----------------------------------------------------------------------------------
    [Server]
    public void QuestsOnKilled(Entity victim)
    {
#if _iMMOQUESTS
        IncreaseQuestKillCounterFor(victim);
#endif
    }


    // -----------------------------------------------------------------------------------
    // OnDamageDealtToPlayer
    // @Server
    // -----------------------------------------------------------------------------------
    [Server]
    public void OnDamageDealtToPlayer(Player player)
    {
        if (!player.IsOffender() && !player.IsMurderer())
        {
            // did we kill him? then start/reset murder status
            // did we just attack him? then start/reset offender status
            // (unless we are already a murderer)
            if (player.health.current == 0)
            {
                player.StartMurderer();
#if _iMMOQUESTS
                QuestsOnKilled(player);
#endif
            }
            else if (!player.IsMurderer()) player.StartOffender();
        }
    }

    // -----------------------------------------------------------------------------------
    // IncreaseQuestKillCounterFor
    // -----------------------------------------------------------------------------------
    [Server]
    public void IncreaseQuestKillCounterFor(Entity victim)
    {
        if (victim == null) return;

        if (victim is Monster)
        {
            IncreaseQuestMonsterKillCounterFor((Monster)victim);
        }
        else if (victim is Player)
        {
            IncreaseQuestPlayerKillCounterFor((Player)victim);
        }
    }

    // -----------------------------------------------------------------------------------
    // IncreaseQuestMonsterKillCounterFor
    // -----------------------------------------------------------------------------------
    [Server]
    public void IncreaseQuestMonsterKillCounterFor(Monster victim)
    {
        for (int i = 0; i < extendedQuests.Count; ++i)
        {
            int index = i;

            if ((!extendedQuests[index].completed || !extendedQuests[index].completedAgain) && extendedQuests[index].killTarget.Length > 0 && extendedQuests[index].killTarget.Any(x => x.target.name == victim.name))
            {
                ExtendedQuest quest = extendedQuests[index];
                //bool bChanged = false;

                for (int j = 0; j < quest.killTarget.Length; ++j)
                {
                    int idx = j;
                    if (quest.killTarget[idx].target.name == victim.name && quest.killedTarget[idx] < quest.killTarget[idx].amount)
                    {
                        quest.killedTarget[idx]++;
                        quest.counter++;
                        //bChanged = true;
                        break;
                    }
                }

                extendedQuests[index] = quest;
                //if (bChanged) CheckQuestCompletion(index);
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // IncreaseQuestPlayerKillCounterFor
    // -----------------------------------------------------------------------------------
    [Server]
    public void IncreaseQuestPlayerKillCounterFor(Player victim)
    {
#if _iMMOPVP && _iMMOQUESTS
        for (int i = 0; i < extendedQuests.Count; ++i)
        {
            int index = i;

            if ((!extendedQuests[index].completed || !extendedQuests[index].completedAgain) && extendedQuests[index].pvpTarget.Length > 0)
            {
                ExtendedQuest quest = extendedQuests[index];

                for (int j = 0; j < quest.pvpTarget.Length; ++j)
                {
                    int idx = j;

                    if (CheckPvPTarget(victim, quest.pvpTarget[idx]))
                    {
                        quest.pvpedTarget[idx]++;
                        quest.counter++;
                        break;
                    }
                }

                extendedQuests[index] = quest;
            }
        }
#endif
    }

    [Server]
    public void IncreaseExplorationCounterFor(string area)
    {
#if _iMMOEXPLORATION && _iMMOQUESTS
        for (int i = 0; i < extendedQuests.Count; ++i)
        {
            int index = i;

            if ((!extendedQuests[index].completed || !extendedQuests[index].completedAgain) && extendedQuests[index].exploreTarget.Length > 0)
            {
                ExtendedQuest quest = extendedQuests[index];
                int countExplore = quest.exploreTarget.Length;
                if (!quest.explored.Contains(area.GetStableHashCode()))
                {
                    for (int j = 0; j < countExplore; ++j)
                    {
                        int idx = j;
                        if (quest.exploreTarget[j].name.GetStableHashCode() == area.GetStableHashCode())
                        {
                            quest.explored[j] = area.GetStableHashCode();
                            quest.counter++;
                            break;
                        }
                    }
                }
                extendedQuests[index] = quest;
            }
        }
#endif
    }
    // -----------------------------------------------------------------------------------
    // CheckPvPTarget
    // -----------------------------------------------------------------------------------
#if _iMMOPVP && _iMMOQUESTS

    public bool CheckPvPTarget(Player victim, PvpTarget target)
    {
        // -- Check Level Range

        if (target.levelRange != 0)
        {
            int minLevel = Mathf.Max(1, player.level.current - target.levelRange);
            int maxLevel = player.level.current + target.levelRange;

            if (victim.level.current < minLevel || victim.level.current > maxLevel)
                return false;
        }

        // -- Check Type

        if (target.type != PvpTarget.pvpType.Any)
        {
            if (target.type == PvpTarget.pvpType.MyParty || target.type == PvpTarget.pvpType.OtherParty)
            {
                if (!player.party.InParty() || !victim.party.InParty())
                    return false;

                if (target.type == PvpTarget.pvpType.AnyParty && !victim.party.InParty())
                    return false;

                if (target.type == PvpTarget.pvpType.MyParty && (!player.party.party.Contains(victim.name) || victim.party.party.Contains(name)))
                    return false;

                if (target.type == PvpTarget.pvpType.OtherParty && (!player.party.party.Contains(victim.name) || victim.party.party.Contains(name)))
                    return false;
            }

            if (target.type == PvpTarget.pvpType.MyGuild || target.type == PvpTarget.pvpType.OtherGuild)
            {
                if (!player.guild.InGuild() || !victim.guild.InGuild())
                    return false;

                if (target.type == PvpTarget.pvpType.AnyGuild && !victim.guild.InGuild())
                    return false;

                if (target.type == PvpTarget.pvpType.MyGuild && player.guild.name != victim.guild.guild.name)
                    return false;

                if (target.type == PvpTarget.pvpType.OtherGuild && player.guild.guild.name == victim.guild.guild.name)
                    return false;
            }

            if (target.type == PvpTarget.pvpType.MyRealm || target.type == PvpTarget.pvpType.OtherRealm)
            {
                //if (Realm == 0 || alliedRealm == 0 || victim.Realm == 0 || victim.alliedRealm == 0)
                //	return false;

                if (target.type == PvpTarget.pvpType.MyRealm && !player.GetAlliedRealms(victim))
                    return false;

                if (target.type == PvpTarget.pvpType.OtherRealm && player.GetAlliedRealms(victim))
                    return false;
            }
        }

        return true;
    }

#endif

    // -----------------------------------------------------------------------------------
    // IncreaseHarvestNodeCounterFor
    // -----------------------------------------------------------------------------------
#if _iMMOHARVESTING && _iMMOQUESTS

    [Server]
    public void IncreaseHarvestNodeCounterFor(HarvestingProfessionTemplate profession)
    {
        for (int i = 0; i < extendedQuests.Count; ++i)
        {
            if ((!extendedQuests[i].completed || !extendedQuests[i].completedAgain) &&
                extendedQuests[i].harvestTarget.Length > 0 &&
                extendedQuests[i].harvestTarget.Any(x => x.target == profession)
                )
            {
                ExtendedQuest quest = extendedQuests[i];
                //bool bChanged = false;

                for (int j = 0; j < quest.harvestTarget.Length; ++j)
                {
                    if (quest.harvestTarget[j].target == profession &&
                        quest.harvestedTarget[j] < quest.harvestTarget[j].amount)
                    {
                        int idx = j;
                        quest.harvestedTarget[idx]++;
                        quest.counter++;
                        //bChanged = true;
                        break;
                    }
                }

                extendedQuests[i] = quest;
                //if (bChanged) CheckQuestCompletion(i);
            }
        }
    }

#endif

    // -----------------------------------------------------------------------------------
    // IncreaseCraftCounterFor
    // -----------------------------------------------------------------------------------
#if _iMMOCRAFTING && _iMMOQUESTS

    [Server]
    //public void IncreaseCraftCounterFor(Tmpl_CraftingRecipe recipe)
    public void IncreaseCraftCounterFor(Tmpl_Recipe recipe)
    {
        for (int i = 0; i < extendedQuests.Count; ++i)
        {
            if ((!extendedQuests[i].completed || !extendedQuests[i].completedAgain) &&
                extendedQuests[i].craftTarget.Length > 0 &&
                extendedQuests[i].craftTarget.Any(x => x.target == recipe)
                )
            {
                ExtendedQuest quest = extendedQuests[i];
                //bool bChanged = false;

                for (int j = 0; j < quest.craftTarget.Length; ++j)
                {
                    if (quest.craftTarget[j].target == recipe &&
                        quest.craftedTarget[j] < quest.craftTarget[j].amount)
                    {
                        int idx = j;
                        quest.craftedTarget[idx]++;
                        quest.counter++;
                        //bChanged = true;
                        break;
                    }
                }

                extendedQuests[i] = quest;
                //if (bChanged) CheckQuestCompletion(i);
            }
        }
    }

#endif

    // -----------------------------------------------------------------------------------
    // IncreaseQuestLootCounterFor
    // -----------------------------------------------------------------------------------
#if _iMMOCHEST && _iMMOQUESTS

    [Server]
    public void IncreaseQuestLootCounterFor(string lootcrateName)
    {
        for (int i = 0; i < extendedQuests.Count; ++i)
        {
            if ((!extendedQuests[i].completed || !extendedQuests[i].completedAgain) &&
                extendedQuests[i].lootTarget.Length > 0 &&
                extendedQuests[i].lootTarget.Any(x => x.target.name == lootcrateName)
                )
            {
                ExtendedQuest quest = extendedQuests[i];
                //bool bChanged = false;

                for (int j = 0; j < quest.lootTarget.Length; ++j)
                {
                    if (quest.lootTarget[j].target.name == lootcrateName &&
                        quest.lootedTarget[j] < quest.lootTarget[j].amount)
                    {
                        int idx = j;
                        quest.lootedTarget[idx]++;
                        quest.counter++;
                        //bChanged = true;
                        break;
                    }
                }

                extendedQuests[i] = quest;
            }
        }
    }

#endif

    // -----------------------------------------------------------------------------------
    // FinishQuest
    // @Server
    // -----------------------------------------------------------------------------------
    protected void FinishQuest(int index)
    {
        if (index != -1)
        {
            ExtendedQuest quest = extendedQuests[index];

            if (CanCompleteQuest(quest.name))
            {
                // -- remove accept items (optional)
                if (quest.removeAtCompletion && quest.acceptItems.Length > 0)
                {
                    foreach (RewardItem rewardItem in quest.acceptItems)
                        inventory.Remove(new Item(rewardItem.item), rewardItem.amount);
                }

                // -- remove gathered items
                if (!quest.DontDestroyGathered)
                {
                    foreach (GatherTarget gatherTarget in quest.gatherTarget)
                    {
                        inventory.Remove(new Item(gatherTarget.target), gatherTarget.amount);
                    }
                }

                // -- determine the correct reward
                if (quest.questRewards.Length > 0)
                {
                    QuestReward reward = GetQuestReward(quest);

                    // -- gain basic rewards
                    player.gold += reward.rewardGold;
                    player.experience.current += reward.rewardExperience;
                    player.itemMall.coins += reward.rewardCoins;

                    // -- reward items
                    if (reward.rewardItem.Length > 0)
                    {
                        foreach (RewardItem rewardItem in reward.rewardItem)
                            inventory.Add(new Item(rewardItem.item), rewardItem.amount);
                    }
#if _iMMOTITLES
                    if (reward.title != null)
                        player.playerTitles.EarnTitle(reward.title);
#endif
                    // -- unlock travelroutes
#if _iMMOTRAVEL
                    foreach (Unlockroute route in reward.rewardUnlockroutes)
                        player.playerTravelroute.UnlockTravelroute(route);
#endif

                    // -- reward honor currency
#if _iMMOHONORSHOP
                    foreach (HonorShopCurrencyCost currency in reward.honorCurrency)
                        player.playerHonorShop.AddHonorCurrency(currency.honorCurrency, currency.amount);
#endif

                    // -- apply realm change
#if _iMMOPVP
                    player.SetRealm(reward.changeRealm, reward.changeAlliedRealm);
#endif
                }

                // -- apply faction modifiers
#if _iMMOFACTIONS
                foreach (FactionModifier factionModifier in quest.factionModifiers)
                {
                    player.playerFactions.AddFactionRating(factionModifier.faction, factionModifier.amount);
                }
#endif

                // -- apply world events
#if _iMMOWORLDEVENTS
                if (quest.worldEvent != null)
                    player.playerAddonsConfigurator.ModifyWorldEventCount(quest.worldEvent, quest.worldEventModifier);
#endif
                // -- apply world events


                // -- complete quest
                quest.completed = true;
                quest.counter++;

                if (quest.repeatable > 0)
                {
                    quest.resetQuest();
                    quest.completedAgain = true;
                    quest.lastCompleted = DateTime.UtcNow.ToString("s");
                }

                extendedQuests[index] = quest;
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // GetQuestReward
    // @Server
    // -----------------------------------------------------------------------------------
    public QuestReward GetQuestReward(ExtendedQuest quest)
    {
        if (quest.questRewards.Length == 1)
            return quest.questRewards[0];

        // -- check class based rewards
        for (int i = 0; i < quest.questRewards.Length; i++)
        {
            if (quest.questRewards[i].availableToClass != null && quest.questRewards[i].availableToClass.Length > 0)
            {
                if (player.Tools_checkHasClass(quest.questRewards[i].availableToClass))
                    return quest.questRewards[i];
            }
        }

        // -- check randomized rewards
        foreach (QuestReward questReward in quest.questRewards)
        {
            if (UnityEngine.Random.value <= questReward.rewardChance)
                return questReward;
        }

        // -- return the very first reward if no one is found
        return quest.questRewards[0];
    }

    // -----------------------------------------------------------------------------------
    // ShareQuest
    // @Server
    // -----------------------------------------------------------------------------------
    [Server]
    protected void ShareQuest(string questName)
    {
        List<Player> closeMembers = player.party.InParty() ? player.party.GetMembersInProximity() : new List<Player>();
        foreach (Player member in closeMembers)
        {
            member.playerExtendedQuest.AcceptQuest(questName);
        }
    }














#endif
    #endregion SERVER ONLY

}
