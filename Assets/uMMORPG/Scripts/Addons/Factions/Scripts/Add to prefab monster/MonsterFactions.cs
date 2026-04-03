using Mirror;
using UnityEngine;

[HelpURL("https://mmo-indie.com/addon.php?name=faction")]
public class MonsterFactions : NetworkBehaviour
{
    public Monster monster;
    public Health health;

    [Header("[-=-[ FACTION ]-=-]")]
    public Tmpl_Faction myFaction;

    [Header("[-=-[ FACTIONS Modifiers ]-=-]")]
    public FactionModifier[] factionModifiers;

#if _SERVER
    public override void OnStartServer()
    {
        health.onEmpty.AddListener(OnDeath);
    }

    // -----------------------------------------------------------------------------------
    // OnDeath_Factions
    // -----------------------------------------------------------------------------------
    [Server]
    public void OnDeath()
    {
        if (monster.lastAggressor != null && monster.lastAggressor is Player player)
        {
            foreach (FactionModifier factionModifier in factionModifiers)
            {
                if (factionModifier.faction != null && factionModifier.amount != 0)
                {
                    player.playerFactions.AddFactionRating(factionModifier.faction, factionModifier.amount);
                }
            }
        }
    }

#endif
    // -----------------------------------------------------------------------------------
    // CheckFactionThreshold
    // -----------------------------------------------------------------------------------
    public bool CheckFactionThreshold(Entity entity)
    {
#if _iMMOPVP
        if ( entity is Player && myFaction != null && ((Player)entity).playerFactions.GetFactionRating(myFaction) <= myFaction.aggressiveThreshold)
        {
            return false;
        }
#endif
        return true;
    }

    // -----------------------------------------------------------------------------------
}