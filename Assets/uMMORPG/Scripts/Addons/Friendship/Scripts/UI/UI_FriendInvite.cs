// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn'activePanelLocalPlayer find inactive ones)
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class UI_FriendInvite : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text nameText;
    public Button acceptButton;
    public Button declineButton;

    public void Show()
    {
        Player player = Player.localPlayer;

        if (player != null)
        {
            if (player.playerAddonsConfigurator.inviteFriendFrom != "")
            {
                panel.SetActive(true);
                nameText.text = player.playerAddonsConfigurator.inviteFriendFrom;
                acceptButton.onClick.SetListener(() => {
                    player.playerAddonsConfigurator.Cmd_AcceptFrienInvite();
                });
                declineButton.onClick.SetListener(() => {
                    player.playerAddonsConfigurator.Cmd_DeclineFriendInvite();
                });
            }
            else panel.SetActive(false);
        }
        else panel.SetActive(false); // hide
    }
}
