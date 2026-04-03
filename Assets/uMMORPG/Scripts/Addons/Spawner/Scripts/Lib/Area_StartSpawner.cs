using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// START SPAWNER AREA
#if _iMMO2D
[RequireComponent(typeof(CircleCollider2D))]
#else
[RequireComponent(typeof(SphereCollider))]
#endif
public class Area_StartSpawner : NetworkBehaviour
{
    [Header("[-=-[ SPAWNER ]-=-]")]
    public LayerMask doNotSpawnAt;

    [Header("[-=-[ SPAWN LIST ]-=-]")]
    [Range(0, 999)] public int maxObjects;
    public StartSpawnableGameObject[] spawnableGameObjects;

    protected int maxIterationCycles = 75;

    // Define the collider based on _iMMO2D directive
#if _iMMO2D
    public CircleCollider2D areaCollider;
#else
    public SphereCollider areaCollider;
#endif
#if _SERVER
    private void Start()
    {
        OnSpawn();
        NetworkServer.Destroy(gameObject);
    }

    private void OnSpawn()
    {
        System.Random random = new System.Random();

        var spawnCandidates = new List<StartSpawnableGameObject>();
        foreach (StartSpawnableGameObject spawnableObject in spawnableGameObjects)
        {
            if (spawnableObject.probability > 0 && random.NextDouble() <= spawnableObject.probability)
            {
                spawnCandidates.Add(spawnableObject);
                if (maxObjects > 0 && spawnCandidates.Count >= maxObjects)
                    break;
            }
        }

        foreach (var spawnCandidate in spawnCandidates)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            Quaternion spawnRotation = GetRandomSpawnRotation();
            SpawnGameObject(spawnCandidate.gameobject, spawnPosition, spawnRotation);
        }
    }

    [Server]
    private GameObject SpawnGameObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject go = Instantiate(prefab, position, rotation);
        go.name = prefab.name;
        NetworkServer.Spawn(go);
        return go;
    }

    private Quaternion GetRandomSpawnRotation()
    {
#if _iMMO2D
        return Quaternion.Euler(new Vector3(0, 0, 0));
#else
        return Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
#endif
    }

#if _iMMO2D
    private Vector2 GetRandomSpawnVector(float radius)
    {
        return new Vector2(
            transform.position.x + Random.Range(radius * -1, radius),
            transform.position.y + Random.Range(radius * -1, radius)
        );
    }
#else
    private Vector3 GetRandomSpawnVector(float radius)
    {
        return new Vector3(
            transform.position.x + Random.Range(radius * -1, radius),
            transform.position.y + Random.Range(radius * -1, radius),
            transform.position.z + Random.Range(radius * -1, radius)
        );
    }
#endif

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 spawnPosition = Vector3.zero;
        int i = 0;
        float radius = areaCollider.radius;

        while (i <= maxIterationCycles)
        {
            spawnPosition = GetRandomSpawnVector(radius);
            bool validSpawn = false;

#if _iMMO2D
            validSpawn = NavMesh2D.SamplePosition(Vector2.zero, out NavMeshHit2D hit, radius, NavMesh2D.AllAreas)
                && !Physics2D.Raycast(spawnPosition, Vector2.down, radius, doNotSpawnAt.value);
#else
            validSpawn = NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, radius, NavMesh.AllAreas)
                && !Physics.Raycast(spawnPosition, Vector3.down, radius, doNotSpawnAt.value);
#endif

            if (validSpawn)
                return spawnPosition;

            i++;
        }

        return spawnPosition;
    }
#endif
}
