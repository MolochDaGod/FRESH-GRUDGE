using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

// ADMINISTRATION - CONSOLE
public partial class PlayerAdminChatCommand : NetworkBehaviour
{
    public Player player;

#if _iMMOCOMPLETECHAT
    public PlayerCompleteChat playerChat;
#else
    public PlayerChat playerChat;
#endif
    public AdminCommandList adminCommands;

    protected Tmpl_AdminCommand currentCommandTmpl;
    protected NetworkManagerMMO _NetworkManagerMMO;

    // -----------------------------------------------------------------------------------
    // Start
    // -----------------------------------------------------------------------------------
    private void Start()
    {
        _NetworkManagerMMO = FindObjectOfType<NetworkManagerMMO>();
        //player = GetComponentInParent<player>();
    }

    public override void OnStartLocalPlayer()
    {
        playerChat.onSubmit.AddListener(OnSubmit_Administration);
    }
    private void OnSubmit_Administration(string text)
    {
        Player player = Player.localPlayer;
        if (!player || player.AdminLevel <= 0 || string.IsNullOrWhiteSpace(text)) return;

        ProcessCommand(text);
    }
    // ==================================== GENERAL  =====================================

    // -----------------------------------------------------------------------------------
    // ProcessCommand
    // -----------------------------------------------------------------------------------
    public void ProcessCommand(string commandText)
    {
        if (string.IsNullOrWhiteSpace(commandText) || player.AdminLevel <= 0) return;

        foreach (Tmpl_AdminCommand command in adminCommands.commands)
        {

            /**
             * faut vérifier si le joueur a un status admin ou non
             **/
            if (commandText.StartsWith(command.commandName) && !string.IsNullOrWhiteSpace(command.functionName))
            {
                // Tout les joueurs peuvent utilisé ces commands
                if (player.AdminLevel == 1 && command.commandLevel == 1)
                {
                    currentCommandTmpl = command;

                    string[] parsedArgs = getParsed(commandText);

                    if (parsedArgs != null)
                    {
                        //player.Tools_AddMessage("[Sys] Executing admin command...");
                        callCommand(parsedArgs);

                        break;
                    }
                }
                else if (player.AdminLevel > 1)
                {
                    if (player.AdminLevel >= command.commandLevel)
                    {
                        currentCommandTmpl = command;

                        string[] parsedArgs = getParsed(commandText);

                        if (parsedArgs != null)
                        {
                            //player.Tools_AddMessage("[Sys] Executing admin command...");
                            callCommand(parsedArgs);

                            break;
                        }
                    }
                    else
                    {
                        player.Tools_AddMessage("[Sys] You do not have the admin rights for this command!");
                    }
                }
            }
            /*else
            {
                player.Tools_AddMessage("[Sys] /Help command for open Help panel");
            }*/
        }
    }

    // -----------------------------------------------------------------------------------
    // callCommand
    // -----------------------------------------------------------------------------------
    protected void callCommand(string[] parsedArgs)
    {
        string functionName = "Admin_" + currentCommandTmpl.functionName;
        Type thisType = this.GetType();
        MethodInfo targetMethod = thisType.GetMethod(functionName);
        targetMethod.Invoke(this, new object[] { parsedArgs });
    }

    // -----------------------------------------------------------------------------------
    // ParseGeneral
    // -----------------------------------------------------------------------------------
    protected string ParseGeneral(string command, string msg)
    {
        return msg.StartsWith(command + " ") ? msg.Substring(command.Length + 1) : "";
    }

    // -----------------------------------------------------------------------------------
    // ParseCommand
    // -----------------------------------------------------------------------------------
    protected string[] ParseCommand(string command, string msg, int spaceCount)
    {
        string[] temp = new string[spaceCount];

        string content = ParseGeneral(command, msg);

        if (content != "")
        {
            int startIndex = 0;

            if (spaceCount > 0)
            {
                for (int no = 0; no < spaceCount; no++)
                {
                    int i = content.IndexOf(" ");
                    if (i >= 0)
                    {
                        if (no != spaceCount - 1)
                        {
                            temp[no] = content.Substring(0, i);
                            content = content.Remove(0, i + 1);
                        }
                        else
                        {
                            temp[no] = content.Substring(startIndex);
                        }
                    }
                    else
                    {
                        temp[no] = content.Substring(startIndex);
                    }
                }
            }
        }
        return temp;
    }

    // -----------------------------------------------------------------------------------
    // getPlayerTargets
    // -----------------------------------------------------------------------------------
    protected List<Player> getPlayerTargets(string targetString, string targetName)
    {
        if (string.IsNullOrWhiteSpace(targetString) || string.IsNullOrWhiteSpace(targetName)) return null;

        List<Player> players = new List<Player>();

        // -- Add online player by name
        if (targetString == adminCommands.tagTargetPlayer && Player.onlinePlayers.ContainsKey(targetName))
        {
            players.Add(Player.onlinePlayers[targetName]);

            // -- Add all online party members
        }
        else if (targetString == adminCommands.tagTargetParty)
        {
            Player player = Player.onlinePlayers[targetName];
            if (player && player.party.InParty())
            {
                foreach (string name in player.party.party.members)
                {
                    if (Player.onlinePlayers.ContainsKey(name))
                        players.Add(Player.onlinePlayers[name]);
                }
            }

            // -- Add all online guild members
        }
        else if (targetString == adminCommands.tagTargetGuild)
        {
            Player player = Player.onlinePlayers[targetName];
            if (player && player.guild.InGuild())
            {
                foreach (GuildMember member in player.guild.guild.members)
                {
                    if (Player.onlinePlayers.ContainsKey(member.name))
                        players.Add(Player.onlinePlayers[member.name]);
                }
            }

            // -- Add all online realm members
#if _iMMOPVP
        }
        else if (targetString == adminCommands.tagTargetRealm)
        {
            Player player = Player.onlinePlayers[targetName];
            if (player)
                players.AddRange(Player.onlinePlayers.Values.Where(x => x.GetAlliedRealms(player)).ToList());
#endif

            // -- Add all online members
        }
        else if (targetString == adminCommands.tagTargetAll)
        {
            players.AddRange(Player.onlinePlayers.Values.ToList());
        }

        return players;
    }

    // -----------------------------------------------------------------------------------
    // getItem
    // -----------------------------------------------------------------------------------
    protected ScriptableItem getItem(string itemName)
    {
        ScriptableItem item;
        ScriptableItem.All.TryGetValue(itemName.GetStableHashCode(), out item);
        return item;
    }

    // -----------------------------------------------------------------------------------
    // getParsed
    // -----------------------------------------------------------------------------------
    protected string[] getParsed(string currentCommandText)
    {
        if (currentCommandTmpl == null || string.IsNullOrWhiteSpace(currentCommandText)) return null;

        string[] parsed = new string[currentCommandTmpl.arguments.Length];

        parsed = ParseCommand(currentCommandTmpl.commandName, currentCommandText, currentCommandTmpl.arguments.Length);

        for (int i = 0; i < currentCommandTmpl.arguments.Length; ++i)
        {
            if (string.IsNullOrWhiteSpace(parsed[i]) ||
                !checkArgument(parsed[i], currentCommandTmpl.arguments[i].argumentType)
            )
            {
                player.Tools_TargetAddMessage("[Sys] Format error, use: " + currentCommandTmpl.getFormat());
                return null;
            }
        }

        return parsed;
    }

    // -----------------------------------------------------------------------------------
    // checkArgument
    // -----------------------------------------------------------------------------------
    protected bool checkArgument(string argument, AdminCommandArgument.AdminCommandArgumentType type)
    {
        int n;

        if (type == AdminCommandArgument.AdminCommandArgumentType.TargetType)
        {
            return (argument == adminCommands.tagTargetPlayer ||
                    argument == adminCommands.tagTargetParty ||
                    argument == adminCommands.tagTargetGuild ||
                    argument == adminCommands.tagTargetRealm ||
                    argument == adminCommands.tagTargetAll);
        }
        else if (type == AdminCommandArgument.AdminCommandArgumentType.PlayerName)
        {
            if (!Player.onlinePlayers.ContainsKey(argument))
            {
                player.Tools_TargetAddMessage("[Sys] player not online or invalid name");
                return false;
            }
        }
        else if (type == AdminCommandArgument.AdminCommandArgumentType.ItemName)
        {
            if (!ScriptableItem.All.ContainsKey(argument.GetStableHashCode()))
            {
                player.Tools_TargetAddMessage("[Sys] Invalid item name");
                return false;
            }
        }
        else if (type == AdminCommandArgument.AdminCommandArgumentType.Integer)
        {
            return int.TryParse(argument, out n);
        }

        return true;
    }

    // ===================================================================================
    // =================================== COMMANDS  =====================================
    // ===================================================================================
    #region AdminCommand
    // -----------------------------------------------------------------------------------
    // Cmd_Admin_SetAdmin
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    public void Admin_SetAdmin(string[] parsedArgs)
    {
        string adminTargets = parsedArgs[0];
        string adminTargetName = parsedArgs[1];
        int adminValue = int.Parse(parsedArgs[2]);

        if (adminValue < 0 || adminValue > 255) return;

        Cmd_Admin_SetAdmin(adminTargets, adminTargetName, adminValue);

        player.Tools_AddMessage("[Sys] Target(s) admin level is now: " + adminValue.ToString());
    }

    [Command]
    public void Cmd_Admin_SetAdmin(string adminTargets, string adminTargetName, int adminValue)
    {
        if (adminValue < 0 || adminValue > 255) return;

        List<Player> players = new List<Player>();
        players = getPlayerTargets(adminTargets, adminTargetName);

        foreach (Player plyr in players)
        {
            string adminAccount = Database.singleton.GetAccountName(plyr.name);
            Database.singleton.SetAdminAccount(adminAccount, adminValue);
            plyr.AdminLevel = adminValue;
            plyr.Tools_TargetAddMessage("[Admin] Your admin level was adjusted to: " + adminValue.ToString());
        }
    }

    // -----------------------------------------------------------------------------------
    // Cmd_Admin_GiveItem
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    public void Admin_GiveItem(string[] parsedArgs)
    {
        string adminTargets = parsedArgs[0];
        string adminTargetName = parsedArgs[1];
        int adminValue = int.Parse(parsedArgs[2]);
        string adminItemName = parsedArgs[3];

        ScriptableItem item = getItem(adminItemName);
        if (item == null) return;

        Cmd_Admin_GiveItem(adminTargets, adminTargetName, adminValue, adminItemName);

        player.Tools_AddMessage("[Sys] Target(s) received " + item.name + " x" + adminValue.ToString());
    }

    [Command]
    public void Cmd_Admin_GiveItem(string adminTargets, string adminTargetName, int adminValue, string adminItemName)
    {
        ScriptableItem item = getItem(adminItemName);
        if (item == null) return;

        List<Player> players = new List<Player>();
        players = getPlayerTargets(adminTargets, adminTargetName);

        foreach (Player plyr in players)
        {
            if (plyr.inventory.CanAdd(new Item(item), adminValue) &&
                   plyr.inventory.Add(new Item(item), adminValue))
            {
                plyr.Tools_TargetAddMessage("[Admin] You just received " + item.name + " x" + adminValue.ToString());
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // Cmd_Admin_GiveGold
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    public void Admin_GiveGold(string[] parsedArgs)
    {
        string adminTargets = parsedArgs[0];
        string adminTargetName = parsedArgs[1];
        int adminValue = int.Parse(parsedArgs[2]);

        if (adminValue == 0) return;

        Cmd_Admin_GiveGold(adminTargets, adminTargetName, adminValue);

        player.Tools_AddMessage("[Sys] Target(s) received gold: " + adminValue.ToString());
    }

    [Command]
    public void Cmd_Admin_GiveGold(string adminTargets, string adminTargetName, int adminValue)
    {
        if (adminValue == 0) return;

        List<Player> players = new List<Player>();
        players = getPlayerTargets(adminTargets, adminTargetName);

        foreach (Player plyr in players)
        {
            plyr.gold += adminValue;
            plyr.Tools_TargetAddMessage("[Admin] You just received gold: " + adminValue.ToString());
        }
    }

    // -----------------------------------------------------------------------------------
    // Cmd_Admin_GiveExp
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    public void Admin_GiveExp(string[] parsedArgs)
    {
        string adminTargets = parsedArgs[0];
        string adminTargetName = parsedArgs[1];
        int adminValue = int.Parse(parsedArgs[2]);

        if (adminValue == 0) return;

        Cmd_Admin_GiveExp(adminTargets, adminTargetName, adminValue);

        player.Tools_AddMessage("[Sys] Target(s) received experience: " + adminValue.ToString());
    }

    [Command]
    public void Cmd_Admin_GiveExp(string adminTargets, string adminTargetName, int adminValue)
    {
        if (adminValue == 0) return;

        List<Player> players = new List<Player>();
        players = getPlayerTargets(adminTargets, adminTargetName);

        foreach (Player plyr in players)
        {
            plyr.experience.current += adminValue;
            plyr.Tools_TargetAddMessage("[Admin] You just received experience: " + adminValue.ToString());
        }
    }

    // -----------------------------------------------------------------------------------
    // Cmd_Admin_GiveCoins
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    public void Admin_GiveCoins(string[] parsedArgs)
    {
        string adminTargets = parsedArgs[0];
        string adminTargetName = parsedArgs[1];
        int adminValue = int.Parse(parsedArgs[2]);

        if (adminValue == 0) return;

        Cmd_Admin_GiveCoins(adminTargets, adminTargetName, adminValue);

        player.Tools_AddMessage("[Sys] Target(s) received coins: " + adminValue.ToString());
    }

    [Command]
    public void Cmd_Admin_GiveCoins(string adminTargets, string adminTargetName, int adminValue)
    {
        if (adminValue == 0) return;

        List<Player> players = new List<Player>();
        players = getPlayerTargets(adminTargets, adminTargetName);

        foreach (Player plyr in players)
        {
            plyr.itemMall.coins += adminValue;
            plyr.Tools_TargetAddMessage("[Admin] You just received coins: " + adminValue.ToString());
        }
    }

    // -----------------------------------------------------------------------------------
    // Cmd_Admin_KillPlayer
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    public void Admin_KillPlayer(string[] parsedArgs)
    {
        string adminTargets = parsedArgs[0];
        string adminTargetName = parsedArgs[1];

        Cmd_Admin_KillPlayer(adminTargets, adminTargetName);

        player.Tools_AddMessage("[Sys] Target(s) successfully killed.");
    }

    [Command]
    public void Cmd_Admin_KillPlayer(string adminTargets, string adminTargetName)
    {
        List<Player> players = new List<Player>();
        players = getPlayerTargets(adminTargets, adminTargetName);

        foreach (Player plyr in players)
        {
            plyr.health.current = 0;
            plyr.Tools_TargetAddMessage("[Admin] You where just killed by admin!");
        }
    }

    // -----------------------------------------------------------------------------------
    // Cmd_Admin_BanAccount
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    public void Admin_BanAccount(string[] parsedArgs)
    {
        string adminTargets = parsedArgs[0];
        string adminTargetName = parsedArgs[1];

        Cmd_Admin_BanAccount(adminTargets, adminTargetName);

        player.Tools_AddMessage("[Sys] Target(s) have been banned.");
    }

    [Command]
    public void Cmd_Admin_BanAccount(string adminTargets, string adminTargetName)
    {
        List<Player> players = new List<Player>();
        players = getPlayerTargets(adminTargets, adminTargetName);

        foreach (Player plyr in players)
        {
            string adminAccount = Database.singleton.GetAccountName(plyr.name);
            plyr.Tools_TargetAddMessage("[Admin] Your account was just banned by admin!");
            Database.singleton.BanAccount(adminAccount);
            plyr.connectionToClient.Disconnect();
        }
    }

    // -----------------------------------------------------------------------------------
    // Cmd_Admin_UnbanAccount
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    public void Admin_UnbanAccount(string[] parsedArgs)
    {
        string adminAccountName = parsedArgs[0];

        if (adminAccountName == "") return;

        Cmd_Admin_UnbanAccount(adminAccountName);

        player.Tools_AddMessage("[Sys] Account " + adminAccountName + " was unbanned.");
    }

    [Command]
    public void Cmd_Admin_UnbanAccount(string adminAccountName)
    {
        if (adminAccountName == "") return;

        Database.singleton.UnbanAccount(adminAccountName);
    }

    // -----------------------------------------------------------------------------------
    // Cmd_Admin_GetAccountName
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    public void Admin_GetAccountName(string[] parsedArgs)
    {
        string adminPlayerName = parsedArgs[0];

        if (adminPlayerName == "") return;

        Cmd_Admin_GetAccountName(adminPlayerName);
    }

    [Command]
    public void Cmd_Admin_GetAccountName(string adminPlayerName)
    {
        if (adminPlayerName == "") return;
        string adminAccountName = Database.singleton.GetAccountName(adminPlayerName);
        player.Tools_TargetAddMessage("[Sys] " + adminPlayerName + "'s account name is: " + adminAccountName);
    }

    // -----------------------------------------------------------------------------------
    // Cmd_Admin_CleanDatabase
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    public void Admin_CleanDatabase(string[] parsedArgs)
    {
        Cmd_Admin_CleanDatabase(parsedArgs);
        player.Tools_AddMessage("[Sys] Starting Database Cleanup...");
    }

    [Command]
    public void Cmd_Admin_CleanDatabase(string[] parsedArgs)
    {
#if _iMMODBCLEANER && _SERVER
        //_NetworkManagerMMO.networkManagerMMODatabaseCleaner.Cleanup();
#endif
    }

    // -----------------------------------------------------------------------------------
    // Cmd_Admin_TeleportPlayer
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    public void Admin_TeleportPlayer(string[] parsedArgs)
    {
        string adminTargets = parsedArgs[0];
        string adminTargetName = parsedArgs[1];

        Cmd_Admin_TeleportPlayer(adminTargets, adminTargetName);

        player.Tools_AddMessage("[Sys] Target(s) successfully teleported to your location.");
    }

    [Command]
    public void Cmd_Admin_TeleportPlayer(string adminTargets, string adminTargetName)
    {
        List<Player> players = new List<Player>();
        players = getPlayerTargets(adminTargets, adminTargetName);

        foreach (Player plyr in players)
        {
            plyr.Tools_TargetAddMessage("[Admin] You where summoned to admins location!");
            plyr.movement.Warp(transform.position);
        }
    }

    // -----------------------------------------------------------------------------------
    // Cmd_Admin_OnlinePlayers
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    public void Admin_OnlinePlayers(string[] parsedArgs)
    {
        Cmd_Admin_OnlinePlayers(parsedArgs);
    }

    [Command]
    public void Cmd_Admin_OnlinePlayers(string[] parsedArgs)
    {
        int playerCount = Player.onlinePlayers.Count;

        player.Tools_TargetAddMessage("[Sys] There are currently <" + playerCount + "> players online.");
    }

    // -----------------------------------------------------------------------------------
    // Cmd_Admin_SummonMonster
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    public void Admin_SummonMonster(string[] parsedArgs)
    {
        string adminTargetName = parsedArgs[0];
        int adminValue = int.Parse(parsedArgs[1]);

        if (adminValue <= 0) return;

        Cmd_Admin_SummonMonster(adminTargetName, adminValue);

        player.Tools_AddMessage("[Sys] You just summoned " + adminValue.ToString() + " " + adminTargetName + "'s at your location.");
    }

    [Command]
    public void Cmd_Admin_SummonMonster(string adminTargetName, int adminValue)
    {
        if (adminValue <= 0) return;

        Monster monster = _NetworkManagerMMO.networkManagerMMOAdministration.CachedMonsters().Find(x => x.name.ToLower() == adminTargetName.ToLower());

        if (monster)
        {
            for (int j = 1; j <= adminValue; ++j)
            {
                GameObject go = Instantiate(monster.gameObject, player.transform.position, player.transform.rotation);
                NetworkServer.Spawn(go);
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // Cmd_Admin_SummonNpc
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    public void Admin_SummonNpc(string[] parsedArgs)
    {
        string adminTargetName = parsedArgs[0];

        Cmd_Admin_SummonNpc(adminTargetName);

        player.Tools_AddMessage("[Sys] You just summoned " + adminTargetName + " at your location.");
    }

    [Command]
    public void Cmd_Admin_SummonNpc(string adminTargetName)
    {
        Npc npc = _NetworkManagerMMO.networkManagerMMOAdministration.CachedNpcs().Find(x => x.name.ToLower() == adminTargetName.ToLower());

        GameObject go = Instantiate(npc.gameObject, player.transform.position, player.transform.rotation);
        NetworkServer.Spawn(go);
    }

    // -----------------------------------------------------------------------------------
    // Cmd_Admin_UnsummonEntity
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    public void Admin_UnsummonEntity(string[] parsedArgs)
    {
        if (player.target != null && (player.target is Monster || player.target is Npc))
        {
            Cmd_Admin_UnsummonEntity(parsedArgs);
            player.Tools_AddMessage("[Sys] You just unsummoned " + player.target.name + ".");
        }
    }

    [Command]
    public void Cmd_Admin_UnsummonEntity(string[] parsedArgs)
    {
        if (player.target != null && (player.target is Monster || player.target is Npc))
        {
            NetworkServer.Destroy(player.target.gameObject);
        }
    }

    // -----------------------------------------------------------------------------------
    // Cmd_Admin_KickPlayer
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    public void Admin_KickPlayer(string[] parsedArgs)
    {
        string adminTargets = parsedArgs[0];
        string adminTargetName = parsedArgs[1];

        Cmd_Admin_KickPlayer(adminTargets, adminTargetName);

        player.Tools_AddMessage("[Sys] Target(s) successfully kicked.");
    }

    [Command]
    public void Cmd_Admin_KickPlayer(string adminTargets, string adminTargetName)
    {
        List<Player> players = new List<Player>();
        players = getPlayerTargets(adminTargets, adminTargetName);

        foreach (Player plyr in players)
        {
            plyr.Tools_TargetAddMessage("[Admin] You where kicked by admin!");
            plyr.connectionToClient.Disconnect();
        }
    }

    // -----------------------------------------------------------------------------------
    // Cmd_Admin_DeletePlayer
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    public void Admin_DeletePlayer(string[] parsedArgs)
    {
        string adminTargets = parsedArgs[0];
        string adminTargetName = parsedArgs[1];

        Cmd_Admin_DeletePlayer(adminTargets, adminTargetName);

        player.Tools_AddMessage("[Sys] Target(s) successfully kicked and deleted.");
    }

    [Command]
    public void Cmd_Admin_DeletePlayer(string adminTargets, string adminTargetName)
    {
        List<Player> players = new List<Player>();
        players = getPlayerTargets(adminTargets, adminTargetName);

        foreach (Player plyr in players)
        {
            plyr.Tools_TargetAddMessage("[Admin] You where deleted by admin!");

            Database.singleton.SetCharacterDeleted(plyr.name, true);
            plyr.connectionToClient.Disconnect();
        }
    }

    // -----------------------------------------------------------------------------------
    // Cmd_Admin_UndeletePlayer
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    public void Admin_UndeletePlayer(string[] parsedArgs)
    {
        string adminTargetName = parsedArgs[0];

        if (adminTargetName == "") return;

        Cmd_Admin_UndeletePlayer(adminTargetName);

        player.Tools_AddMessage("[Sys] Target(s) successfully undeleted.");
    }

    [Command]
    public void Cmd_Admin_UndeletePlayer(string adminTargetName)
    {
        if (adminTargetName == "") return;
        Database.singleton.SetCharacterDeleted(adminTargetName, false);
    }

    // -----------------------------------------------------------------------------------
    // Cmd_Admin_SendMessage
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    public void Admin_SendMessage(string[] parsedArgs)
    {
        string adminTargets = parsedArgs[0];
        string adminTargetName = parsedArgs[1];
        string adminTargetMessage = parsedArgs[2];

        Cmd_Admin_SendMessage(adminTargets, adminTargetName, adminTargetMessage);

        player.Tools_AddMessage("[Sys] Target(s) received the message.");
    }

    [Command]
    public void Cmd_Admin_SendMessage(string adminTargets, string adminTargetName, string adminTargetMessage)
    {
        List<Player> players = new List<Player>();
        players = getPlayerTargets(adminTargets, adminTargetName);

        foreach (Player plyr in players)
        {
            plyr.Tools_ShowPopup(adminTargetMessage);
            plyr.Tools_TargetAddMessage(adminTargetMessage);
        }
    }
    #endregion AdminCommand

    #region UserCommand


    public void Admin_Roll(string[] parsedArgs)
    {
        Cmd_Admin_Roll(parsedArgs);
    }

    [Command]
    public void Cmd_Admin_Roll(string[] parsedArgs)
    {
        //player.movement.LookAtY(new Vector3(locX, 0, locY));
        //player.Tools_AddMessage("[Sys] Look At: " + locX + "x, " + locY + "y");
        Roll("");
    }

    private void Roll(string prefix)
    {
        System.Random rnd = new System.Random();
        int roll = rnd.Next(1, 100);
        if (!string.IsNullOrEmpty(prefix) && prefix == "p")
        {
            if (player.party.InParty())
            {
                foreach (string playerName in player.party.party.members)
                {
                    Player.onlinePlayers[playerName].Tools_TargetAddMessage("/p [Roll : " + player.name + " ] got (party) a score of  " + roll);
                }
            }
            else
            {
                player.Tools_AddMessage("Hello " + player.name + ", Roll's system is party only");
            }
        }
        else if (!string.IsNullOrEmpty(prefix) && prefix == "g")
        {
            if (player.party.InParty())
            {
                foreach (GuildMember playerName in player.guild.guild.members)
                {
                    Player.onlinePlayers[playerName.name].Tools_TargetAddMessage("/g [Roll : " + player.name + " ] got (guild) a score of  " + roll);
                }
            }
            else
            {
                player.Tools_AddMessage("Hello " + player.name + ", Roll's system is guild only");
            }
        }
        else
        {
            //ici on devrais ajouter le roll pour les joueurs juste autour du joueur qui le lance
            //player.onlinePlayers[playerName.name].Tools_TargetAddMessage("[Roll : " + player.name + " ] got a score of  " + roll);
            List<Entity> correctedTargets = new List<Entity>();

            int layerMask = ~(1 << 2);

#if _iMMO2D
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, player.interactionRange, layerMask);
            foreach (Collider2D hitCollider in hitColliders)
#else
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, player.interactionRange * 2, layerMask);
            foreach (Collider hitCollider in hitColliders)
#endif
            {

                Entity target = hitCollider.GetComponentInParent<Entity>();
                if (target is Player && target != player)
                {
                    Player.onlinePlayers[target.name].Tools_TargetAddMessage("[Roll : " + target.name + " ] got (general) a score of  " + roll);
                }
                //if (target != null && target != this && player.CanAttack(target) && target.isAlive && !correctedTargets.Any(x => x == target))
                //    correctedTargets.Add(target);
            }
            player.Tools_AddMessage("[Roll : " + player.name + " ] got a score of  " + roll);
        }
    }




    public void Admin_Faceloc(string[] parsedArgs)
    {
        int locX = int.Parse(parsedArgs[0]);
        int locY = int.Parse(parsedArgs[1]);

        //Cmd_Admin_Faceloc(locX, locY);
#if !_iMMO2D
        player.movement.LookAtY(new Vector3(locX, 0, locY));
        player.Tools_AddMessage("[Sys] Look At: " + locX + "x, " + locY + "y");
#endif

    }


    // Faceloc
    public void Admin_Hello(string[] parsedArgs)
    {
        if (player.target != null && player.target is Player)
        {
            Cmd_Admin_Hello(parsedArgs);
            player.Tools_AddMessage("[Roleplay] You say hello to " + player.target.name + ".");
        }
    }
    [Command]
    public void Cmd_Admin_Faceloc(int x, int y)
    {
#if !_iMMO2D
        player.movement.LookAtY(new Vector3(x, 0, y));
#endif
    }


    [Command]
    public void Cmd_Admin_Hello(string[] parsedArgs)
    {
        // Check if player is online before send message
        if (Player.onlinePlayers.ContainsKey(player.target.name))
        {
            Player.onlinePlayers[player.target.name].Tools_TargetAddMessage("[RolePlay] "+ player.name +" playername says hello to you ");
        }
    }
#endregion UserCommand
    // -----------------------------------------------------------------------------------
}