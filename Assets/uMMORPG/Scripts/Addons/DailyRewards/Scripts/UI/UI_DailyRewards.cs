using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ===================================================================================
// DAILY REWARDS UI
// ===================================================================================
[RequireComponent(typeof(GameEventListener))]
public partial class UI_DailyRewards : MonoBehaviour
{
    public GameObject panel;
    public Transform content;
    public ScrollRect scrollRect;
    public UI_DailyRewardsSlot textPrefab;
    public UI_DailyRewardsItemSlot itemRewardSlot;
    public Transform contentItem;
    public GameObject Claimed;
    public Button claimReward;

    public Sprite goldIcon;
    public Sprite coinsIcon;
    public Sprite expIcon;
    public Sprite skillExpIcon;

    [HideInInspector] public List<DailyReward> dreward;

    private void OnEnable()
    {

        RegisterDailyRewards();
        Show();
    }

    // -----------------------------------------------------------------------------------
    // Show
    // -----------------------------------------------------------------------------------
    public void Show()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        //Change interactable if already claim
        claimReward.interactable= (!player.playerDailyRewards.claimed);

        // Clear Content exemple
        content.RemoveAllChildren();

        if (player.playerDailyRewards.dailyRewardsv2 != null)
        {
            // count number day in this month
            int dayInMonth = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);

            //Debug.Log(">>" + dayInMonth + " / "+ player.playerDailyRewards.dailyRewardCounter);

            // for each day create a slot reward
            for (int i = 0; i < dayInMonth; i++)
            {
                int dayNumber = i + 1;
                UI_DailyRewardsSlot go = Instantiate(textPrefab);
                
                // Change Number day displayer
                go.dayNumber.text = dayNumber.ToString();

                // Change sprite 
                if (dreward[i].spriteClaimed != null && i <= (player.playerDailyRewards.dailyRewardCounter-1))
                {
                    go.slotIcon.sprite = dreward[i].spriteClaimed;
                }
                // Show claimed 
                go.claimed.SetActive((dayNumber <= player.playerDailyRewards.dailyRewardCounter));
                
                // Add effect on daily available
                if (player.playerDailyRewards.dailyRewardCounter == i)
                {
                    if (!player.playerDailyRewards.claimed) {
                        go.claimButton.interactable = true;
                        go.notClaimed.SetActive(true);
                        DisplayReward(player.playerDailyRewards.dailyRewardCounter + 1);
                    }
                    else
                    {
                        DisplayReward(player.playerDailyRewards.dailyRewardCounter);
                    }
                           
                }

                // Add display reward on all slot
                go.showReward.onClick.AddListener( ()=>DisplayReward(dayNumber) );

                go.transform.SetParent(content.transform, false);
            }

        }
    }

    public void DisplayReward(int day)
    {
        int lday = day - 1; // We assign the value of I -1 because a table starts at zero and not at 1
        contentItem.RemoveAllChildren();
        if (dreward[lday] != null)
        {
            DailyReward reward = dreward[lday];
            if (reward.rewardGold > 0)
            {
                UI_DailyRewardsItemSlot go = Instantiate(itemRewardSlot);
                go.amountText.text = reward.rewardGold.ToString();
                go.image.sprite = goldIcon;
                go.tooltip.text = reward.rewardGold + " Gold";
                go.transform.SetParent(contentItem.transform, false);
            }
            if (reward.rewardCoins > 0)
            {
                UI_DailyRewardsItemSlot go = Instantiate(itemRewardSlot);
                go.amountText.text = reward.rewardCoins.ToString();
                go.image.sprite = coinsIcon;
                go.tooltip.text = reward.rewardCoins + " Coins";
                go.transform.SetParent(contentItem.transform, false);
            }
            if (reward.rewardExperience > 0)
            {
                UI_DailyRewardsItemSlot go = Instantiate(itemRewardSlot);
                go.amountText.text = reward.rewardExperience.ToString();
                go.image.sprite = expIcon;
                go.tooltip.text = reward.rewardExperience + " Experience";
                go.transform.SetParent(contentItem.transform, false);
            }
            if (reward.rewardSkillExperience > 0)
            {
                UI_DailyRewardsItemSlot go = Instantiate(itemRewardSlot);
                go.amountText.text = reward.rewardSkillExperience.ToString();
                go.image.sprite = skillExpIcon;
                go.tooltip.text = reward.rewardSkillExperience + " Skill Experience";
                go.transform.SetParent(contentItem.transform, false);
            }
            if (reward.rewardItems.Length > 0)
            {
                foreach (Tools_ItemRequirement item in reward.rewardItems)
                {
                    UI_DailyRewardsItemSlot go = Instantiate(itemRewardSlot);
                    ScriptableItem itemData = item.item;
                    go.tooltip.enabled = true;
                    go.tooltip.text = itemData.ToolTip();
                    go.image.sprite = itemData.image;
                    go.amountOverlay.SetActive(item.amount > 1);
                    go.amountText.text = item.amount.ToString();
                    go.transform.SetParent(contentItem.transform, false);
                }
            }
#if _iMMOHONORSHOP
            if (reward.honorCurrencies.Length > 0)
            {
                foreach (HonorShopCurrencyDrop item in reward.honorCurrencies)
                {

                    UI_DailyRewardsItemSlot go = Instantiate(itemRewardSlot);
                    go.tooltip.enabled = true;
                    go.tooltip.text = item.amount + " " + item.honorCurrency.name + " coin";
                    go.image.sprite = item.honorCurrency.image;
                    go.amountOverlay.SetActive(item.amount > 1);
                    go.amountText.text = item.amount.ToString();
                    go.transform.SetParent(contentItem.transform, false);
                }
            }
#endif

        }
    }

    private void RegisterDailyRewards()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        bool isDefaultReward = true;
        int dayInMonth = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
        for (int i = 1; i < (dayInMonth+1); i++)
        {
            foreach (DailyReward reward in player.playerDailyRewards.dailyRewardsv2.montlyRewards[(DateTime.Today.Month - 1)].rewards)
            {
                if (i == reward.dayNumber)
                {
                    dreward.Add(reward);
                    isDefaultReward = false;
                }
            }
            if (isDefaultReward)
            {
                dreward.Add(player.playerDailyRewards.dailyRewardsv2.montlyRewards[(DateTime.Today.Month - 1)].reward);
            }
            isDefaultReward = true;
        }
    }

    public void ClaimReward() 
    {
        Player player = Player.localPlayer;
        if (!player) return;
        if(!player.playerDailyRewards.claimed)
            player.playerDailyRewards.Cmd_ClaimReward();
    }
}