using UnityEngine;

// ===================================================================================
// SPAWNABLE ENTITY - WAVE
// ===================================================================================
[System.Serializable]
public partial class WaveSpawnableEntity
{
    public GameObject entity;
    [Range(0.01f, 1)] public float probability;
    [Range(1, 999)] public int maxAmount;
    [HideInInspector] public int amount;
}

// ===================================================================================
// SPAWNABLE ENTITY - FIXED
// ===================================================================================
[System.Serializable]
public partial class FixedSpawnableGameObject : SpawnableGameObject
{
    public Transform transform;
}

// ===================================================================================
// SPAWNABLE ENTITY - MOB
// ===================================================================================
[System.Serializable]
public partial class MobSpawnableGameObject : SpawnableGameObject
{
    [Tooltip("[Optional] Target transform for this object (leave empty to randomize within area)")]
    public Transform transform;
    [Tooltip("[Optionm] Gives the possibility to the monster to spawn in a radius around the given position")]
    [Range(0, 9999)] public int aroundTransform;

    [Range(0.01f, 1)] public float probability;
    public Area_Spawner.ScaleLevel scaleLevel;
    public int levelAdjustment = 0;
}

// ===================================================================================
// SPAWNABLE ENTITY - BASE
// ===================================================================================
[System.Serializable]
public partial class StartSpawnableGameObject : SpawnableGameObject
{
    [Range(0.01f, 1)] public float probability;
}

// ===================================================================================
// SPAWNABLE ENTITY - BASE
// ===================================================================================
[System.Serializable]
public abstract partial class SpawnableGameObject
{
    public GameObject gameobject;
}

// ===================================================================================
// SPAWN DESTINATION
// ===================================================================================
[System.Serializable]
public partial class SpawnDestination
{
    public Transform transform;

    [Tooltip("[Required] Probabilities of all elements must sum up to 1.0 (= 100%)")]
    [Range(0, 1)] public float probability;
}
