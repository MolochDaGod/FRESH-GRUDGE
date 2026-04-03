using UnityEngine;
using UnityEngine.UI;

// ===================================================================================
// FRIENDLIST TARGET
// ===================================================================================
public partial class UI_TargetButtonFriendInvite : MonoBehaviour
{
    public Button friendAddButton;

    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    private void OnEnable()
    {
        Player player = Player.localPlayer;
        if (!player) return;

#if _iMMOPVP
        if (player.target && player.target is Player && player.target != player && player.Tools_SameRealm((Player)player.target))
        {
            friendAddButton.gameObject.SetActive(true);
            friendAddButton.interactable = player.playerAddonsConfigurator.Friends.FindIndex(x => x.name == ((Player)(player.target)).name) == -1 ? true : false;
            friendAddButton.onClick.SetListener(() =>
            {
                player.playerAddonsConfigurator.Cmd_AddFriend(((Player)(player.target)).name);
                friendAddButton.interactable = false;
            });
        }
        else friendAddButton.gameObject.SetActive(false);
#else
 		if (player.target && player.target is Player && player.target != player) {
            friendAddButton.gameObject.SetActive(true);
            friendAddButton.interactable = player.playerAddonsConfigurator.Friends.FindIndex(x=> x.name == ((Player)(player.target)).name) == -1  ? true : false;
            friendAddButton.onClick.SetListener(() => {
                player.playerAddonsConfigurator.Cmd_AddFriend(((Player)(player.target)).name);
            });
        }
        else friendAddButton.gameObject.SetActive(false);
#endif
    }

    // -----------------------------------------------------------------------------------
}