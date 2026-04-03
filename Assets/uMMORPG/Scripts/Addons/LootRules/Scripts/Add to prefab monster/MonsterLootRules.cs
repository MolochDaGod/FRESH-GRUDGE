using Mirror;
using UnityEngine;

// ===================================================================================
// LOOT RULES - MONSTER
// ===================================================================================
public class MonsterLootRules : NetworkBehaviour
{
    public Monster monster;

    [Header("[-=-[ Loot Rules (Victor can always loot) ]-=-]")]
    public Tmpl_LootRulesEntity lootRules;

    [SyncVar] protected bool lootRulesLifted;
#if _SERVER
    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    [ServerCallback]
    private void Update()
    {
        if (lootRules)
            lootRulesLifted = (lootRules.LiftRulesAfter > 0 && NetworkTime.time > (monster.deathTimeEnd - monster.deathTime) + lootRules.LiftRulesAfter);
    }
#endif

    // -----------------------------------------------------------------------------------
    // ValidateTaggedLooting
    // -----------------------------------------------------------------------------------
    public bool ValidateTaggedLooting(Player player) //TODO : this is an function server 
    {
        if (lootRules.LootEverybody || monster.lastAggressor == null || monster.lastAggressor == player || (lootRules.LiftRulesAfter > 0 && lootRulesLifted) ) return true;
        if (lootRules.LootVictorParty && monster.ValidateTaggedLootingParty(player)) return true;
        if (lootRules.LootVictorGuild && monster.ValidateTaggedLootingGuild(player)) return true;
#if _iMMOPVP
        if (lootRules.LootVictorRealm && monster.ValidateTaggedLootingRealm(player)) return true;
#endif
        return false;
    }

    // -----------------------------------------------------------------------------------
}