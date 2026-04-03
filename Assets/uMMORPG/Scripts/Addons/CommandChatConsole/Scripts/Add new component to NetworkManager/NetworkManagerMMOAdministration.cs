using System.Collections.Generic;
using UnityEngine;

// ADMINISTRATION - NETWORK MANAGER MMO
public class NetworkManagerMMOAdministration :  MonoBehaviour
{
    public NetworkManagerMMO networkManagerMMO;

    private List<Monster> cacheMonster = new List<Monster>();
    private List<Npc> cacheNpc = new List<Npc>();

    // -----------------------------------------------------------------------------------
    // OnServerCharacterCreate_Tools_Administration
    // -----------------------------------------------------------------------------------
#if _SERVER
    public void Start()
    {
        networkManagerMMO.onServerCharacterCreate.AddListener(OnServerCharacterCreate_Administration);
    }

    private void OnServerCharacterCreate_Administration(CharacterCreateMsg message, Player player)
    {
        Database.singleton.SetAdminAccount(player.account, (Database.singleton.GetAccountCount() <= 1) ? 255 : 1); // if over 1 account, first account is admin
    }
#endif
    // -----------------------------------------------------------------------------------
    // cachedMonsters
    // -----------------------------------------------------------------------------------
    public List<Monster> CachedMonsters()
    {
        if (cacheMonster.Count <= 0)
        {
            foreach (GameObject monster in networkManagerMMO.spawnPrefabs)
            {
                if (monster.GetComponent<Monster>() != null)
                    cacheMonster.Add(monster.GetComponent<Monster>());
            }
        }
        return cacheMonster;
    }

    // -----------------------------------------------------------------------------------
    // cachedNpcs
    // -----------------------------------------------------------------------------------
    public List<Npc> CachedNpcs()
    {
        if (cacheNpc.Count <= 0)
        {
            foreach (GameObject npc in networkManagerMMO.spawnPrefabs)
            {
                if (npc.GetComponent<Npc>() != null)
                    cacheNpc.Add(npc.GetComponent<Npc>());
            }
        }
        return cacheNpc;
    }
}