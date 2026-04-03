using Mirror;
using UnityEngine;

// ===================================================================================
// ITEM DROP - MONSTER
// ===================================================================================
public partial class MonsterItemDrop : NetworkBehaviour
{
    public Monster monster;
    public Health health;

    [Header("[-=-[ ITEM DROP ]-=-]")]
    public Tmpl_ItemDropConfiguration itemDropConfiguration = null;

    protected int dropSolverAttempts = 3; // attempts to drop without being behind a wall, etc.

#if _SERVER
    // ----------------------------------------------------------------------------------
    // Auto Add in Health event
    // ---------------------------------------------------------------------------------- 
    public override void OnStartServer()
    {
        if (monster.monsterItemDrop && itemDropConfiguration != null && itemDropConfiguration.isActive)
            health.onEmpty.AddListener(OnDeath);
    }
    // -----------------------------------------------------------------------------------
    // OnDeath
    // -----------------------------------------------------------------------------------
    [ServerCallback]
    public void OnDeath()
    {
        // ---------- drop gold
        if (itemDropConfiguration.prefabGoldItemDrop != null && monster.gold > 0)
        {
            Vector3 position = UMMO_Tools.ReachableRandomUnitCircleOnNavMesh(transform.position, itemDropConfiguration.rangeDropItem, dropSolverAttempts);
            
            GameObject go = Instantiate(itemDropConfiguration.prefabGoldItemDrop.gameObject, position, Quaternion.identity);
            ItemDrop drop = go.GetComponent<ItemDrop>();
            drop.amount = 0;
            drop.gold = monster.gold;
            monster.gold = 0;

#if _iMMOLOOTRULES && _iMMOITEMDROP
            drop.LiftRulesAfter = monster.monsterLootRules.lootRules.LiftRulesAfter;
            drop.LootVictorParty = monster.monsterLootRules.lootRules.LootVictorParty;
            drop.LootVictorGuild = monster.monsterLootRules.lootRules.LootVictorGuild;
#if _iMMOPVP
            drop.LootVictorRealm = monster.monsterLootRules.lootRules.LootVictorRealm;
            drop.hashRealm = monster.hashRealm;
            drop.hashAlly = monster.hashAlly;
#endif
            drop.LootEverybody = monster.monsterLootRules.lootRules.LootEverybody;
            drop.lastAggressor = (monster.lastAggressor is Summonable summonable) ? summonable.owner : monster.lastAggressor;
            drop.owner = monster;
#endif

            drop.pickUpAuto = itemDropConfiguration.autoPickupGold;
            NetworkServer.Spawn(go);
        }
        else if (itemDropConfiguration.prefabGoldItemDrop == null && monster.gold > 0)
        {
            Debug.LogWarning("You forgot to assign prefabGoldItemDrop in ItemDrop Configuration! ");
        }
        //Debug.Log("Total Item drop : " + monster.inventory.slots.Count);
        
        // ---------- drop items
        foreach (ItemSlot itemSlot in monster.inventory.slots)
        {
            //GameLog.LogMessage(itemSlot.item.name + " -> " + itemSlot.amount);
            if (itemSlot.amount > 0)
            {
                if (itemSlot.item.data.dropPrefab == null && !itemDropConfiguration.defaultPrefabItemDrop)
                {
                    GameLog.LogWarning("You forgot to assign a item drop to: " + itemSlot.item.name + " and prefabItemDrop in ItemDrop Configuration is empty");
                    continue;
                }
                ItemDrop prefab = (itemSlot.item.data.dropPrefab) ? itemSlot.item.data.dropPrefab : itemDropConfiguration.defaultPrefabItemDrop;
                Vector3 position = UMMO_Tools.ReachableRandomUnitCircleOnNavMesh(transform.position, itemDropConfiguration.rangeDropItem, dropSolverAttempts);

                GameObject go = Instantiate(prefab.gameObject, position, Quaternion.identity);
                ItemDrop drop = go.GetComponent<ItemDrop>();
#if _iMMOITEMRARITY
                string rarity = itemSlot.item.data.rarity.ToString();
#endif
                drop.itemData = itemSlot.item.data;
                drop.amount = itemSlot.amount;
                drop.gold = 0;
                drop.item = new Item(itemSlot.item.data);
#if _iMMOLOOTRULES && _iMMOITEMDROP
                drop.LiftRulesAfter = monster.monsterLootRules.lootRules.LiftRulesAfter;
                drop.LootVictorParty = monster.monsterLootRules.lootRules.LootVictorParty;
                drop.LootVictorGuild = monster.monsterLootRules.lootRules.LootVictorGuild;
#if _iMMOPVP
                drop.LootVictorRealm = monster.monsterLootRules.lootRules.LootVictorRealm;
                drop.hashRealm = monster.hashRealm;
                drop.hashAlly = monster.hashAlly;
#endif
                drop.LootEverybody = monster.monsterLootRules.lootRules.LootEverybody;
                drop.lastAggressor = (monster.lastAggressor is Summonable summonable) ? summonable.owner : monster.lastAggressor;
                drop.owner = monster;
#endif

                drop.pickUpAuto = (itemDropConfiguration.overrideItemAutoPickup) ? itemDropConfiguration.itemAutoPickup : itemSlot.item.data.autoPickUp;

                NetworkServer.Spawn(go);
            }
        }

        monster.inventory.slots.Clear();
    }
#endif

    // -----------------------------------------------------------------------------------
}
