using Mirror;
using System.Collections;
using UnityEngine;


// ===================================================================================
// LOOT RULES - PLAYER
// ===================================================================================
public class PlayerLootRules : NetworkBehaviour
{

    public Player player;
    public Health health;
#if _iMMOLOOTRULES && _iMMOITEMDROP
    [Header("[-=-[ ITEM DROP & LOOT RULES ]-=-]")]
    //public LootRules lootRules;
    public Tmpl_LootRulesEntity lootRulesEntity;
    //public Tmpl_LootRulesPlayer lootRulesPlayer;

    [HideInInspector] public int dropSolverAttempts = 3;
    [HideInInspector] public double deathTime;
    [HideInInspector, SyncVar] public bool lootRulesLifted;

    public override void OnStartLocalPlayer()
    {
        StartCoroutine(nameof(CR_LootRules));
    }

    [ServerCallback]
    IEnumerator CR_LootRules()
    {
        lootRulesLifted = (lootRulesEntity.LiftRulesAfter > 0 && NetworkTime.time > deathTime + lootRulesEntity.LiftRulesAfter);
        yield return new WaitForSeconds(1);
    }



    // -----------------------------------------------------------------------------------
    // ValidateTaggedLooting
    // -----------------------------------------------------------------------------------
    public bool ValidateTaggedLooting(Player player)
    {
        if (lootRulesEntity.LootEverybody || player.lastAggressor == null || player.lastAggressor == player || (lootRulesEntity.LiftRulesAfter > 0 && NetworkTime.time > deathTime + lootRulesEntity.LiftRulesAfter) ) return true;
        if (lootRulesEntity.LootVictorParty && player.ValidateTaggedLootingParty(player)) return true;
        if (lootRulesEntity.LootVictorGuild && player.ValidateTaggedLootingGuild(player)) return true;
#if _iMMOPVP
        if (lootRulesEntity.LootVictorRealm && player.ValidateTaggedLootingRealm(player)) return true;
#endif
        return false;
    }

    // -----------------------------------------------------------------------------------
#else
    public bool requireItemDropAddon;
#endif
}
