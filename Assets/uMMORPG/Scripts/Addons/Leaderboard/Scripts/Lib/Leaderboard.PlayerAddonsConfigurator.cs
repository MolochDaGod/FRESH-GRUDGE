using Mirror;
using UnityEngine;

// ===================================================================================
// player
// ===================================================================================
public partial class PlayerAddonsConfigurator
{
    [Header("[-=-[ player Leaderboard Configuration ]-=-]")]
    public bool countPveDeath;

    [SyncVar, HideInInspector] public int playerKill;
    [SyncVar, HideInInspector] public int monsterkill;
    [SyncVar, HideInInspector] public int death;

    public readonly SyncList<LeaderboardPlayer> currentOnlinePlayers = new SyncList<LeaderboardPlayer>();

    protected int maxPlayers = 50;


#if _SERVER
    public void OnStartServer_Leaderboard()
    {
        health.onEmpty.AddListener(OnDeathLeaderboard);
    }
#endif


    // -----------------------------------------------------------------------------------
    // Cmd_AllPlayersOnline
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_AllPlayersOnline()
    {
#if _SERVER
        // we wrap it in another function, because we want to call it only server-side
        UpdatePlayersOnline();
#endif
    }

#if _SERVER

    private void OnDeathLeaderboard()
    {
        if(player.lastAggressor != null)
        {
            if(player.lastAggressor is Player playerTarget)
            {
                ++death;
                ++playerTarget.playerAddonsConfigurator.playerKill;

            }
            else if (countPveDeath)
            {
                ++death;
            }
        }

    }

    // -----------------------------------------------------------------------------------
    // UpdatePlayersOnline
    // @Server
    // -----------------------------------------------------------------------------------
    [ServerCallback]
    public void UpdatePlayersOnline()
    {
        currentOnlinePlayers.Clear();

        int i = 0;

        foreach (Player plyr in Player.onlinePlayers.Values)
        {
            LeaderboardPlayer ldplyr = new LeaderboardPlayer(plyr.name, plyr.level.current, plyr.gold, plyr.playerAddonsConfigurator.death, plyr.playerAddonsConfigurator.playerKill, plyr.playerAddonsConfigurator.monsterkill);

            currentOnlinePlayers.Add(ldplyr);

            i++;

            if (i == maxPlayers) break;
        }
    }
#endif
    // -----------------------------------------------------------------------------------
}