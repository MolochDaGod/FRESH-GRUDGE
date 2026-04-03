using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// BASE SPAWNER AREA
#if _iMMO2D
[RequireComponent(typeof(Collider2D))]
#else
[RequireComponent(typeof(SphereCollider))]
#endif
public abstract class Area_Spawner : NetworkBehaviour
{
    public enum ScaleLevel { None, Override, Add, Average, Player, PlayerAverage }

    [Header("[-=-[ SPAWNER ]-=-]")]
    [Tooltip("[Optional] Do entities unspawn when the last player leaves the area?")]
    public bool unspawnOnPlayerExit = false;

    [Tooltip("[Optional] Delay Tools_timer after a player exits the area (default 4 seconds)")]
    public float unspawnDelay = 4f;

    [Tooltip("[Optional] Select a Layer to prevent objects being spawned on that layer")]
    public LayerMask doNotSpawnAt;

    [Tooltip("[Optional] Optional activation requirements for the entering player")]
    public Tools_ActivationRequirements enterActivationRequirements;

    public List<Player> players;
    public List<GameObject> gameObjects;
    protected int maxGameObjects;
    protected int currentPlayerLevel;
    protected int maxIterationCycles = 75;

    [Tooltip("Select Box or sphere collider")]
#if _iMMO2D
    public Collider2D colliderToSpawn;
#else
    public Collider colliderToSpawn;
#endif
    protected virtual void OnSpawn() {}

    protected virtual void OnUnspawn() {}

    protected virtual void UpdateMaxGameObjects() {}


#if _SERVER
    // -----------------------------------------------------------------------------------
    // Start
    // -----------------------------------------------------------------------------------
    [ServerCallback]
    public virtual void Start()
    {
        if (!colliderToSpawn)
            Debug.LogWarning("[Warning] " + gameObject.name + " (Area_Spawner) actually \"colliderToSpawn\" is empty, add a box or sphere collider !");
        if (!enterActivationRequirements.hasRequirements()) return;
        enterActivationRequirements.setParent(this.gameObject);
        players = new List<Player>();
        gameObjects = new List<GameObject>();

        StartCoroutine(CheckPlayersPeriodically());
    }

    private IEnumerator CheckPlayersPeriodically()
    {
        while (true)
        {
            // Vérifier l'état des joueurs dans le collider du spawner
            CheckPlayersInCollider();

            // Attendre 30 secondes avant la prochaine vérification
            yield return new WaitForSeconds(30f);
        }
    }

    private void CheckPlayersInCollider()
    {
        // Créer une liste temporaire pour stocker les joueurs encore présents dans le collider
        List<Player> playersInCollider = new List<Player>();

        // Vérifier chaque joueur pour voir s'il est toujours présent dans le jeu
        foreach (Player player in players)
        {
            if (player != null && Player.onlinePlayers.ContainsKey(player.name) && enterActivationRequirements.checkRequirements(player) && IsPlayerInCollider(player))
            {
                // Le joueur est toujours présent, donc l'ajouter à la liste temporaire
                playersInCollider.Add(player);
            }
        }

        // Remplacer la liste des joueurs du collider par la liste temporaire
        players = playersInCollider;

        // Si aucun joueur n'est présent dans le collider, réinitialiser le spawner
        if (players.Count == 0 && gameObjects.Count > 0 && unspawnOnPlayerExit)
        {
            Debug.Log("Check Collider Area_Spawner clear");
            UnspawnGameObjects();
        }
    }

    private bool IsPlayerInCollider(Player player)
    {
        // Vérifie si le joueur est en contact avec le collider de spawn
#if _iMMO2D
    return colliderToSpawn.OverlapPoint(player.transform.position);
#else
        return colliderToSpawn.bounds.Contains(player.transform.position);
#endif
    }

    [ServerCallback]
#if _iMMO2D
    private void OnTriggerStay2D(Collider2D co)
#else
    private void OnTriggerStay(Collider co)
#endif
    {
        Player player = co.GetComponentInParent<Player>();
        if (player && !players.Contains(player) && Player.onlinePlayers.ContainsKey(player.name))
            players.Add(player);
        if (player && enterActivationRequirements.checkRequirements(player))
        {
            if (gameObjects.Count < maxGameObjects)
            {
                CancelInvoke(nameof(UnspawnGameObjects));

                currentPlayerLevel = player.level.current;
                OnSpawn();
            }
        }
    }

    //
    //-----------------------------------------------------------------------------------
    // OnTriggerEnter
    // -----------------------------------------------------------------------------------
    [ServerCallback]
#if _iMMO2D
    private void OnTriggerEnter2D(Collider2D co)
#else
    private void OnTriggerEnter(Collider co)

#endif
    {
        Player player = co.GetComponentInParent<Player>();
        if (player && !players.Contains(player) && Player.onlinePlayers.ContainsKey(player.name))
            players.Add(player);
        if (player && enterActivationRequirements.checkRequirements(player))
        {
            if (gameObjects.Count < maxGameObjects)
            {
                CancelInvoke(nameof(UnspawnGameObjects));

                currentPlayerLevel = player.level.current;
                OnSpawn();
            }
        }
        
    }

    // -----------------------------------------------------------------------------------
    // OnTriggerExit
    // -----------------------------------------------------------------------------------
    [ServerCallback]
#if _iMMO2D
    private void OnTriggerExit2D(Collider2D co)
#else
    private void OnTriggerExit(Collider co)
#endif
    {
        Player player = co.GetComponentInParent<Player>();

        if (player && unspawnOnPlayerExit)
        {
            if (players.Contains(player)) { 
                players.Remove(player);
            }
            if (players.Count < 1)
            {
                if (unspawnDelay > 0)
                    Invoke(nameof(UnspawnGameObjects), unspawnDelay);
                else
                    UnspawnGameObjects();
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // unspawnGameObjects
    // -----------------------------------------------------------------------------------
    [Server]
    protected void UnspawnGameObjects()
    {
        if (players.Count < 1)
        {
            foreach (GameObject entity in gameObjects)
            {
                if (entity != null)
                    NetworkServer.Destroy(entity);
            }
            gameObjects.Clear();
            players.Clear();
            CancelInvoke(nameof(UnspawnGameObjects));
            OnUnspawn();
        }
    }

    // -----------------------------------------------------------------------------------
    // unspawnGameObjects
    // -----------------------------------------------------------------------------------
    [Server]
    protected GameObject SpawnGameObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
#if _iMMO2D
        GameObject go = Instantiate(prefab, position, new Quaternion());
#else
        GameObject go = Instantiate(prefab, position, rotation);
#endif
        go.name = prefab.name; 													// avoid "(Clone)"
        NetworkServer.Spawn(go);
        gameObjects.Add(go);
        return go;
    }
#endif
    // -----------------------------------------------------------------------------------
    // getRandomSpawnPosition
    // -----------------------------------------------------------------------------------
    public Quaternion GetRandomSpawnRotation()
    {
        return Quaternion.Euler(new Vector3(90, Random.Range(0, 360), 0));
    }

    // -----------------------------------------------------------------------------------
    // getRandomSpawnVector
    // -----------------------------------------------------------------------------------
    public Vector3 GetRandomSpawnVector(float radius)
    {
        return new Vector3( gameObject.transform.position.x + Random.Range(radius * -1, radius), gameObject.transform.position.y, gameObject.transform.position.z + Random.Range(radius * -1, radius) );
        
    }
    public static Vector3 GetRandomSpawnVector(Bounds bounds)
    {
#if _iMMO2D
        return new Vector3(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y), 0f);
#else
        return new Vector3( Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y), Random.Range(bounds.min.z, bounds.max.z) );
#endif
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
        var radius = colliderToSpawn.GetType() == typeof(CircleCollider2D) ? spawnArea.GetComponent<CircleCollider2D>().radius : spawnArea.GetComponent<BoxCollider2D>().bounds.size.y;
#else
        var radius = colliderToSpawn.GetType() == typeof(SphereCollider) ? spawnArea.GetComponent<SphereCollider>().radius : spawnArea.GetComponent<BoxCollider>().bounds.size.y;
#endif

        while (!pass)
        {
            i++;
            spawnPosition = GetRandomSpawnVector(radius);

#if _iMMO2D
            if (NavMesh2D.SamplePosition(spawnPosition, out NavMeshHit2D hit, radius, NavMesh.AllAreas) && !Physics2D.Raycast(spawnPosition, Vector2.down, radius, doNotSpawnAt.value))
#else
            if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, radius, NavMesh.AllAreas) && !Physics.Raycast(spawnPosition, Vector3.down, radius, doNotSpawnAt.value))
#endif
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

    // -----------------------------------------------------------------------------------

}