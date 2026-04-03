using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if _iMMOCRAFTING

// PLAYER

public class PlayerCraftingExtended : NetworkBehaviour
{
    public Player player;

    [Header("[-=-[ Crafting Template ]-=-]")]
    public Tmpl_PlayerCrafting playerCraftingConfiguration;

    public List<Tmpl_Recipe> playerSpecificRecipes;

    public readonly SyncList<CraftingProfession> Crafts = new SyncList<CraftingProfession>();
    public readonly SyncList<string> _recipes = new SyncList<string>();
    [HideInInspector] public InteractableWorkbench selectedWorkbench;

    [HideInInspector] public bool quickCraft;

    protected bool craftBooster = false;
    protected int craftAmount = 1;
    protected Tmpl_Recipe myRecipe;
    protected UI_Crafting _UI_Crafting;


    // -----------------------------------------------------------------------------------
    // OnSelect_InteractableWorkbench
    // Selection handling for a clicked workbench, client side only
    // @Client
    // -----------------------------------------------------------------------------------
    [Client]
    public void OnSelect_InteractableWorkbench(InteractableWorkbench _selectedWorkbench)
    {
        selectedWorkbench = _selectedWorkbench;

        Tools_CraftingProfessionRequirement requiredProfession = getRequiredCraftingProfession();

        if (requiredProfession != null)
        {
            List<Tmpl_Recipe> recipes = new List<Tmpl_Recipe>();

            // -- filter recipes that match the workbench's profession
            // -- filter recipes that match the players recipe list
            recipes.AddRange(
                    Tmpl_Recipe.All.Values.ToList().Where(
                        x => x.requiredCraft == requiredProfession.template &&
                        _recipes.Any(r => r == x.name)
                        )
                    );

            if (recipes.Count > 0)
            {
                if (!_UI_Crafting)
                    _UI_Crafting = FindObjectOfType<UI_Crafting>();

                _UI_Crafting.Show(selectedWorkbench.gameObject, requiredProfession.template, recipes);
            }
            else
            {
                player.Tools_PopupShow(selectedWorkbench.nothingMessage);
            }
        }
        else
        {
            player.Tools_PopupShow(selectedWorkbench.nothingMessage);
        }
    }

    // -----------------------------------------------------------------------------------
    // Crafting_LearnRecipe
    // -----------------------------------------------------------------------------------
    public bool Crafting_LearnRecipe(Tmpl_Recipe recipe)
    {
        if (!_recipes.Any(s => s == recipe.name))
        {
            _recipes.Add(recipe.name);
            player.Tools_ShowPopup(playerCraftingConfiguration.craftingPopupMessages.learnedMessage + recipe.name);
            return true;
        }

        return false;
    }

    // -----------------------------------------------------------------------------------
    // Crafting_LearnRecipe
    // -----------------------------------------------------------------------------------
    public bool Crafting_LearnRecipe(Tmpl_Recipe[] recipes)
    {
        bool valid = false;

        foreach (Tmpl_Recipe recipe in recipes)
            valid = Crafting_LearnRecipe(recipe);

        return valid;
    }

    // -----------------------------------------------------------------------------------
    // Crafting_isBusy
    // -----------------------------------------------------------------------------------
    public bool Crafting_isBusy()
    {
        return myRecipe != null && player.Tools_checkTimer();
    }

    // -----------------------------------------------------------------------------------
    // Crafting_CraftValidation
    // -----------------------------------------------------------------------------------
    public bool Crafting_CraftValidation(Tmpl_Recipe recipe, int amount, bool boost, bool craftable = true)
    {
        bool valid = true;

        // ----- set crafting booster
        craftBooster = boost;

        // ----- check profession level
        if (!HasCraftingProfessionLevel(recipe.requiredCraft, recipe.minSkillLevel))
            valid = false;

        if (!craftable) return valid;

        // ----- check ingredients
        foreach (CraftingRecipeIngredient ingredient in recipe.ingredients)
        {
            if (player.inventory.Count(new Item(ingredient.item)) < (ingredient.amount * amount))
                valid = false;
        }

        // ----- check mana
        if (recipe.manaCost > 0 && player.mana.current < recipe.manaCost)
            valid = false;

        // ----- check tools
        int check = 0;
        foreach (CraftingRecipeTool tool in recipe.tools)
        {
            if ((!tool.equippedItem && player.inventory.Count(new Item(tool.requiredItem)) >= 1) || (tool.equippedItem && player.Tools_checkHasEquipment(tool.requiredItem)))
                check++;
        }

        if (recipe.requiresAllTools)
        {
            if (check < recipe.tools.Length) valid = false;
        }
        else
        {
            if (check <= 0 && recipe.tools.Length > 0) valid = false;
        }

        return valid;
    }

    // -----------------------------------------------------------------------------------
    // OnDamageDealt_cancelCrafting
    // Custom Hook
    // -----------------------------------------------------------------------------------
    private void OnDamageDealt_cancelCrafting()
    {
        CancelCrafting(quickCraft);

        if (isServer)
            Target_Crafting_cancelCraftingClient(connectionToClient, quickCraft);
    }

    // -----------------------------------------------------------------------------------
    // Cmd_Crafting_unlearnRecipe
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_Crafting_unlearnRecipe(string recipeName)
    {
        _recipes.Remove(recipeName);
    }

    // -----------------------------------------------------------------------------------
    // Crafting_startCrafting
    // -----------------------------------------------------------------------------------
    public void Crafting_startCrafting(Tmpl_Recipe recipe, int amount, bool boost, bool quickCraft)
    {
        // Debug: Check if the recipe is null  
        if (recipe == null)
        {
            Debug.LogError("Crafting_startCrafting: Recipe is null!");
            return;
        }
        else
        {
            Debug.Log($"Crafting_startCrafting: Recipe '{recipe.name}' is provided.");
        }

        // Debug: Check if player is null  
        if (player == null)
        {
            Debug.LogError("Crafting_startCrafting: Player is null!");
            return;
        }
        else
        {
            Debug.Log("Crafting_startCrafting: Player is initialized.");
        }

        myRecipe = recipe;
        craftAmount = amount;
        this.quickCraft = quickCraft;

        // Debug: Check validation states  
        bool workbenchValid = quickCraft || WorkbenchAndQuickCraftValidation();
        bool craftingValid = Crafting_CraftValidation(myRecipe, amount, boost);

        // Add condition for quick craft with amount 1  
        if (quickCraft && amount != 1)
        {
            Debug.LogWarning("Crafting_startCrafting: Quick crafting is enabled, but amount is not 1. Adjusting to 1.");
            amount = 1; // Set amount to 1 for quick crafting  
        }

        Debug.Log($"Crafting_startCrafting: Workbench Valid: {workbenchValid}, Crafting Valid: {craftingValid}");

        if (workbenchValid && craftingValid)
        {
            Debug.Log("Crafting_startCrafting: Starting crafting process.");

            player.Tools_addTask();
            Debug.Log("Crafting_startCrafting: Added crafting task to player's tools.");

            float duration = CraftingDuration(myRecipe, boost) * amount;
            Debug.Log($"Crafting_startCrafting: Calculated duration: {duration}");

            player.Tools_setTimer(duration);
            player.Tools_CastbarShow(myRecipe.name, duration);
            Debug.Log($"Crafting_startCrafting: Timer set for {duration} seconds for recipe '{myRecipe.name}'.");

            var animation = CraftingAnimation();
            if (animation == null)
            {
                Debug.LogError("Crafting_startCrafting: Animation object is null!");
                return;
            }

            if (animation.playerAnimation == null)
            {
                Debug.LogError("Crafting_startCrafting: Player animation is null!");
                return;
            }

            if (animation.startPlayerSound == null)
            {
                Debug.LogError("Crafting_startCrafting: Start player sound is null!");
                return;
            }

            // Debug: Starting animation  
            Debug.Log("Crafting_startCrafting: Starting animation.");
            player.StartAnimation(animation.playerAnimation, animation.startPlayerSound);
        }
        else
        {
            Debug.LogWarning("Crafting_startCrafting: Crafting process cannot start due to validation failure.");
        }
    }

    // -----------------------------------------------------------------------------------
    // Cmd_Crafting_finishCrafting
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_Crafting_finishCrafting(GameObject _selectedWorkbench, string recipeName, int amount, bool boost, bool quickCraft)
    {
#if _SERVER

        if (!quickCraft)
            selectedWorkbench = _selectedWorkbench.GetComponent<InteractableWorkbench>();

        CraftingResult craftingResult = CraftingResult.None;

        // -- validation (in case something changed during the craft duration)

        if ((!quickCraft ? selectedWorkbench : true) && Tmpl_Recipe.All.TryGetValue(recipeName.GetStableHashCode(), out myRecipe))
        {
            if (Crafting_CraftValidation(myRecipe, amount, boost))
            {
                player.Tools_removeTask();

                // --------------------------------------------------------------------------- Experience

                var prof = getCraftingProfession(myRecipe.requiredCraft);
                int oldLevel = prof.level;
                int exp = CraftingExperience(myRecipe, amount, boost);
                prof.experience += exp;
                SetCraftingProfession(prof);

#if _iMMOTOOLS
                if (oldLevel < prof.level && !quickCraft)
                    player.Tools_ShowPopup(selectedWorkbench.levelUpMessage + prof.templateName + " [Level : " + prof.level + " ]");
#endif
#if _iMMOTITLES
                if (oldLevel < prof.level && prof.template.eanTitles.Count() > 0 && prof.template.eanTitles[prof.level - 1] != null)
                    player.playerTitles.EarnTitle(prof.template.eanTitles[prof.level - 1]);
#endif
                // --------------------------------------------------------------------------- Skill Check

                float successProbability = CraftingProbability(myRecipe, boost);
                float criticalProbability = CraftingCriticalProbability(myRecipe, boost);

                for (int i = 1; i <= amount; i++)

                    if (UnityEngine.Random.value <= successProbability)
                    {
                        // ---- Success

                        craftingResult = CraftingResult.Success;

                        player.experience.current += myRecipe.experience;
                       // ((PlayerSkills)player.skills).skillExperience += myRecipe.skillExp;

                        float critProba = myRecipe.criticalProbability += myRecipe.criticalResultPerSkillLevel * oldLevel;

                        // ---- gain default or critical item

                        if (UnityEngine.Random.value <= criticalProbability)
                        {
                            craftingResult = CraftingResult.CriticalSuccess;

                            if (myRecipe.criticalResult.Length > 0)
                            {
                                foreach (CraftingRecipeIngredient result in myRecipe.criticalResult)
                                    player.inventory.Add(new Item(result.item), result.amount);
                            }
                            else
                            {
                                foreach (CraftingRecipeIngredient result in myRecipe.defaultResult)
                                    player.inventory.Add(new Item(result.item), result.amount);
                            }
                        }
                        else
                        {
                            foreach (CraftingRecipeIngredient result in myRecipe.defaultResult)
                                player.inventory.Add(new Item(result.item), result.amount);
                        }

#if _iMMOCRAFTING && _iMMOQUESTS
                        //IncreaseCraftCounterFor(myRecipe);
                        player.playerExtendedQuest.IncreaseCraftCounterFor(myRecipe);
#endif
                    }
                    else
                    {
                        // --------------------------------------------------------------------------- Failure

                        craftingResult = CraftingResult.Failure;

                        // --------------------------------------------------------------------------- [Optional] Gain failure item

                        float failProba = myRecipe.failureProbability += myRecipe.failureResultPerSkillLevel * oldLevel;

                        if (UnityEngine.Random.value <= failProba)
                        {
                            foreach (CraftingRecipeIngredient result in myRecipe.failureResult)
                                player.inventory.Add(new Item(result.item), result.amount);
                        }
                    }

                // --------------------------------------------------------------------------- Deplete Ingredients

                foreach (CraftingRecipeIngredient ingredient in myRecipe.ingredients)
                {
                    if (craftingResult == CraftingResult.Failure && ingredient.DontDestroyOnFailure) continue;
                    if (craftingResult == CraftingResult.CriticalSuccess && ingredient.DontDestroyOnCriticalSuccess) continue;
                    Item newItem = new Item(ingredient.item);
                    player.inventory.Remove(new Item(ingredient.item), (ingredient.amount * amount));
                }

                // --------------------------------------------------------------------------- Deplete other Costs (mana etc.)
                if (myRecipe.manaCost > 0)
                    player.mana.current -= myRecipe.manaCost;
#if _iMMOSTAMINA
                if (myRecipe.staminaCost > 0)
                    player.stamina.current -= myRecipe.staminaCost;
#endif

                // --------------------------------------------------------------------------- Check Tool breakage

                foreach (CraftingRecipeTool tool in myRecipe.tools)
                {
                    if ((!tool.equippedItem && player.inventory.Count(new Item(tool.requiredItem)) >= 1) || (tool.equippedItem && player.Tools_checkHasEquipment(tool.requiredItem)))
                    {
                        if (UnityEngine.Random.value <= tool.toolDestroyChance)
                        {
                            if (tool.equippedItem)
                                player.Tools_removeEquipment(tool.requiredItem);
                            else
                                player.inventory.Remove(new Item(tool.requiredItem), 1);
                            player.Tools_TargetAddMessage(playerCraftingConfiguration.craftingPopupMessages.breakMessage);
                        }
                    }
                }

                if (boost)
                {
                    foreach (CraftingRecipeTool tool in myRecipe.optionalTools)
                    {
                        if ((!tool.equippedItem && player.inventory.Count(new Item(tool.requiredItem)) >= amount) || (tool.equippedItem && player.Tools_checkHasEquipment(tool.requiredItem)))
                        {
                            if (UnityEngine.Random.value <= tool.toolDestroyChance)
                            {
                                if (tool.equippedItem)
                                    player.Tools_removeEquipment(tool.requiredItem);
                                else
                                    player.inventory.Remove(new Item(tool.requiredItem), amount);
                                player.Tools_TargetAddMessage(playerCraftingConfiguration.craftingPopupMessages.boosterMessage);
                            }
                        }
                    }
                }

                // --------------------------------------------------------------------------- Popup Message

                if (craftingResult == CraftingResult.Failure)
                {
                    player.Tools_ShowPopup(playerCraftingConfiguration.craftingPopupMessages.failMessage);
                }
                else if (craftingResult == CraftingResult.Success)
                {
                    player.Tools_ShowPopup(playerCraftingConfiguration.craftingPopupMessages.successMessage);
                }
                else if (craftingResult == CraftingResult.CriticalSuccess)
                {
                    player.Tools_ShowPopup(playerCraftingConfiguration.craftingPopupMessages.critMessage);
                }

                // --------------------------------------------------------------------------- Cleanup
                if (!quickCraft)
                    selectedWorkbench.OnCrafted();

                Target_Crafting_cancelCraftingClient(connectionToClient, quickCraft);
            }
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // Target_Crafting_finishCraftingClient
    // -----------------------------------------------------------------------------------
    [TargetRpc]
    public void Target_Crafting_cancelCraftingClient(NetworkConnection target, bool quickCraft)
    {
        CancelCrafting(quickCraft);
    }

    // -----------------------------------------------------------------------------------
    // CraftingProfessionTemplate
    // -----------------------------------------------------------------------------------
    public CraftingProfessionTemplate CraftingAnimation()
    {
        Tools_CraftingProfessionRequirement requiredProfession = getRequiredCraftingProfession();

        if (requiredProfession != null)
            return requiredProfession.template;

        return null;
    }

    // -----------------------------------------------------------------------------------
    // CraftingValidation
    // -----------------------------------------------------------------------------------
    public bool WorkbenchAndQuickCraftValidation()
    {
        bool bValid = ((selectedWorkbench != null && selectedWorkbench.checkInteractionRange(player) && selectedWorkbench.interactionRequirements.checkState(player)) || this.quickCraft);

        if (!bValid)
            CancelCrafting(quickCraft);

        return bValid;
    }

    // -----------------------------------------------------------------------------------
    // finishCraftingClient
    // -----------------------------------------------------------------------------------
    public void CancelCrafting(bool quickCraft)
    {
        if ((selectedWorkbench != null && !quickCraft) || (myRecipe != null && quickCraft))
        {
            if (this.quickCraft)
                this.quickCraft = false;
            player.Tools_stopTimer();
            player.Tools_removeTask();
            player.Tools_CastbarHide();

            if (myRecipe != null && CraftingAnimation() != null)
                player.StopAnimation(CraftingAnimation().playerAnimation, CraftingAnimation().stopPlayerSound);

            craftBooster = false;
            craftAmount = 1;

            myRecipe = null;
            selectedWorkbench = null;
        }
    }

    // -----------------------------------------------------------------------------------
    // LateUpdate
    // -----------------------------------------------------------------------------------
    [ClientCallback]
    private void LateUpdate()
    {
        if (WorkbenchAndQuickCraftValidation())
        {
            if (myRecipe && player.Tools_checkTimer())
            {
                Cmd_Crafting_finishCrafting(!quickCraft ? selectedWorkbench.gameObject : null, myRecipe.name, craftAmount, craftBooster, quickCraft);
                CancelCrafting(quickCraft);
            }
        }
    }

    // ============================== DURATION/PROBABILITY ===============================

    // -----------------------------------------------------------------------------------
    // Crafting_CanBoost
    // -----------------------------------------------------------------------------------
    public bool Crafting_CanBoost(Tmpl_Recipe recipe, int amount)
    {
        if (recipe.optionalTools.Length <= 0) return false;

        bool bValid = false;

        foreach (CraftingRecipeTool tool in recipe.optionalTools)
        {
            if (
                (!tool.equippedItem && player.inventory.Count(new Item(tool.requiredItem)) >= amount) ||
                (tool.equippedItem && tool.requiredItem.maxStack == 1 && player.Tools_checkHasEquipment(tool.requiredItem)) ||
                (tool.equippedItem && tool.requiredItem.maxStack > 1 && player.Tools_checkDepletableEquipment(tool.requiredItem))
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
    // CraftingProbability
    // -----------------------------------------------------------------------------------
    public float CraftingProbability(Tmpl_Recipe recipe, bool boost)
    {
        float proba = 0f;
        int level = 0;

        if (recipe != null)
        {
            proba = recipe.probability;

            // -- Modificator: Skill

            if (recipe.requiredCraft)
                level = getCraftingProfessionLevel(recipe.requiredCraft);

            proba += level * recipe.probabilityPerSkillLevel;

            // -- Modificator: Required Tools

            foreach (CraftingRecipeTool tool in recipe.tools)
            {
                if ((!tool.equippedItem && player.inventory.Count(new Item(tool.requiredItem)) >= 1) || (tool.equippedItem && player.Tools_checkHasEquipment(tool.requiredItem)))
                {
                    proba += tool.modifyProbability;
                    if (!recipe.requiresAllTools)
                        break;
                }
            }

            // -- Modificator: Optional Tools

            if (boost)
            {
                foreach (CraftingRecipeTool tool in recipe.optionalTools)
                {
                    if ((!tool.equippedItem && player.inventory.Count(new Item(tool.requiredItem)) >= 1) || (tool.equippedItem && player.Tools_checkHasEquipment(tool.requiredItem)))
                    {
                        proba += tool.modifyProbability;
                        break;
                    }
                }
            }
        }

        return proba;
    }

    // -----------------------------------------------------------------------------------
    // CraftingCriticalProbability
    // -----------------------------------------------------------------------------------
    public float CraftingCriticalProbability(Tmpl_Recipe recipe, bool boost)
    {
        float proba = 0f;
        int level = 0;

        if (recipe != null)
        {
            proba = recipe.criticalProbability;

            // -- Modificator: Skill

            if (recipe.requiredCraft)
                level = getCraftingProfessionLevel(recipe.requiredCraft);

            proba += level * recipe.probabilityPerSkillLevel;

            // -- Modificator: Required Tools

            foreach (CraftingRecipeTool tool in recipe.tools)
            {
                if ((!tool.equippedItem && player.inventory.Count(new Item(tool.requiredItem)) >= 1) || (tool.equippedItem && player.Tools_checkHasEquipment(tool.requiredItem)))
                {
                    proba += tool.modifyCriticalProbability;
                    if (!recipe.requiresAllTools)
                        break;
                }
            }

            // -- Modificator: Optional Tools

            if (boost)
            {
                foreach (CraftingRecipeTool tool in recipe.optionalTools)
                {
                    if ((!tool.equippedItem && player.inventory.Count(new Item(tool.requiredItem)) >= 1) || (tool.equippedItem && player.Tools_checkHasEquipment(tool.requiredItem)))
                    {
                        proba += tool.modifyCriticalProbability;
                        break;
                    }
                }
            }
        }

        return proba;
    }

    // -----------------------------------------------------------------------------------
    // CraftingDuration
    // -----------------------------------------------------------------------------------
    public float CraftingDuration(Tmpl_Recipe recipe, bool boost)
    {
        float duration = 0f;
        int level = 0;

        if (recipe != null)
        {
            duration = recipe.duration;

            // -- Modificator: Skill

            if (recipe.requiredCraft)
                level = getCraftingProfessionLevel(recipe.requiredCraft);

            duration += level * recipe.durationPerSkillLevel;

            // -- Modificator: Required Tools

            foreach (CraftingRecipeTool tool in recipe.tools)
            {
                if ((!tool.equippedItem && player.inventory.Count(new Item(tool.requiredItem)) >= 1) || (tool.equippedItem && player.Tools_checkHasEquipment(tool.requiredItem)))
                {
                    duration += tool.modifyDuration;
                    if (!recipe.requiresAllTools)
                        break;
                }
            }

            // -- Modificator: Optional Tools

            if (boost)
            {
                foreach (CraftingRecipeTool tool in recipe.optionalTools)
                {
                    if ((!tool.equippedItem && player.inventory.Count(new Item(tool.requiredItem)) >= 1) || (tool.equippedItem && player.Tools_checkHasEquipment(tool.requiredItem)))
                    {
                        duration += tool.modifyDuration;
                        break;
                    }
                }
            }
        }

        return duration;
    }

    // -----------------------------------------------------------------------------------
    // CraftingExperience
    // -----------------------------------------------------------------------------------
    public int CraftingExperience(Tmpl_Recipe recipe, int amount, bool boost)
    {
        int experience = 0;

        if (recipe != null)
        {
            experience = UnityEngine.Random.Range(recipe.ProfessionExperienceRewardMin, recipe.ProfessionExperienceRewardMax) * amount;

            // -- Modificator: Required Tools

            foreach (CraftingRecipeTool tool in recipe.tools)
            {
                if ((!tool.equippedItem && player.inventory.Count(new Item(tool.requiredItem)) >= 1) || (tool.equippedItem && player.Tools_checkHasEquipment(tool.requiredItem)))
                {
                    experience += UnityEngine.Random.Range(tool.modifyExperienceMin, tool.modifyExperienceMax);
                    if (!recipe.requiresAllTools)
                        break;
                }
            }

            // -- Modificator: Optional Tools

            if (boost)
            {
                foreach (CraftingRecipeTool tool in recipe.optionalTools)
                {
                    if ((!tool.equippedItem && player.inventory.Count(new Item(tool.requiredItem)) >= 1) || (tool.equippedItem && player.Tools_checkHasEquipment(tool.requiredItem)))
                    {
                        experience += UnityEngine.Random.Range(tool.modifyExperienceMin, tool.modifyExperienceMax);
                        break;
                    }
                }
            }
        }

        return experience;
    }

    // ============================== PROFESSION ===============================

    // -----------------------------------------------------------------------------------
    // getCraftingProfession
    // -----------------------------------------------------------------------------------
    public CraftingProfession getCraftingProfession(CraftingProfessionTemplate aProf)
    {
        return Crafts.First(pr => pr.templateName == aProf.name);
    }

    // -----------------------------------------------------------------------------------
    // getCraftingProfessionLevel
    // -----------------------------------------------------------------------------------
    public int getCraftingProfessionLevel(CraftingProfessionTemplate aProf)
    {
        return Crafts.First(pr => pr.templateName == aProf.name).level;
    }

    // -----------------------------------------------------------------------------------
    // getCraftingExp
    // -----------------------------------------------------------------------------------
    public long getCraftingExp(CraftingProfession aProf)
    {
        int id = Crafts.FindIndex(prof => prof.templateName == aProf.templateName);
        return Crafts[id].experience;
    }

    // -----------------------------------------------------------------------------------
    // setCraftingProfession
    // -----------------------------------------------------------------------------------
    public void SetCraftingProfession(CraftingProfession aProf)
    {
        int id = Crafts.FindIndex(pr => pr.templateName == aProf.template.name);
        Crafts[id] = aProf;
    }

    // -----------------------------------------------------------------------------------
    // HasCraftingProfession
    // -----------------------------------------------------------------------------------
    public bool HasCraftingProfession(CraftingProfessionTemplate aProf)
    {
        return Crafts.Any(prof => prof.templateName == aProf.name);
    }

    // -----------------------------------------------------------------------------------
    // getRequiredCraftingProfession
    // Returns the required profession to access the selected workbench
    // -----------------------------------------------------------------------------------
    public Tools_CraftingProfessionRequirement getRequiredCraftingProfession()
    {
        if (selectedWorkbench == null) return null;

        foreach (Tools_CraftingProfessionRequirement tmpl in selectedWorkbench.interactionRequirements.craftProfessionRequirements)
        {
            if (tmpl != null && HasCraftingProfessionLevel(tmpl.template, tmpl.level))
            {
                return tmpl;
            }
        }

        return null;
    }

    // -----------------------------------------------------------------------------------
    // HasCraftingProfessions
    // -----------------------------------------------------------------------------------
    public bool HasCraftingProfessions(Tools_CraftingProfessionRequirement[] tmpls, bool requiresAll = false)
    {
        if (tmpls == null || tmpls.Length == 0) return true;

        bool valid = false;

        foreach (Tools_CraftingProfessionRequirement tmpl in tmpls)
        {
            if (HasCraftingProfessionLevel(tmpl.template, tmpl.level))
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
    // HasCraftingProfessionLevel
    // -----------------------------------------------------------------------------------
    public bool HasCraftingProfessionLevel(CraftingProfessionTemplate aProf, int level)
    {
        if (aProf == null || level <= 0) return true;

        if (HasCraftingProfession(aProf))
        {
            var tmpProf = getCraftingProfession(aProf);
            if (tmpProf.level >= level) return true;
        }

        return false;
    }
    // -----------------------------------------------------------------------------------
}
#endif