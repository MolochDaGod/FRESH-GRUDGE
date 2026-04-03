using UnityEngine;

public partial class Player
{

    [Header("[Component : Player PVP Zone]")]
    public PlayerPVPZone playerPVPZone;

    // -----------------------------------------------------------------------------------
    // GetInPvpRegion
    // -----------------------------------------------------------------------------------
    public override bool GetInPvpRegion()
    {
        return playerPVPZone.inAreaOpenPvp || playerPVPZone.inAreaRealmPvp || playerPVPZone.inAreaPartyPvp || playerPVPZone.inAreaGuildPvp;
    }

    // -----------------------------------------------------------------------------------
    // GetAttackAllowance
    // @Server / @Client
    // -----------------------------------------------------------------------------------
    public override bool GetAttackAllowance(Entity target)
    {

        if (_cannotCast)
        {
            return false;
        }

        // ---------- Basic PVP Check
        if (target is Player playerTarget)
        {
            
            // ---------- Check for Requirements
            if ((!playerPVPZone.CheckPvPSettingsMet()))
            {
                return false;
            }

            // ---------- Open PvP - You can attack every player
            if (playerPVPZone.inAreaOpenPvp && playerTarget.playerPVPZone.inAreaOpenPvp)
            {
                return true;
            }

            // ---------- Check Realm PVP
            // You can attack other realm, but not own realm or allied realm
            if (playerPVPZone.inAreaRealmPvp && playerTarget.playerPVPZone.inAreaRealmPvp)
            {
                if (!GetAlliedRealms(target))
                {
                    return true;
                }
            }

            // ---------- Party PVP - You can attack other party members
            if (playerPVPZone.inAreaPartyPvp && playerTarget.playerPVPZone.inAreaPartyPvp)
            {
                if (party.InParty() && playerTarget.party.InParty() && !party.party.Contains(target.name))
                {
                    return true;
                }
            }

            // ---------- You can NEVER attack your own party members
            if (party.InParty() && playerTarget.party.InParty() && party.party.Contains(target.name))
            {
                return false;
            }

            // ---------- Guild PVP - You can attack other guild members
            if (playerPVPZone.inAreaGuildPvp && playerTarget.playerPVPZone.inAreaGuildPvp)
            {
                if (guild.InGuild() && playerTarget.guild.InGuild() && guild.guild.name != playerTarget.guild.guild.name)
                {
                    return true;
                }
            }

            // ---------- You can NEVER attack your own guild members
            if (guild.InGuild() && playerTarget.guild.InGuild() && guild.guild.name == playerTarget.guild.guild.name)
            {
                return false;
            }

            return false;
        }

        // ---------- Check Realm PVE
        // You can attack other realm/neutral, but not own realm or allied realm
        return GetHostileRealms(target);
    }


    // -----------------------------------------------------------------------------------
}
