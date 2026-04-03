using Mirror;
using UnityEngine;

// PLAYER
public class PlayerGuildUpgrades : NetworkBehaviour
{
    public Player player;
    [Header("[-=-[ GUILD UPGRADES ]-=-]")]
    public Tmpl_GuildUpgrades guildUpgrades;

#if _iMMOGUILDUPGRADES

    //[SyncVar] private int _guildUpgradeLevel = 0; // TODO : cela pourrait simplement être une valeur classique de cache et sans acoir besoin de la sync, 
    private int _guildUpgradeLevel = 0; // TODO : cela pourrait simplement être une valeur classique de cache et sans acoir besoin de la sync, 


    public override void OnStartLocalPlayer()
    {
        Cmd_LoadGuildUpgrades();
    }

    // -----------------------------------------------------------------------------------
    // guildLevel
    // ----------------------------------------------------------------------------- ------
    public int guildLevel
    {
        get { return _guildUpgradeLevel; }
        set { _guildUpgradeLevel = value; }
    }

    // -----------------------------------------------------------------------------------
    // guildCapacity
    // -----------------------------------------------------------------------------------
    public int guildCapacity
    {
        get { 
            Cmd_LoadGuildUpgrades();
            return guildUpgrades.guildCapacity.Get(guildLevel+1); 
        }
    }

    // -----------------------------------------------------------------------------------
    // CanInvite
    // -----------------------------------------------------------------------------------
    public bool GuildCapacity_CanInvite()
    {
        return player.guild.guild.members.Length < guildCapacity;
    }
    // ================================== COMMANDS =======================================

    // -----------------------------------------------------------------------------------
    // Cmd_LoadGuildUpgrades
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_LoadGuildUpgrades()
    {
#if _SERVER
        if (!player.guild.InGuild()) return;
        LoadGuildUpgrades();
#endif
    }


    // -----------------------------------------------------------------------------------
    // Cmd_SaveGuildUpgrades
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_SaveGuildUpgrades()
    {
#if _SERVER
        if (!player.guild.InGuild()) return;
#endif
    }

       
    // -----------------------------------------------------------------------------------
    // Cmd_UpgradeGuild
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_UpgradeGuild()
    {
#if _SERVER
        UpgradeGuild();
#endif
    }

    // -----------------------------------------------------------------------------------
    // CanUpgradeGuildWarehouse
    // @Client OR @Server
    // -----------------------------------------------------------------------------------
    public bool CanUpgradeGuild()
    {
        return ( guildUpgrades.guildUpgradeCost.Length > 0 && guildUpgrades.guildUpgradeCost.Length > guildLevel && guildUpgrades.guildUpgradeCost[guildLevel].CheckCost(player) && CanAddRewardItem() );
    }

    // -----------------------------------------------------------------------------------
    // CanAddRewardItem
    // @Client OR @Server
    // -----------------------------------------------------------------------------------
    public bool CanAddRewardItem()
    {
        return ( guildUpgrades.rewardItem.Length == 0 || guildUpgrades.rewardItem.Length < guildLevel || ( guildUpgrades.rewardItem.Length >= guildLevel && player.inventory.CanAdd(new Item(guildUpgrades.rewardItem[guildLevel].item), guildUpgrades.rewardItem[guildLevel].amount) ) );
    }

    // -----------------------------------------------------------------------------------
    // UpgradeGuild
    // @Server
    // -----------------------------------------------------------------------------------
#if _SERVER

    [Server]
    private void LoadGuildUpgrades()
    {
        Database.singleton.LoadGuildUpgrades(player);
    }

    [Server]
    public void UpgradeGuild()
    {

        if (CanUpgradeGuild())
        {
            guildUpgrades.guildUpgradeCost[guildLevel].PayCost(player);

            if (guildUpgrades.rewardItem.Length > 0 && guildUpgrades.rewardItem.Length >= guildLevel)
            {
                player.inventory.Add(new Item(guildUpgrades.rewardItem[guildLevel].item), guildUpgrades.rewardItem[guildLevel].amount);
            }

            guildLevel++;

            BroadCastPopupToOnlineGuildMembers(guildUpgrades.guildUpgradeLabel);
            Database.singleton.SaveGuildUpgrades(player);
        }

    }
#endif
    // -----------------------------------------------------------------------------------
    // BroadCastPopupToOnlineGuildMembers
    // @Server
    // -----------------------------------------------------------------------------------
#if _SERVER
    public void BroadCastPopupToOnlineGuildMembers(string message)
    {
        foreach (Player oPlayer in Player.onlinePlayers.Values)
            if (player.guild.InGuild() && oPlayer.guild.InGuild() && player.guild.guild.name == oPlayer.guild.guild.name)
                oPlayer.Tools_ShowPopup(message);
    }
#endif
    // -----------------------------------------------------------------------------------
#endif
}