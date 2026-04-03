using UnityEngine;
using System;

// -----------------------------------------------------------------------------------
// AttachmentChild
// -----------------------------------------------------------------------------------
[Serializable]
public partial class AttachmentChild
{
    [Tooltip("Only one object according to the level of the parent is spawned/unspawned")]
    public GameObject[] childGameObjects;

    public bool autoSpawn = true;
    public bool autoUnspawn = true;

    [Tooltip("[Optional] Makes each child object die instead of just removing it (only works with Entities)")]
    public bool killOnUnspawn = false;

#if _iMMOMINION
    public bool followMaster = true;
#endif
    public Transform mountPoint;
    [Range(1,10)] public int aroundSpawnPoint = 2;
    public AttachmentChildCondition[] childConditions;
    [HideInInspector] public GameObject cacheGameObject;
}

// -----------------------------------------------------------------------------------
// AttachmentChildCondition
// -----------------------------------------------------------------------------------
[Serializable]
public partial class AttachmentChildCondition
{
    [Tooltip("Health of the parent must be 'below' or 'above' the Health threshold")]
    public Monster.ParentThreshold parentThreshold;

    [Tooltip("Health treshold of the parent in order to trigger condition")]
    [Range(0, 1)] public float parentHealth;

    [Tooltip("Parent must have this active Buff in order to trigger condition")]
    public BuffSkill activeBuff;

    [Tooltip("Parent must have this item equipped in order to trigger condition")]
    public ScriptableItem equippedItem;

    [Tooltip("Parent must have this item in inventory in order to trigger condition")]
    public ScriptableItem inventoryItem;

    [Tooltip("When triggered - is the child spawned or destroyed?")]
    public Monster.ChildAction childAction;
}
