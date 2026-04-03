using Mirror;
using UnityEngine;

public partial class MonsterWorldBossMessage : NetworkBehaviour
{

    public Monster monster;
    public Combat combat;
    public Health health;

    public bool displayMessageInChat = true;
    public bool isWorldBoss;

    [Header("[-=-[ World Boss killed Message ]-=-]")]
    [BoolShowConditional(conditionFieldName: "isWorldBoss", conditionValue: true)]
    public Tools_PopupClass bossKilled = new Tools_PopupClass() { message = "The world boss {0} was killed by {1}" , suffix = "", iconId = 0, soundId = 0 };


    [Header("[-=-[ World Boss Respawn Message ]-=-]")]
    [BoolShowConditional(conditionFieldName: "isWorldBoss", conditionValue: true)]
    public Tools_PopupClass bossRespawn = new Tools_PopupClass() { message = "The world boss {0} is respawned", suffix = "", iconId = 0, soundId = 0 };



#if _SERVER && _iMMOWORLDBOSSMESSAGE
    // ----------------------------------------------------------------------------------
    // Auto Add in Health event
    // ---------------------------------------------------------------------------------- 
    public override void OnStartServer()
    {
        health.onEmpty.AddListener(OnDeath_WorldBossMessage);
    }

    [Server]
    public void OnDeath_WorldBossMessage()
    {
        // This function not included on client
        if (isWorldBoss && Player.onlinePlayers.Count > 0 && monster.lastAggressor != null && monster.lastAggressor is Player)
        {
            foreach (Player playerOnline in Player.onlinePlayers.Values)
            {
                playerOnline.Target_Tools_ShowPopup(playerOnline.connectionToClient, string.Format(bossKilled.message, monster.name, monster.lastAggressor.name), bossKilled.iconId, bossKilled.soundId);
                if (displayMessageInChat)
                    playerOnline.Tools_TargetAddMessage(string.Format(bossKilled.message, monster.name, monster.lastAggressor.name));
            }
        }
    }

    [Server]
    public void OnReviveMessage()
    {
        if (isWorldBoss)
        {
            foreach (Player playerOnline in Player.onlinePlayers.Values)
            {
                playerOnline.Target_Tools_ShowPopup(playerOnline.connectionToClient, string.Format(bossRespawn.message, monster.name, monster.lastAggressor.name), bossRespawn.iconId, bossRespawn.soundId);
                if (displayMessageInChat)
                    playerOnline.Tools_TargetAddMessage(string.Format(bossRespawn.message, monster.name, monster.lastAggressor.name));
            }
        }
    }
#endif
}