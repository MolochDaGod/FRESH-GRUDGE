using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class UI_OnlinePlayer : MonoBehaviour
{
    public KeyCode hotKey = KeyCode.F12;
    public GameObject panel;
    public UI_SlotOnlinePlayer slotPrefab;
    public Transform onlinePlayerContent;

    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            // hotkey (not while typing in chat, etc.)
            if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
                panel.SetActive(!panel.activeSelf);

            // only update the panel if it's active
            if (panel.activeSelf)
            {
                player.playerAddonsConfigurator.Cmd_ListPlayersOnline();

                int totalOnline = Player.onlinePlayers.Count;
                // instantiate/destroy enough slots
                UIUtils.BalancePrefabs(slotPrefab.gameObject, totalOnline, onlinePlayerContent);

                // refresh all members
                int i = 0;
                Debug.Log("total player online = " + totalOnline);
                foreach(Player playerOnline  in Player.onlinePlayers.Values)
                {
                    UI_SlotOnlinePlayer slot = onlinePlayerContent.GetChild(i).GetComponent<UI_SlotOnlinePlayer>();
#if _iMMOPVP
                    slot.factionImage.sprite = playerOnline.Realm.image;
#else
                    slot.factionImage.sprite = playerOnline.classIcon;
#endif
                    slot.nameText.text = playerOnline.name;
                    slot.levelText.text = playerOnline.level.current.ToString();
                    slot.className.text = playerOnline.className;
                    ++i;
                    if (player.name != playerOnline.name)
                    {
#if _iMMOFRIENDS
                        slot.friendButton.interactable = (
                            player.playerAddonsConfigurator.friendshipTemplate && player.playerAddonsConfigurator.Friends.Count < player.playerAddonsConfigurator.friendshipTemplate.maxFriends && playerOnline.playerAddonsConfigurator.friendshipTemplate && playerOnline.playerAddonsConfigurator.Friends.Count < playerOnline.playerAddonsConfigurator.friendshipTemplate.maxFriends &&
                            !player.playerAddonsConfigurator.IsFriend(playerOnline.name) && player.name != playerOnline.name
                            );
                        slot.friendButton.onClick.SetListener(() =>
                        {
                            player.playerAddonsConfigurator.Cmd_AddFriend(playerOnline.name);
                        });
#else
                        slot.friendButton.interactable = false;
#endif

                        slot.partyButton.interactable = (!player.party.InParty() || !player.party.party.IsFull()) && player.party.party.master == player.name && !playerOnline.party.InParty() && NetworkTime.time >= player.nextRiskyActionTime;
                        slot.partyButton.onClick.SetListener(() =>
                        {
                            player.party.CmdInvite(playerOnline.name);
                        });

                        slot.guildButton.interactable = (player.guild.InGuild() || player.guild.guild.CanInvite(player.name, playerOnline.name)) && player.guild.guild.master == player.name && !playerOnline.guild.InGuild() && NetworkTime.time >= player.nextRiskyActionTime;
                        slot.partyButton.onClick.SetListener(() =>
                        {
                            player.guild.CmdInviteTarget();
                        });

                        slot.tradingButton.interactable = (playerOnline.lastCombatTime >= 10 && player.trading.CanStartTradeWith(playerOnline));
                        slot.tradingButton.onClick.SetListener(() =>
                        {
                            player.trading.CmdSendRequest();
                        });
                    }
                    else
                    {
                        slot.friendButton.gameObject.SetActive(false);
                        slot.guildButton.gameObject.SetActive(false);
                        slot.partyButton.gameObject.SetActive(false);
                        slot.tradingButton.gameObject.SetActive(false);
                    }
                }
            }
        }
        else panel.SetActive(false);
    }
}
