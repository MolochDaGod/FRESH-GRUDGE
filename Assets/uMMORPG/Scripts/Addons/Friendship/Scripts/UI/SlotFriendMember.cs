using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotFriendMember : MonoBehaviour
{
    public TMP_Text playerName;
    public TMP_Text guildName;
    public TMP_Text playerLevel;
    public TMP_Text friendPoints;

    public Image statusImage;
    public Sprite offlineImage;
    public Sprite onlineImage;

    public Button buttonGiftFriend;
    public Button buttonMarryMe;
    public TMP_Text couleMeText;
    public Button buttonGroupInvite;
    public Button buttonGuildInvite;
    public Button buttonRemoveFriend;

    [Header("* Edit text & color *")]
    public bool editTextAndColor = false;
    [BoolShowConditional(conditionFieldName: "editTextAndColor", conditionValue: true)]
    public string marryMeMarryText = "Marry Me";
    [BoolShowConditional(conditionFieldName: "editTextAndColor", conditionValue: true)]
    public string marryMeDivorceText = "Divorce";
    [BoolShowConditional(conditionFieldName: "editTextAndColor", conditionValue: true)]
    public string marryMeMarriedText = "-";
    [BoolShowConditional(conditionFieldName: "editTextAndColor", conditionValue: true)]
    public Color marryColorMarry = Color.magenta;
    [BoolShowConditional(conditionFieldName: "editTextAndColor", conditionValue: true)]
    public Color marryColorDivorce = Color.red;
    [BoolShowConditional(conditionFieldName: "editTextAndColor", conditionValue: true)]
    public Color marryColorMarried = Color.grey;

    // -----------------------------------------------------------------------------------
    // ´SetData
    // -----------------------------------------------------------------------------------
    public void SetData(Friend frnd)
    {
        Player player = Player.localPlayer;
        
        if (!player) return;

        
        // -- Friend Data
        playerName.text = frnd.name;

        // -- Friend Level 
        playerLevel.text =  (frnd.level > 0) ? frnd.level.ToString() : "-";

        // -- Friend Guild
        guildName.text = !string.IsNullOrWhiteSpace(frnd.guild) ? frnd.guild : "-";

        // -- Friend Points 
        friendPoints.text = frnd.friendPoint.ToString();

        // -- Friend Online
        statusImage.sprite = (!frnd.online) ? offlineImage : onlineImage;

        // -- Button: Party Invite
        if (buttonGroupInvite != null) {
            if (frnd.online && (!player.party.InParty() || !player.party.party.IsFull()) && !frnd.inParty)
            {
                buttonGroupInvite.interactable = true;
                buttonGroupInvite.onClick.SetListener(() =>
                {
                    player.party.CmdInvite(frnd.name);
                    buttonGroupInvite.interactable = false;
                }
                );
            }
            else
            {
                buttonGroupInvite.interactable = false;
            }
        }

        // -- Button: Guild Invite
        if (buttonGuildInvite != null)
        {
            if (frnd.online && player.guild.InGuild() && string.IsNullOrWhiteSpace(frnd.guild) && player.guild.guild.CanInvite(player.name, frnd.name))
            {
                buttonGuildInvite.interactable = true;
                buttonGuildInvite.onClick.SetListener(() =>
                {
                    player.playerAddonsConfigurator.Cmd_GuildInvite(frnd.name);
                    buttonGuildInvite.interactable = false;
                }
                );
            }
            else
            {
                buttonGuildInvite.interactable = false;
            }
        }

        // -- Button: Gift Friend
        if (buttonGiftFriend != null)
        {
            if (player.playerAddonsConfigurator.friendshipTemplate.allowGifts && UMMO_Tools.FindOnlinePlayerByName(frnd.name))
            {
                buttonGiftFriend.gameObject.SetActive(true);
                if (player.playerAddonsConfigurator.CanGift(player, frnd))
                {
                    buttonGiftFriend.interactable = true;
                    buttonGiftFriend.onClick.SetListener(() =>
                    {
                        player.playerAddonsConfigurator.Cmd_GiftFriend(frnd.name);
                        buttonGiftFriend.interactable = false;
                    });
                }
                else
                {
                    buttonGiftFriend.interactable = false;
                }
            }
            else
            {
                buttonGiftFriend.interactable = false;
            }
        }

        // -- Button: Friend Remove
        if (buttonRemoveFriend != null)
        {
            bool canRemove = true;
            canRemove = player.playerAddonsConfigurator.CanGift(player, frnd);

            // TODO : si les joueurs sont marié il ne peuvent pas supprimé l'amitier, il doivent divorcé en premier
            //          il ne doident pas avoir caliner durant le labs de temps imparti 
            if (!frnd.coupled && canRemove)
            {
                buttonRemoveFriend.interactable = true;

                buttonRemoveFriend.onClick.SetListener(() => {
                    UIConfirmation.singleton.Show("Are you sure you want to delete your <b><color=yellow>"+frnd.name+"</color></b> friend? \r\n deleting a friend deletes all your acquired points", () => {
                        player.playerAddonsConfigurator.Cmd_RemoveFriend(frnd.name);
                    });
                });
            }
            else
            {
                buttonRemoveFriend.interactable = false;
            }
        }


        Player friend = (frnd.online) ? UMMO_Tools.FindOnlinePlayerByName(frnd.name) : null;
        // -- Button: Marry Me
        if (buttonMarryMe != null)
        {

            // si le joueur est marié
            if (player.playerAddonsConfigurator.coupled)
            {
                // pas marié avec cette amis
                if (!frnd.coupled)
                {
                    couleMeText.text = marryMeMarriedText;
                    couleMeText.color = marryColorMarried;
                    buttonMarryMe.interactable = false;
                }
                // sinon si le joueur est marieravec cette amis
                else
                {
                    buttonMarryMe.interactable = true;
                    couleMeText.text = marryMeDivorceText;
                    couleMeText.color = marryColorDivorce;
                    buttonMarryMe.onClick.SetListener(() =>
                    {
                        UIConfirmation.singleton.Show("Are you sure you want to divorce your friend <b><color=yellow>" + frnd.name + "</color></b>?", () => {
                            player.playerAddonsConfigurator.Cmd_Divorce(frnd.name);
                        });
                        buttonMarryMe.interactable = false;
                    });
                }
            }
            // si le joueur est en ligne que le joueur actuel est pas marié et que le joueur en face est pas marié
            else if (frnd.online && !player.playerAddonsConfigurator.coupled && !friend.playerAddonsConfigurator.coupled)
            {
                couleMeText.text = marryMeMarryText;
                couleMeText.color = marryColorMarry;
                buttonMarryMe.interactable = true;
                buttonMarryMe.onClick.SetListener(() =>
                {
                    player.playerAddonsConfigurator.Cmd_CoupleInvite(frnd.name);
                    buttonMarryMe.interactable = false;
                });
            }

            else
            {
                couleMeText.text = marryMeMarryText;
                couleMeText.color = marryColorMarry;
                buttonMarryMe.interactable = false;
            }
        }

    }
}
