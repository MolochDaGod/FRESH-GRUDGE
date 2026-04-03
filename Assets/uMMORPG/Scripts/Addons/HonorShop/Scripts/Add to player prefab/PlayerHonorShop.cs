using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// PLAYER
public class PlayerHonorShop : NetworkBehaviour
{
    public Player player;
    public Health health;

    [Header("[-=-=-[ Honor Shop Game Event ]-=-=-]")]
    public GameEvent honnorShopCurrencyEvent;

    public string MSG_GAINED = "Gained ";
    public readonly SyncList_HonorShopCurrency honorCurrencies = new SyncList_HonorShopCurrency();


    public override void OnStartLocalPlayer()
    {
        health.onEmpty.AddListener(player.OnDeath_HonorShop);
    }

    // -----------------------------------------------------------------------------------
    // AddHonorCurrency
    // -----------------------------------------------------------------------------------
    public void AddHonorCurrency(Tmpl_HonorCurrency honorCurrency, long currencyAmount)
    {
        int idx = honorCurrencies.FindIndex(x => x.honorCurrency.name == honorCurrency.name);

        if (idx == -1)
        {
            HonorShopCurrency hsc = new HonorShopCurrency();
            hsc.honorCurrency = honorCurrency;
            hsc.amount = currencyAmount;
            hsc.total += currencyAmount;
            honorCurrencies.Add(hsc);
        }
        else
        {
            HonorShopCurrency hsc = honorCurrencies.FirstOrDefault(x => x.honorCurrency.name == honorCurrency.name);
            hsc.amount += currencyAmount;
            honorCurrencies[idx] = hsc;
        }
        RefreshHonorShopCurrency();
    }

    [ClientRpc]
    public void RefreshHonorShopCurrency()
    {
        if(honnorShopCurrencyEvent)
            honnorShopCurrencyEvent.TriggerEvent();
        else
            Debug.Log(player.gameObject.name + "Game Event in PlayerHonnorShop is not assigned");
    }

    // -----------------------------------------------------------------------------------
    // GetHonorCurrency
    // -----------------------------------------------------------------------------------
    public long GetHonorCurrency(Tmpl_HonorCurrency honorCurrency)
    {
        int idx = honorCurrencies.FindIndex(x => x.honorCurrency.name == honorCurrency.name);
        if (idx != -1)
            return honorCurrencies[idx].amount;
        return 0;
    }

    // -----------------------------------------------------------------------------------
    // CheckHonorCurrencyCost
    // -----------------------------------------------------------------------------------
    public bool CheckHonorCurrencyCost(HonorShopCurrencyCost[] currencyCost)
    {
        bool valid = true;

        foreach (HonorShopCurrencyCost currency in currencyCost)
        {
            if (GetHonorCurrency(currency.honorCurrency) < currency.amount)
            {
                valid = false;
                break;
            }
        }

        return valid;
    }

    // -----------------------------------------------------------------------------------
    // CheckHonorCurrencyCost
    // -----------------------------------------------------------------------------------
    public bool CheckHonorCurrencyCost(HonorShopCurrencyDrop[] currencyCost)
    {
        bool valid = true;

        foreach (HonorShopCurrencyDrop currency in currencyCost)
        {
            if (GetHonorCurrency(currency.honorCurrency) < currency.amount)
            {
                valid = false;
                break;
            }
        }

        return valid;
    }

    // -----------------------------------------------------------------------------------
    // PayHonorCurrencyCost
    // -----------------------------------------------------------------------------------
    public void PayHonorCurrencyCost(HonorShopCurrency[] currencyCost)
    {
        foreach (HonorShopCurrency currency in currencyCost)
            AddHonorCurrency(currency.honorCurrency, currency.amount * -1);
    }

    // -----------------------------------------------------------------------------------
    // PayHonorCurrencyCost
    // -----------------------------------------------------------------------------------
    public void PayHonorCurrencyCost(HonorShopCurrencyCost[] currencyCost)
    {
        foreach (HonorShopCurrencyCost currency in currencyCost)
            AddHonorCurrency(currency.honorCurrency, currency.amount * -1);
    }

    // -----------------------------------------------------------------------------------
    // PayHonorCurrencyCost
    // -----------------------------------------------------------------------------------
    public void PayHonorCurrencyCost(HonorShopCurrencyDrop[] currencyCost)
    {
        foreach (HonorShopCurrencyDrop currency in currencyCost)
            AddHonorCurrency(currency.honorCurrency, currency.amount * -1);
    }

    // -----------------------------------------------------------------------------------
    // GiveHonorCurrency
    // -----------------------------------------------------------------------------------
    public void GiveHonorCurrency(HonorShopCurrencyDrop[] currencyCost, int bonusPercent = 0)
    {
        foreach (HonorShopCurrencyDrop currency in currencyCost)
        {
            long amountCalc = currency.amount + ((bonusPercent > 0) ? ((currency.amount * bonusPercent) / 100) : 0);
            AddHonorCurrency(currency.honorCurrency, currency.amount);
        }
    }
    
    // -----------------------------------------------------------------------------------
    // Cmd_HonorShop
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_HonorShop(int categoryIndex, int itemIndex)
    {
#if _SERVER
        if (player.target != null && player.target.isAlive && player.isAlive && player.target is Npc npc && Utils.ClosestDistance(player, player.target) <= player.interactionRange)
        {
            Tmpl_HonorCurrency honorCurrency = npc.npcHonnorShop.itemShopCategories[categoryIndex].honorCurrency;

            long amount = GetHonorCurrency(honorCurrency);
            if (amount == -1) amount = 0;

            if (0 <= categoryIndex && categoryIndex <= npc.npcHonnorShop.itemShopCategories.Length &&
             0 <= itemIndex && itemIndex <= npc.npcHonnorShop.itemShopCategories[categoryIndex].items.Length &&
             0 < amount)
            {
                Item item = new Item(npc.npcHonnorShop.itemShopCategories[categoryIndex].items[itemIndex]);
                long currencyCost = item.GetHonorCurrency(honorCurrency);

                if (0 < item.GetHonorCurrency(honorCurrency) && currencyCost <= amount)
                {
                    if (player.inventory.Add(item, 1))
                    {
                        AddHonorCurrency(honorCurrency, currencyCost * -1);
                    }
                }
            }
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // HonorCurrency_ShareToParty
    // -----------------------------------------------------------------------------------
    public void HonorCurrency_ShareToParty(Tmpl_HonorCurrency honorCurrency, long amount)
    {
        List<Player> closeMembers = player.party.GetMembersInProximity();

        // calculate the share via ceil, so that uneven numbers
        // still result in at least total gold in the end.
        // e.g. 4/2=2 (good); 5/2=2 (1 gold got lost)
        long share = (long)Mathf.Ceil((float)amount / (float)closeMembers.Count);

        // now distribute
        foreach (Player member in closeMembers)
            member.playerHonorShop.AddHonorCurrency(honorCurrency, share);

        player.Tools_TargetAddMessage(MSG_GAINED + honorCurrency.name + " x" + share.ToString());
    }

    // -----------------------------------------------------------------------------------
    // HonorCurrency_ShareToGuild
    // -----------------------------------------------------------------------------------
    public void HonorCurrency_ShareToGuild(Tmpl_HonorCurrency honorCurrency, long amount)
    {
        List<Player> players = new List<Player>();

        foreach (GuildMember member in player.guild.guild.members)
        {
            if (Player.onlinePlayers.ContainsKey(member.name))
            {
                players.Add(Player.onlinePlayers[member.name]);
            }
        }

        if (players.Count > 0)
        {
            long share = (long)Mathf.Ceil((float)amount / (float)players.Count);

            foreach (Player player in players)
            {
                player.playerHonorShop.AddHonorCurrency(honorCurrency, share);
            }

            player.Tools_TargetAddMessage(MSG_GAINED + honorCurrency.name + " x" + share.ToString());
        }
    }

    // -----------------------------------------------------------------------------------
    // HonorCurrency_ShareToRealm
    // -----------------------------------------------------------------------------------
#if _iMMOPVP

    public void HonorCurrency_ShareToRealm(Tmpl_HonorCurrency honorCurrency, long amount)
    {
        List<Player> players = new List<Player>();

        foreach (Player player in Player.onlinePlayers.Values)
        {
            if (player.GetAlliedRealms(player))
            {
                players.Add(Player.onlinePlayers[player.name]);
            }
        }

        if (players.Count > 0)
        {
            long share = (long)Mathf.Ceil((float)amount / (float)players.Count);

            foreach (Player player in players)
            {
                player.playerHonorShop.AddHonorCurrency(honorCurrency, share);
            }
        }
    }

#endif

    // -----------------------------------------------------------------------------------
}
