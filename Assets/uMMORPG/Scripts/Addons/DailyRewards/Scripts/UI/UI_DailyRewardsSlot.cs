using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_DailyRewardsSlot : MonoBehaviour
{
    public Image slotIcon;
    public TMP_Text dayNumber;
    public GameObject claimed;
    public GameObject notClaimed;
    public Button claimButton;
    public Button showReward;

    public void ClaimReward()
    {
        Player player = Player.localPlayer;
        if (!player) return;
        if (!player.playerDailyRewards.claimed)
            player.playerDailyRewards.Cmd_ClaimReward();
    }
}
