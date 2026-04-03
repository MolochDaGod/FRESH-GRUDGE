using Mirror;
using UnityEngine;

#if _iMMOLOOTRULES

// LOOT RULES - PLAYER
[System.Serializable]
public partial class LootRules
{
    [Header("[LOOT RULES]")]
    [Tooltip("After x seconds loot rules are set to 'LootEverybody'. Set to 0 to disable")]
    public float LiftRulesAfter;

    public bool LootVictorParty;
    public bool LootVictorGuild;
#if _iMMOPVP
    public bool LootVictorRealm;
#endif
    public bool LootEverybody;

    [Header("[Drop Only Entity]")]
    public EntityType entityType;

    [Header("[Enable/Disable Drop system]")]
    public bool enableDropEquipment = false;
    public bool enableDropGold = false;
    public bool enableDropInventory = false;

    [Header("[DROP SETTINGS]")]
    [Tooltip("[Optional] Modifies the drop chance of every item equipped")]
    [Range(-1, 1)] public float equipmentDropModifier;

    [Tooltip("[Optional] Modifies the drop chance of every item in the inventory")]
    [Range(-1, 1)] public float inventoryDropModifier;

#if _iMMOITEMDROP
    [Header("[ITEM DROP]")]
    public float radiusMultiplier = 1;

    [Tooltip("[Required] Drop prefab used for gold drops")]
    public ItemDrop goldDropPrefab;
#endif

    [Tooltip("[Required] Percentage of gold dropped on death")]
    [Range(0, 1)] public float goldPercentage;

    [HideInInspector] public int dropSolverAttempts = 3;
    [HideInInspector] public double deathTime;
    [HideInInspector, SyncVar] public bool lootRulesLifted;

    // -----------------------------------------------------------------------------------
}
#endif