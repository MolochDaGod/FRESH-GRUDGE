using Mirror;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

#if _iMMOHARVESTING
// PLAYER

public class PlayerHarvesting : NetworkBehaviour
{
    public Player player;
    public Combat combat;

    [Header("[-=-[ Harvesting (See Tooltips) ]-=-]")]
    [Tooltip("[Optional] Default recipes the player starts the game with.")]
    public HarvestingProfessionTemplate[] startingProfession;

    public readonly SyncList<HarvestingProfession> Professions = new SyncList<HarvestingProfession>();

    protected UI_HarvestingLoot harvestingUIInstance;
    public ResourceNode selectedNode;

    [Header("[Events]")]
    public GameEvent harvestingProfessionEvent;
    public GameEvent harvestingLootEvent;
    public UnityEvent onHarvestingProfession;

    protected bool harvestBooster = false;

    public void Start()
    {
        if (!isServer && !isClient) return;
        Professions.Callback += OnHarvestingProfessionUpdated;
    }
    // -----------------------------------------------------------------------------------
    // OnHarvestingProfessionUpdated
    // @Client
    // -----------------------------------------------------------------------------------
    void OnHarvestingProfessionUpdated(SyncList<HarvestingProfession>.Operation op, int index, HarvestingProfession oldIvalue, HarvestingProfession newValue)
    {
        harvestingProfessionEvent.TriggerEvent();
    }

    
    // -----------------------------------------------------------------------------------
    // OnSelect_ResourceNode
    // @Client
    // -----------------------------------------------------------------------------------
    [Client]
    public void OnSelect_ResourceNode(ResourceNode _selectedResourceNode)
    {
        //Debug.Log(">>" + player.target.name);

        player.movement.Reset();
        if (harvestingUIInstance)
            harvestingUIInstance.Hide(false);

        selectedNode = _selectedResourceNode;
#if !_iMMO2D
        if (selectedNode.playerLookAtResourcesNode)
            player.movement.LookAtY(selectedNode.gameObject.transform.position);
#endif
        if (!selectedNode._isUsed)
            Cmd_checkResourceNodeAccess(selectedNode);
        else
            Debug.Log("Already used Node");
    }

    // -----------------------------------------------------------------------------------
    // Cmd_checkResourceNodeAccess
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    protected void Cmd_checkResourceNodeAccess(ResourceNode _selectedResourceNode)
    {
#if _SERVER
        selectedNode = _selectedResourceNode;
        if (ResourceNodeValidation() && HarvestingValidation())
        {
            if (selectedNode.IsDepleted() && !selectedNode.HasResources())
            {
                player.Tools_ShowPrompt(selectedNode.depletedMessage);
            }
            else
            {
                player.movement.Reset();
                //if (!selectedNode.HasResources())
                //    selectedNode.OnRefill();
                player.Tools_setTimer(HarvestingDuration(harvestBooster));
                Target_startResourceNodeAccess(connectionToClient);
            }
        }
        else
        {
            if (selectedNode != null && selectedNode.checkInteractionRange(player))
            {
                if (selectedNode.IsDepleted())
                {
                    player.Tools_ShowPrompt(selectedNode.depletedMessage);
                }
                else
                {
                    player.Tools_ShowPrompt(selectedNode.requirementsMessage);
                }

                CancelResourceNode();
            }
            else
            {
                player.movement.Navigate(player.collider.ClosestPointOnBounds(transform.position), 0.5f);
            }
        }
#endif
    }

    [Server]
    public string UpdateServer_HARVESTING()
    {
        /*if (player.playerHarvesting.selectedNode != null)
        {
            Debug.Log("lol ");
        }*/

        if (selectedNode != null)
            return "HARVESTING";
        else
            return "IDLE";
    }

    // -----------------------------------------------------------------------------------
    // LateUpdate
    // @Client
    // -----------------------------------------------------------------------------------
    [ClientCallback]
    private void LateUpdate()
    {
        if (player.playerHarvesting.selectedNode != null)
        {
            if (ResourceNodeValidation())
            {
                player.Tools_OverrideState("HARVESTING");
                //Debug.Log(" State : " + player.state);
                if (HarvestingValidation() && player.Tools_timerRunning)
                {
                    HarvestingProfessionTemplate requiredProfession = getHarvestingProfessionTemplate();

                    player.animator.SetBool(requiredProfession.animatorState, requiredProfession.playTimeRemaining > 0);
                }

                if (player.Tools_checkTimer())
                {
                    player.Tools_stopTimer();
                    player.Tools_removeTask();
                    player.Tools_CastbarHide();

                    HarvestingProfessionTemplate requiredProfession = getHarvestingProfessionTemplate();

                    player.animator.SetBool(requiredProfession.animatorState, false);
                    player.StopAnimation(requiredProfession.animatorState, requiredProfession.stopPlayerSound);

                    Destroy(player.indicator.indicator);

                    Cmd_FinishHarvest();
                }
            }
            else
            {

                player.Tools_OverrideState("IDLE");
                //Debug.Log("Unselect current node");
                selectedNode = null;
            }

        }
        else if(player.state == "HARVESTING" && player.playerHarvesting.selectedNode == null)
        {
            //Debug.Log("lol");
//            player.Tools_OverrideState("IDLE");
        }
    }
    
    // -----------------------------------------------------------------------------------
    // OnDamageDealt_cancelHarvesting
    // Custom Hook
    // -----------------------------------------------------------------------------------
    private void OnDamageDealt_cancelHarvesting()
    {
        CancelHarvesting();

        if (isServer)
            Target_Harvesting_cancelHarvestingClient(connectionToClient);
    }

    // -----------------------------------------------------------------------------------
    // CancelHarvesting
    // -----------------------------------------------------------------------------------
    public void CancelHarvesting()
    {
        if (selectedNode != null)
        {
            player.Tools_stopTimer();
            player.Tools_removeTask();
            player.Tools_CastbarHide();

            HarvestingProfessionTemplate requiredProfession = getHarvestingProfessionTemplate();

            //if (requiredProfession != null)
            player.StopAnimation(requiredProfession.animatorState, requiredProfession.stopPlayerSound);

            selectedNode = null;
        }
    }

    // -----------------------------------------------------------------------------------
    // ResourceNodeValidation
    // Client / Server
    // -----------------------------------------------------------------------------------
    public bool ResourceNodeValidation()
    {
        return (
                selectedNode != null && 
                (!selectedNode.IsDepleted() || selectedNode.HasResources()) &&
                selectedNode.checkInteractionRange(player) &&
                selectedNode.interactionRequirements.checkState(player)
            );
    }

    // -----------------------------------------------------------------------------------
    // HarvestingValidation (Client/Server)
    // -----------------------------------------------------------------------------------
    public bool HarvestingValidation()
    {
#if _iMMOSTAMINA
        bool bValid = ResourceNodeValidation() && player.mana.current >= selectedNode.manaCost && player.stamina.current >= selectedNode.staminaCost;
#else
        bool bValid = ResourceNodeValidation() && player.mana.current >= selectedNode.manaCost;
#endif

        //Debug.Log("bValid : " + bValid);

        if (bValid)
        {
            // ----- check tools
            HarvestingProfessionTemplate requiredProfession = getHarvestingProfessionTemplate();

            int check = 0;
            if(requiredProfession.tools.Length > 0)
                foreach (HarvestingTool tool in requiredProfession.tools)
                {
                    if ((!tool.equippedItem && player.inventory.Count(new Item(tool.requiredItem)) >= 1) || (tool.equippedItem && player.Tools_checkHasEquipment(tool.requiredItem)))
                        check++;
            }

            if (requiredProfession.requiresAllTools)
            {
                if (check < requiredProfession.tools.Length) bValid = false;
            }
            else
            {
                if (check <= 0 && requiredProfession.tools.Length > 0) bValid = false;
            }
        }

        // ---- Cancel
        /*
        if (!bValid)
        	CancelHarvesting();
        */
        return bValid;
    }

    // -----------------------------------------------------------------------------------
    // Target_startResourceNodeAccess
    // @Server -> @Client
    // -----------------------------------------------------------------------------------
    [TargetRpc]
    public void Target_startResourceNodeAccess(NetworkConnection target)
    {
        //if (ResourceNodeValidation() && HarvestingValidation())
        if (HarvestingValidation())
        {
            player.Tools_addTask();
            player.Tools_setTimer(HarvestingDuration(harvestBooster));
            player.Tools_CastbarShow(selectedNode.accessLabel, HarvestingDuration(harvestBooster));

            //agent.ResetPath();
            //LookAtY(selectedNode.transform.position);

            HarvestingProfessionTemplate requiredProfession = getHarvestingProfessionTemplate();
            //Debug.Log("Start animation "+ requiredProfession.animatorState);
            player.animator.SetBool("HARVESTING", true);
            player.animator.SetBool(requiredProfession.animatorState, requiredProfession.playTimeRemaining > 0);
            //player.StartAnimation(requiredProfession.animatorState, requiredProfession.startPlayerSound);
        }
        else
        {
            Debug.Log("ERROOORR !!");
        }
    }

    // -----------------------------------------------------------------------------------
    // Target_Harvesting_cancelHarvestingClient
    // -----------------------------------------------------------------------------------
    [TargetRpc]
    public void Target_Harvesting_cancelHarvestingClient(NetworkConnection target)
    {
        CancelHarvesting();
    }

    // -----------------------------------------------------------------------------------
    // Target_finishResourceNodeAccess
    // @Server -> @Client
    // -----------------------------------------------------------------------------------
    [TargetRpc]
    public void Target_finishResourceNodeAccess(NetworkConnection target)
    {
        if (ResourceNodeValidation())
        {
            //if (!harvestingUIInstance)
            //    harvestingUIInstance = FindObjectOfType<UI_HarvestingLoot>();
            harvestingLootEvent.TriggerEvent();
            //harvestingUIInstance.Show();
        }
    }

    // -----------------------------------------------------------------------------------
    // Cmd_FinishHarvest (Server)
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_FinishHarvest()
    {
#if _SERVER
        if (HarvestingValidation())
        {
            player.Tools_removeTask();
            player.Tools_stopTimer();
           
            HarvestingResult harvestingResult = HarvestingResult.None;

            // --------------------------------------------------------------------------- Experience

            HarvestingProfessionRequirement requiredProfession = getRequiredHarvestingProfession();
            HarvestingProfession profession = getHarvestingProfession(requiredProfession);

            harvestBooster = Harvesting_CanBoost(); //TODO BUG ?

            float harvestChance = HarvestingProbability(harvestBooster);
            float harvestCritChance = HarvestingCriticalProbability(harvestBooster);
            int nodeLevel = requiredProfession.level;
            int oldLevel = profession.level;
            int exp = HarvestingExperience(harvestBooster);
            profession.experience += exp;

            SetHarvestingProfession(profession);

            if (exp > 0)
                player.Tools_TargetAddMessage(exp.ToString() + " " + profession.template.name + " " + selectedNode.experienceMessage);

            if (oldLevel < profession.level)
                player.Tools_ShowPopup(selectedNode.levelUpMessage + profession.templateName + " [L" + profession.level + "]");

#if _iMMOTITLES
            if (oldLevel < profession.level && profession.template.rewardTitle.Count() > 0 && profession.template.rewardTitle[profession.level - 1] != null)
                player.playerTitles.EarnTitle(profession.template.rewardTitle[profession.level - 1]);
#endif

            // --------------------------------------------------------------------------- Skill Check

            if (UnityEngine.Random.value <= harvestChance)
            {
                harvestingResult = HarvestingResult.Success;

                if (UnityEngine.Random.value <= harvestCritChance)
                    harvestingResult = HarvestingResult.CriticalSuccess;
            }
            else
            {
                harvestingResult = HarvestingResult.Failure;
            }

            // --------------------------------------------------------------------------- Deplete other Costs (mana etc.)
            if (selectedNode.manaCost > 0)
                player.mana.current -= selectedNode.manaCost;
#if _iMMOSTAMINA
            if (selectedNode.staminaCost > 0)
                player.stamina.current -= selectedNode.staminaCost;
#endif

            // --------------------------------------------------------------------------- Check Tool breakage

            foreach (HarvestingTool tool in profession.template.tools)
            {
                if ((!tool.equippedItem && player.inventory.Count(new Item(tool.requiredItem)) >= 1) || (tool.equippedItem && player.Tools_checkHasEquipment(tool.requiredItem)))
                {
                    if (UnityEngine.Random.value <= tool.toolDestroyChance)
                    {
                        if (tool.equippedItem)
                            player.Tools_removeEquipment(tool.requiredItem);
                        else
                            player.inventory.Remove(new Item(tool.requiredItem), 1);
                        player.Tools_TargetAddMessage(selectedNode.breakMessage);
                    }
                }
            }

            if (harvestBooster)
            {
                foreach (HarvestingTool tool in profession.template.optionalTools)
                {
                    if ((!tool.equippedItem && player.inventory.Count(new Item(tool.requiredItem)) >= 1) || (tool.equippedItem && player.Tools_checkHasEquipment(tool.requiredItem)))
                    {
                        if (UnityEngine.Random.value <= tool.toolDestroyChance)
                        {
                            if (tool.equippedItem)
                                player.Tools_removeEquipment(tool.requiredItem);
                            else
                                player.inventory.Remove(new Item(tool.requiredItem), 1);
                            player.Tools_TargetAddMessage(selectedNode.boosterMessage);
                        }
                    }
                }
            }

            // --------------------------------------------------------------------------- Resources

            if (harvestingResult != HarvestingResult.Failure)
            {

                selectedNode.UsedNode();
                if (harvestingResult == HarvestingResult.CriticalSuccess)
                {
                    selectedNode.OnCritical();
                    player.Tools_TargetAddMessage(selectedNode.criticalSuccessMessage);
                }
                else
                {
                    player.Tools_TargetAddMessage(selectedNode.successMessage);
                }

                Target_finishResourceNodeAccess(connectionToClient);

                selectedNode.OnHarvested();
                selectedNode.OnDepleted();

#if _iMMOQUESTS
                player.playerExtendedQuest.IncreaseHarvestNodeCounterFor(profession.template);
#endif
            }
            else
            {
                player.Tools_ShowPrompt(selectedNode.failedMessage);
                //Debug.Log("Failed harvest");
                selectedNode = null;
            }

            // --------------------------------------------------------------------------- Cleanup

            //CancelHarvesting();
            harvestBooster = false;
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // OnDamageDealt_cancelResourceNode
    // Custom Hook
    // -----------------------------------------------------------------------------------
    private void OnDamageDealt_cancelResourceNode()
    {
        CancelResourceNode();
    }

    // -----------------------------------------------------------------------------------
    // CancelResourceNode
    // -----------------------------------------------------------------------------------
    public void CancelResourceNode()
    {
        player.Tools_stopTimer();
        player.Tools_removeTask();
        player.Tools_CastbarHide();
        selectedNode = null;
        harvestBooster = false;
    }

    // -----------------------------------------------------------------------------------
    // Cmd_TakeHarvestingResources
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_TakeHarvestingResources(int index)
    {
#if _SERVER
        if (ResourceNodeValidation() && 0 <= index && index < selectedNode.inventory.Count && selectedNode.inventory[index].amount > 0)
            {
                ItemSlot slot = selectedNode.inventory[index];

                // try to add it to the inventory, clear monster slot if it worked
                if (player.inventory.Add(slot.item, slot.amount))
                {
                    slot.amount = 0;
                    selectedNode.inventory[index] = slot;
                }

                // check if resource is depleted (otherwise it takes too long to update)
                if (selectedNode is ResourceNode node && node.EventDepleted())
                    node.OnDepleted();

                if (!ResourceNodeValidation())
                    player.playerHarvesting.selectedNode = null;
            }
#endif
    }

    // ============================== DURATION/PROBABILITY ===============================

    // -----------------------------------------------------------------------------------
    // Harvesting_CanBoost
    // -----------------------------------------------------------------------------------
    public bool Harvesting_CanBoost()
    {
        HarvestingProfessionRequirement requiredProfession = getRequiredHarvestingProfession();
        HarvestingProfession profession = getHarvestingProfession(requiredProfession);

        if (profession.template.optionalTools.Length <= 0) return false;

        bool bValid = false;

        foreach (HarvestingTool tool in profession.template.optionalTools)
        {
            if (
                (!tool.equippedItem && player.inventory.Count(new Item(tool.requiredItem)) >= 1) ||
                (tool.equippedItem && player.Tools_checkHasEquipment(tool.requiredItem))
                )
            {
                bValid = true;
            }
            else
            {
                bValid = false;
            }
        }

        return bValid;
    }

    // -----------------------------------------------------------------------------------
    // HarvestingProbability
    // -----------------------------------------------------------------------------------
    public float HarvestingProbability(bool boost)
    {
        float proba = 0f;

        HarvestingProfessionRequirement requiredProfession = getRequiredHarvestingProfession();
        HarvestingProfession profession = getHarvestingProfession(requiredProfession);

        // -- Modificator: Profession Skill

        proba = profession.template.baseHarvestChance;

        // -- Modificator: Node Level vs. Skill Level

        if (profession.level > requiredProfession.level)
            proba += (profession.level - requiredProfession.level) * profession.template.probabilityPerSkillLevel;

        // -- Modificator: Required Tools

        foreach (HarvestingTool tool in profession.template.tools)
        {
            if ((!tool.equippedItem && player.inventory.Count(new Item(tool.requiredItem)) >= 1) || (tool.equippedItem && player.Tools_checkHasEquipment(tool.requiredItem)))
            {
                proba += tool.modifyProbability;
                if (!profession.template.requiresAllTools)
                    break;
            }
        }

        // -- Modificator: Optional Tools

        if (boost)
        {
            foreach (HarvestingTool tool in profession.template.optionalTools)
            {
                if ((!tool.equippedItem && player.inventory.Count(new Item(tool.requiredItem)) >= 1) || (tool.equippedItem && player.Tools_checkHasEquipment(tool.requiredItem)))
                {
                    proba += tool.modifyProbability;
                    break;
                }
            }
        }

        return proba;
    }

    // -----------------------------------------------------------------------------------
    // HarvestingCriticalProbability
    // -----------------------------------------------------------------------------------
    public float HarvestingCriticalProbability(bool boost)
    {
        float proba = 0f;

        HarvestingProfessionRequirement requiredProfession = getRequiredHarvestingProfession();
        HarvestingProfession profession = getHarvestingProfession(requiredProfession);

        // -- Modificator: Profession Skill

        proba = profession.template.criticalHarvestChance + (profession.level * profession.template.criticalProbabilityPerSkillLevel);

        // -- Modificator: Required Tools

        foreach (HarvestingTool tool in profession.template.tools)
        {
            if ((!tool.equippedItem && player.inventory.Count(new Item(tool.requiredItem)) >= 1) || (tool.equippedItem && player.Tools_checkHasEquipment(tool.requiredItem)))
            {
                proba += tool.modifyCriticalProbability;
                if (!profession.template.requiresAllTools)
                    break;
            }
        }

        // -- Modificator: Optional Tools

        if (boost)
        {
            foreach (HarvestingTool tool in profession.template.optionalTools)
            {
                if ((!tool.equippedItem && player.inventory.Count(new Item(tool.requiredItem)) >= 1) || (tool.equippedItem && player.Tools_checkHasEquipment(tool.requiredItem)))
                {
                    proba += tool.modifyCriticalProbability;
                    break;
                }
            }
        }

        return proba;
    }

    // -----------------------------------------------------------------------------------
    // HarvestingDuration
    // -----------------------------------------------------------------------------------
    public float HarvestingDuration(bool boost)
    {
        float duration = 0f;
        int level = 0;

        HarvestingProfessionRequirement requiredProfession = getRequiredHarvestingProfession();
        HarvestingProfession profession = getHarvestingProfession(requiredProfession);

        duration = selectedNode.harvestDuration;

        // -- Modificator: Skill

        duration += level * profession.template.durationPerSkillLevel;

        // -- Modificator: Required Tools

        foreach (HarvestingTool tool in profession.template.tools)
        {
            if ((!tool.equippedItem && player.inventory.Count(new Item(tool.requiredItem)) >= 1) || (tool.equippedItem && player.Tools_checkHasEquipment(tool.requiredItem)))
            {
                duration += tool.modifyDuration;
                if (!profession.template.requiresAllTools)
                    break;
            }
        }

        // -- Modificator: Optional Tools

        if (boost)
        {
            foreach (HarvestingTool tool in profession.template.optionalTools)
            {
                if ((!tool.equippedItem && player.inventory.Count(new Item(tool.requiredItem)) >= 1) || (tool.equippedItem && player.Tools_checkHasEquipment(tool.requiredItem)))
                {
                    duration += tool.modifyDuration;
                    break;
                }
            }
        }

        return duration;
    }

    // -----------------------------------------------------------------------------------
    // HarvestingExperience
    // -----------------------------------------------------------------------------------
    public int HarvestingExperience(bool boost)
    {
        int exp = 0;

        HarvestingProfessionRequirement requiredProfession = getRequiredHarvestingProfession();
        HarvestingProfession profession = getHarvestingProfession(requiredProfession);

        // -- Modificator: Resource Node

        exp = UnityEngine.Random.Range(selectedNode.ProfessionExperienceRewardMin, selectedNode.ProfessionExperienceRewardMax);

        // -- Modificator: Required Tools

        foreach (HarvestingTool tool in profession.template.tools)
        {
            if ((!tool.equippedItem && player.inventory.Count(new Item(tool.requiredItem)) >= 1) || (tool.equippedItem && player.Tools_checkHasEquipment(tool.requiredItem)))
            {
                exp += UnityEngine.Random.Range(tool.modifyExperienceMin, tool.modifyExperienceMax);
                if (!profession.template.requiresAllTools)
                    break;
            }
        }

        // -- Modificator: Optional Tools

        if (boost)
        {
            foreach (HarvestingTool tool in profession.template.optionalTools)
            {
                if ((!tool.equippedItem && player.inventory.Count(new Item(tool.requiredItem)) >= 1) || (tool.equippedItem && player.Tools_checkHasEquipment(tool.requiredItem)))
                {
                    exp += UnityEngine.Random.Range(tool.modifyExperienceMin, tool.modifyExperienceMax);
                    break;
                }
            }
        }

        return exp;
    }

    // ================================== PROFESSIONS ====================================

    // -----------------------------------------------------------------------------------
    // getHarvestingProfession
    // -----------------------------------------------------------------------------------
    public HarvestingProfession getHarvestingProfession(HarvestingProfessionTemplate tmpl)
    {
        if (HasHarvestingProfession(tmpl))
        {
            int id = Professions.FindIndex(x => x.templateName == tmpl.name);
            return Professions[id];
        }

        return new HarvestingProfession();
    }

    // -----------------------------------------------------------------------------------
    // getHarvestingProfession
    // Returns the required profession of the player, that the player is capable of using
    // -----------------------------------------------------------------------------------
    public HarvestingProfession getHarvestingProfession(HarvestingProfessionRequirement[] tmpls)
    {
        foreach (HarvestingProfessionRequirement tmpl in tmpls)
        {
            if (HasHarvestingProfessionLevel(tmpl.template, tmpl.level))
            {
                int id = Professions.FindIndex(x => x.templateName == tmpl.template.name);
                return Professions[id];
            }
        }

        return new HarvestingProfession();
    }

    // -----------------------------------------------------------------------------------
    // getHarvestingProfession
    // Returns the required profession of the player, that the player is capable of using
    // -----------------------------------------------------------------------------------
    public HarvestingProfession getHarvestingProfession(HarvestingProfessionRequirement tmpl)
    {
        if (HasHarvestingProfessionLevel(tmpl.template, tmpl.level))
        {
            int id = Professions.FindIndex(x => x.templateName == tmpl.template.name);
            return Professions[id];
        }

        return new HarvestingProfession();
    }

    // -----------------------------------------------------------------------------------
    // getRequiredHarvestingProfession
    // Returns the required profession to harvest the selected resource node
    // -----------------------------------------------------------------------------------
    public HarvestingProfessionRequirement getRequiredHarvestingProfession()
    {
        if (selectedNode == null) return null;

        foreach (HarvestingProfessionRequirement tmpl in selectedNode.interactionRequirements.harvestProfessionRequirements)
        {
            if (HasHarvestingProfessionLevel(tmpl.template, tmpl.level))
            {
                return tmpl;
            }
        }

        return null;
    }

    // -----------------------------------------------------------------------------------
    // getHarvestingProfessionTemplate
    // Returns the required template to harvest the selected resource node
    // -----------------------------------------------------------------------------------
    public HarvestingProfessionTemplate getHarvestingProfessionTemplate()
    {
        if (selectedNode == null) return null;

        foreach (HarvestingProfessionRequirement tmpl in selectedNode.interactionRequirements.harvestProfessionRequirements)
        {
            if (HasHarvestingProfessionLevel(tmpl.template, tmpl.level))
            {
                return tmpl.template;
            }
        }

        return null;
    }

    // -----------------------------------------------------------------------------------
    // getProfessionExp
    // -----------------------------------------------------------------------------------
    public long getHarvestingProfessionExp(HarvestingProfession aProf)
    {
        int id = Professions.FindIndex(prof => prof.templateName == aProf.templateName);
        return Professions[id].experience;
    }

    // -----------------------------------------------------------------------------------
    // SetProfession
    // -----------------------------------------------------------------------------------
    public void SetHarvestingProfession(HarvestingProfession aProf)
    {
        int id = Professions.FindIndex(pr => pr.templateName == aProf.template.name);
        Professions[id] = aProf;
    }

    // -----------------------------------------------------------------------------------
    // HasHarvestingProfession
    // -----------------------------------------------------------------------------------
    public bool HasHarvestingProfession(HarvestingProfessionTemplate profession)
    {
        return Professions.Any(x => x.templateName == profession.name);
    }


    // -----------------------------------------------------------------------------------
    // HasHarvestingProfessions
    // -----------------------------------------------------------------------------------
    public bool HasHarvestingProfessions(HarvestingProfessionRequirement[] tmpls, bool requiresAll = false)
    {
        if (tmpls == null || tmpls.Length == 0) return true;

        bool valid = false;

        foreach (HarvestingProfessionRequirement tmpl in tmpls)
        {
            if (HasHarvestingProfessionLevel(tmpl.template, tmpl.level))
            {
                valid = true;
                if (!requiresAll) return valid;
            }
            else
            {
                valid = false;
            }
        }

        return valid;
    }

    // -----------------------------------------------------------------------------------
    // HasHarvestingProfessionLevel
    // -----------------------------------------------------------------------------------
    public bool HasHarvestingProfessionLevel(HarvestingProfessionTemplate aProf, int level)
    {
        if (HasHarvestingProfession(aProf))
        {
            var tmpProf = getHarvestingProfession(aProf);
            if (tmpProf.level >= level) return true;
        }
        return false;
    }

#region SERVER ONLY


    // -----------------------------------------------------------------------------------
    // HarvestingProfession
    // -----------------------------------------------------------------------------------
    public HarvestingProfession getHarvestingProfessionData(HarvestingProfessionTemplate tmpl)
    {
        return Professions.First(x => x.templateName == tmpl.name);
    }
#endregion SERVER ONLY
    // -----------------------------------------------------------------------------------
}

#endif
