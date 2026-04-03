using UnityEngine;

// ===================================================================================
// player
// ===================================================================================
public class MonsterLeaderboard : MonoBehaviour
{
    public Monster monster;
    public Health health;

#if _SERVER
    private void Start()
    {

        health.onEmpty.AddListener(OnDeathLeaderboard);
    }
    
    private void OnDeathLeaderboard()
    {
        if(monster.lastAggressor != null)
        {
            if(monster.lastAggressor is Player player)
            {
                ++player.playerAddonsConfigurator.monsterkill;
            }

        }
    }
#endif
}