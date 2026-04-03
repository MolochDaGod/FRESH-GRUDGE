using Mirror;
using System;
using System.Globalization;
using UnityEngine;

// PLAYER
public class PlayerDailyRewards : NetworkBehaviour
{
    public Player player;

    [Header("[-=-=-[ DAILY REWARDS ]-=-=-]")]
    public Tmpl_DailyRewards dailyRewardsv2;

    [SyncVar] public string lastClaimReward = "";
    private DateTime lastRewardDate = DateTime.MinValue;
    [SyncVar] public bool claimed = false;

    [SyncVar(hook = nameof(ShowDailyRewards))] public int dailyRewardCounter = 0;

    public GameEvent dailyRewards;
    public GameEvent buttonRewards;


    readonly int today = DateTime.Today.Day;
    readonly int month = DateTime.Today.Month;
    readonly int year = DateTime.Today.Year;

#if _SERVER
    public override void OnStartServer()
    {
        Start_DailyRewardsv2();
    }

    [Server]
    private void Start_DailyRewardsv2()
    {
        // Première chose si notre dailyrewards est vide on ne fait absolument rien
        if (dailyRewardsv2 != null)
        {
            DateTime result = DateTime.ParseExact((string.IsNullOrEmpty(lastClaimReward) ? "2023-02-28" : lastClaimReward), "yyyy-MM-dd", CultureInfo.InvariantCulture);

            // On vérifie que le mois en cours et l'année en cours ne sont pas supérieure sinon réset 
            if (ResetReward())
            {
                dailyRewardCounter = 0;
            }
            /*else
            {
                //dailyRewardCounter += 10;
                Debug.Log( " on reset pas (" + result.Year + "-" + result.Month + "-" + result.Day + ")");
            }*/

            if (dailyRewardsv2.autoClaimRewards && ValidRewardDate())
            {
                //Alors on va afficher la fenetre et validé automatiquement le reward etc quoi qu'il arrive
                ClaimReward();
            }
           // Debug.Log(lastClaimReward + " / " + year + "-" + month + "-" + today);
            claimed = isClaimed();
            Invoke(nameof(Rpc_ButtonDailyRewards), 3f);
        }
    }


    private bool isClaimed()
    {
        DateTime result = DateTime.ParseExact((string.IsNullOrEmpty(lastClaimReward) ? "2023-02-28" : lastClaimReward), "yyyy-MM-dd", CultureInfo.InvariantCulture);
        return (today == result.Day && month == result.Month && year == result.Year);
    }

    [Server]
    private void ClaimReward()
    {
        
        lastClaimReward = DateTime.Today.ToString("yyyy-MM-dd");
        claimed = isClaimed();
        bool isDefaultReward = true;
        // On parcoure la liste des rewards particulier par jours pour savoir si il est dans la liste
        foreach (var claimReward in dailyRewardsv2.montlyRewards[(DateTime.Today.Month - 1)].rewards)
        {
            if(claimReward.dayNumber == dailyRewardCounter)
            {
                AddRewardToPlayer(claimReward);
                isDefaultReward = false;
                break;
            }
        }

        //Si le jour n'est pas dans la liste alors on prend le reward par defaut du mois
        if(isDefaultReward)
        {
            AddRewardToPlayer(dailyRewardsv2.montlyRewards[(DateTime.Today.Month - 1)].reward);
        }

        
    }

    [Server]
    private bool ResetReward()
    {
        DateTime result = DateTime.ParseExact((string.IsNullOrEmpty(lastClaimReward) ? "2023-02-28" : lastClaimReward), "yyyy-MM-dd", CultureInfo.InvariantCulture);
        return (month > result.Month || year > result.Year);
    }

    [Server]
    private bool ValidRewardDate()
    {
        DateTime result = DateTime.ParseExact((string.IsNullOrEmpty(lastClaimReward) ? "2020-02-20" : lastClaimReward), "yyyy-MM-dd", CultureInfo.InvariantCulture);
        return (today > result.Day || month > result.Month || year > result.Year);
    }

    [Server]
    private void AddRewardToPlayer(DailyReward reward)
    {
        if (reward.rewardSkillExperience > 0)
        {
            ((PlayerSkills)player.skills).skillExperience += reward.rewardSkillExperience;
            player.Tools_TargetAddMessage("Daily Reward : You received " + reward.rewardSkillExperience + " Skills Experience");
        }
        if (reward.rewardSkillExperience > 0)
        {
            player.experience.current += reward.rewardExperience;
            player.Tools_TargetAddMessage("Daily Reward : You received " + reward.rewardExperience + " points Experience");
        }

        if (reward.rewardGold > 0)
        {
            player.gold += reward.rewardGold;
            player.Tools_TargetAddMessage("Daily Reward : You received " + reward.rewardGold + " gold");
        }

        if (reward.rewardCoins > 0)
        {
            player.itemMall.coins += reward.rewardCoins;
            player.Tools_TargetAddMessage("Daily Reward : You received " + reward.rewardCoins + " coins");
        }

        if (reward.rewardItems.Length > 0)
            foreach (Tools_ItemRequirement rewardItem in reward.rewardItems)
            {
                if (rewardItem.item && rewardItem.amount > 0)
                {
                    player.inventory.Add(new Item(rewardItem.item), rewardItem.amount);
                    player.Tools_TargetAddMessage("Daily Reward : You received " + rewardItem.amount + " " + rewardItem.item.name + " ");
                }
            }

#if _iMMOHONORSHOP
        if(reward.honorCurrencies.Length > 0)
            foreach (HonorShopCurrencyDrop currency in reward.honorCurrencies)
            {
                if (currency.amount > 0 && currency.honorCurrency != null)
                {
                    player.playerHonorShop.AddHonorCurrency(currency.honorCurrency, currency.amount);
                    player.Tools_TargetAddMessage("Daily Reward : You received " + currency.amount + " " + currency.honorCurrency.name + " ");
                }
            }
#endif
        
    }


#endif

    [Command]
    public void Cmd_ClaimReward()
    {
#if _SERVER
        if (!isClaimed())
        {
            claimed= true;
            dailyRewardCounter++;
            ClaimReward();
        }
#endif
    }

    public void ShowDailyRewards(int oldValue, int newValue)
    {
        dailyRewards.TriggerEvent();
        buttonRewards.TriggerEventBool(!claimed);
    }

    //[ClientRpc]
    [TargetRpc]
    private void Rpc_ButtonDailyRewards()
    {
        buttonRewards.TriggerEventBool(!claimed);
    }
    // -----------------------------------------------------------------------------------        
}