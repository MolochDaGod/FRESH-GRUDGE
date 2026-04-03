// ===================================================================================
// LOOT RULES - ENTITY
// ===================================================================================
public partial class Entity
{
    // -----------------------------------------------------------------------------------
    // ValidateTaggedLootingParty
    // -----------------------------------------------------------------------------------
    public bool ValidateTaggedLootingParty(Player player)
    {
        if (lastAggressor == null) return true;


        if (lastAggressor is Player plyr)
            return (plyr.party.InParty() && plyr.party.party.Contains(player.name)) || (player.party.InParty() && player.party.party.Contains(plyr.name));
        else return true;
    }


    // -----------------------------------------------------------------------------------
    // ValidateTaggedLootingGuild
    // -----------------------------------------------------------------------------------
    public bool ValidateTaggedLootingGuild(Player player)
    {
        if (lastAggressor == null) return true;


        if (lastAggressor is Player plyr)
            return plyr.guild.InGuild() && player.guild.InGuild() && plyr.guild.guild.name != "" && player.guild.guild.name != "" && plyr.guild.guild.name == player.guild.guild.name;
        else return true;
    }

    // -----------------------------------------------------------------------------------
    // ValidateTaggedLootingRealm
    // -----------------------------------------------------------------------------------
#if _iMMOPVP
    public bool ValidateTaggedLootingRealm(Player player)
    {
        return GetAlliedRealms(player);
    }
#endif

    // -----------------------------------------------------------------------------------
}