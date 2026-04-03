using Mirror;
using UnityEngine;
using System.Text;
public partial class PlayerAddonsConfigurator
{
#if _SERVER || UNITY_EDITOR
    [Header("[-=-[ Level Up Template ]-=-]")]
    [SerializeField] private Tmpl_LevelUp playerLevelUpTemplate = null;
#endif

#if _SERVER
    public void OnStartServer_LevelUp()
    {
        playerExperience.onLevelUp.AddListener(OnLevelUp_LevelUp);
    }

    // -----------------------------------------------------------------------------------
    // OnLevelUp_LevelUp
    // -----------------------------------------------------------------------------------
    [Server]
    private void OnLevelUp_LevelUp()
    {
        if (playerLevelUpTemplate != null)
        {
            // if an effect is added on on scriptable
            if(playerLevelUpTemplate.levelUpEffect != null)
                ShowEffect();

            // show pop up
            player.Target_Tools_ShowPopup(connectionToClient, playerLevelUpTemplate.levelUpNotice.message + player.level.current + playerLevelUpTemplate.levelUpNotice.suffix);

            // if skill experience for each level is not empty
            int skillExpReward = playerLevelUpTemplate.skillExpOnLevelUp.Get(player.level.current);
            if (skillExpReward > 0)
            {
                ((PlayerSkills)player.skills).skillExperience += playerLevelUpTemplate.skillExpOnLevelUp.Get(player.level.current);

                StringBuilder rewardText = new StringBuilder(playerLevelUpTemplate.rewardSkillExpchatText);
                rewardText.Replace("{LEVELUP}", player.level.current.ToString());
                rewardText.Replace("{AMOUNT}", skillExpReward.ToString());
                player.Tools_TargetAddMessage(rewardText.ToString());
            }

            // if reward is not null
            if (playerLevelUpTemplate.levelUpRewards.Length > 0)
            {
                foreach (LevelUpReward reward in playerLevelUpTemplate.levelUpRewards)
                {
                    if (reward.playerLevelUp == player.level.current)
                    {
                        // Add gold
                        if (reward.gold > 0)
                        {
                            player.gold += reward.gold;

                            StringBuilder rewardText = new StringBuilder(playerLevelUpTemplate.rewardGoldchatText);
                            rewardText.Replace("{LEVELUP}", player.level.current.ToString());
                            rewardText.Replace("{AMOUNT}", reward.gold.ToString());
                            rewardText.Replace("{S}", (reward.gold > 1) ? "s" : "");
                            player.Tools_TargetAddMessage(rewardText.ToString());
                        }
#if _iMMOTITLES
                        if (reward.eanTitles.Length > 0 && player.playerTitles != null)
                        {
                            foreach (var title in reward.eanTitles)
                            {
                                player.playerTitles.EarnTitle(title);

                                StringBuilder rewardText = new StringBuilder(playerLevelUpTemplate.rewardTitlechatText);
                                rewardText.Replace("{LEVELUP}", player.level.current.ToString());
                                rewardText.Replace("{TITLENAME}", title.name);
                                player.Tools_TargetAddMessage(rewardText.ToString());
                            }

                        }
#endif
                        // Add Skill experience 
                        if (reward.skillExpReward > 0)
                        {
                            ((PlayerSkills)player.skills).skillExperience += reward.skillExpReward;

                            StringBuilder rewardText = new StringBuilder(playerLevelUpTemplate.rewardSkillExpchatText);
                            rewardText.Replace("{LEVELUP}", player.level.current.ToString());
                            rewardText.Replace("{AMOUNT}", reward.skillExpReward.ToString());
                            player.Tools_TargetAddMessage(rewardText.ToString());
                        }

#if _iMMOHONORSHOP
                        if (reward.honors.Length > 0 && player.playerHonorShop != null)
                        {
                            foreach (var honor in reward.honors)
                            {
                                player.playerHonorShop.AddHonorCurrency(honor.honors, honor.amount);

                                StringBuilder rewardText = new StringBuilder(playerLevelUpTemplate.rewardHonnorCurrencychatText);
                                rewardText.Replace("{LEVELUP}", player.level.current.ToString());
                                rewardText.Replace("{CURRENCYNAME}", honor.honors.name);
                                rewardText.Replace("{AMOUNT}", honor.amount.ToString());
                                player.Tools_TargetAddMessage(rewardText.ToString());
                            }
                        }
#endif

                        if (reward.item.Length > 0)
                        {
                            foreach (RewardItem item in reward.item)
                            {
                                if (player.inventory.CanAdd(new Item(item.item), item.amount))
                                {
                                    player.inventory.Add(new Item(item.item), item.amount);

                                    StringBuilder rewardText = new StringBuilder(playerLevelUpTemplate.rewardItemChatText);
                                    rewardText.Replace("{LEVELUP}", player.level.current.ToString());
                                    rewardText.Replace("{ITEM}", item.item.name);
                                    rewardText.Replace("{AMOUNT}", item.amount.ToString());
                                    player.Tools_TargetAddMessage(rewardText.ToString());
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    [Server]
    private void ShowEffect()
    {
        GameObject go = Instantiate(playerLevelUpTemplate.levelUpEffect.gameObject, player.transform.position, Quaternion.identity);
        go.GetComponent<OneTimeTargetSkillEffect>().caster = player;
        go.GetComponent<OneTimeTargetSkillEffect>().target = player;
        NetworkServer.Spawn(go);
    }
#endif
}
