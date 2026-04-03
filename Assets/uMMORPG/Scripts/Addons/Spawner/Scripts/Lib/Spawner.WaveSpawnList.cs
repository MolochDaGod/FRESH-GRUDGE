
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ===================================================================================
// WAVESPAWN LIST
// ===================================================================================
[System.Serializable]
public partial class WaveSpawnList
{
    public enum ScaleLevel { None, Override, Add, Average, Player, PlayerAverage }

    [Header("[SPAWN WAVE]")]
    [Tooltip("[Optional] A popup with this message will be sent to all players inside the spawner area when this wave arrives.")]
    public string waveMessage;

    public WaveSpawnableEntity[] spawnablePrefabs;
    public int minObjectSpawn = 0;
    public int maxObjectSpawn = 3;
    public ScaleLevel scaleLevel;
    public bool scaleWithPlayerLevel;
    public int levelAdjustment = 0;

    [Tooltip("[Optional] Delay in seconds before spawning starts, after the wave is triggered.")]
    public float spawnDelay = 10f;

    [Tooltip("Set to false to disable the spawned objects default respawn (recommended for OnPlayerEnter)")]
    public bool canObjectsRespawn = false;

    protected int _lastPlayerLevel, _levelAdjustment, totalObjectsToSpawn, totalObjectsSpawned;
    protected Area_WaveSpawner parentSpawnArea;
    protected List<GameObject> objects;
    protected bool spawned = false;
    protected int maxIterationCycles = 75;
    protected int parentSpawnWave;

#if _SERVER
    // -----------------------------------------------------------------------------------
    // Prepare
    // -----------------------------------------------------------------------------------
    public void Prepare(Area_WaveSpawner parent, int waveIndex)
    {
        parentSpawnArea = parent;
        parentSpawnWave = waveIndex;

        objects = new List<GameObject>();
        _levelAdjustment = levelAdjustment;
        totalObjectsSpawned = 0;
        spawned = false;
    }

    // -----------------------------------------------------------------------------------
    // Refresh
    // -----------------------------------------------------------------------------------
    public void Refresh(int level)
    {
        _lastPlayerLevel = level;

        if (scaleWithPlayerLevel)
            _levelAdjustment = _lastPlayerLevel + levelAdjustment;
    }

    // -----------------------------------------------------------------------------------
    // IsCompleted
    // -----------------------------------------------------------------------------------
    public bool IsCompleted()
    {
        return spawned && totalObjectsSpawned <= 0;
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public void Unspawn()
    {
        foreach (GameObject obj in objects)
        {
            foreach (WaveSpawnableEntity spawnableEntity in spawnablePrefabs)
            {
                if (obj && spawnableEntity.entity.name == obj.name)
                {
                    spawnableEntity.amount = 0;
                }
            }

            if (obj)
                NetworkServer.Destroy(obj);
        }

        objects.Clear();

        spawned = false;
        totalObjectsSpawned = 0;
    }

    // -----------------------------------------------------------------------------------
    // Spawn
    // -----------------------------------------------------------------------------------
    public void Spawn()
    {
        totalObjectsToSpawn = UnityEngine.Random.Range(minObjectSpawn, maxObjectSpawn);

        if (!spawned && totalObjectsToSpawn > 0)
            parentSpawnArea.SpawnOnChild(this);
    }

    // -----------------------------------------------------------------------------------
    // SpawnObject
    // -----------------------------------------------------------------------------------
    public IEnumerator SpawnObject()
    {
        if (spawnDelay > 0)
            yield return new WaitForSeconds(spawnDelay);

        if (spawnablePrefabs.Length > 0 &&
            (parentSpawnArea.GetPlayerCountInArea() > 0 || parentSpawnArea.triggerOnServerLaunch) &&
            !spawned
            )
        {
            int i = 0;
            int idx;

            while (totalObjectsSpawned < totalObjectsToSpawn)
            {
                idx = -1;
                i++;

                if (spawnablePrefabs.Length > 1)
                    spawnablePrefabs = ShuffleEntities(spawnablePrefabs);

                foreach (WaveSpawnableEntity spawnableEntity in spawnablePrefabs)
                {
                    if (spawnableEntity.entity != null)
                    {
                        if ((spawnableEntity.amount < spawnableEntity.maxAmount || spawnableEntity.maxAmount == 0) && Random.value <= spawnableEntity.probability)
                        {
                            idx = System.Array.IndexOf(spawnablePrefabs, spawnableEntity);
                            break;
                        }
                    }
                }

                if (idx != -1 && (spawnablePrefabs[idx].amount < spawnablePrefabs[idx].maxAmount || spawnablePrefabs[idx].maxAmount == 0))
                {
                    Transform spawnArea = parentSpawnArea.GetRandomSpawnArea();
                    Vector3 spawnPosition = parentSpawnArea.GetRandomSpawnPosition(spawnArea);
                    string origName = spawnablePrefabs[idx].entity.name;

                    GameObject go = parentSpawnArea.InstantiateOnChild(spawnablePrefabs[idx].entity, spawnPosition, parentSpawnArea.GetRandomSpawnRotation());
                    go.name = origName;

                    if (go.GetComponent<Entity>())
                    {
                        // -- we need to keep track of the area and wave on the monster
                        go.GetComponent<Entity>().parentSpawnArea = parentSpawnArea;
                        go.GetComponent<Entity>().parentWaveIndex = parentSpawnWave;

                        if (scaleLevel == ScaleLevel.Add)
                        {
                            go.GetComponent<Entity>().level.current += _levelAdjustment;
                        }
                        else if (scaleLevel == ScaleLevel.Average)
                        {
                            go.GetComponent<Entity>().level.current = (go.GetComponent<Entity>().level.current + _levelAdjustment) / 2;
                        }
                        else if (scaleLevel == ScaleLevel.Override)
                        {
                            go.GetComponent<Entity>().level.current = _levelAdjustment;
                        }
                        else if (scaleLevel == ScaleLevel.Player)
                        {
                            go.GetComponent<Entity>().level.current = _lastPlayerLevel;
                        }
                        else if (scaleLevel == ScaleLevel.PlayerAverage)
                        {
                            go.GetComponent<Entity>().level.current = (go.GetComponent<Entity>().level.current + _lastPlayerLevel + _levelAdjustment) / 2;
                        }
                    }

                    if (go.GetComponent<Monster>())
                    {
                        go.GetComponent<Monster>().respawn = canObjectsRespawn;
                        go.GetComponent<Monster>().setStartPosition(spawnPosition);
                    }
#if _iMMOCHEST
                    if (go.GetComponent<Lootcrate>())
                    {
                        go.GetComponent<Lootcrate>().respawn = canObjectsRespawn;
                    }
#endif
#if _iMMOHARVESTING
                    if (go.GetComponent<ResourceNode>())
                    {
                        go.GetComponent<ResourceNode>().respawn = canObjectsRespawn;
                    }
#endif

                    NetworkServer.Spawn(go);

                    objects.Add(go);
                    spawnablePrefabs[idx].amount++;
                    totalObjectsSpawned++;
                }

                if (i > maxIterationCycles)
                {
                    break;                                                              //emergency break in case of nothing found after x passes
                }
            }

            if (!string.IsNullOrWhiteSpace(waveMessage))
                parentSpawnArea.NotifyPlayersInArea(waveMessage);

            spawned = true;
        }

        yield return new WaitForEndOfFrame();
    }
#endif
    // -------------------------------------------------------------------------------
    // updateMemberPopulation
    // -------------------------------------------------------------------------------
    public void updateMemberPopulation(int nameHash)
    {
        totalObjectsSpawned--;

        int idx = -1;

        foreach (WaveSpawnableEntity spawnableEntity in spawnablePrefabs)
        {
            if (spawnableEntity.entity != null && spawnableEntity.amount > 0 && spawnableEntity.entity.name.GetStableHashCode() == nameHash)
            {
                idx++;
                spawnablePrefabs[idx].amount--;
                break;
            }
        }
    }

    // -------------------------------------------------------------------------------
    // ShuffleEntities
    // -------------------------------------------------------------------------------
    private WaveSpawnableEntity[] ShuffleEntities(WaveSpawnableEntity[] e)
    {
        for (int i = e.Length - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i);
            WaveSpawnableEntity temp = e[i];
            e[i] = e[rnd];
            e[rnd] = temp;
        }

        return e;
    }

    // -----------------------------------------------------------------------------------
}
