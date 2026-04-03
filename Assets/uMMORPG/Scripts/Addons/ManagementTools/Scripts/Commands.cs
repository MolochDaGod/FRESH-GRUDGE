using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//using UnityEngine.Profiling;

#if _SQLITE
public partial class Player
{
    public static Dictionary<string, Player> onlineStaffMembers = new Dictionary<string, Player>();

    void Start_StaffMembersUpdate()
    {
        if (CheckAccess())
        {
            onlineStaffMembers[name] = this;
        }
    }

    void OnDestroy_StaffMembersUpdate()
    {
        if (CheckAccess())
        {
            onlineStaffMembers.Remove(name);
        }
    }

    #region Main Panel
    [Command]
    public void CmdShowOnline(int page)
    {
        //Profiler.BeginSample("Management CmdShowOnline");

        // always put my character's name first
        Player[] players = onlinePlayers.Values.OrderByDescending(i => i.name == name).ToArray();

        // max 10 names per page
        string[] playerNames = players.Select(x => x.name).Skip(page * 10).Take(10).ToArray();
        int onlineCount = onlinePlayers.Values.Count;

        TargetShowOnline(playerNames, onlineCount);

        //Profiler.EndSample();
    }

    [Command]
    public void CmdSearchOnline(int page, string letter)
    {
        //Profiler.BeginSample("Management CmdSearchOnline");

        // always put my character's name first
        Player[] players = onlinePlayers.Values.OrderByDescending(i => i.name == name).ToArray();

        // max 10 names per page, starting with the chosen letter
        string[] playerNames = players.Select(x => x.name).Where(x => x.StartsWith(letter)).Skip(page * 10).Take(10).ToArray();

        // only count players whose name starts with the chosen letter
        // this is important for pages to function properly when searching
        Player[] total = players.Where(x => x.name.StartsWith(letter)).ToArray();
        int onlineCount = total.Length;

        TargetShowOnline(playerNames, onlineCount);

        //Profiler.EndSample();
    }

    [TargetRpc]
    public void TargetShowOnline(string[] playerNames, int onlineCount)
    {
        //Profiler.BeginSample("Management TargetShowOnline");

        if (UIManagement.singleton.canvas.enabled)
        {
            UIManagement.singleton.online = playerNames;
            UIManagement.singleton.onlineCount = onlineCount;
            UIManagement.singleton.PlayerButtons();
        }

        //Profiler.EndSample();
    }
    #endregion

    #region Database Information
    [Command]
    public void CmdDisconnectAllPlayers()
    {
        if (!CheckAccess())
            return;

        foreach (Player onlinePlayer in onlinePlayers.Values)
        {
            // disconnect all currently logged in players except my character
            if (onlinePlayer.name != name)
            {
                onlinePlayer.connectionToClient.Disconnect();
            }
        }
    }

    [Command]
    public void CmdMsgDeleteAccounts()
    {
        // only access level 3
        if (!OwnerAccess())
            return;

        foreach (Player onlinePlayer in onlinePlayers.Values)
        {
            // disconnect all currently logged in players without access level 3
            if (!onlinePlayer.OwnerAccess())
            {
                // to prevent saving after deleting
                onlinePlayer.canBeSaved = false;

                onlinePlayer.connectionToClient.Disconnect();
            }
        }

        // delete all accounts with characters and other related tables from the database except for access level 3
        Database.singleton.DeleteAccounts();

        string message = "All accounts have been deleted from the database!";
        chat.TargetMsgInfo(message);
    }

    [Command]
    public void CmdMsgDeleteMonsters()
    {
        if (!CheckAccess())
            return;

        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");

        foreach (GameObject monster in monsters)
        {
            if (monster.TryGetComponent(out Monster go))
            {
                if (go.uniqueId != "")
                {
                    NetworkServer.Destroy(monster);
                }
            }
        }

        Database.singleton.MonstersDelete();

        string message = "All monsters have been deleted from the database!";
        chat.TargetMsgInfo(message);
    }

    [Command]
    public void CmdMsgDeleteNpcs()
    {
        if (!CheckAccess())
            return;

        GameObject[] npcs = GameObject.FindGameObjectsWithTag("Npc");

        foreach (GameObject npc in npcs)
        {
            if (npc.TryGetComponent(out Npc go))
            {
                if (go.uniqueId != "")
                {
                    NetworkServer.Destroy(npc);
                }
            }
        }

        Database.singleton.NpcsDelete();

        string message = "All npcs have been deleted from the database!";
        chat.TargetMsgInfo(message);
    }

    [Command]
    public void CmdMsgDeleteEnvironments()
    {
        if (!CheckAccess())
            return;

        GameObject[] environments = GameObject.FindGameObjectsWithTag("Environment");

        foreach (GameObject environment in environments)
        {
            if (environment.TryGetComponent(out M_Environment go))
            {
                if (go.uniqueId != "")
                {
                    NetworkServer.Destroy(environment);
                }
            }
        }

        Database.singleton.EnvironmentsDelete();

        string message = "All environments have been deleted from the database!";
        chat.TargetMsgInfo(message);
    }

    [Command]
    public void CmdShowDatabaseInfo()
    {
        //Profiler.BeginSample("Management CmdShowDatabaseInfo");

        // load data from the database
        string text0 = Database.singleton.CountAccounts();
        string text1 = Database.singleton.CountAccountsByType(0);
        string text2 = Database.singleton.CountAccountsByType(1);
        string text3 = Database.singleton.CountStaffAccounts();

        string text4 = Database.singleton.CountCharacters();
        string text5 = Database.singleton.MaxGoldCharacter();
        string text6 = Database.singleton.CountSpawners();
        string text7 = Database.singleton.CountCheckpoints();
        string text8 = Database.singleton.CountBugReportsByStatus(1);
        string text9 = Database.singleton.CountBugReportsByStatus(2);
        string text10 = Database.singleton.CountHelpRequests();
        string text11 = Database.singleton.CountMonsters();
        string text12 = Database.singleton.CountNpcs();
        string text13 = Database.singleton.CountEnvironments();
        // Item Drop
#if ITEM_DROP_C || ITEM_DROP_R
        string text14 = Database.singleton.CountItems();
#endif

        // initialize a new array with loaded data, which also define its size
        string[] info = new string[]
        {
            text0,
            text1,
            text2,
            text3,

            text4,
            text5,
            text6,
            text7,
            text8,
            text9,
            text10,
            text11,
            text12,
            text13,
            // Item Drop
#if ITEM_DROP_C || ITEM_DROP_R
            text14
#endif
        };

        TargetShowDatabaseInfo(info);

        //Profiler.EndSample();
    }

    [TargetRpc]
    public void TargetShowDatabaseInfo(string[] info)
    {
        //Profiler.BeginSample("Management TargetShowDatabaseInfo");

        UIDatabaseInfo.singleton.info = info;
        UIDatabaseInfo.singleton.RefreshInfo();

        //Profiler.EndSample();
    }

    [Command]
    public void CmdMsgSave()
    {
        // players
        List<Player> players = onlinePlayers.Values.ToList();
        Database.singleton.CharacterSaveMany(players);

        // Item Drop
#if ITEM_DROP_C || ITEM_DROP_R
        string message = AddonItemDrop.SaveItems() ? "All items have been saved." : "All items are already saved.";
        chat.TargetMsgInfo($"Saved \n players: <color=#00FA9A>{players.Count}</color>\n items: <color=#00FA9A>{message}</color>");
#else
        string message = $"Saved \n players: <color=#00FA9A>{players.Count}</color>";
        chat.TargetMsgInfo(message);
#endif
    }
    #endregion

    #region Item Manager
    [Command]
    public void CmdMsgAdd(string playerName, int hashCode, int amount, bool infoWindow)
    {
        if (!CheckAccess())
            return;

        // try to find a player with that name
        if (onlinePlayers.TryGetValue(playerName, out var onlinePlayer))
        {
            // try to find an item in the item dictionary by hashCode
#if UMMORPG_3D_C
            if (ScriptableItem.dict.TryGetValue(hashCode, out var itemData))
            {
                // add the selected item if there is enough space in his inventory
                if (onlinePlayer.InventoryAdd(new Item(itemData), amount))
                {
                    // if you have installed Item Drop the following message will be shown with a link to the item
#if ITEM_DROP_C || ITEM_DROP_R
                    string message = amount == 1 ? $"[{itemData.name}] has been placed in your backpack." : $"[{itemData.name}] (<color=#ADFF2F>{amount}</color>) has been placed in your backpack.";
#else
                    string message = $"<color=#00FA9A>{itemData.name}</color> (<color=#ADFF2F>{amount}</color>) has been placed in your backpack.";
#endif
                    onlinePlayer.chat.TargetMsgInfo(message);

                    // this message will also be shown as a small popup dialog if you have selected this option
                    if (infoWindow)
                    {
                        onlinePlayer.TargetStaffMessage(message);
                    }
                }
                // notify me about the lack of space in his inventory
                else
                {
                    chat.TargetMsgInfo("Target backpack is full!");
                }
            }
#endif
#if UMMORPG_2D_C
            if (ScriptableItem.All.TryGetValue(hashCode, out var itemData))
            {
                // add the selected item if there is enough space in his inventory
                if (onlinePlayer.InventoryAdd(new Item(itemData), amount))
                {
                    // if you have installed Item Drop the following message will be shown with a link to the item
#if ITEM_DROP_C || ITEM_DROP_R
                    string message = amount == 1 ? $"[{itemData.name}] has been placed in your backpack." : $"[{itemData.name}] (<color=#ADFF2F>{amount}</color>) has been placed in your backpack.";
#else
                    string message = $"<color=#00FA9A>{itemData.name}</color> (<color=#ADFF2F>{amount}</color>) has been placed in your backpack.";
#endif
                    onlinePlayer.chat.TargetMsgInfo(message);

                    // this message will also be shown as a small popup dialog if you have selected this option
                    if (infoWindow)
                    {
                        onlinePlayer.TargetStaffMessage(message);
                    }
                }
                // notify me about the lack of space in his inventory
                else
                {
                    chat.TargetMsgInfo("Target backpack is full!");
                }
            }
#endif
#if UMMORPG_3D_R || UMMORPG_2D_R
            if (ScriptableItem.All.TryGetValue(hashCode, out var itemData))
            {
                // add the selected item if there is enough space in his inventory
                if (onlinePlayer.inventory.Add(new Item(itemData), amount))
                {
                    // if you have installed Item Drop the following message will be shown with a link to the item
#if ITEM_DROP_C || ITEM_DROP_R
                    string message = amount == 1 ? $"[{itemData.name}] has been placed in your backpack." : $"[{itemData.name}] (<color=#ADFF2F>{amount}</color>) has been placed in your backpack.";
#else
                    string message = $"<color=#00FA9A>{itemData.name}</color> (<color=#ADFF2F>{amount}</color>) has been placed in your backpack.";
#endif
                    onlinePlayer.chat.TargetMsgInfo(message);

                    // this message will also be shown as a small popup dialog if you have selected this option
                    if (infoWindow)
                    {
                        onlinePlayer.TargetStaffMessage(message);
                    }
                }
                // notify me about the lack of space in his inventory
                else
                {
                    chat.TargetMsgInfo("Target backpack is full!");
                }
            }
#endif
        }
        else
        {
            chat.TargetMsgInfo($"{playerName} could not be found!");
        }
    }
    #endregion

    #region Character Info
    [Command]
    public void CmdCharacterInfo(string playerName)
    {
        //Profiler.BeginSample("Management CmdSelected");

        // try to find a player with that name
        if (onlinePlayers.TryGetValue(playerName, out var onlinePlayer))
        {
            PlayerInfo selectedPlayer = new PlayerInfo
            {
#if UMMORPG_3D_C || UMMORPG_2D_C
                healthValue = onlinePlayer.health,
                healthMax = onlinePlayer.healthMax,
                healthPercent = onlinePlayer.HealthPercent(),
                manaValue = onlinePlayer.mana,
                manaMax = onlinePlayer.manaMax,
                manaPercent = onlinePlayer.ManaPercent(),
                account = onlinePlayer.account,
                access = onlinePlayer.Access,
                className = onlinePlayer.className,
                level = onlinePlayer.level,
                coins = onlinePlayer.coins,
                gold = onlinePlayer.gold,
                strength = onlinePlayer.strength,
                intelligence = onlinePlayer.intelligence,
                immortal = onlinePlayer.invincible,
                invisible = onlinePlayer.invisible,
                frozen = onlinePlayer.frozen,
                muted = onlinePlayer.muted
#endif
#if UMMORPG_3D_R || UMMORPG_2D_R
                healthValue = onlinePlayer.health.current,
                healthMax = onlinePlayer.health.max,
                healthPercent = onlinePlayer.health.Percent(),
                manaValue = onlinePlayer.mana.current,
                manaMax = onlinePlayer.mana.max,
                manaPercent = onlinePlayer.mana.Percent(),
                account = onlinePlayer.account,
                access = onlinePlayer.Access,
                className = onlinePlayer.className,
                level = onlinePlayer.level.current,
                coins = onlinePlayer.itemMall.coins,
                gold = onlinePlayer.gold,
                immortal = onlinePlayer.combat.invincible,
                invisible = onlinePlayer.invisible,
                frozen = onlinePlayer.frozen,
                muted = onlinePlayer.muted
#endif
            };

            TargetCharacterInfo(selectedPlayer);
        }
        //Profiler.EndSample();
    }

    [TargetRpc]
    public void TargetCharacterInfo(PlayerInfo selectedPlayer)
    {
        if (UICommands.singleton.Health != selectedPlayer.healthValue)
        {
            UICommands.singleton.Health = selectedPlayer.healthValue;
            UICommands.singleton.healthSlider.value = selectedPlayer.healthPercent;
        }

        if (UICommands.singleton.HealthMax != selectedPlayer.healthMax)
            UICommands.singleton.HealthMax = selectedPlayer.healthMax;

        if (UICommands.singleton.Mana != selectedPlayer.manaValue)
        {
            UICommands.singleton.Mana = selectedPlayer.manaValue;
            UICommands.singleton.manaSlider.value = selectedPlayer.manaPercent;
        }

        if (UICommands.singleton.ManaMax != selectedPlayer.manaMax)
            UICommands.singleton.ManaMax = selectedPlayer.manaMax;

        if (UICommands.singleton.Account != selectedPlayer.account)
            UICommands.singleton.Account = selectedPlayer.account;

        if (UICommands.singleton.Access != selectedPlayer.access)
            UICommands.singleton.Access = selectedPlayer.access;

        if (UICommands.singleton.ClassName != selectedPlayer.className)
            UICommands.singleton.ClassName = selectedPlayer.className;

        if (UICommands.singleton.Level != selectedPlayer.level)
            UICommands.singleton.Level = selectedPlayer.level;

        if (UICommands.singleton.Coins != selectedPlayer.coins)
            UICommands.singleton.Coins = selectedPlayer.coins;

        if (UICommands.singleton.Gold != selectedPlayer.gold)
            UICommands.singleton.Gold = selectedPlayer.gold;

        if (UICommands.singleton.Strength != selectedPlayer.strength)
            UICommands.singleton.Strength = selectedPlayer.strength;

        if (UICommands.singleton.Intelligence != selectedPlayer.intelligence)
            UICommands.singleton.Intelligence = selectedPlayer.intelligence;

        if (UICommands.singleton.Immortal != selectedPlayer.immortal)
            UICommands.singleton.Immortal = selectedPlayer.immortal;

        if (UICommands.singleton.Invisible != selectedPlayer.invisible)
            UICommands.singleton.Invisible = selectedPlayer.invisible;

        if (UICommands.singleton.Frozen != selectedPlayer.frozen)
            UICommands.singleton.Frozen = selectedPlayer.frozen;

        if (UICommands.singleton.Muted != selectedPlayer.muted)
            UICommands.singleton.Muted = selectedPlayer.muted;
    }
    #endregion

    #region Command - Change Access
    [Command]
    public void CmdMsgChangeAccess(string playerName, int access, bool infoWindow)
    {
        if (!CheckAccess())
            return;

        // try to find a player with that name
        if (onlinePlayers.TryGetValue(playerName, out var onlinePlayer))
        {
            // 0 banned
            // 1 player
            // 2 special access
            Database.singleton.ChangeAccess(onlinePlayer.account, access);
            onlinePlayer.Access = access;

            // try to find a staff member with that name
            if (onlinePlayers.TryGetValue(playerName, out var onlineStaffMember))
            {
                // a special access has been lost
                if (onlineStaffMember.Access == 0 || onlineStaffMember.Access == 1)
                {
                    // close his panels
                    onlineStaffMember.TargetClosePanels();

                    // remove the staff member from the dictionary
                    onlineStaffMembers.Remove(playerName);
                }
            }

            // a special access has been granted
            if (onlinePlayer.Access == 2)
            {
                // add new staff member to the dictionary
                onlineStaffMembers.Add(playerName, onlinePlayer);
            }

            // ban
            if (onlinePlayer.Access == 0)
            {
                string message = $"<color=#00FA9A>{onlinePlayer.name}</color> have been banned.";

                // notify all currently logged in players
                foreach (Player player in onlinePlayers.Values)
                {
                    if (player != onlinePlayer)
                    {
                        player.chat.TargetMsgInfo(message);

                        // this message will also be shown as a small popup dialog if you have selected this option
                        if (infoWindow)
                        {
                            player.TargetStaffMessage(message);
                        }
                    }
                }

                // this message will be shown as a small popup dialog
                onlinePlayer.TargetStaffMessage("You have been banned!");

                // disconnect him
                onlinePlayer.connectionToClient.Disconnect();
            }
            else
            {
                string message = $"Your access has been changed to <color=#00FA9A>{access}</color>.";
                onlinePlayer.chat.TargetMsgInfo(message);

                // this message will also be shown as a small popup dialog if you have selected this option
                if (infoWindow)
                {
                    onlinePlayer.TargetStaffMessage(message);
                }
            }
        }
    }

    [TargetRpc]
    void TargetClosePanels()
    {
        if (UIManagement.singleton.canvas.enabled)
        {
            UIManagement.singleton.Hide();
        }

        if (UIManipulationTools.singleton.canvas.enabled)
        {
            UIManipulationTools.singleton.Hide();
        }
    }
    #endregion

    #region Command - Immortal
    [Command]
    public void CmdMsgImmortal(string playerName, bool infoWindow)
    {
        if (!CheckAccess())
            return;

        // try to find a player with that name
        if (onlinePlayers.TryGetValue(playerName, out var onlinePlayer))
        {
            // immortal
            if (!onlinePlayer.IsImmortal())
            {
#if UMMORPG_3D_C || UMMORPG_2D_C
                onlinePlayer.invincible = true;
#endif
#if UMMORPG_3D_R || UMMORPG_2D_R
                onlinePlayer.combat.invincible = true;
#endif
                string message = "You are immortal.";
                onlinePlayer.chat.TargetMsgInfo(message);

                // this message will also be shown as a small popup dialog if you have selected this option
                if (infoWindow)
                {
                    onlinePlayer.TargetStaffMessage(message);
                }
            }
            // mortal
            else
            {
#if UMMORPG_3D_C || UMMORPG_2D_C
                onlinePlayer.invincible = false;
#endif
#if UMMORPG_3D_R || UMMORPG_2D_R
                onlinePlayer.combat.invincible = false;
#endif
                string message = "You are no longer immortal.";
                onlinePlayer.chat.TargetMsgInfo(message);

                // this message will also be shown as a small popup dialog if you have selected this option
                if (infoWindow)
                {
                    onlinePlayer.TargetStaffMessage(message);
                }
            }
        }
    }
    #endregion

    #region Command - Hide
    [Command]
    public void CmdMsgHide(string playerName, bool infoWindow)
    {
        if (!CheckAccess())
            return;

        // try to find a player with that name
        if (onlinePlayers.TryGetValue(playerName, out var onlinePlayer))
        {
            // hide
            if (!onlinePlayer.IsHidden())
            {
                onlinePlayer.invisible = true;
                onlinePlayer.Hide();

                string message = "You are invisible.";
                onlinePlayer.chat.TargetMsgInfo(message);

                // this message will also be shown as a small popup dialog if you have selected this option
                if (infoWindow)
                {
                    onlinePlayer.TargetStaffMessage(message);
                }
            }
            // unhide
            else
            {
                onlinePlayer.invisible = false;
                onlinePlayer.Show();

                string message = "You are no longer invisible.";
                onlinePlayer.chat.TargetMsgInfo(message);

                // this message will also be shown as a small popup dialog if you have selected this option
                if (infoWindow)
                {
                    onlinePlayer.TargetStaffMessage(message);
                }
            }
        }
    }
    #endregion

    #region Command - Freeze
    [Command]
    public void CmdMsgFreeze(string playerName, bool infoWindow)
    {
        if (!CheckAccess())
            return;

        // try to find a player with that name
        if (onlinePlayers.TryGetValue(playerName, out var onlinePlayer))
        {
            // freeze
            if (!onlinePlayer.IsFrozen())
            {
                // reset player target
                onlinePlayer.target = null;

                // stop the player's movement
                onlinePlayer.TargetFreeze();

                onlinePlayer.frozen = true;

                string message = "You have been frozen!";
                onlinePlayer.chat.TargetMsgInfo(message);

                // this message will also be shown as a small popup dialog if you have selected this option
                if (infoWindow)
                {
                    onlinePlayer.TargetStaffMessage(message);
                }
            }
            // unfreeze
            else
            {
                onlinePlayer.frozen = false;

                string message = "Your are no longer frozen.";
                onlinePlayer.chat.TargetMsgInfo(message);

                // this message will also be shown as a small popup dialog if you have selected this option
                if (infoWindow)
                {
                    onlinePlayer.TargetStaffMessage(message);
                }
            }
        }
    }

    [TargetRpc]
    void TargetFreeze()
    {
#if UMMORPG_3D_C
        if (agent.hasPath || agent.pathPending)
        {
            ResetMovement();
            SetIndicatorViaParent(transform);
        }
#endif
#if UMMORPG_2D_C
        if (agent.hasPath || agent.pathPending)
        {
            agent.ResetMovement();
            SetIndicatorViaParent(transform);
        }
#endif
#if UMMORPG_3D_R || UMMORPG_2D_R
        if (movement.IsMoving())
        {
            movement.Reset();
            indicator?.SetViaParent(transform);
        }
#endif
    }
    #endregion

    #region Command - Mute
    [Command]
    public void CmdMute(string playerName)
    {
        if (!CheckAccess())
            return;

        // try to find a player with that name
        if (onlinePlayers.TryGetValue(playerName, out var onlinePlayer))
        {
            // mute
            if (!onlinePlayer.IsMuted())
            {
                float counter = onlinePlayer.StartMute();
                onlinePlayer.TargetMute(counter);
            }
            // unmute
            else
            {
#if UMMORPG_3D_C || UMMORPG_2D_C
                int index = onlinePlayer.GetBuffIndexByName(MToolsSettings.Settings.muteBuff.name);
                onlinePlayer.buffs.RemoveAt(index);
#endif
#if UMMORPG_3D_R || UMMORPG_2D_R
                int index = onlinePlayer.skills.GetBuffIndexByName(MToolsSettings.Settings.muteBuff.name);
                onlinePlayer.skills.buffs.RemoveAt(index);
#endif
                onlinePlayer.TargetUnmute();
            }
        }
    }

    [TargetRpc]
    void TargetMute(float counter)
    {
        if (UIChat.singleton)
            UIChat.singleton.ChatMute();
        StartCoroutine(nameof(MuteCountdown), counter);
    }

    [TargetRpc]
    void TargetUnmute()
    {
        UIChat.singleton.ChatUnmute();
        StopCoroutine(nameof(MuteCountdown));
    }

    // additional commands to make it works with buff icons after relog
    [Command]
    public void CmdMsgMute()
    {
        muted = true;

        string message = "You have been muted!";
        chat.TargetMsgInfo(message);

        // this message will also be shown as a small popup dialog
        TargetStaffMessage(message);
    }

    [Command]
    public void CmdMsgUnmute()
    {
        muted = false;

        string message = "Your are no longer muted.";
        chat.TargetMsgInfo(message);

        // this message will also be shown as a small popup dialog
        TargetStaffMessage(message);
    }
    #endregion

    #region Command - Kick
    [Command]
    public void CmdMsgKick(string playerName, bool infoWindow)
    {
        if (!CheckAccess())
            return;

        // try to find a player with that name
        if (onlinePlayers.TryGetValue(playerName, out var onlinePlayer))
        {
            // only players
            if (onlinePlayer.CheckAccess())
            {
                chat.TargetMsgInfo("You can use this command only on players.");
                return;
            }

            string message = $"<color=#00FA9A>{onlinePlayer.name}</color> have been kicked from this server.";

            // notify all currently logged in players
            foreach (Player player in onlinePlayers.Values)
            {
                // except the chosen one
                if (player != onlinePlayer)
                {
                    player.chat.TargetMsgInfo(message);

                    // this message will also be shown as a small popup dialog if you have selected this option
                    if (infoWindow)
                    {
                        player.TargetStaffMessage(message);
                    }
                }
            }

            // this message will be shown as a small popup dialog
            onlinePlayer.TargetStaffMessage("You have been kicked from this server!");

            // disconnect him
            onlinePlayer.connectionToClient.Disconnect();
        }
    }
    #endregion

    #region Command - Kill
    [Command]
    public void CmdMsgKill(string playerName, bool infoWindow)
    {
        if (!CheckAccess())
            return;

        // try to find a player with that name
        if (onlinePlayers.TryGetValue(playerName, out var onlinePlayer))
        {
#if UMMORPG_3D_C || UMMORPG_2D_C
            // resurrect
            if (onlinePlayer.health == 0)
            {
                // resurrect where they died with a full health and mana bar
                onlinePlayer.resurrectRequested = true;

                // standard respawn (closest spawn point, 50% health)
                //onlinePlayer.respawnRequested = true;

                string message = "You have been resurrected.";
                onlinePlayer.chat.TargetMsgInfo(message);

                // this message will also be shown as a small popup dialog if you have selected this option
                if (infoWindow)
                {
                    onlinePlayer.TargetStaffMessage(message);
                }
            }
            // kill
            else
            {
                onlinePlayer.health = 0;

                string message = "You have been killed!";
                onlinePlayer.chat.TargetMsgInfo(message);

                // this message will also be shown as a small popup dialog if you have selected this option
                if (infoWindow)
                {
                    onlinePlayer.TargetStaffMessage(message);
                }
            }
#endif
#if UMMORPG_3D_R || UMMORPG_2D_R
            // resurrect
            if (onlinePlayer.health.current == 0)
            {
                // resurrect where they died with a full health and mana bar
                onlinePlayer.resurrectRequested = true;

                // standard respawn (closest spawn point, 50% health)
                //onlinePlayer.respawnRequested = true;

                string message = "You have been resurrected.";
                onlinePlayer.chat.TargetMsgInfo(message);

                // this message will also be shown as a small popup dialog if you have selected this option
                if (infoWindow)
                {
                    onlinePlayer.TargetStaffMessage(message);
                }
            }
            // kill
            else
            {
                onlinePlayer.health.current = 0;

                string message = "You have been killed!";
                onlinePlayer.chat.TargetMsgInfo(message);

                // this message will also be shown as a small popup dialog if you have selected this option
                if (infoWindow)
                {
                    onlinePlayer.TargetStaffMessage(message);
                }
            }
#endif
        }
    }
    #endregion

    #region Command - Fetch
    [Command]
    public void CmdMsgFetch(string playerName)
    {
        if (!CheckAccess())
            return;

#if UMMORPG_3D_C || UMMORPG_3D_R
        if (InDungeon())
        {
            string message = "You are in the dungeon instance, unable to fetch a player!";
            chat.TargetMsgInfo(message);
            return;
        }
#endif
        // try to find a player with that name
        if (onlinePlayers.TryGetValue(playerName, out var onlinePlayer))
        {
            // stop my movement
            TargetFreeze();

            Vector3 position = onlinePlayer.transform.position;
            if (transform.position != position)
            {
#if UMMORPG_3D_C || UMMORPG_3D_R
                if (onlinePlayer.InDungeon())
                {
                    onlinePlayer.DungeonControl = false;
                }
#endif
                // reset my target
                target = null;

                // reset the selected player target
                onlinePlayer.target = null;

                // teleport
                onlinePlayer.Teleport(transform.position);

                // reset the selected player indicator
                onlinePlayer.TargetResetIndicator();

                string message = $"<color=#00FA9A>{onlinePlayer.name}</color> have been moved to you.";
                chat.TargetMsgInfo(message);
            }
            else
            {
                string message = $"You have already summoned <color=#00FA9A>{onlinePlayer.name}</color>.";
                chat.TargetMsgInfo(message);
            }
        }
    }

    public void Teleport(Vector3 position)
    {
#if UMMORPG_3D_C
        Warp(position);
#endif
#if UMMORPG_2D_C
        if (Vector3.Distance(position, transform.position) > agent.speed * 2)
        {
            agent.Warp(position);
        }
        else
        {
            RpcTeleport(position);
        }
#endif
#if UMMORPG_3D_R || UMMORPG_2D_R
        movement.Warp(position);
#endif
    }

    [ClientRpc]
    public void RpcTeleport(Vector3 position)
    {
#if UMMORPG_2D_C
        agent.Warp(position);
#endif
    }

    [TargetRpc]
    public void TargetResetIndicator()
    {
#if UMMORPG_3D_C || UMMORPG_2D_C
        SetIndicatorViaParent(transform);
#endif
#if UMMORPG_3D_R || UMMORPG_2D_R
        indicator?.SetViaParent(transform);
#endif
    }
    #endregion

    #region Command - Go To
    [Command]
    public void CmdMsgGoTo(string playerName)
    {
        if (!CheckAccess())
            return;

        // try to find a player with that name
        if (onlinePlayers.TryGetValue(playerName, out var onlinePlayer))
        {
#if UMMORPG_3D_C || UMMORPG_3D_R
            if (onlinePlayer.InDungeon())
            {
                string message = $"<color=#00FA9A>{onlinePlayer.name}</color> is in a dungeon instance, unable to go there!";
                chat.TargetMsgInfo(message);
                return;
            }
#endif
            // stop the selected player movement
            onlinePlayer.TargetFreeze();

            Vector3 position = onlinePlayer.transform.position;
            if (transform.position != position)
            {
                // reset my target
                target = null;

                // reset the selected player target
                onlinePlayer.target = null;

                // teleport
                Teleport(position);

                string message = $"You have been moved to <color=#00FA9A>{onlinePlayer.name}</color>.";
                chat.TargetMsgInfo(message);

                // reset my indicator
                TargetResetIndicator();
            }
            else
            {
                string message = $"You are already where <color=#00FA9A>{onlinePlayer.name}</color> is.";
                chat.TargetMsgInfo(message);
            }
        }
    }
    #endregion

    #region Command - Go To Location
    [Command]
    public void CmdMsgGoToLocation(Vector3 position)
    {
        if (!CheckAccess())
            return;

        if (transform.position != position)
        {
            // reset my target
            target = null;

            // teleport
            Teleport(position);

            string message = $"You have been moved to \n<color=#00FA9A>{position}</color>.";
            chat.TargetMsgInfo(message);

            // reset my indicator
            TargetResetIndicator();
        }
        else
        {
            string message = $"You are already in the right place \n<color=#00FA9A>{position}</color>.";
            chat.TargetMsgInfo(message);
        }
    }
    #endregion    

    #region Command - Gold
    [Command]
    public void CmdMsgGold(string playerName, long amount, bool infoWindow)
    {
        if (!CheckAccess())
            return;

        // try to find a player with that name
        if (onlinePlayers.TryGetValue(playerName, out var onlinePlayer))
        {
            onlinePlayer.gold += amount;

            string message = $"<color=#00FA9A>Gold</color> (<color=#00FA9A>{amount}</color>) has been placed in your backpack.";
            onlinePlayer.chat.TargetMsgInfo(message);

            // this message will also be shown as a small popup dialog if you have selected this option
            if (infoWindow)
            {
                onlinePlayer.TargetStaffMessage(message);
            }
        }
    }
    #endregion

    #region Command - Coins
    [Command]
    public void CmdMsgCoins(string playerName, long amount, bool infoWindow)
    {
        if (!CheckAccess())
            return;

        // try to find a player with that name
        if (onlinePlayers.TryGetValue(playerName, out var onlinePlayer))
        {
#if UMMORPG_3D_C || UMMORPG_2D_C
            onlinePlayer.coins += amount;
#endif
#if UMMORPG_3D_R || UMMORPG_2D_R
            onlinePlayer.itemMall.coins += amount;
#endif
            string message = $"<color=#00FA9A>Coins</color> (<color=#00FA9A>{amount}</color>) has been added to your character.";
            onlinePlayer.chat.TargetMsgInfo(message);

            // this message will also be shown as a small popup dialog if you have selected this option
            if (infoWindow)
            {
                onlinePlayer.TargetStaffMessage(message);
            }
        }
    }
    #endregion

    #region Command - Level
    [Command]
    public void CmdMsgLevel(string playerName, int value, bool infoWindow)
    {
        if (!CheckAccess())
            return;

        // try to find a player with that name
        if (onlinePlayers.TryGetValue(playerName, out var onlinePlayer))
        {
#if UMMORPG_3D_C || UMMORPG_2D_C
            onlinePlayer.level = Mathf.Clamp(value, 1, onlinePlayer.maxLevel);
            onlinePlayer.health = onlinePlayer.healthMax;
            onlinePlayer.mana = onlinePlayer.manaMax;
#endif
#if UMMORPG_3D_R || UMMORPG_2D_R
            onlinePlayer.level.current = Mathf.Clamp(value, 1, onlinePlayer.level.max);
            onlinePlayer.health.current = onlinePlayer.health.max;
            onlinePlayer.mana.current = onlinePlayer.mana.max;
#endif
            string message = $"Your level has been set to <color=#00FA9A>{value}</color>.";
            onlinePlayer.chat.TargetMsgInfo(message);

            // this message will also be shown as a small popup dialog if you have selected this option
            if (infoWindow)
            {
                onlinePlayer.TargetStaffMessage(message);
            }
        }
    }
    #endregion

    #region Command - Exp
    [Command]
    public void CmdMsgExp(string playerName, int amount, bool infoWindow)
    {
        if (!CheckAccess())
            return;

        // try to find a player with that name
        if (onlinePlayers.TryGetValue(playerName, out var onlinePlayer))
        {
#if UMMORPG_3D_C || UMMORPG_2D_C
            onlinePlayer.experience += amount;
#endif
#if UMMORPG_3D_R || UMMORPG_2D_R
            onlinePlayer.experience.current += amount;
#endif
            string message = $"You have gained <color=#00FA9A>{amount}</color> experience.";
            onlinePlayer.chat.TargetMsgInfo(message);

            // this message will also be shown as a small popup dialog if you have selected this option
            if (infoWindow)
            {
                onlinePlayer.TargetStaffMessage(message);
            }
        }
    }
    #endregion

    #region Command - Strength
    [Command]
    public void CmdMsgStrength(string playerName, int value, bool infoWindow)
    {
        if (!CheckAccess())
            return;

        // try to find a player with that name
        if (onlinePlayers.TryGetValue(playerName, out var onlinePlayer))
        {
#if UMMORPG_3D_C || UMMORPG_2D_C
            onlinePlayer.strength = value;
            onlinePlayer.health = onlinePlayer.healthMax;
#endif
#if UMMORPG_3D_R || UMMORPG_2D_R
            onlinePlayer.health.current = onlinePlayer.health.max;
#endif
            string message = $"Your <color=#00FA9A>strength</color> has been set to <color=#00FA9A>{value}</color>.";
            onlinePlayer.chat.TargetMsgInfo(message);

            // this message will also be shown as a small popup dialog if you have selected this option
            if (infoWindow)
            {
                onlinePlayer.TargetStaffMessage(message);
            }
        }
    }
    #endregion

    #region Command - Intelligence
    [Command]
    public void CmdMsgIntelligence(string playerName, int value, bool infoWindow)
    {
        if (!CheckAccess())
            return;

        // try to find a player with that name
        if (onlinePlayers.TryGetValue(playerName, out var onlinePlayer))
        {
#if UMMORPG_3D_C || UMMORPG_2D_C
            onlinePlayer.intelligence = value;
            onlinePlayer.mana = onlinePlayer.manaMax;
#endif
#if UMMORPG_3D_R || UMMORPG_2D_R
            onlinePlayer.mana.current = onlinePlayer.mana.max;
#endif
            string message = $"Your <color=#00FA9A>intelligence</color> has been set to <color=#00FA9A>{value}</color>.";
            onlinePlayer.chat.TargetMsgInfo(message);

            // this message will also be shown as a small popup dialog if you have selected this option
            if (infoWindow)
            {
                onlinePlayer.TargetStaffMessage(message);
            }
        }
    }
    #endregion      

    #region Spawn Manager
    [Command]
    public void CmdMsgAddSpawner(Spawner spawnerData)
    {
        if (!CheckAccess())
            return;

        // save data to the database
        Database.singleton.SaveSpawner(spawnerData);

        // check if this spawner has not been activated yet
        if (AddonManagement.spawners.TryGetValue(spawnerData.uniqueId, out var activeSpawner))
        {
            // update existing one
            SpawnerUpdate(activeSpawner, spawnerData);

            string message = $"<color=#ADFF2F>{spawnerData.spawnName}</color> spawner has been updated by <color=#00FA9A>{name}</color>.";

            // notify all currently logged in staff members
            // and show changes in open panels right away, except for my character
            foreach (Player player in onlineStaffMembers.Values)
            {
                player.chat.TargetMsgInfo(message);

                // you already have this spawner in your panel, so skip me
                if (player.name != name)
                {
                    player.TargetChangeSpawner(spawnerData);
                }
            }
        }
        else
        {
            // create spawner on the server and add it to the dictionary
            Database.singleton.SpawnerActivate(spawnerData);

            string message = $"A new spawner has been created by <color=#00FA9A>{name}</color>.";

            // notify all currently logged in staff members
            // and show changes in open panels right away, except for my character
            foreach (Player player in onlineStaffMembers.Values)
            {
                player.chat.TargetMsgInfo(message);

                // you already have this spawner in your panel, so skip me
                if (player.name != name)
                {
                    player.TargetAddNewSpawner(spawnerData);
                }
            }
        }
    }

    void SpawnerUpdate(M_Spawner activeSpawner, Spawner spawnerData)
    {
        // create a new list and assign all groups based on data loaded from the spawn manager
        List<M_Spawner.SpawnGroup> spawnGroups = new List<M_Spawner.SpawnGroup>();
        for (int i = 0; i < spawnerData.groups.Length; ++i)
        {
            SpawnGroup group = spawnerData.groups[i];
            string monsterName = group.monsterName;
            int monsterAmount = group.amount;

            Monster prefab = AddonManagement.Monsters.Find(p => p.name == monsterName);
            if (prefab != null)
            {
                M_Spawner.SpawnGroup go = new M_Spawner.SpawnGroup
                {
                    monster = prefab,
                    amount = monsterAmount
                };
                spawnGroups.Add(go);
            }
        }

        // remove all currently spawned monsters within this spawner from the game
        foreach (Monster monster in activeSpawner.activeMonsters)
        {
            NetworkServer.Destroy(monster.gameObject);
        }
        activeSpawner.activeMonsters.Clear();

        // change data
        activeSpawner.transform.position = spawnerData.position;
        activeSpawner.range = spawnerData.area;
        activeSpawner.spawnGroups = spawnGroups.ToArray();

        // call the Spawn function
        activeSpawner.Spawn();
    }

    [TargetRpc]
    public void TargetAddNewSpawner(Spawner spawnerData)
    {
        if (UISpawnManager.singleton.canvas.enabled)
        {
            UISpawnManager.singleton.AddNewSpawner(spawnerData);
        }
    }

    [TargetRpc]
    public void TargetChangeSpawner(Spawner spawnerData)
    {
        if (UISpawnManager.singleton.canvas.enabled)
        {
            UISpawnManager.singleton.ChangeSpawner(spawnerData);
        }
    }

    [Command]
    public void CmdShowSpawners()
    {
        // load all spawners from the database
        Spawner[] spawners = Database.singleton.LoadSpawners();
        if (spawners != null)
        {
            TargetShowSpawners(spawners);
        }
        else TargetNoSpawners();
    }

    [TargetRpc]
    public void TargetShowSpawners(Spawner[] spawners)
    {
        UISpawnManager.singleton.LoadSpawners(spawners);
    }

    [TargetRpc]
    public void TargetNoSpawners()
    {
        UISpawnManager.singleton.NoSpawners();
    }

    [Command]
    public void CmdMsgSpawnerDelete(string uniqueId)
    {
        if (!CheckAccess())
            return;

        // delete data from the database by unique ID
        Database.singleton.SpawnerDelete(uniqueId);

        if (AddonManagement.spawners.TryGetValue(uniqueId, out var spawner))
        {
            // remove all currently spawned monsters within this spawner from the game
            foreach (Monster monster in spawner.activeMonsters)
            {
                NetworkServer.Destroy(monster.gameObject);
            }

            // remove spawner from the server
            NetworkServer.Destroy(spawner.gameObject);

            // and also from the dictionary
            AddonManagement.spawners.Remove(uniqueId);

            string message = $"One of the spawners has been deleted by <color=#00FA9A>{name}</color>.";

            // notify all currently logged in staff members
            // and show changes in open panels right away
            foreach (Player player in onlineStaffMembers.Values)
            {
                player.chat.TargetMsgInfo(message);
                player.TargetDestroySpawnerButton(uniqueId);
            }
        }
    }

    [TargetRpc]
    public void TargetDestroySpawnerButton(string uniqueId)
    {
        if (UISpawnManager.singleton.canvas.enabled)
        {
            UISpawnManager.singleton.DestroySpawnerButton(uniqueId);
        }
    }

    [Command]
    public void CmdMsgDeleteSpawners()
    {
        if (!CheckAccess())
            return;

        // delete all spawners from the database
        Database.singleton.SpawnersDelete();

        foreach (M_Spawner spawner in AddonManagement.spawners.Values)
        {
            // remove all currently spawned monsters within this spawner from the game
            foreach (Monster monster in spawner.activeMonsters)
            {
                NetworkServer.Destroy(monster.gameObject);
            }

            // remove spawner from the server
            NetworkServer.Destroy(spawner.gameObject);
        }
        // remove all spawners from the dictionary
        AddonManagement.spawners.Clear();

        string message = $"All spawners has been deleted by <color=#00FA9A>{name}</color>.";

        // notify all currently logged in staff members
        // and show changes in open panels right away
        foreach (Player player in onlineStaffMembers.Values)
        {
            player.chat.TargetMsgInfo(message);
            player.TargetDestroySpawners();
        }
    }

    [TargetRpc]
    public void TargetDestroySpawners()
    {
        if (UISpawnManager.singleton.canvas.enabled)
        {
            UISpawnManager.singleton.DestroySpawnerButtons();
        }
    }
    #endregion

    #region Creator
    [Command]
#if UMMORPG_3D_C || UMMORPG_3D_R
    public void CmdMsgCreateObject(string objName, Vector3 position, Quaternion rotation)
#else
    public void CmdMsgCreateObject(string objName, Vector2 position)
#endif
    {
        if (!CheckAccess())
            return;

#if UMMORPG_3D_C || UMMORPG_3D_R
        if (InDungeon())
        {
            string message = "You are in the dungeon instance, unable to place any object there!";
            chat.TargetMsgInfo(message);
            return;
        }
#endif
        Transform prefab = ((NetworkManagerMMO)NetworkManager.singleton).RegisteredPrefabs().Find(p => p.name == objName);
        if (prefab != null)
        {
#if UMMORPG_3D_C || UMMORPG_3D_R
            GameObject go = Instantiate(prefab.gameObject, position, rotation);
#else
            GameObject go = Instantiate(prefab.gameObject, position, prefab.transform.rotation);
#endif
            if (prefab.CompareTag("Monster"))
            {
                if (go.TryGetComponent(out Monster monster))
                {
                    monster.name = prefab.name;
                    monster.uniqueId = Guid.NewGuid().ToString();
#if UMMORPG_3D_C
                    monster.Warp(position);
#endif
#if UMMORPG_2D_C
                    monster.agent.Warp(position);
#endif
#if UMMORPG_3D_R || UMMORPG_2D_R
                    monster.movement.Warp(position);
#endif
                    NetworkServer.Spawn(go);
                    Database.singleton.MonsterSave(monster, false);
                }
            }
            else if (prefab.CompareTag("Npc"))
            {
                if (go.TryGetComponent(out Npc npc))
                {
                    npc.name = prefab.name;
                    npc.uniqueId = Guid.NewGuid().ToString();
#if UMMORPG_3D_C
                    npc.Warp(position);
#endif
#if UMMORPG_2D_C
                    npc.agent.Warp(position);
#endif
#if UMMORPG_3D_R || UMMORPG_2D_R
                    npc.movement.Warp(position);
#endif
                    NetworkServer.Spawn(go);
                    Database.singleton.NpcSave(npc, false);
                }
            }
            else if (prefab.CompareTag("Environment"))
            {
                if (go.TryGetComponent(out M_Environment environment))
                {
                    environment.name = prefab.name;
                    environment.uniqueId = Guid.NewGuid().ToString();
                    environment.transform.position = position;

                    NetworkServer.Spawn(go);
                    Database.singleton.EnvironmentSave(environment, false);
                }
            }
        }
    }
    #endregion

    #region Note
    [Command]
    public void CmdShowNote(string playerName)
    {
        if (!CheckAccess())
            return;

        // try to find a player with that name
        if (onlinePlayers.TryGetValue(playerName, out var onlinePlayer))
        {
            // load a note about this player from the database
            string note = Database.singleton.LoadNote(playerName);
            TargetShowNote(note);
            //Debug.Log(string.Format("Note about {0}: {1}", playerName, note));
        }
    }

    [TargetRpc]
    public void TargetShowNote(string note)
    {
        UINote.singleton.noteInputField.text = note;
    }

    [Command]
    public void CmdMsgChangeNote(string playerName, string note)
    {
        if (!CheckAccess())
            return;

        // try to find a player with that name
        if (onlinePlayers.TryGetValue(playerName, out var onlinePlayer))
        {
            // save changes to the database
            Database.singleton.ChangeNote(playerName, note);

            string message = $"Note about <color=#00FA9A>{playerName}</color> has been changed by <color=#00FA9A>{name}</color>.";

            // notify all currently logged in staff members
            // and show changes in open panels right away
            foreach (Player player in onlineStaffMembers.Values)
            {
                player.chat.TargetMsgInfo(message);
                player.TargetChangeNote(playerName, note);
            }
        }
    }

    [TargetRpc]
    public void TargetChangeNote(string playerName, string note)
    {
        if (UINote.singleton.canvas.enabled)
        {
            if (UIManagement.singleton.currentName == playerName)
            {
                UINote.singleton.noteInputField.text = note;
            }
        }
    }
    #endregion

    #region Checkpoints
    [Command]
    public void CmdMsgAddCheckpoint(Checkpoint checkpoint)
    {
        if (!CheckAccess())
            return;

        Database.singleton.SaveCheckpoint(checkpoint);

        string message = $"A new checkpoint has been created by <color=#00FA9A>{name}</color>.";

        // notify all currently logged in staff members
        // and show changes in open panels right away
        foreach (Player player in onlineStaffMembers.Values)
        {
            player.chat.TargetMsgInfo(message);
            player.TargetAddCheckpoint(checkpoint);
        }
    }

    [TargetRpc]
    public void TargetAddCheckpoint(Checkpoint checkpoint)
    {
        if (UICheckpoints.singleton.canvas.enabled)
        {
            UICheckpoints.singleton.AddCheckpointButton(checkpoint);
        }
    }

    [Command]
    public void CmdShowCheckpoints()
    {
        // load all spawners from the database
        Checkpoint[] checkpoints = Database.singleton.LoadCheckpoints();
        if (checkpoints != null)
        {
            TargetShowCheckpoints(checkpoints);
        }
        else TargetNoCheckpoints();
    }

    [TargetRpc]
    public void TargetShowCheckpoints(Checkpoint[] checkpoints)
    {
        UICheckpoints.singleton.LoadCheckpoints(checkpoints);
    }

    [TargetRpc]
    public void TargetNoCheckpoints()
    {
        UICheckpoints.singleton.NoCheckpoints();
    }

    [Command]
    public void CmdMsgCheckpointDelete(string uniqueId)
    {
        if (!CheckAccess())
            return;

        // delete data from the database by unique ID
        Database.singleton.CheckpointDelete(uniqueId);

        string message = $"One of the checkpoint has been deleted by <color=#00FA9A>{name}</color>.";

        // notify all currently logged in staff members
        // and show changes in open panels right away
        foreach (Player player in onlineStaffMembers.Values)
        {
            player.chat.TargetMsgInfo(message);
            player.TargetDestroyCheckpoint(uniqueId);
        }
    }

    [TargetRpc]
    public void TargetDestroyCheckpoint(string uniqueId)
    {
        if (UICheckpoints.singleton.canvas.enabled)
        {
            UICheckpoints.singleton.DestroyCheckpointButton(uniqueId);
        }
    }

    [Command]
    public void CmdMsgDeleteCheckpoints()
    {
        if (!CheckAccess())
            return;

        // delete all checkpoints from the database
        Database.singleton.CheckpointsDelete();

        string message = $"All checkpoints has been deleted by <color=#00FA9A>{name}</color>.";

        // notify all currently logged in staff members
        // and show changes in open panels right away
        foreach (Player player in onlineStaffMembers.Values)
        {
            player.chat.TargetMsgInfo(message);
            player.TargetDestroyCheckpoints();
        }
    }

    [TargetRpc]
    public void TargetDestroyCheckpoints()
    {
        if (UICheckpoints.singleton.canvas.enabled)
        {
            UICheckpoints.singleton.DestroyCheckpointButtons();
        }
    }
    #endregion

    #region Help Requests
    [Command]
    public void CmdMsgAddHelpRequest(HelpRequest helpRequest)
    {
        // save data to the database
        Database.singleton.SaveHelpRequest(helpRequest);

        // the number of help requests loaded from the database
        string numHelpRequest = Database.singleton.CountHelpRequests();

        string message = $"Your help request has been sent. You are <color=#00FA9A>{numHelpRequest}</color> in the queue.";
        chat.TargetMsgInfo(message);

        string messageToStaff = $"A new help request has been sent by <color=#00FA9A>{helpRequest.characterName}</color>.";

        // notify all currently logged in staff members
        // and show changes in open panels right away
        foreach (Player player in onlineStaffMembers.Values)
        {
            player.chat.TargetMsgInfo(messageToStaff);
            player.TargetAddHelpRequest(helpRequest);
        }
    }

    [TargetRpc]
    public void TargetAddHelpRequest(HelpRequest helpRequest)
    {
        if (UIHelpRequests.singleton.canvas.enabled)
        {
            UIHelpRequests.singleton.AddHelpRequest(helpRequest);
        }
    }

    [Command]
    public void CmdShowHelpRequests()
    {
        if (!CheckAccess())
            return;

        // load all help requests from the database
        HelpRequest[] helpRequests = Database.singleton.LoadHelpRequests();
        if (helpRequests != null)
        {
            TargetShowHelpRequests(helpRequests);
        }
        else TargetNoHelpRequests();
    }

    [TargetRpc]
    public void TargetShowHelpRequests(HelpRequest[] helpRequests)
    {
        UIHelpRequests.singleton.LoadHelpRequests(helpRequests);
    }

    [TargetRpc]
    public void TargetNoHelpRequests()
    {
        UIHelpRequests.singleton.NoHelpRequests();
    }

    [Command]
    public void CmdMsgHelpRequestDelete(string uniqueId)
    {
        if (!CheckAccess())
            return;

        // delete data from the database by unique ID
        Database.singleton.HelpRequestDelete(uniqueId);

        string message = $"One of the help requests has been deleted by <color=#00FA9A>{name}</color>.";

        // notify all currently logged in staff members
        // and show changes in open panels right away
        foreach (Player player in onlineStaffMembers.Values)
        {
            player.chat.TargetMsgInfo(message);
            player.TargetDestroyHelpRequest(uniqueId);
        }
    }

    [TargetRpc]
    public void TargetDestroyHelpRequest(string uniqueId)
    {
        if (UIHelpRequests.singleton.canvas.enabled)
        {
            UIHelpRequests.singleton.DestroyHelpRequestButton(uniqueId);
        }
    }

    [Command]
    public void CmdMsgDeleteHelpRequests()
    {
        if (!CheckAccess())
            return;

        // delete all help requests from the database
        Database.singleton.HelpRequestsDelete();

        string message = $"All help requests has been deleted by <color=#00FA9A>{name}</color>.";

        // notify all currently logged in staff members
        // and show changes in open panels right away
        foreach (Player player in onlineStaffMembers.Values)
        {
            player.chat.TargetMsgInfo(message);
            player.TargetDestroyHelpRequests();
        }
    }

    [TargetRpc]
    public void TargetDestroyHelpRequests()
    {
        if (UIHelpRequests.singleton.canvas.enabled)
        {
            UIHelpRequests.singleton.DestroyHelpRequestButtons();
        }
    }
    #endregion

    #region Bug Reports
    [Command]
    public void CmdMsgAddBugReport(BugReport bugReport)
    {
        // save data to the database
        Database.singleton.SaveBugReport(bugReport);

        string message = "Your bug report has been sent.";
        chat.TargetMsgInfo(message);

        string messageToStaff = $"A new bug has been reported by <color=#00FA9A>{name}</color>.";

        // notify all currently logged in staff members
        // and show changes in open panels right away
        foreach (Player player in onlineStaffMembers.Values)
        {
            player.chat.TargetMsgInfo(messageToStaff);
            player.TargetAddNewBugReport(bugReport);
        }
    }

    [TargetRpc]
    public void TargetAddNewBugReport(BugReport bugReport)
    {
        if (UIBugReports.singleton.canvas.enabled)
        {
            if (UIBugReports.singleton.dropDown.value == 0)
            {
                UIBugReports.singleton.AddBugReport(bugReport);
            }
        }
    }

    [Command]
    public void CmdMsgEditBugReportStatus(BugReport bugReport)
    {
        if (!CheckAccess())
            return;

        // save changes to the database
        Database.singleton.EditBugReportStatus(bugReport.uniqueId, bugReport.status);

        switch (bugReport.status)
        {
            case 1:
                // verified                
                string messageVerified = $"One of the bug reports has been verified by <color=#00FA9A>{name}</color>.";

                // notify all currently logged in staff members
                // and show changes in open panels right away
                foreach (Player player in onlineStaffMembers.Values)
                {
                    player.chat.TargetMsgInfo(messageVerified);
                    player.TargetDestroyBugReport(bugReport.uniqueId);
                    player.TargetAddVerifiedBugReport(bugReport);
                }
                break;
            case 2:
                // fixed
                string messageFixed = $"One of the bug reports has been set as fixed by <color=#00FA9A>{name}</color>.";

                // notify all currently logged in staff members
                // and show changes in open panels right away
                foreach (Player player in onlineStaffMembers.Values)
                {
                    player.chat.TargetMsgInfo(messageFixed);
                    player.TargetDestroyBugReport(bugReport.uniqueId);
                    player.TargetAddFixedBugReport(bugReport);
                }
                break;
        }
    }

    [TargetRpc]
    public void TargetAddVerifiedBugReport(BugReport bugReport)
    {
        if (UIBugReports.singleton.canvas.enabled)
        {
            if (UIBugReports.singleton.dropDown.value == 1)
            {
                UIBugReports.singleton.AddBugReport(bugReport);
            }
        }
    }

    [TargetRpc]
    public void TargetAddFixedBugReport(BugReport bugReport)
    {
        if (UIBugReports.singleton.canvas.enabled)
        {
            if (UIBugReports.singleton.dropDown.value == 2)
            {
                UIBugReports.singleton.AddBugReport(bugReport);
            }
        }
    }

    [Command]
    public void CmdMsgEditBugReportDescription(string uniqueId, string description)
    {
        if (!CheckAccess())
            return;

        // save changes to the database
        Database.singleton.EditBugReportDescription(uniqueId, description);

        string message = $"One of the bug reports has been edited by <color=#00FA9A>{name}</color>.";

        // notify all currently logged in staff members
        // and show changes in open panels right away
        foreach (Player player in onlineStaffMembers.Values)
        {
            player.chat.TargetMsgInfo(message);
            player.TargetEditBugReportDescription(uniqueId, description);
        }
    }

    [TargetRpc]
    public void TargetEditBugReportDescription(string uniqueId, string description)
    {
        if (UIBugReports.singleton.canvas.enabled)
        {
            UIBugReports.singleton.EditBugReport(uniqueId, description);
        }
    }

    [Command]
    public void CmdShowBugReports(int status)
    {
        // load all bug reports from the database by status
        BugReport[] bugReports = Database.singleton.LoadBugReports(status);
        if (bugReports != null)
        {
            TargetShowBugReports(bugReports);
        }
        else TargetNoBugReports();
    }

    [TargetRpc]
    public void TargetShowBugReports(BugReport[] bugReports)
    {
        UIBugReports.singleton.LoadBugReports(bugReports);
    }

    [TargetRpc]
    public void TargetNoBugReports()
    {
        UIBugReports.singleton.NoBugReports();
    }

    [Command]
    public void CmdMsgBugReportDelete(string uniqueId)
    {
        if (!CheckAccess())
            return;

        // delete data from the database by unique ID
        Database.singleton.BugReportDelete(uniqueId);

        string message = $"One of the bug report has been deleted by <color=#00FA9A>{name}</color>.";

        // notify all currently logged in staff members
        // and show changes in open panels right away
        foreach (Player player in onlineStaffMembers.Values)
        {
            player.chat.TargetMsgInfo(message);
            player.TargetDestroyBugReport(uniqueId);
        }
    }

    [TargetRpc]
    public void TargetDestroyBugReport(string uniqueId)
    {
        if (UIBugReports.singleton.canvas.enabled)
        {
            UIBugReports.singleton.DestroyBugReportButton(uniqueId);
        }
    }

    [Command]
    public void CmdMsgDeleteBugReports(int status)
    {
        if (!CheckAccess())
            return;

        // delete all bug reports from the database by status
        Database.singleton.BugReportsDelete(status);

        string message = $"All bug reports has been deleted by <color=#00FA9A>{name}</color>.";

        // notify all currently logged in staff members
        // and show changes in open panels right away
        foreach (Player player in onlineStaffMembers.Values)
        {
            player.chat.TargetMsgInfo(message);
            player.TargetDestroyBugReports(status);
        }
    }

    [TargetRpc]
    public void TargetDestroyBugReports(int status)
    {
        if (UIBugReports.singleton.canvas.enabled)
        {
            if (UIBugReports.singleton.dropDown.value == status)
            {
                UIBugReports.singleton.DestroyBugReportButtons();
            }
        }
    }
    #endregion

    #region Info Messenger
    [Command]
    public void CmdSendPublicMessage(string message, bool infoWindow)
    {
        if (!CheckAccess())
            return;

        if (infoWindow)
        {
            foreach (Player player in onlinePlayers.Values)
            {
                // send a message to all currently logged in players
                player.chat.TargetMsgInfo(message);

                // with a small popup dialog
                player.TargetStaffMessage(message);
            }
        }
        else
        {
            foreach (Player player in onlinePlayers.Values)
            {
                // send a message to all currently logged in players
                player.chat.TargetMsgInfo(message);
            }
        }
    }

    [Command]
    public void CmdSendStaffMessage(string message, bool infoWindow)
    {
        if (!CheckAccess())
            return;

        if (infoWindow)
        {
            foreach (Player player in onlineStaffMembers.Values)
            {
                // send a message to all currently logged in staff members
                player.chat.TargetMsgInfo(message);

                // with a small popup dialog
                player.TargetStaffMessage(message);
            }
        }
        else
        {
            foreach (Player player in onlineStaffMembers.Values)
            {
                // send a message to all currently logged in staff members
                player.chat.TargetMsgInfo(message);
            }
        }
    }

    [Command]
    public void CmdSendIndividualMessage(string playerName, string message, bool infoWindow)
    {
        if (!CheckAccess())
            return;

        // try to find a player with that name
        if (onlinePlayers.TryGetValue(playerName, out var onlinePlayer))
        {
            // send an individual message to the selected player
            onlinePlayer.chat.TargetMsgInfo(message);

            // this message will also be shown as a small popup dialog if you have selected this option
            if (infoWindow)
            {
                onlinePlayer.TargetStaffMessage(message);
            }
        }
    }

    [TargetRpc]
    public void TargetStaffMessage(string message)
    {
        UIInfo.singleton.Show(message);
    }
    #endregion

    #region Manipulation Tools
    [Command]
    public void CmdObjectRotateX(GameObject target, float value)
    {
        if (!CheckAccess())
            return;

        if (target != null)
        {
            Vector3 _rotation = target.transform.eulerAngles;
            _rotation.x += value;
            target.transform.eulerAngles = _rotation;

            RpcRotateObject(target, _rotation);
        }
    }

    [Command]
    public void CmdObjectRotateY(GameObject target, float value)
    {
        if (!CheckAccess())
            return;

        if (target != null)
        {
            Vector3 _rotation = target.transform.eulerAngles;
            _rotation.y += value;
            target.transform.eulerAngles = _rotation;

            RpcRotateObject(target, _rotation);
        }
    }

    [Command]
    public void CmdObjectRotateZ(GameObject target, float value)
    {
        if (!CheckAccess())
            return;

        if (target != null)
        {
            Vector3 _rotation = target.transform.eulerAngles;
            _rotation.z += value;
            target.transform.eulerAngles = _rotation;

            RpcRotateObject(target, _rotation);
        }
    }

    [ClientRpc]
    public void RpcRotateObject(GameObject target, Vector3 rot)
    {
        if (target != null)
        {
            target.transform.eulerAngles = rot;
        }
    }

    [Command]
    public void CmdObjectPositionY(GameObject target, float value)
    {
        if (!CheckAccess())
            return;

        if (target != null)
        {
            Vector3 _position = target.transform.position;
            _position.y += value;
            target.transform.position = _position;

            RpcObjectPosition(target, _position);
        }
    }

    [ClientRpc]
    public void RpcObjectPosition(GameObject target, Vector3 pos)
    {
        if (target != null)
        {
            target.transform.position = pos;
        }
    }

    [Command]
    public void CmdObjectDelete(GameObject target)
    {
        if (!CheckAccess())
            return;

        if (target != null)
        {
            //string message = $"name: {target.name} position: {target.transform.position}";
            //chat.TargetMsgInfo(message);

            // close all manipulation tools panels for this object
            foreach (Player player in onlineStaffMembers.Values)
            {
                player.TargetCloseManipulationTools(target);
            }

            if (target.CompareTag("Monster"))
            {
                if (target.TryGetComponent(out Monster monster))
                {
                    Database.singleton.MonsterDelete(monster.uniqueId);
                }
            }
            else if (target.CompareTag("Npc"))
            {
                if (target.TryGetComponent(out Npc npc))
                {
                    Database.singleton.NpcDelete(npc.uniqueId);
                }
            }
            else if (target.CompareTag("Environment"))
            {
                if (target.TryGetComponent(out M_Environment environment))
                {
                    Database.singleton.EnvironmentDelete(environment.uniqueId);
                }
            }
            NetworkServer.Destroy(target);
        }
    }

    [TargetRpc]
    public void TargetCloseManipulationTools(GameObject target)
    {
        if (UIManipulationTools.singleton.canvas.enabled)
        {
            if (UIManipulationTools.singleton.MyTarget == target)
            {
                UIManipulationTools.singleton.canvas.enabled = false;
            }
        }
    }

    [Command]
    public void CmdEntityKill(GameObject target)
    {
        if (!CheckAccess())
            return;

        if (target != null)
        {
            if (target.TryGetComponent(out Entity entity))
            {
#if UMMORPG_3D_C || UMMORPG_2D_C
                if (entity.health != 0) entity.health = 0;
#endif
#if UMMORPG_3D_R || UMMORPG_2D_R
                if (entity.health.current != 0) entity.health.current = 0;
#endif
            }
        }
    }

    [Command]
    public void CmdObjectSave(GameObject target)
    {
        if (!CheckAccess())
            return;

        if (target != null)
        {
            if (target.CompareTag("Monster"))
            {
                if (target.TryGetComponent(out Monster monster))
                {
                    Database.singleton.MonsterSave(monster, false);
                }
            }
            else if (target.CompareTag("Npc"))
            {
                if (target.TryGetComponent(out Npc npc))
                {
                    Database.singleton.NpcSave(npc, false);
                }
            }
            else if (target.CompareTag("Environment"))
            {
                if (target.TryGetComponent(out M_Environment environment))
                {
                    Database.singleton.EnvironmentSave(environment, false);
                }
            }
        }
    }
    #endregion

    #region Password Change
    [Command]
    public void CmdMsgChangePassword(string newPassword)
    {
        Database.singleton.ChangePassword(account, newPassword);

        string message = "Your password has been changed.";
        chat.TargetMsgInfo(message);

        TargetChangePassword(newPassword);
    }

    [TargetRpc]
    void TargetChangePassword(string newPassword)
    {
        UIPasswordChange.singleton.authenticator.loginPassword = newPassword;
    }
    #endregion
}
#endif
