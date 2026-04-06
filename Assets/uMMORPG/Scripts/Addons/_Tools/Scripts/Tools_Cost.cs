#if _iMMOTOOLS
using UnityEngine;

// COST - CLASS
[System.Serializable]
public class Tools_Cost
{
    [Tooltip("[Optional] These items will be removed from players inventory")]
    public Tools_ItemRequirement[] itemCost;
    public long goldCost = 0;
    public long coinCost = 0;
    public int healthCost = 0;
    public int manaCost = 0;
#if _iMMOSTAMINA
    public int staminaCost = 0;
#endif
    public int experienceCost = 0;
    public int skillExperienceCost = 0;

#if _iMMOHONORSHOP
    [Tooltip("[Optional] Honor Currency costs to use the teleporter")]
    public HonorShopCurrencyCost[] honorCurrencyCost;
#endif

    // -----------------------------------------------------------------------------------
    // checkCost
    // -----------------------------------------------------------------------------------
    public bool checkCost(Player player)
    {
        if (!player) return false;

        bool valid = true;

        valid = (itemCost == null || itemCost.Length == 0 || player.Tools_checkHasItems(player, itemCost)) ? valid : false;
        valid = (goldCost == 0 || player.gold >= goldCost) ? valid : false;
        valid = (coinCost == 0 || player.itemMall.coins >= coinCost) ? valid : false;
        valid = (healthCost == 0 || player.health.current > healthCost) ? valid : false;
        valid = (manaCost == 0 || player.mana.current >= manaCost) ? valid : false;
    #if _iMMOSTAMINA
        valid = (staminaCost == 0 || player.stamina.current >= staminaCost) ? valid : false;
    #endif
        valid = (experienceCost == 0 || player.experience.current >= experienceCost) ? valid : false;
        valid = (skillExperienceCost == 0 || ((PlayerSkills)player.skills).skillExperience >= skillExperienceCost) ? valid : false;

#if _iMMOHONORSHOP
        valid = (player.playerHonorShop.CheckHonorCurrencyCost(honorCurrencyCost)) ? valid : false;
#endif

        return valid;
    }

    // -----------------------------------------------------------------------------------
    // payCost
    // -----------------------------------------------------------------------------------
    public void payCost(Player player)
    {
        if (!player || !checkCost(player)) return;

        player.Tools_removeItems(player, itemCost ?? new Tools_ItemRequirement[0]);

        if(goldCost > 0)
            player.gold -= goldCost;
        if(coinCost > 0)
            player.itemMall.coins -= coinCost;
        if(healthCost > 0)
        player.health.current -= healthCost;
        if(manaCost > 0)
            player.mana.current -= manaCost;
#if _iMMOSTAMINA
        if (staminaCost > 0)
            player.stamina.current -= staminaCost;
#endif
        if(experienceCost > 0)
            player.experience.current -= experienceCost;
        if(skillExperienceCost > 0)
            ((PlayerSkills)player.skills).skillExperience -= skillExperienceCost;

#if _iMMOHONORSHOP
        player.playerHonorShop.PayHonorCurrencyCost(honorCurrencyCost);
#endif
    }

    public bool CheckCost(Player player) => checkCost(player);

    public void PayCost(Player player) => payCost(player);

    public bool HasCosts()
    {
        return (itemCost != null && itemCost.Length > 0) ||
               goldCost > 0 ||
               coinCost > 0 ||
               healthCost > 0 ||
               manaCost > 0 ||
#if _iMMOSTAMINA
               staminaCost > 0 ||
#endif
               experienceCost > 0 ||
               skillExperienceCost > 0;
    }

    public System.Collections.Generic.List<ListCostValue> ListCost(Player player)
    {
        var values = new System.Collections.Generic.List<ListCostValue>();

        if (itemCost != null)
        {
            foreach (Tools_ItemRequirement item in itemCost)
            {
                if (item != null && item.item != null && item.amount > 0)
                {
                    values.Add(new ListCostValue
                    {
                        requireName = item.item.name,
                        amout = item.amount,
                        completed = player != null && player.Tools_checkHasItems(player, new[] { item })
                    });
                }
            }
        }

        if (goldCost > 0)
            values.Add(new ListCostValue { requireName = "Gold", amout = goldCost, completed = player != null && player.gold >= goldCost });
        if (coinCost > 0)
            values.Add(new ListCostValue { requireName = "Coins", amout = coinCost, completed = player != null && player.itemMall.coins >= coinCost });
        if (healthCost > 0)
            values.Add(new ListCostValue { requireName = "Health", amout = healthCost, completed = player != null && player.health.current > healthCost });
        if (manaCost > 0)
            values.Add(new ListCostValue { requireName = "Mana", amout = manaCost, completed = player != null && player.mana.current >= manaCost });
#if _iMMOSTAMINA
        if (staminaCost > 0)
            values.Add(new ListCostValue { requireName = "Stamina", amout = staminaCost, completed = player != null && player.stamina.current >= staminaCost });
#endif
        if (experienceCost > 0)
            values.Add(new ListCostValue { requireName = "Experience", amout = experienceCost, completed = player != null && player.experience.current >= experienceCost });
        if (skillExperienceCost > 0)
            values.Add(new ListCostValue { requireName = "Skill Experience", amout = skillExperienceCost, completed = player != null && ((PlayerSkills)player.skills).skillExperience >= skillExperienceCost });

        return values;
    }
}

[System.Serializable]
public class ListCostValue
{
    public string requireName;
    public long amout;
    public bool completed;
}
#endif