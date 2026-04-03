using Mirror;
using UnityEngine;

#if _iMMOITEMDROP && _iMMOLOOTRULES

// ===================================================================================
// TAGGED LOOTING
// ===================================================================================
public partial class ItemDrop
{
    [HideInInspector, SyncVar] public float LiftRulesAfter;
    [HideInInspector, SyncVar] public bool LootVictorParty;
    [HideInInspector, SyncVar] public bool LootVictorGuild;
#if _iMMOPVP
    [HideInInspector, SyncVar] public bool LootVictorRealm;
#endif
    [HideInInspector, SyncVar] public bool LootEverybody;

    [HideInInspector, SyncVar] public GameObject _lastAggressor;
    [HideInInspector, SyncVar] public GameObject _owner;
    [HideInInspector, SyncVar] public int hashRealm;
    [HideInInspector, SyncVar] public int hashAlly;

    // -----------------------------------------------------------------------------------
    // ValidateTaggedLooting
    // -----------------------------------------------------------------------------------
    public bool ValidateTaggedLooting(Player player)
    {
        if (LootEverybody || lastAggressor == null || lastAggressor == player || owner == player || (LiftRulesAfter > 0 && NetworkTime.time > (decayTimeEnd - destroyAfter) + LiftRulesAfter)) return true;
        if (LootVictorParty && ValidateTaggedLootingParty(player)) return true;
        if (LootVictorGuild && ValidateTaggedLootingGuild(player)) return true;
#if _iMMOPVP
        if (LootVictorRealm && ValidateTaggedLootingRealm(player)) return true;
#endif
        return false;
    }

    // -----------------------------------------------------------------------------------
    // ValidateTaggedLootingParty
    // -----------------------------------------------------------------------------------
    public bool ValidateTaggedLootingParty(Player player)
    {
        if (lastAggressor == null) return true;

        Player plyr;

        if (lastAggressor is Player player1)
            plyr = player1;
        else
            return true;

        bool valid = (((plyr.party.InParty() && plyr.party.party.Contains(player.name)) || (player.party.InParty() && player.party.party.Contains(plyr.name))));

        return valid;
    }

    // -----------------------------------------------------------------------------------
    // ValidateTaggedLootingGuild
    // -----------------------------------------------------------------------------------
    public bool ValidateTaggedLootingGuild(Player player)
    {
        if (lastAggressor == null) return true;

        Player plyr;

        if (lastAggressor is Player player1)
            plyr = player1;
        else
            return true;

        return (plyr.guild.InGuild() && player.guild.InGuild() && plyr.guild.guild.name != "" && player.guild.guild.name != "" && plyr.guild.guild.name == player.guild.guild.name);
    }

    // -----------------------------------------------------------------------------------
    // ValidateTaggedLootingRealm
    // -----------------------------------------------------------------------------------
#if _iMMOPVP

    public bool ValidateTaggedLootingRealm(Player player)
    {
        if (hashRealm == 0 && player.hashRealm == 0 || hashAlly == 0 && player.hashAlly == 0)
            return true;

        if (hashRealm == player.hashRealm || hashAlly == player.hashAlly || hashRealm == player.hashAlly || hashAlly == player.hashRealm)
            return true;

        return false;
    }

#endif

    // -----------------------------------------------------------------------------------
    // lastAggressor
    // -----------------------------------------------------------------------------------
    public Entity lastAggressor
    {
        get { return _lastAggressor != null ? _lastAggressor.GetComponent<Entity>() : null; }
        set { _lastAggressor = value != null ? value.gameObject : null; }
    }

    // -----------------------------------------------------------------------------------
    // owner
    // -----------------------------------------------------------------------------------
    public Entity owner
    {
        get { return _owner != null ? _owner.GetComponent<Entity>() : null; }
        set { _owner = value != null ? value.gameObject : null; }
    }

    // -----------------------------------------------------------------------------------
}
#endif