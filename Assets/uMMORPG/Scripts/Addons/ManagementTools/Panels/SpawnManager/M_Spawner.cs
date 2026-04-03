#if _SQLITE
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class M_Spawner : NetworkBehaviour
{
    public float waitTime = 1f;

    [Serializable]
    public struct SpawnGroup
    {
        public Monster monster;
        public int amount;
    }
    [HideInInspector] public SpawnGroup[] spawnGroups;
    [HideInInspector] public string uniqueId;
    [HideInInspector] public int range = 1;
    [HideInInspector] public List<Monster> activeMonsters = new List<Monster>();

#if UMMORPG_3D_C || UMMORPG_3D_R
    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = (UnityEngine.Random.insideUnitSphere * range) + center;
        if (NavMesh.SamplePosition(randomPoint, out var hit, range, 1))
        {
            result = hit.position;
            return true;
        }
        result = Vector3.zero;
        return false;
    }
#endif
#if UMMORPG_2D_C || UMMORPG_2D_R
    bool RandomPoint2D(Vector2 center, float range, out Vector2 result)
    {
        Vector2 randomPoint = (UnityEngine.Random.insideUnitCircle * range) + center;
        if (NavMesh2D.SamplePosition(randomPoint, out var hit, range, 1))
        {
            result = hit.position;
            return true;
        }
        result = Vector2.zero;
        return false;
    }
#endif

    public override void OnStartServer()
    {
        // call the Spawn function after a delay of the waitTime
        Invoke(nameof(Spawn), waitTime);
    }

    public void Spawn()
    {
        for (int i = 0; i < spawnGroups.Length; i++)
        {
            SpawnGroup group = spawnGroups[i];
            for (int j = 0; j < group.amount; j++)
            {
#if UMMORPG_3D_C || UMMORPG_3D_R
                if (RandomPoint(transform.position, range, out var point))
                {
                    // instantiate a monster with a random rotation
                    Monster monster = Instantiate(group.monster, point, Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0));
                    monster.name = group.monster.name;

                    // create this monster on the server
                    NetworkServer.Spawn(monster.gameObject);

                    // add it to the list for easy removal from the game after spawner edit or delete
                    activeMonsters.Add(monster);
                }
#endif
#if UMMORPG_2D_C || UMMORPG_2D_R
                if (RandomPoint2D(transform.position, range, out var point))
                {
                    // instantiate a monster
                    Monster monster = Instantiate(group.monster, point, Quaternion.identity);
                    monster.name = group.monster.name;

                    // create this monster on the server
                    NetworkServer.Spawn(monster.gameObject);

                    // add it to the list for easy removal from the game after spawner edit or delete
                    activeMonsters.Add(monster);
                }
#endif
            }
        }
    }
}

#endif
