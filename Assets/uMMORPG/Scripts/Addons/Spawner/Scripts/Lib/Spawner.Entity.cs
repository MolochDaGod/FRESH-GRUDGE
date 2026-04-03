using UnityEngine;

// ENTITY

public partial class Entity
{
    [HideInInspector] public Area_WaveSpawner parentSpawnArea;
    [HideInInspector] public int parentWaveIndex;

    // -----------------------------------------------------------------------------------
    // OnDeath
    // -----------------------------------------------------------------------------------
    public void OnDeath_WaveSpawnerEntity()
    {
#if _SERVER
        if (parentSpawnArea == null) return;
        parentSpawnArea.UpdateMemberPopulation(name.GetStableHashCode(), parentWaveIndex);
        parentSpawnArea = null;
#endif
    }

    // -----------------------------------------------------------------------------------
}
