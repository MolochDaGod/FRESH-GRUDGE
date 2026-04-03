using UnityEngine;
using UnityEngine.UI;

// UI GUILD UPGRADE PANEL
public partial class UI_GuildUpgrade : MonoBehaviour
{
    public GameObject panel;
    public Button acceptButton;
    public Button declineButton;

    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    public void UpdateEvent()
    {
#if _iMMOGUILDUPGRADES

        Player player = Player.localPlayer;
        if (!player) return;
        panel.SetActive(true);

        // use collider point(s) to also work with big entities
        if (player.target != null && player.target is Npc && Utils.ClosestDistance(player, player.target) <= player.interactionRange)
        {
            acceptButton.interactable = player.playerGuildUpgrades.CanUpgradeGuild();

            acceptButton.onClick.SetListener(() =>
            {
                if (Utils.ClosestDistance(player, player.target) <= player.interactionRange)
                {
                    player.playerGuildUpgrades.Cmd_UpgradeGuild();
                    panel.SetActive(false);
                }
                else
                {
                    player.Tools_AddMessage("Oops you are too far from the npc, cancel upgrade your guild !");
                }
            });

            declineButton.onClick.SetListener(() =>
            {
                panel.SetActive(false);
            });
        }
        else panel.SetActive(false);
#endif
    }
    // -----------------------------------------------------------------------------------
}