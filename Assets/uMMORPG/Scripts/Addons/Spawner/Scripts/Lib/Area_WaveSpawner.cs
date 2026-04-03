    using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

// ===================================================================================
// WAVE SPAWNER AREA
// ===================================================================================
#if _iMMO2D
[RequireComponent(typeof(Collider2D))]
#else
[RequireComponent(typeof(SphereCollider))]
#endif
public class Area_WaveSpawner : NetworkBehaviour
{
    [Header("[WAVE SPAWNER]")]
    [Tooltip("One click de-activation")]
    public bool isActive = true;

    [Tooltip("Does this spawner trigger when the server is launched (Once)?")]
    public bool triggerOnServerLaunch = true;

    [Tooltip("Does this spawner trigger when a player enters the area (repeating)?")]
    public bool triggerOnPlayerEnter = false;

    [Tooltip("[Optional] Optional activation requirements for the entering player")]
    public Tools_ActivationRequirements enterActivationRequirements;

    [Tooltip("Do the monsters unspawn when the last player leaves the area (repeating)?")]
    public bool unspawnOnPlayerExit = false;

    [Tooltip("Delay Tools_timer after a player exits the area (default 4 seconds)")]
    public float unspawnDelay = 4f;

    [Header("[Spawn Destinations]")]
    [Tooltip("[Optional] Choose one or more SpawnerArea's as destination. Leave empty to use this area as destination.")]
    public SpawnDestination[] spawnDestinations;

    [Tooltip("[Optional] Select a Layer to prevent objects being spawned on that layer")]
    public LayerMask doNotSpawnAt;

#if _iMMOMONSTERWAYPOINTS

    [Header("[Dynamic Waypoints (replace default waypoints)]")]
    public Transform[] waypoints;

#endif

    [Header("[Waves]")]
    public WaveSpawnList[] spawnLists;

    [Header("[Rewards]")]

    [Tooltip("[Optional] A popup with this message will be sent to all players inside the spawner area when all waves are beaten.")]
    public string completedAllWavesMessage;

    [Tooltip("[Optional] All players inside the spawner area will receive these rewards when all waves are beaten.")]
    public Tools_InteractionRewards completionRewards;

    protected Transform center;
    protected float radius;
    protected int x, y;
    protected List<Player> players;

    protected int maxIterationCycles = 75;
    protected bool _isCompleted = false;

#if _SERVER
    // -----------------------------------------------------------------------------------
    // Start
    // -----------------------------------------------------------------------------------
    private void Start()
    {
        if (!isActive) return;

        players = new List<Player>();

        if (spawnLists.Length > 0)
        {
            for (int i = 0; i < spawnLists.Length; i++)
            {
                spawnLists[i].Prepare(this, i);
            }
        }

        if (triggerOnServerLaunch)
        {
            foreach (WaveSpawnList spawnList in spawnLists)
            {
                spawnList.Refresh(1);
                spawnList.Spawn();
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // isBeaten
    // -----------------------------------------------------------------------------------
    public bool IsCompleted()
    {
        return spawnLists.All(x => x.IsCompleted());
    }

    // -------------------------------------------------------------------------------
    // OnTriggerEnter
    // -------------------------------------------------------------------------------
    [ServerCallback]
#if _iMMO2D
    private void OnTriggerEnter2D(Collider2D co)
#else
    private void OnTriggerEnter(Collider co)
#endif
    {
        Player player = co.GetComponentInParent<Player>();
        if (player && enterActivationRequirements.checkRequirements(player))
        {
            if (isActive && triggerOnPlayerEnter)
            {
                // -- clear existing objects if we are the first player in a while
                if (players.Count == 0)
                {
                    _isCompleted = false;
                    UnspawnGameObjects();
                    players.Clear();
                }

                // -- spawn new objects
                foreach (WaveSpawnList spawnList in spawnLists)
                {
                    spawnList.Refresh(player.level.current);
                    spawnList.Spawn();
                }

                players.Add(player);
                CancelInvoke(nameof(UnspawnGameObjects));
            }
        }
    }

    // -------------------------------------------------------------------------------
    // OnTriggerExit
    // -------------------------------------------------------------------------------
    [ServerCallback]
#if _iMMO2D
    private void OnTriggerExit2D(Collider2D co)
#else
    private void OnTriggerExit(Collider co)
#endif
    {
        Player player = co.GetComponentInParent<Player>();
        if (player && player.isAlive)
        {
            if (isActive && unspawnOnPlayerExit)
            {
                players.Remove(player);
                if (players.Count <= 0)
                {
                    Invoke(nameof(UnspawnGameObjects), unspawnDelay);
                    players.Clear();
                }
            }
        }
    }

    // -------------------------------------------------------------------------------
    // unspawnGameObjects
    // -------------------------------------------------------------------------------
    [Server]
    private void UnspawnGameObjects()
    {
        CancelInvoke(nameof(UnspawnGameObjects));
        foreach (WaveSpawnList spawnList in spawnLists)
            spawnList.Unspawn();
    }

    // -------------------------------------------------------------------------------
    // notifyPlayersInArea
    // -------------------------------------------------------------------------------
    [ServerCallback]
    public void NotifyPlayersInArea(string msg)
    {
        foreach (Player player in players)
        {
            if (player)
            {
                player.Tools_ShowPopup(msg);
            }
        }
    }
#endif
 
#if _SERVER
    // -------------------------------------------------------------------------------
    // getPlayerCountInArea
    // -------------------------------------------------------------------------------
    public int GetPlayerCountInArea()
    {
        return players.Count;
    }

    // -------------------------------------------------------------------------------
    // SpawnChild
    // -------------------------------------------------------------------------------
    [Server]
    public void SpawnOnChild(WaveSpawnList child)
    {
        StartCoroutine(child.SpawnObject());
    }

    // -------------------------------------------------------------------------------
    // InstantiateOnChild
    // -------------------------------------------------------------------------------
    [Server]
    public GameObject InstantiateOnChild(GameObject go, Vector3 pos, Quaternion rot)
    {
        GameObject gob = Instantiate(go, pos, rot);

#if _iMMOMONSTERWAYPOINTS
        if (gob.GetComponent<Monster>() && waypoints.Length > 0)
            gob.GetComponent<Monster>().monsterWaypoints.waypoints = waypoints;
#endif

        return gob;
    }

    // -------------------------------------------------------------------------------
    // updateMemberPopulation
    // -------------------------------------------------------------------------------
    [ServerCallback]
    public void UpdateMemberPopulation(int nameHash, int waveIndex)
    {
        // -- update member population
        spawnLists[waveIndex].updateMemberPopulation(nameHash);

        // -- check for spawner completion
        if (IsCompleted() && !_isCompleted)
        {
            _isCompleted = true;

            if (!string.IsNullOrWhiteSpace(completedAllWavesMessage))
                NotifyPlayersInArea(completedAllWavesMessage);

            foreach (Player player in players)
            {
                if (player)
                {
                    completionRewards.gainRewards(player);
                }
            }
        }
    }

    // -------------------------------------------------------------------------------
    // getRandomSpawnArea
    // -------------------------------------------------------------------------------
    public Transform GetRandomSpawnArea()
    {
        if (spawnDestinations == null || spawnDestinations.Length == 0)
            return this.GetComponent<Transform>();

        foreach (SpawnDestination spawnDestination in spawnDestinations)
        {
            if (Random.value <= spawnDestination.probability)
                return spawnDestination.transform;
        }

        return this.GetComponent<Transform>();
    }

    // -----------------------------------------------------------------------------------
    // getRandomSpawnPosition
    // -----------------------------------------------------------------------------------
    public Quaternion GetRandomSpawnRotation()
    {
#if _iMMO2D
        return new Quaternion();
#else
        return Quaternion.Euler(new Vector3(90, Random.Range(0, 360), 0));
#endif
    }

    // -----------------------------------------------------------------------------------
    // getRandomSpawnVector
    // -----------------------------------------------------------------------------------
    public Vector3 GetRandomSpawnVector(Transform spawnArea, float radius)
    {
        return new Vector3(spawnArea.position.x + Random.Range(radius * -1, radius), spawnArea.position.y, spawnArea.position.z + Random.Range(radius * -1, radius));
    }

    // -----------------------------------------------------------------------------------
    // getRandomSpawnPosition
    // -----------------------------------------------------------------------------------
    public Vector3 GetRandomSpawnPosition(Transform spawnArea = null)
    {
        Vector3 spawnPosition = Vector3.zero;

        if (spawnArea == null)
            spawnArea = this.gameObject.transform;

        int i = 0;
        bool pass = false;
#if _iMMO2D
        float radius = spawnArea.GetComponent<CircleCollider2D>().radius;
#else
        float radius = spawnArea.GetComponent<SphereCollider>().radius;
#endif
        while (!pass)
        {
            i++;
            spawnPosition = GetRandomSpawnVector(spawnArea, radius);
#if _iMMO2D
            if (NavMesh2D.SamplePosition(Vector2.zero, out NavMeshHit2D hit, radius, NavMesh2D.AllAreas) && !Physics2D.Raycast(spawnPosition, Vector2.down, radius, doNotSpawnAt.value))
#else
            if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, radius, NavMesh.AllAreas) && !Physics.Raycast(spawnPosition, Vector3.down, radius, doNotSpawnAt.value) )
#endif
            //if (NavMesh.SamplePosition(spawnPosition, out hit, radius, NavMesh.AllAreas) && !Physics.Raycast(spawnPosition, Vector3.down, radius, doNotSpawnAt.value))
            {
                return spawnPosition;
            }

            if (i > maxIterationCycles)
            {
                break;                                                              //emergency break in case of nothing found after x passes
            }
        }

        return spawnPosition;
    }
#endif
    // -----------------------------------------------------------------------------------
}
