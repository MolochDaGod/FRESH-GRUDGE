using Mirror;
using System.Collections.Generic;
using UnityEngine;

// NETWORK SPAWNER
public class NetworkSpawner : NetworkBehaviour
{
    public NetworkSpawn[] networkSpawnObjects;

    protected bool init;
    protected NetworkManagerMMO manager;
    protected List<GameObject> cacheSpawnPrefabs = new List<GameObject>();

    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------

    private void Update()
    {
        if (!init && NetworkServer.active)
        {
            startSpawn();
        }
    }

    [Server]
    private void startSpawn()
    {
        manager = FindObjectOfType<NetworkManagerMMO>();

        cacheSpawnPrefabs = manager.spawnPrefabs;

        // make sure we are the server and have objects in the spawnable prefabs list
        if (cacheSpawnPrefabs.Count == 0 || !NetworkServer.active || init) return;

        OnSpawn();
        init = true;
    }

    // -----------------------------------------------------------------------------------
    // OnSpawn
    // -----------------------------------------------------------------------------------
    [Server]
    protected void OnSpawn()
    {
        foreach (NetworkSpawn networkSpawn in networkSpawnObjects)
        {
            // make sure spawnable object is not null and is in the spawnable prefabs list
            if (networkSpawn.gameObject != null && cacheSpawnPrefabs.Contains(networkSpawn.gameObject))
            {
                GameObject gob = Instantiate(networkSpawn.gameObject, networkSpawn.spawnPosition.position, networkSpawn.spawnPosition.rotation);
                gob.name = networkSpawn.gameObject.name;
                NetworkServer.Spawn(gob);
            }
        }

        Destroy(this.gameObject);
    }
    // -----------------------------------------------------------------------------------
}