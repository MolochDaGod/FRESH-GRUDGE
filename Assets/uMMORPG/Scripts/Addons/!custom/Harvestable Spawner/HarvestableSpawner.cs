using UnityEngine;
using Mirror; // Import the Mirror namespace
using System.Collections.Generic; // Import the generic collections namespace
using System.Collections; // Import for coroutines

[System.Serializable]
public class HarvestablePrefab
{
    public GameObject prefab; // The prefab to spawn
    public int maxCount; // The maximum allowed instances of this prefab

    public HarvestablePrefab(GameObject prefab, int maxCount)
    {
        this.prefab = prefab;
        this.maxCount = maxCount;
    }
}

public class HarvestableSpawner : NetworkBehaviour
{
    public HarvestablePrefab[] harvestablePrefabs; // Array of harvestable prefabs with their max counts
    public float spawnAreaRadius = 10f; // Radius to search for spawn points
    public LayerMask spawnLayer; // Settable layer for spawning
    public LayerMask doNotSpawnAtLayer; // Layer(s) to avoid when spawning

    [Header("Spawn Settings")]
    public float spawnRate = 1f; // Time in seconds between spawns

    private List<GameObject>[] spawnedHarvestables; // Array of lists to track spawned harvestables for each prefab

    void Start()
    {
        if (isServer) // Only the server should spawn objects
        {
            // Initialize the array of lists
            spawnedHarvestables = new List<GameObject>[harvestablePrefabs.Length];
            for (int i = 0; i < spawnedHarvestables.Length; i++)
            {
                spawnedHarvestables[i] = new List<GameObject>();
            }

            StartCoroutine(SpawnHarvestables()); // Start the spawning coroutine
        }
    }

    void Update()
    {
        // Check for destroyed harvestables and clean up the lists
        for (int i = 0; i < spawnedHarvestables.Length; i++)
        {
            for (int j = spawnedHarvestables[i].Count - 1; j >= 0; j--)
            {
                if (spawnedHarvestables[i][j] == null)
                {
                    spawnedHarvestables[i].RemoveAt(j);
                }
            }
        }
    }

    [Server] // This method can only be called on the server
    private IEnumerator SpawnHarvestables()
    {
        // Keep spawning until we reach the maximum for each prefab
        while (true) // Loop indefinitely
        {
            GameObject prefabToSpawn = ChooseRandomPrefab();
            if (prefabToSpawn != null)
            {
                SpawnHarvestable(prefabToSpawn);
            }
            yield return new WaitForSeconds(spawnRate); // Wait for the specified time before the next spawn
        }
    }

    [Server]
    void SpawnHarvestable(GameObject prefab)
    {
        Vector3 spawnPosition = GetValidGroundPosition();
        if (spawnPosition != Vector3.zero)
        {
            GameObject newHarvestable = Instantiate(prefab, spawnPosition, Quaternion.identity);
            NetworkServer.Spawn(newHarvestable); // Use Mirror to spawn on the network
            // Find the index of the prefab to add it to the correct list
            for (int i = 0; i < harvestablePrefabs.Length; i++)
            {
                if (harvestablePrefabs[i].prefab == prefab)
                {
                    spawnedHarvestables[i].Add(newHarvestable); // Add the new instance to the appropriate list
                    break;
                }
            }
        }
    }

    Vector3 GetValidGroundPosition()
    {
        for (int i = 0; i < 10; i++) // Try 10 times to find a valid position
        {
            Vector3 randomPosition = transform.position + Random.insideUnitSphere * spawnAreaRadius;
            randomPosition.y = 10; // Start above ground

            // Use raycast to find the actual ground position
            if (Physics.Raycast(randomPosition, Vector3.down, out RaycastHit hit, 20f, spawnLayer))
            {
                // Check if the hit object is not in the "do not spawn" layer
                if ((1 << hit.collider.gameObject.layer & doNotSpawnAtLayer) == 0)
                {
                    return hit.point; // Valid spawn position found
                }
            }
        }
        return Vector3.zero; // No valid spawn position found
    }

    GameObject ChooseRandomPrefab()
    {
        List<GameObject> availablePrefabs = new List<GameObject>();

        for (int i = 0; i < harvestablePrefabs.Length; i++)
        {
            if (spawnedHarvestables[i].Count < harvestablePrefabs[i].maxCount)
            {
                availablePrefabs.Add(harvestablePrefabs[i].prefab); // Add prefab to the available list if it hasn't reached max count
            }
        }

        if (availablePrefabs.Count > 0)
        {
            return availablePrefabs[Random.Range(0, availablePrefabs.Count)];
        }

        return null; // No available prefabs to spawn
    }
}