using UnityEngine;

// FIXED SPAWNER AREA
public class Area_FixedSpawner : Area_Spawner
{
    [Header("[-=-[ SPAWN LIST ]-=-]")]
    public FixedSpawnableGameObject[] spawnableGameObjects;

#if _SERVER
    // -----------------------------------------------------------------------------------
    // Start
    // -----------------------------------------------------------------------------------
    public override void Start()
    {
        base.Start();
        UpdateMaxGameObjects();
    }

    // -----------------------------------------------------------------------------------
    // UpdateMaxGameObjects
    // -----------------------------------------------------------------------------------
    protected override void UpdateMaxGameObjects()
    {
        maxGameObjects = spawnableGameObjects.Length;
    }

    // -----------------------------------------------------------------------------------
    // OnSpawn
    // -----------------------------------------------------------------------------------
    protected override void OnSpawn()
    {
        foreach (FixedSpawnableGameObject spawnableEntity in spawnableGameObjects)
            if (spawnableEntity.transform != null)
            {
                SpawnGameObject(spawnableEntity.gameobject, spawnableEntity.transform.position, spawnableEntity.transform.rotation);
            }
            else
            {
                SpawnGameObject(spawnableEntity.gameobject, GetRandomSpawnPosition(), new Quaternion());
            }
    }

    // -----------------------------------------------------------------------------------
    // OnUnspawn
    // -----------------------------------------------------------------------------------
    protected override void OnUnspawn() { }
#endif
    // -----------------------------------------------------------------------------------
}
