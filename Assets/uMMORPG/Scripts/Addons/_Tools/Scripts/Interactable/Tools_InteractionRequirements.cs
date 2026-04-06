#if _iMMOTOOLS
using System.Linq;
using UnityEngine;

// INTERACTION REQUIREMENTS CLASS
// THIS CLASS IS PRIMARILY FOR OBJECTS THE PLAYER CAN CHOOSE TO INTERACT WITH

[System.Serializable]
public partial class Tools_InteractionRequirements : Tools_Requirements
{
    [Header("[-=-=-[ COSTS (Removed after checking Requirements) ]-=-=-]")]
    [Tooltip("[Optional] These items will be removed from players inventory")]
    public Tools_ItemRequirement[] removeItems;

    [Tooltip("[Optional] Gold cost to interact")]
    public long goldCost = 0;
    [Tooltip("[Optional] Coins cost to interact")]
    public long coinCost = 0;
    [Tooltip("[Optional] Health cost to interact")]
    public int healthCost = 0;
    [Tooltip("[Optional] Mana cost to interact")]
    public int manaCost = 0;

#if _iMMOHONORSHOP
    [Tooltip("[Optional] Honor Currency costs to interact")]
    public HonorShopCurrencyDrop[] honorCurrencyCost;
#endif

    [Header("[-=-=-[ REWARDS (awarded after checks & costs (repetitive)) ]-=-=-]")]
    public int expRewardMin                     = 0;
    public int expRewardMax                     = 0;
    public int skillExpRewardMin                = 0;
    public int skillExpRewardMax                = 0;

    [Header("[-=-=-[ Chat Messages (automatic activation only) ]-=-=-]")]
    public string labelMinLevel                 = " Minimum Level: ";
    public string labelMaxLevel                 = " Maximum Level: ";
    public string labelMinHealth                = " Min. Health Percent: ";
    public string labelMinMana                  = " Min. Mana Percent: ";
    public string labelDayStart                 = " Start Day: ";
    public string labelDayEnd                   = " End Day: ";
    public string labelActiveMonth              = " Active Month: ";
    public string labelRequiredSkills           = " Skill(s): ";
    public string labelLevel                    = "LV";
    public string labelAllowedClasses           = " Allowed Class(es): ";
    public string labelRequiresGuild            = " Guild Membership";
    public string labelRequiresParty            = " Party Membership";
#if _iMMOPRESTIGECLASSES
    public string labelAllowedPrestigeClasses   = " Allowed Prestige Class(es): ";
#endif
#if _iMMOPVP
    public string labelRequiresRealm            = " Limited to Specific Realm";
#endif
    public string labelRequiresQuest            = " Quest: ";
    public string labelInProgressQuest          = "[Must be in progress]";
#if _iMMOFACTIONS
    public string labelFactionRequirements      = " Faction Ratings: ";
#endif
    public string labelRequiredEquipment        = " Equipment: ";
    public string labelRequiredItems            = " Item(s): ";
    public string labelDestroyItem              = "[Destroyed on use]";
#if _iMMOHARVESTING
    public string requiredHarvestProfessions    = " Harvesting Profession(s): ";
#endif
#if _iMMOCRAFTING
    public string requiredCraftProfessions      = " Craft Profession(s): ";
#endif
#if _iMMOMOUNTS
    public string labelMountedOnly              = " Accessible only while mounted.";
    public string labelUnmountedOnly            = " Accessible only while unmounted.";
#endif
#if _iMMOTRAVEL
    public string labelTravelroute              = " Travelroute: ";
#endif
#if _iMMOWORLDEVENTS
    public string labelWorldEvent               = " World Event: ";
#endif
#if _iMMOGUILDUPGRADES
    public string labelGuildUpgrades            = " Guild Level: ";
#endif
#if _iMMOACCOUNTUNLOCKABLES
    public string labelAccountUnlockable        = " Account Unlockable: ";
#endif
#if _iMMOPATREON
    public string labelPatreonSubscription      = " - Requires active Patreon subscription.";
#endif
    [Header("Error Chat message")]
    [Tooltip("You will need to enter the SlotChatMessage, if you are using CompleteChat make sure you have a Slot for this chat")]
    public ChannelInfo requires = new ChannelInfo("", "(Requires)", "(Requires)", null);

    public Tools_Cost requierementCost
    {
        get
        {
            return new Tools_Cost
            {
                itemCost = removeItems,
                goldCost = goldCost,
                coinCost = coinCost,
                healthCost = healthCost,
                manaCost = manaCost,
#if _iMMOSTAMINA
                staminaCost = 0,
#endif
                experienceCost = 0,
                skillExperienceCost = 0
            };
        }
    }

    // -----------------------------------------------------------------------------------
    // checkRequirements
    // -----------------------------------------------------------------------------------
    public override bool checkRequirements(Player player)
    {
        bool valid = true;

        valid = base.checkRequirements(player);

        valid = checkCosts(player, valid);

        return valid;
    }

    // -----------------------------------------------------------------------------------
    // checkCosts
    // -----------------------------------------------------------------------------------
    public bool checkCosts(Player player, bool valid)
    {
        valid = (removeItems.Length == 0 || player.Tools_checkHasItems(player, removeItems)) ? valid : false;
        valid = (goldCost == 0 || player.gold >= goldCost) ? valid : false;
        valid = (coinCost == 0 || player.itemMall.coins >= coinCost) ? valid : false;
        valid = (healthCost == 0 || player.health.current >= healthCost) ? valid : false;
        valid = (manaCost == 0 || player.mana.current >= manaCost) ? valid : false;
#if _iMMOHONORSHOP
        valid = (player.playerHonorShop.CheckHonorCurrencyCost(honorCurrencyCost)) ? valid : false;
#endif

        return valid;
    }

    // -----------------------------------------------------------------------------------
    // hasCosts
    // -----------------------------------------------------------------------------------
    public bool hasCosts()
    {
        return removeItems.Length > 0 ||
                goldCost > 0 ||
                coinCost > 0 ||
                healthCost > 0 ||
                manaCost > 0
#if _iMMOHONORSHOP
                || honorCurrencyCost.Any(x => x.amount > 0)
#endif
                ;
    }

    // -----------------------------------------------------------------------------------
    // payCosts
    // -----------------------------------------------------------------------------------
    public void payCosts(Player player)
    {
        if (checkRequirements(player))
        {
            player.Tools_removeItems(player, removeItems, true);

            if(goldCost > 0)
                player.gold -= goldCost;
            
            if(coinCost > 0)
                player.itemMall.coins -= coinCost;

            if(manaCost > 0)
               player.mana.current -= manaCost;

            if (healthCost > 0)
            {
                if (player.health.current > healthCost)
                    player.health.current -= healthCost;
                else
                    player.health.current = 1;
            }

#if _iMMOHONORSHOP
            player.playerHonorShop.PayHonorCurrencyCost(honorCurrencyCost);
#endif
        }
    }

    // -----------------------------------------------------------------------------------
    // grantRewards
    // -----------------------------------------------------------------------------------
    public void grantRewards(Player player)
    {
        if (checkRequirements(player))
        {
            player.experience.current += Random.Range(expRewardMin, expRewardMax);
            ((PlayerSkills)player.skills).skillExperience += Random.Range(skillExpRewardMin, skillExpRewardMax);
        }
    }


    // -----------------------------------------------------------------------------------
    // Update Required Chat (Auto Activation)
    // @Client
    // -----------------------------------------------------------------------------------
    public virtual void UpdateRequirementChat()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        // -- Requirements

        if (minLevel > 0)
            SendMessageChat("", requires.identifierIn, labelMinLevel + minLevel.ToString(), "", requires.textPrefab);

        if (maxLevel > 0)
            SendMessageChat("", requires.identifierIn, labelMaxLevel + maxLevel.ToString(), "", requires.textPrefab);

        if (minHealth > 0)
            SendMessageChat("", requires.identifierIn, labelMinHealth + minHealth.ToString(), "", requires.textPrefab);

        if (minMana > 0)
            SendMessageChat("", requires.identifierIn, labelMinMana + minMana.ToString(), "", requires.textPrefab);

        // TIME
        if (dayStart > 0)
            SendMessageChat("", requires.identifierIn, labelDayStart + dayStart.ToString(), "", requires.textPrefab);

        if (dayEnd > 0)
            SendMessageChat("", requires.identifierIn, labelDayEnd + dayEnd.ToString(), "", requires.textPrefab);

        if (activeMonth > 0)
            SendMessageChat("", requires.identifierIn, labelActiveMonth + activeMonth.ToString(), "", requires.textPrefab);

        if (requiredSkills.Length > 0)
        {
            foreach (Tools_SkillRequirement skill in requiredSkills)
                SendMessageChat("", requires.identifierIn, labelRequiredSkills + skill.skill.name + labelLevel + skill.level.ToString(), "", requires.textPrefab);
        }

        // CLASS

        if (allowedClasses.Length > 0)
        {
            string temp_classes = "";
            foreach (GameObject classes in allowedClasses)
                temp_classes += " " + classes.name;
            SendMessageChat("", requires.identifierIn, labelAllowedClasses + temp_classes, "", requires.textPrefab);
        }

        // PARTY & GUILD

        if (requiresParty)
            SendMessageChat("", requires.identifierIn, labelRequiresParty, "", requires.textPrefab);

        if (requiresGuild)
            SendMessageChat("", requires.identifierIn, labelRequiresGuild, "", requires.textPrefab);

#if _iMMOPRESTIGECLASSES
        if (allowedPrestigeClasses.Length > 0)
        {
            string temp_classes = "";
            foreach (PrestigeClassTemplate classes in allowedPrestigeClasses)
                temp_classes += " " + classes.name;
            SendMessageChat("", requires.identifierIn, labelAllowedPrestigeClasses + temp_classes, "", requires.textPrefab);
        }
#endif

#if _iMMOPVP
        if (requiredRealm != null && requiredAlly != null)
            SendMessageChat("", requires.identifierIn, labelRequiresRealm, "", requires.textPrefab);
#endif

#if _iMMOQUESTS
        if (requiredQuest != null)
        {
            if (!questMustBeInProgress)
            {
                SendMessageChat("", requires.identifierIn, labelRequiresQuest + requiredQuest.name, "", requires.textPrefab);
            }
            else
            {
                SendMessageChat("", requires.identifierIn, labelRequiresQuest + requiredQuest.name + labelInProgressQuest, "", requires.textPrefab);
            }
        }
#else
        if (requiredQuest != null)
            SendMessageChat("", requires.identifierIn, labelRequiresQuest + requiredQuest.name, "", requires.textPrefab);
#endif

#if _iMMOFACTIONS
        if (factionRequirements.Length > 0)
        {
            foreach (FactionRequirement factionRequirement in factionRequirements)
                SendMessageChat("", requires.identifierIn, labelFactionRequirements + factionRequirement.faction.name, "", requires.textPrefab);
        }
#endif

        if (requiredEquipment.Length > 0)
        {
            foreach (EquipmentItem item in requiredEquipment)
                SendMessageChat("", requires.identifierIn, labelRequiredEquipment + item.name, "", requires.textPrefab);
        }

        if (requiredItems.Length > 0)
        {
            foreach (Tools_ItemRequirement item in requiredItems)
                SendMessageChat("", requires.identifierIn, labelRequiredItems + item.item.name, "", requires.textPrefab);
        }

#if _iMMOHARVESTING
        if (harvestProfessionRequirements.Length > 0)
        {
            foreach (HarvestingProfessionRequirement prof in harvestProfessionRequirements)
            {
                if (requires.textPrefab != null)
                    SendMessageChat("", requires.identifierIn, requiredHarvestProfessions + prof.template.name + " " + labelLevel + prof.level, "", requires.textPrefab);
            }


        }
#endif

#if _iMMOCRAFTING
        if (craftProfessionRequirements.Length > 0)
        {
            foreach (Tools_CraftingProfessionRequirement prof in craftProfessionRequirements)
                SendMessageChat("", requires.identifierIn, requiredCraftProfessions + prof.template.name + " " + labelLevel + prof.level, "", requires.textPrefab);
        }
#endif

#if _iMMOMOUNTS
        if (mountType == Tools_Requirements.MountType.Mounted)
        {
            SendMessageChat("", requires.identifierIn, labelMountedOnly, "", requires.textPrefab);
        }
        else if (mountType == Tools_Requirements.MountType.Unmounted)
        {
            SendMessageChat("", requires.identifierIn, labelUnmountedOnly, "", requires.textPrefab);
        }
#endif

#if _iMMOTRAVEL
        if (!string.IsNullOrWhiteSpace(requiredTravelrouteName))
        {
            SendMessageChat("", requires.identifierIn, labelTravelroute + requiredTravelrouteName, "", requires.textPrefab);
        }
#endif

#if _iMMOWORLDEVENTS
        if (worldEvent != null)
        {
            if (player.playerWorldEvents.CheckWorldEvent(worldEvent, minEventCount, maxEventCount))
            {
                if (maxEventCount == 0)
                    SendMessageChat("", requires.identifierIn, labelWorldEvent + worldEvent.name + " (" + player.playerWorldEvents.GetWorldEventCount(worldEvent) + "/" + minEventCount.ToString() + ")", "", requires.textPrefab);
                else
                    SendMessageChat("", requires.identifierIn, labelWorldEvent + worldEvent.name + " (" + minEventCount.ToString() + "-" + maxEventCount.ToString() + ") [" + player.playerWorldEvents.GetWorldEventCount(worldEvent) + "]", "", requires.textPrefab);
            }
            else
            {
                if (maxEventCount == 0)
                    SendMessageChat("", requires.identifierIn, labelWorldEvent + worldEvent.name + " (" + player.playerWorldEvents.GetWorldEventCount(worldEvent) + "/" + minEventCount.ToString() + ")", "", requires.textPrefab);
                else
                    SendMessageChat("", requires.identifierIn, labelWorldEvent + worldEvent.name + " (" + minEventCount.ToString() + "-" + maxEventCount.ToString() + ") [" + player.playerWorldEvents.GetWorldEventCount(worldEvent) + "]", "", requires.textPrefab);
            }
        }
#endif

#if _iMMOGUILDUPGRADES
        if (minGuildLevel > 0)
        {
            if (player.guild.InGuild())
                SendMessageChat("", requires.identifierIn, labelGuildUpgrades + player.playerGuildUpgrades.guildLevel.ToString() + "/" + minGuildLevel.ToString(), "", requires.textPrefab);
            else
                SendMessageChat("", requires.identifierIn, labelGuildUpgrades + "0/" + minGuildLevel.ToString(), "", requires.textPrefab);
        }
#endif

#if _iMMOACCOUNTUNLOCKABLES
        if (!string.IsNullOrWhiteSpace(accountUnlockable))
        {
            if (player.playerAccountUnlockables.HasAccountUnlock(accountUnlockable))
                SendMessageChat("", requires.identifierIn, labelAccountUnlockable + accountUnlockable, "", requires.textPrefab);
            else
                SendMessageChat("", requires.identifierIn, labelAccountUnlockable + accountUnlockable, "", requires.textPrefab);
        }
#endif

#if _iMMOPATREON
        if (activeMinPatreon > 0)
        {
            if (player.playerPatreonManager.HasActivePatreonSubscription(activeMinPatreon))
               SendMessageChat("", requires.identifierIn, labelPatreonSubscription, "", requires.textPrefab);
            else
                SendMessageChat("", requires.identifierIn, labelPatreonSubscription, "", requires.textPrefab);
        }
#endif

    }

    private void SendMessageChat(string sender, string identifier, string message, string replyPrefix, GameObject textPrefab)
    {
        if (requires.textPrefab != null)
        {
#if _iMMOCOMPLETECHAT
            UICompleteChat.singleton.AddMessage(new ChatMessage(sender, identifier, message, replyPrefix, textPrefab));
#else
            UIChat.singleton.AddMessage(new ChatMessage(sender, identifier, message, replyPrefix, textPrefab));
#endif
        }
        else
        {
            Debug.LogWarning("Warning" + this +" does not contain a slotmessage so it cannot be displayed on the chat!");
        }
    }
    // -----------------------------------------------------------------------------------
}
#endif