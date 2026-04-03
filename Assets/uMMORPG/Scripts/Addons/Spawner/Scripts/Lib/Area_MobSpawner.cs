
using UnityEngine;

// MOB SPAWNER AREA
public class Area_MobSpawner : Area_Spawner
{
    [Header("[-=-[ SPAWN LIST ]-=-]")]
    public MobSpawnableGameObject[] spawnableGameObjects;

    [Tooltip("[Optional] Spawner only triggers when X or more players are inside its collider")]
    public int minPlayerCount;

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
        if (players.Count < minPlayerCount) return;

        foreach (MobSpawnableGameObject spawnableEntity in spawnableGameObjects)
        {
            if (spawnableEntity.probability > 0 && UnityEngine.Random.value <= spawnableEntity.probability)
            {
                Vector3 spawnPosition;
                spawnPosition = ((spawnableEntity.transform == null) ? GetRandomSpawnPosition() : (spawnableEntity.aroundTransform != 0) ? spawnableEntity.transform.position + (Random.insideUnitSphere * spawnableEntity.aroundTransform) : spawnableEntity.transform.position);
                //spawnPosition = ((spawnableEntity.transform == null) ? getRandomSpawnPosition() : spawnableEntity.transform.position);

                GameObject go = SpawnGameObject(spawnableEntity.gameobject, spawnPosition, GetRandomSpawnRotation());

                if (go.GetComponent<Entity>() != null)
                {
                    if (spawnableEntity.scaleLevel == ScaleLevel.Add)
                    {
                        go.GetComponent<Entity>().level.current += spawnableEntity.levelAdjustment;
                    }
                    else if (spawnableEntity.scaleLevel == ScaleLevel.Average)
                    {
                        go.GetComponent<Entity>().level.current = (go.GetComponent<Monster>().level.current + spawnableEntity.levelAdjustment) / 2;
                    }
                    else if (spawnableEntity.scaleLevel == ScaleLevel.Override)
                    {
                        go.GetComponent<Entity>().level.current = spawnableEntity.levelAdjustment;
                    }
                    else if (spawnableEntity.scaleLevel == ScaleLevel.Player)
                    {
                        go.GetComponent<Entity>().level.current = currentPlayerLevel + spawnableEntity.levelAdjustment;
                    }
                    else if (spawnableEntity.scaleLevel == ScaleLevel.PlayerAverage)
                    {
                        go.GetComponent<Entity>().level.current = (go.GetComponent<Entity>().level.current + currentPlayerLevel + spawnableEntity.levelAdjustment) / 2;
                    }
                }
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // OnUnspawn
    // -----------------------------------------------------------------------------------
    protected override void OnUnspawn() { }

    // -----------------------------------------------------------------------------------
#endif
}
