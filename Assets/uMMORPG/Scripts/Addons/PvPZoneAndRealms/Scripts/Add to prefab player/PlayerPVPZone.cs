using Mirror;
using UnityEngine;

// PLAYER

public class PlayerPVPZone : NetworkBehaviour
{

    public Player player;
    public Combat combat;

    // Bit of overhead but this aproach has the advantage that is not hackable because
    // of being a SyncVar. Otherwise, if the client would change that it would result
    // in a catastrophe!
    [SyncVar, HideInInspector] public bool inAreaOpenPvp = false;
    [SyncVar, HideInInspector] public bool inAreaRealmPvp = false;
    [SyncVar, HideInInspector] public bool inAreaPartyPvp = false;
    [SyncVar, HideInInspector] public bool inAreaGuildPvp = false;

    [Header("[-=-[ AUTOMATIC RETALIATION ]-=-]")]
    public bool autoRetaliation;

    [Header("[-=-[ ENABLE AUTO ATTACK ]-=-]")]
    public bool autoAttack;
    public int skillIndex;

    protected PVPZONE_Settings pvpSettings;



    private void Awake()
    {
        player._originalRealm = player.hashRealm;
        player._originalAlly = player.hashAlly;
    }

#if _CLIENT
    public override void OnStartLocalPlayer()
    {
        combat.onClientReceivedDamage.AddListener(OnServerReceivedDamage_PVPZone);
    }

    private void OnServerReceivedDamage_PVPZone(int degat, DamageType type)
    {
        if (degat > 0) OnDamageDealt_PVPZone();
    }

    // -----------------------------------------------------------------------------------
    // OnDamageDealt_Tools_PVPZone
    // Custom Hook
    // -----------------------------------------------------------------------------------
    [ClientCallback]
    public void OnDamageDealt_PVPZone()
    {
        if (autoRetaliation && (player.target == null || player.target == player.lastAggressor) && player.lastAggressor != null)
        {
            if (player.CanAttack(player.lastAggressor))
            {
                if (player.skills.skills.Count > 0 && player.skills.skills[skillIndex].IsReady())
                {
                    ((PlayerSkills)player.skills).TryUse(skillIndex, true);
                    player.target = player.lastAggressor;
                }
            }
        }


    }
#endif

    // -----------------------------------------------------------------------------------
    // CheckPvPSettingsMet
    // -----------------------------------------------------------------------------------
    public bool CheckPvPSettingsMet()
    {
        return true;
    }


#if _SERVER

    // -----------------------------------------------------------------------------------
    // PvpRegionEnter
    // @Server
    // -----------------------------------------------------------------------------------
    [Server]
    public void PvpRegionEnter(PVPZONE_Settings settings)
    {
        pvpSettings = settings;

        inAreaOpenPvp = pvpSettings.OpenPvp;
        inAreaRealmPvp = pvpSettings.RealmPvp;
        inAreaPartyPvp = pvpSettings.PartyPvp;
        inAreaGuildPvp = pvpSettings.GuildPvp;
    }

    // -----------------------------------------------------------------------------------
    // PvpRegionLeave
    // @Server
    // -----------------------------------------------------------------------------------
    [Server]
    public void PvpRegionLeave(PVPZONE_Settings settings)
    {
        pvpSettings = settings;

        inAreaOpenPvp = false;
        inAreaRealmPvp = false;
        inAreaPartyPvp = false;
        inAreaGuildPvp = false;

        pvpSettings = null;
    }

#endif
    // -----------------------------------------------------------------------------------
}