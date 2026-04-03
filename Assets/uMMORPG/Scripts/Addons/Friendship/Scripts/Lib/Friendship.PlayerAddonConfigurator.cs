using Mirror;
using System;
using UnityEngine;

// PLAYER
public partial class PlayerAddonsConfigurator
{
    [Header("[-=-[ Friendship ]-=-]")]
    public Tmpl_Friendship friendshipTemplate;

    public readonly SyncList<Friend> Friends = new SyncList<Friend>();

    protected float _cacheFriendship;
    [SyncVar(hook = nameof(On_InvitedFriendFromChanged)), HideInInspector] public string inviteFriendFrom = "";
    [SyncVar(hook = nameof(OnInvitedCoupleFromChanged)), HideInInspector] public string inviteCoupleFrom = "";
    [SyncVar(hook = nameof(On_CoupledChanged)), HideInInspector] public bool coupled = false;


    private void Start_Friendship()
    {
#if _CLIENT
        Friends.Callback += On_FriendListChanged;
#endif
    }

    private void On_CoupledChanged(bool oldValue, bool newValue)
    {
        if (friendshipTemplate.GameEventFriendship)
            friendshipTemplate.GameEventFriendship.TriggerEvent();

    }
    private void On_InvitedFriendFromChanged(string oldValue, string newValue)
    {
        if (friendshipTemplate.GameEventFriendInvite)
            friendshipTemplate.GameEventFriendInvite.TriggerEvent();
    }

    private void OnInvitedCoupleFromChanged(string oldValue, string newValue)
    {
        if (friendshipTemplate.GameEventCoupleInvite)
            friendshipTemplate.GameEventCoupleInvite.TriggerEvent();
    }

    // -----------------------------------------------------------------------------------
    // On_FriendListChanged
    // this is client
    // -----------------------------------------------------------------------------------
    private void On_FriendListChanged(SyncList<Friend>.Operation op, int itemIndex, Friend oldSlot, Friend newSlot)
    {
        Player player = Player.localPlayer;
        if (!player) return;

        if (friendshipTemplate != null)
            friendshipTemplate.GameEventFriendship.TriggerEvent();
    }

    public bool IsFriend(string friendname)
    {
        return (Friends.FindIndex(friend => friend.name == friendname) != -1);
    }

    [Command]
    public void Cmd_ChatCoupleRequierement()
    {
        if (friendshipTemplate != null && friendshipTemplate.allowCouple)
        {
            if(friendshipTemplate.minDayFriend > 0)
                player.Tools_TargetAddMessage(String.Format(friendshipTemplate.msgRequirementCoupleDay, friendshipTemplate.minDayFriend));
#if _iMMOSTAMINA
            if (friendshipTemplate.coupleCostPerCharacter.staminaCost > 0)
                player.Tools_TargetAddMessage(String.Format(friendshipTemplate.msgRequirementCoupleStamina, friendshipTemplate.coupleCostPerCharacter.staminaCost));
#endif
            if (friendshipTemplate.coupleCostPerCharacter.healthCost > 0)
                player.Tools_TargetAddMessage(String.Format(friendshipTemplate.msgRequirementCoupleHealth, friendshipTemplate.coupleCostPerCharacter.healthCost));
            if (friendshipTemplate.coupleCostPerCharacter.coinCost > 0)
                player.Tools_TargetAddMessage(String.Format(friendshipTemplate.msgRequirementCoupleCoin, friendshipTemplate.coupleCostPerCharacter.coinCost));
            if (friendshipTemplate.coupleCostPerCharacter.experienceCost > 0)
                player.Tools_TargetAddMessage(String.Format(friendshipTemplate.msgRequirementCoupleExperience, friendshipTemplate.coupleCostPerCharacter.experienceCost));
            if (friendshipTemplate.coupleCostPerCharacter.goldCost > 0)
                player.Tools_TargetAddMessage(String.Format(friendshipTemplate.msgRequirementCoupleGold, friendshipTemplate.coupleCostPerCharacter.goldCost));
            if (friendshipTemplate.coupleCostPerCharacter.manaCost > 0)
                player.Tools_TargetAddMessage(String.Format(friendshipTemplate.msgRequirementCoupleMana, friendshipTemplate.coupleCostPerCharacter.manaCost));
            if (friendshipTemplate.coupleCostPerCharacter.skillExperienceCost > 0)
                player.Tools_TargetAddMessage(String.Format(friendshipTemplate.msgRequirementCoupleSkillExperience, friendshipTemplate.coupleCostPerCharacter.skillExperienceCost));
#if _iMMOHONORSHOP
            if (friendshipTemplate.coupleCostPerCharacter.honorCurrencyCost.Length > 0)
                foreach(var honnor in friendshipTemplate.coupleCostPerCharacter.honorCurrencyCost)
                    player.Tools_TargetAddMessage(String.Format(friendshipTemplate.msgRequirementCoupleHonnorCurrency, honnor.amount, honnor.honorCurrency.name));
#endif
            if (friendshipTemplate.coupleCostPerCharacter.itemCost.Length > 0)
                foreach (var item in friendshipTemplate.coupleCostPerCharacter.itemCost)
                    player.Tools_TargetAddMessage(String.Format(friendshipTemplate.msgRequirementCoupleItem, item.amount, item.item.name));
        }
    }

    [Command]
    public void Cmd_AcceptFrienInvite()
    {
#if _SERVER
        // valid invitation?
        // note: no distance check because sender might be far away already
        if (!IsFriend(inviteFriendFrom) && inviteFriendFrom != "" && Player.onlinePlayers.TryGetValue(inviteFriendFrom, out Player sender))
        {
            // try playerFriend add. GuildSystem does all the checks.
            AddFriend(inviteFriendFrom);
        }

        // reset guild invite in any case
        inviteFriendFrom = "";
#endif
    }

    [Command]
    public void Cmd_DeclineFriendInvite()
    {
        inviteFriendFrom = "";
    }

    [Command]
    public void Cmd_AcceptCoupleInvite()
    {
#if _SERVER
        // valid invitation?
        // note: no distance check because sender might be far away already
        if (friendshipTemplate.allowCouple && inviteCoupleFrom != "" && Player.onlinePlayers.TryGetValue(inviteCoupleFrom, out Player sender))
        {
            int idx = Friends.FindIndex(x => x.name == inviteCoupleFrom);
            int nombreDeJours = (DateTime.Today - Friends[idx].created).Days;
            if (nombreDeJours >= friendshipTemplate.minDayFriend)
                CreateCouple(inviteCoupleFrom);
            else
            {
                player.Tools_TargetAddMessage(String.Format(friendshipTemplate.msgWeddingNumberDayNoOk, friendshipTemplate.minDayFriend));
                sender.Tools_TargetAddMessage(String.Format(friendshipTemplate.msgWeddingNumberDayNoOk, friendshipTemplate.minDayFriend));
            }
        }

        // reset guild invite in any case
        inviteCoupleFrom = "";
#endif
    }

    [Command]
    public void Cmd_DeclineCoupleInvite()
    {
        inviteCoupleFrom = "";
    }


    // -----------------------------------------------------------------------------------
    // Cmd_AddFriend
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_AddFriend(string friend)
    {
#if _SERVER

        if (string.IsNullOrWhiteSpace(friend)) return;

        // validate
        if (friend != name && Player.onlinePlayers.TryGetValue(friend, out Player other) && NetworkTime.time >= player.nextRiskyActionTime)
        {
            // can only send invite if no party yet or party isn'activePanelLocalPlayer full and
            // have invite rights and other guy isn'activePanelLocalPlayer in party yet
            if ((!IsFriend(friend) || Friends.Count < friendshipTemplate.maxFriends) && other.playerAddonsConfigurator.Friends.Count < friendshipTemplate.maxFriends
#if _iMMOSETTINGS
                //&& !other.playerAddonsConfigurator.isBlockingFriendInvite
#endif
                )
            {
                // send an invite
                other.playerAddonsConfigurator.inviteFriendFrom = name;
            }
        }

        // reset risky TimeLogout no matter what. even if invite failed, we don'activePanelLocalPlayer want
        // players playerFriend be able playerFriend spam the invite button and mass invite random
        // players.
        player.nextRiskyActionTime = NetworkTime.time + friendshipTemplate.inviteFriendWaitSeconds;
#endif
    }

    [Command]
    public void Cmd_Divorce(string coupledName)
    {
#if _SERVER
        if (string.IsNullOrWhiteSpace(coupledName)) return;

        int idFriendRequester = Friends.FindIndex(friend => friend.name == coupledName);

        if (idFriendRequester != -1)
        {
            Friend frnd = Friends[idFriendRequester];
            if (frnd.coupled)
            {
                frnd.coupled = false;
                Friends[idFriendRequester] = frnd;
                coupled = false;
                Database.singleton.UpdateFriend(frnd);


                Player playerRequester = UMMO_Tools.FindOnlinePlayerByName(coupledName);
                if(playerRequester != null)
                    playerRequester.playerAddonsConfigurator.SetFriendUpdate(player, frnd);
            }
        }
#endif
    }

    [Command]
    public void Cmd_CoupleInvite(string friend)
    {
#if _SERVER
        if (string.IsNullOrWhiteSpace(friend)) return;
        if(friend != name && Player.onlinePlayers.TryGetValue(friend, out Player other) && NetworkTime.time >= player.nextRiskyActionTime)
        {
            int idx = Friends.FindIndex(x => x.name == friend);

            if (!coupled && !other.playerAddonsConfigurator.coupled && idx != -1)
            {
                other.playerAddonsConfigurator.inviteCoupleFrom = name;
            }
        }
        else
        {
            player.Tools_TargetAddMessage(friendshipTemplate.msgAlreadyCoupled);
        }

        player.nextRiskyActionTime = NetworkTime.time + friendshipTemplate.inviteFriendWaitSeconds;
#endif
    }

    // -----------------------------------------------------------------------------------
    // Cmd_RemoveFriend
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_RemoveFriend(string name)
    {
#if _SERVER
        if (string.IsNullOrWhiteSpace(name)) return;

        if (Friends.FindIndex(x => x.name == name) != -1)
        {
            int idx = Friends.FindIndex(x => x.name == name);
            Friend frnd = Friends[idx];
            Database.singleton.DeleteFriend(frnd);
            Friends.Remove(Friends[Friends.FindIndex(x => x.name == name)]);

            Player plyr = UMMO_Tools.FindOnlinePlayerByName(name);
            //player plyr = player.onlinePlayers[nameFriend];
            if (plyr)
            {
                plyr.playerAddonsConfigurator.RemoveInFriend(player.name);
            }
            player.Tools_TargetAddMessage(string.Format(friendshipTemplate.msgNoLongerFriend, name));
        }
        else
        {
            player.Tools_TargetAddMessage(string.Format(friendshipTemplate.msgTargetNotFriend, name));
        }
#endif
    }

    private void RemoveInFriend(string name)
    {
        int idx = Friends.FindIndex(x => x.name == name);
        Friend frnd = Friends[idx];
        Friends.Remove(Friends[Friends.FindIndex(x => x.name == name)]);
    }

    // -----------------------------------------------------------------------------------
    // Cmd_GuildInvite
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_GuildInvite(string name)
    {
#if _SERVER
        if (string.IsNullOrWhiteSpace(name) || !player.guild.InGuild()) return;

        Player plyr = Player.onlinePlayers[name];

        if (plyr && !plyr.guild.InGuild() && player.guild.guild.CanInvite(this.name, name) && NetworkTime.time >= player.nextRiskyActionTime)
        {
            plyr.guild.inviteFrom = this.name;
            player.nextRiskyActionTime = Time.time + player.guild.inviteWaitSeconds;
        }
#endif
    }


    // -----------------------------------------------------------------------------------
    // CanGift
    // @Server && @Client
    // -----------------------------------------------------------------------------------
    public bool CanGift(Player player, Friend friend)
    {
        string lastSendGift = ((friend.name_requester == player.name) ? friend.lastGiftedRequester : friend.lastGiftedAccepted);
        if (string.IsNullOrWhiteSpace(lastSendGift)) 
            return true;
        else {
            DateTime time = DateTime.Parse(lastSendGift);
            return ((DateTime.UtcNow - time).TotalHours >= friendshipTemplate.timespanHours);
        }
    }


    // -----------------------------------------------------------------------------------
    // Cmd_GiftFriend
    // @Client -> @Server
    // -----------------------------------------------------------------------------------

    [Command]
    public void Cmd_GiftFriend(string name)
    {
#if _SERVER
        if (string.IsNullOrWhiteSpace(name)) return;

        int idx = Friends.FindIndex(x => x.name == name);

        if (idx != -1)
        {
            Player plyr = UMMO_Tools.FindOnlinePlayerByName(name);
            Friend frnd = Friends[idx];
            if (plyr != null)
            {
                if (CanGift(player, frnd))
                {
                    if (plyr && friendshipTemplate.allowGifts)
                        friendshipTemplate.rewardFriend.GiveReward(plyr, friendshipTemplate.increaseRewardForCouple);
                    // -- Other player gains Honor Currency when online
                    if (frnd.name_requester == player.name)
                        frnd.lastGiftedRequester = DateTime.UtcNow.ToString("s");
                    else
                        frnd.lastGiftedAccepted = DateTime.UtcNow.ToString("s");
                    frnd.friendPoint += friendshipTemplate.rewardPoint + ((frnd.coupled)  ? ((friendshipTemplate.rewardPoint * friendshipTemplate.increaseRewardForCouple) / 100): 0);
                    Friends[idx] = frnd;

                    // -- Gift player gains Honor Currency
                    if (player && friendshipTemplate.allowGifts)
                        friendshipTemplate.rewardCharacter.GiveReward(player, friendshipTemplate.increaseRewardForCouple);
                    plyr.playerAddonsConfigurator.SetFriendUpdate(player,frnd);
                    Database.singleton.UpdateFriend(frnd);
                    player.Tools_TargetAddMessage(string.Format(friendshipTemplate.msgGiftedFriend, name));
                    plyr.Tools_TargetAddMessage(string.Format(friendshipTemplate.msgGiftedFriendBy, player.name));

                }
            }
        }
#endif
    }


    #region SERVER

#if _SERVER

    [Server]
    private void SetFriendUpdate(Player player, Friend friend)
    {
        Player plyr = UMMO_Tools.FindOnlinePlayerByName(player.name);
        if (plyr != null)
        {
            int idx = Friends.FindIndex(x => x.name == player.name);
            if (idx != -1)
            {
                Friend frnd = Friends[idx];
                frnd.friendPoint = friend.friendPoint;
                frnd.lastGiftedRequester = friend.lastGiftedRequester;
                frnd.lastGiftedAccepted = friend.lastGiftedAccepted;
                frnd.coupled = friend.coupled;
                Friends[idx] = frnd;
                coupled = (friend.coupled);
                
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    [ServerCallback]
    private void Update_Friendship()
    {
        if (Time.time < _cacheFriendship) return;

        for (int i = 0; i < Friends.Count; i++)
        {
            Friend frnd = Friends[i];
            Player plyr = UMMO_Tools.FindOnlinePlayerByName(frnd.name);

            if (plyr != null)
            {
                frnd.online = true;
                frnd.inParty = plyr.party.InParty();
                frnd.level = plyr.level.current;
                frnd._class = plyr.className;
                frnd.guild = plyr.guild.guild.name;
            }
            else
            {
                frnd.online = false;
            }

            Friends[i] = frnd;
        }

        _cacheFriendship = Time.time + 4.0f;
    }

    // -----------------------------------------------------------------------------------
    // AddFriend
    // @Server
    // -----------------------------------------------------------------------------------
    [Server]
    public void AddFriend(string requester)
    {
        if (string.IsNullOrWhiteSpace(requester)) return;

        if (Friends.Count >= friendshipTemplate.maxFriends)
        {
            player.Tools_TargetAddMessage(friendshipTemplate.msgMaxFriends);
            return;
        }

        if (requester != this.name)
        {
            if (Database.singleton.CharacterExists(requester))
            {
                if (Player.onlinePlayers.ContainsKey(requester))
                {
                    Player playerRequester = UMMO_Tools.FindOnlinePlayerByName(requester);
#if _iMMOPVP
                    if (player.Tools_SameRealm(playerRequester))
                    {
#endif
                        if (Friends.FindIndex(x => x.name == requester) == -1)
                        {
                            DateTime time = DateTime.UtcNow;
                  
                            int hours = friendshipTemplate.timespanHours * -1;
                            time = time.AddHours(hours);
      
                            Friend requesterFriend = new Friend(playerRequester.name, playerRequester.name, player.name, 0, time, false, "", "");

                            Friend playerFriend = new Friend(player.name, playerRequester.name, player.name, 0, time, false, "", "");

                            Friends.Add(requesterFriend);
                            playerRequester.playerAddonsConfigurator.Friends.Add(playerFriend);

                            player.Tools_TargetAddMessage(string.Format(friendshipTemplate.msgIsNowFriend, requester));
                            playerRequester.Tools_TargetAddMessage(string.Format(friendshipTemplate.msgIsNowFriend, player.name));
                            Database.singleton.AddFriend(requesterFriend);
                        }
                        else
                        {
                            player.Tools_TargetAddMessage(string.Format(friendshipTemplate.msgIsAlreadyFriend, requester));
                        }
#if _iMMOPVP
                    }
                    else
                    {
                        player.Tools_TargetAddMessage(string.Format(friendshipTemplate.msgNoSameRealm, requester));
                    }
#endif
                }
                else
                {
                    player.Tools_TargetAddMessage(string.Format(friendshipTemplate.msgTargetOffline, requester));
                }
            }
            else
            {
                player.Tools_TargetAddMessage(string.Format(friendshipTemplate.msgTargetNotExist, requester));
            }
        }
        else
        {
            player.Tools_TargetAddMessage(friendshipTemplate.msgCannotFriendSelf);
        }
    }


    [Server]
    private void CreateCouple(string requester)
    {
        if (string.IsNullOrWhiteSpace(requester)) return;

        Player playerRequester = UMMO_Tools.FindOnlinePlayerByName(requester);
        int idFriendRequester = Friends.FindIndex(friend => friend.name == requester);

        if (coupled || playerRequester == null || playerRequester.playerAddonsConfigurator.coupled || idFriendRequester == -1)
        {
            player.Tools_TargetAddMessage(friendshipTemplate.msgAlreadyCoupled);
            return;
        }

        Friend frnd = Friends[idFriendRequester];
        bool requesterCostOk = friendshipTemplate.coupleCostPerCharacter.CheckCost(player);
        bool friendCostOk = friendshipTemplate.coupleCostPerCharacter.CheckCost(playerRequester);

        if (requesterCostOk && friendCostOk)
        {
            friendshipTemplate.coupleCostPerCharacter.PayCost(player);
            friendshipTemplate.coupleCostPerCharacter.PayCost(playerRequester);

            frnd.coupled = true;
            Friends[idFriendRequester] = frnd;
            coupled = true;
            Database.singleton.UpdateFriend(frnd);
            playerRequester.playerAddonsConfigurator.SetFriendUpdate(player, frnd);

            player.Tools_TargetAddMessage(string.Format(friendshipTemplate.msgCongrulationCoupled, requester));
            playerRequester.Tools_TargetAddMessage(string.Format(friendshipTemplate.msgCongrulationCoupled, player.name));
        }
        else
        {
            string requesterName = requesterCostOk ? "" : player.name;
            string friendName = friendCostOk ? "" : playerRequester.name;
            string nameNoOkCost = requesterName + (!requesterCostOk && !friendCostOk ? " and " : "") + friendName;

            player.Tools_TargetAddMessage(string.Format(friendshipTemplate.msgWeddingCostNoOk, nameNoOkCost));
            playerRequester.Tools_TargetAddMessage(string.Format(friendshipTemplate.msgWeddingCostNoOk, nameNoOkCost));
        }
    }

    /*private void CreateCouple(string requester)
    {
        if (string.IsNullOrWhiteSpace(requester)) return;

        player playerRequester = UMMO_Tools.FindOnlinePlayerByName(requester);

        int idFriendRequester = Friends.FindIndex(friend => friend.name == requester);

        if (!coupled && !playerRequester.playerAddonsConfigurator.coupled && idFriendRequester != -1)
        {
            Friend frnd = Friends[idFriendRequester];
            bool requesterCostOk = friendshipTemplate.coupleCostPerCharacter.checkCost(player);
            bool friendCostOk = friendshipTemplate.coupleCostPerCharacter.checkCost(playerRequester);

            if (requesterCostOk && friendCostOk)
            {
                friendshipTemplate.coupleCostPerCharacter.payCost(player);
                friendshipTemplate.coupleCostPerCharacter.payCost(playerRequester);

                frnd.coupled = true;
                Friends[idFriendRequester] = frnd;
                coupled = true;
                Database.singleton.UpdateFriend(frnd);
                playerRequester.playerAddonsConfigurator.SetFriendUpdate(player, frnd);

                player.Tools_TargetAddMessage(String.Format(friendshipTemplate.msgCongrulationCoupled, requester));
                playerRequester.Tools_TargetAddMessage(String.Format(friendshipTemplate.msgCongrulationCoupled, player.name));

            }
            else
            {
                string requesterName = (!requesterCostOk) ? player.name : "";
                string friendName = (!friendCostOk) ? playerRequester.name : "";

                string nameNoOkCost = requesterName + ((!requesterCostOk && !friendCostOk) ? " and " : "") + friendName;

                player.Tools_TargetAddMessage(String.Format(friendshipTemplate.msgWeddingCostNoOk, nameNoOkCost));
                playerRequester.Tools_TargetAddMessage(String.Format(friendshipTemplate.msgWeddingCostNoOk, nameNoOkCost));
            }
        }
        else
        {
            player.Tools_TargetAddMessage(friendshipTemplate.msgAlreadyCoupled);
        }

    }*/

#endif

    #endregion SERVER
    // -----------------------------------------------------------------------------------
}