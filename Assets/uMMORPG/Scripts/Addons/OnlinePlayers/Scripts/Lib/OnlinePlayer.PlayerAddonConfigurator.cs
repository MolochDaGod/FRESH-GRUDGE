using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerAddonsConfigurator
{

    List<ListPlayerOnline> onlinePlayers;

    // Update is called once per frame
    [Command]
    public void Cmd_ListPlayersOnline()
    {
#if _SERVER
        ListOnlinePlayers();
#endif
    }

#if _SERVER
    [Server]
    public void ListOnlinePlayers()
    {
        //List<player> players = new();
        List<ListPlayerOnline> playersOnline = new List<ListPlayerOnline>();
        foreach (Player plyer in Player.onlinePlayers.Values)
        {   
            ListPlayerOnline add = new ListPlayerOnline()
            {
                //imageSprite =
#if _iMMOPVP
                                //plyer.Realm.image.,
#else
                                //plyer.classIcon,
#endif
                playerName = plyer.name,
                playerLevel = plyer.level.current,
                playerClass = plyer.className,
                allowTrading = true,
                allowParty = true,
#if _iMMOFRIENDS
                allowFriend = true,
#endif
                allowGuild = true,
            };
            add.allowGuild = true;
            playersOnline.Add(add);
        }

        player.playerAddonsConfigurator.Rpc_SetListPlayerOnline(playersOnline);
    }
#endif
    [TargetRpc]
    public void Rpc_SetListPlayerOnline(List<ListPlayerOnline> value)
    {
        onlinePlayers = value;
    }

}

[Serializable]
public class ListPlayerOnline
{
    //public Sprite imageSprite;
    public string playerName;
    public string playerClass;
    public int playerLevel;
    public bool allowTrading;
    public bool allowParty;
#if _iMMOFRIENDS
    public bool allowFriend;
#endif
    public bool allowGuild;
}
