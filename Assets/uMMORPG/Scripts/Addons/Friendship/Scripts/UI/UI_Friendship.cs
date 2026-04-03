using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Friendship : MonoBehaviour
{

    [Header("[-=-[ Text ]-=-]")]
    public TMP_Text playerStatus;
    public TMP_Text playerRelationship;
    public TMP_Text playerMarriedSince;
    public TMP_Text playerPoint;

    [Header("[-=-[ Add friend & limit ]-=-]")]
    public InputField InputFieldAddFriend;
    public Button buttonAddFriend;
    public Button buttonWeddinInfoCost;
    public TMP_Text totalFriend;
    public TMP_Text maxFriend;

    [Header("[-=-[ Prefab & container ]-=-]")]
    public SlotFriendMember friendRow;
    public Transform content;

    [Header("*Edit Text*")]
    public bool editTextDisplay;

    [BoolShowConditional(conditionFieldName: "editTextDisplay", conditionValue: true)]
    public string playerStatusCoupledText = "Coupled";
    [BoolShowConditional(conditionFieldName: "editTextDisplay", conditionValue: true)]
    public string playerStatusSingleText = "Single";
    [BoolShowConditional(conditionFieldName: "editTextDisplay", conditionValue: true)]
    public string playerMarriedSinceText = "Friend since: ({0} Days)";
    [BoolShowConditional(conditionFieldName: "editTextDisplay", conditionValue: true)]
    public string playerPointText = "Married point: ({0})";


    public void OnEnable()
    {
        Prepare();
        buttonAddFriend.onClick.SetListener(() =>
        {
            AddFriendButton();
        });
    }

    // -----------------------------------------------------------------------------------
    // AddFriendButton
    // -----------------------------------------------------------------------------------
    public void AddFriendButton()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        if (!string.IsNullOrWhiteSpace(InputFieldAddFriend.text))
            player.playerAddonsConfigurator.Cmd_AddFriend(InputFieldAddFriend.text);

        InputFieldAddFriend.text = "";
    }

    // -----------------------------------------------------------------------------------
    // AddFriendButton
    // -----------------------------------------------------------------------------------
    public void MarryMeButton()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        if (!string.IsNullOrWhiteSpace(InputFieldAddFriend.text))
            player.playerAddonsConfigurator.Cmd_AddFriend(InputFieldAddFriend.text);

        InputFieldAddFriend.text = "";
    }

    // -----------------------------------------------------------------------------------
    // Prepare
    // -----------------------------------------------------------------------------------
    public void Prepare()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        buttonAddFriend.interactable = (player.playerAddonsConfigurator.Friends.Count < player.playerAddonsConfigurator.friendshipTemplate.maxFriends);

        maxFriend.text = player.playerAddonsConfigurator.friendshipTemplate.maxFriends.ToString();
        totalFriend.text = player.playerAddonsConfigurator.Friends.Count.ToString();


        UIUtils.BalancePrefabs(friendRow.gameObject, player.playerAddonsConfigurator.Friends.Count, content);

        for (int i = 0; i < player.playerAddonsConfigurator.Friends.Count; i++)
        {
            SlotFriendMember friendRow = content.GetChild(i).GetComponent<SlotFriendMember>();

            Friend frnd = player.playerAddonsConfigurator.Friends[i];
           
            friendRow.SetData(frnd);

            if(frnd.coupled)
            {
                playerStatus.text = playerStatusCoupledText;
                playerRelationship.text = (frnd.name_requester == player.name) ? frnd.name_accepted : frnd.name_requester;

                // Calcul du nombre de jours entre les deux dates
                int nombreDeJours = (DateTime.Today - frnd.created).Days;
                playerMarriedSince.text = String.Format(playerMarriedSinceText, nombreDeJours);
                playerPoint.text = String.Format(playerPointText, frnd.friendPoint);
            }
        }

        if (player.playerAddonsConfigurator.friendshipTemplate.allowCouple)
        {
            buttonWeddinInfoCost.onClick.SetListener(() =>
                player.playerAddonsConfigurator.Cmd_ChatCoupleRequierement()
            );
        }

        if(!player.playerAddonsConfigurator.coupled)
        {
            playerStatus.text = playerStatusSingleText;
            playerRelationship.text = "none";
            playerMarriedSince.text = String.Format(playerMarriedSinceText, "0");
            playerPoint.text = String.Format(playerPointText, "0");
        }
    }
}
