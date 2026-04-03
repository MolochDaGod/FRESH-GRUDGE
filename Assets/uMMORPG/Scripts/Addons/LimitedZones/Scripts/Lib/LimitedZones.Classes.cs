using System.Collections.Generic;
using UnityEngine;

// =======================================================================================
// PlayerCountAreaLimitation
// =======================================================================================
[System.Serializable]
public class PlayerGroupLocations
{
	public Transform[] teleportPosition;
}

// =======================================================================================
// LimitedZonesEntry
// =======================================================================================
[System.Serializable]
public class LimitedZonesEntry
{
	
	public Sprite image;
	public string title;
	public string description;
	
	public int instanceCategory;
	public LimitedZonesArea targetArea;

    [Header("[-=-=-[ Enter Price cost ]-=-=-]")]
    public int goldPrice = 0;
    public int expPointPrice = 0;
    public int skillPointPrice = 0;
    public int coinPrice = 0;


#if _iMMOHONORSHOP
    public HonorShopCurrencyCost entranceCost;
#endif

   	// -----------------------------------------------------------------------------------
    // getPlayerCount
    // Returns the amount of players currently in the target area
    // -----------------------------------------------------------------------------------
	public int getPlayerCount
	{
		get { return targetArea.getPlayerCount; }
	}
	
	// -----------------------------------------------------------------------------------
    // getMaxPlayerCount
    // Returns the maximum amount of players (per group) that can enter the area
    // -----------------------------------------------------------------------------------
	public int getMaxPlayerCount
	{
		get { return targetArea.maxPlayersPerGroup; }
	}
	
	// -----------------------------------------------------------------------------------
    // getGroupCount
    // Returns the maximum amount of groups that can enter the target area
    // -----------------------------------------------------------------------------------
	public int getGroupCount
	{
		get { return targetArea.getGroupCount; }
	}
	
	// -----------------------------------------------------------------------------------
    // getGroupCount
    // Returns the maximum amount of groups that can enter the target area
    // -----------------------------------------------------------------------------------
	public int getMaxGroupCount
	{
		get { return targetArea.getMaxGroupCount; }
	}
   	
   	// -----------------------------------------------------------------------------------
    // canPlayerSee
    // Checks if a player is allowed to see the target area in the list
    // -----------------------------------------------------------------------------------
   	public bool canPlayerSee(Player player)
   	{
   		if (!targetArea || !player) return false;
   		return targetArea.canPlayerSee(player);
   	}
   	
   	// -----------------------------------------------------------------------------------
    // canPlayerEnter
    // Checks if a player is allowed to enter the target area
    // -----------------------------------------------------------------------------------
   	public bool canPlayerEnter(Player player)
   	{
   		if (!targetArea || !player) return false;
   		return checkEntranceCost(player) && targetArea.canPlayerEnter(player);
   	}
   	
   	// ================================= COST FUNCTIONS ====================================
   	
   	// -----------------------------------------------------------------------------------
    // checkEntranceCost
    // 
    // -----------------------------------------------------------------------------------
   	protected bool checkEntranceCost(Player player)
   	{
        bool _goldAccess = true;

        if ((goldPrice > 0 && player.gold < goldPrice) || 
            (expPointPrice > 0 && player.experience.current < expPointPrice) || 
            (((PlayerSkills)player.skills).skillExperience < skillPointPrice) ||
            (coinPrice > 0 && player.itemMall.coins < coinPrice)
            )
        {
            _goldAccess = false;
        }


#if _iMMOHONORSHOP
        if(_goldAccess)
   		_goldAccess = player.playerHonorShop.GetHonorCurrency(entranceCost.honorCurrency) >= entranceCost.amount || entranceCost.honorCurrency == null;
#endif
        return _goldAccess;

    }
      
    // -----------------------------------------------------------------------------------
    // payEntranceCost
    // 
    // -----------------------------------------------------------------------------------
   	public void payEntranceCost(Player player)
   	{
#if _iMMOHONORSHOP
        if (entranceCost.amount > 0)
   			player.playerHonorShop.AddHonorCurrency(entranceCost.honorCurrency, entranceCost.amount * -1);
#endif
        if(goldPrice > 0 && player.gold >= goldPrice)
        {
            player.gold -= goldPrice;
        }
        if(expPointPrice > 0 && player.experience.current >= expPointPrice)
        {
            player.experience.current -= expPointPrice;
        }
        if (((PlayerSkills)player.skills).skillExperience >= skillPointPrice)
        {
            ((PlayerSkills)player.skills).skillExperience -= skillPointPrice;
        }
        if(coinPrice > 0 && player.itemMall.coins >= coinPrice)
        {
            player.itemMall.coins -= coinPrice;
        }
    }
   	
   	// -----------------------------------------------------------------------------------
    // teleportPlayerToInstance
    // -----------------------------------------------------------------------------------
   	public void teleportPlayerToInstance(Player player, int instanceCategory, int instanceIndex)
   	{
   	
   		if (!canPlayerEnter(player)) return;

        int index = targetArea.getGroupIndex(player);
   		
   		if (index == -1)
   			index = 0;

       

        player.Cmd_teleportPlayerToInstance(index, instanceCategory, instanceIndex);

    }
   	
   	// ================================= UI FUNCTIONS ====================================
   	
    // -----------------------------------------------------------------------------------
    // getGroupType
    // 
    // -----------------------------------------------------------------------------------
   	public string getGroupType() 
    {
    	
   		if (targetArea.playerGroupType == GroupType.Party) {
   			return "Party";
   		} else if (targetArea.playerGroupType == GroupType.Guild) {
   			return "Guild";
   		} else if (targetArea.playerGroupType == GroupType.Realm) {
   			return "Realm";
   		}
   		
   		return "Open";
   		
    }
   	
   	// -----------------------------------------------------------------------------------
    // getPlayerCountText
    // 
    // -----------------------------------------------------------------------------------
   	public string getPlayerCountText()
   	{
   		
   		if (targetArea.getMaxPlayerCount > 0)
   			return getPlayerCount + "/" + getMaxPlayerCount;
   		
   		return "1+";
   		
   	}
   	
   	// -----------------------------------------------------------------------------------
    // getGroupCountText
    // Returns a formatted string to show the number of groups currently in the area
    // -----------------------------------------------------------------------------------
   	public string getGroupCountText()
   	{
   		
   		if (targetArea.playerGroupType == GroupType.None)
   			return "";
   			
   		string text = getGroupCount.ToString() + "/" + getMaxGroupCount.ToString();
   		
   		if (targetArea.playerGroupType == GroupType.Party) {
   			text  += " Parties";
   		} else if (targetArea.playerGroupType == GroupType.Guild) {
   			text  += " Guilds";
   		} else if (targetArea.playerGroupType == GroupType.Realm) {
   			text  += " Realms";
   		}
   		
   		if (getGroupCount > 0)
   		{
   			text += " [ ";
   			foreach (string name in targetArea.groupNames)
   				text += name + " ";
   			text += "]";
   		}
   		
   		return text;
   		
   	}
   	
    // -----------------------------------------------------------------------------------
    
}
// =======================================================================================