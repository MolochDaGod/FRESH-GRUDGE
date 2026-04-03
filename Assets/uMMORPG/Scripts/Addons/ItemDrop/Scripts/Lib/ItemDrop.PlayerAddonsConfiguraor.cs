using Mirror;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerAddonsConfigurator
{
    [Header("[-=-[ Item Drop player ]-=-]")]
    public Tmpl_ItemDropConfiguration itemDropConfiguration;


#if _SERVER
    [ServerCallback]
    public void PlayeSoundItemDrop(AudioClip sound)
    {
        player.audioSource.PlayOneShot(sound, 1);
    }


    public void OnStartServer_ItemDropPlayer()
    {
        if (itemDropConfiguration != null && itemDropConfiguration.enablePlayerItemDrop)
        {
            health.onEmpty.AddListener(OnDeath_ItemDrop);
        }
    }
    public void OnDeath_ItemDrop()
    {
        if (itemDropConfiguration != null)
        {
            if (itemDropConfiguration.enablePlayerItemDrop)
            {
                Entity agressor = (player.lastAggressor is Summonable summonable) ? summonable.owner : player.lastAggressor;
                //Debug.Log(" agressor : " + agressor.name);
                int diff = (agressor != null) ? Mathf.Abs(player.level.current - agressor.level.current) : 0;
                if (agressor is Player && diff > itemDropConfiguration.maxLevelDifference) return;
                
                // ---------- drop gold 
                if (itemDropConfiguration.enableDropGold && player.gold > 0)
                {
                    SpawnItemDrop(new ItemSlot(), Mathf.RoundToInt(player.gold * itemDropConfiguration.percentGoldDrop / 100));
                }

                // ---------- Drop items
                if (itemDropConfiguration.enablePlayerItemDrop && itemDropConfiguration.killerEntityForDrop.HasFlag(UMMO_Tools.GetEntityType(player.lastAggressor)))
                {
                    // ----------- Drop inventory
                    if (itemDropConfiguration.enableDropInventory && itemDropConfiguration.percentInventoryItemDrop > 0)
                    {
                        List<int> nonEmptySlotsInventory = new List<int>();
                        for (int i = 0; i < player.inventory.slots.Count; i++)
                        {
                            // pour être ajouter à la liste l'item doit être échangeable et desctructible 
                            if (player.inventory.slots[i].amount > 0 && player.inventory.slots[i].item.destroyable && player.inventory.slots[i].item.tradable)
                            {
                                nonEmptySlotsInventory.Add(i);
                            }
                        }

                        int numItemsToDrop = Mathf.RoundToInt(nonEmptySlotsInventory.Count * itemDropConfiguration.percentInventoryItemDrop / 100);
                        for (int i = 0; i < numItemsToDrop; i++)
                        {
                            int randomIndex = Random.Range(0, nonEmptySlotsInventory.Count);
                            int slotIndex = nonEmptySlotsInventory[randomIndex];

                            SpawnItemDrop(player.inventory.slots[slotIndex]);

                            player.inventory.slots[slotIndex] = new ItemSlot();

                            nonEmptySlotsInventory.RemoveAt(randomIndex);
                        }

                        nonEmptySlotsInventory.Clear();
                        //nonEmptySlotsInventory = null;
                    }


                    // ----------- Drop Equipment
                    if (itemDropConfiguration.enableDropEquipment && itemDropConfiguration.percentEquipmentItemDrop > 0)
                    {
                        List<int> nonEmptySlotsEquipment = new List<int>();
                        for (int i = 0; i < player.equipment.slots.Count; i++)
                        {
                            if (player.equipment.slots[i].amount > 0 && player.equipment.slots[i].item.destroyable && player.equipment.slots[i].item.tradable)
                            {
                                nonEmptySlotsEquipment.Add(i);
                            }
                        }

                        int numItemsToDropEquipment = Mathf.RoundToInt(nonEmptySlotsEquipment.Count * itemDropConfiguration.percentEquipmentItemDrop / 100);
                        for (int i = 0; i < numItemsToDropEquipment; i++)
                        {
                            int randomIndex = Random.Range(0, nonEmptySlotsEquipment.Count);
                            int slotIndex = nonEmptySlotsEquipment[randomIndex];

                            SpawnItemDrop(player.equipment.slots[slotIndex]);

                            player.equipment.slots[slotIndex] = new ItemSlot();
                            nonEmptySlotsEquipment.RemoveAt(randomIndex);
                        }

                        nonEmptySlotsEquipment.Clear();
                        //nonEmptySlotsEquipment = null;
                    }
                }
            }
        }
    }


    private void SpawnItemDrop(ItemSlot itemSlot , long gold = 0)
    {
        if (player.playerLootRules)
        {
            if (itemSlot.amount > 0 && itemSlot.item.data.dropPrefab == null && !itemDropConfiguration.defaultPrefabItemDrop)
            {
                Debug.LogWarning("You forgot to assign a item drop to: " + itemSlot.item.name + " and prefabGoldItemDrop in ItemDrop Configuration is empty");
            }
            else if (gold > 0 && !itemDropConfiguration.prefabGoldItemDrop)
            {
                Debug.LogWarning("You forgot to assign prefabGoldItemDrop in ItemDrop Configuration! ");
            }
            else
            {
                ItemDrop prefab = (itemSlot.amount > 0 && itemSlot.item.data.dropPrefab) ? itemSlot.item.data.dropPrefab : ((gold > 0) ? itemDropConfiguration.prefabGoldItemDrop : itemDropConfiguration.defaultPrefabItemDrop);
                Vector3 position = UMMO_Tools.ReachableRandomUnitCircleOnNavMesh(transform.position, itemDropConfiguration.rangeDropItem, player.playerLootRules.dropSolverAttempts);

                GameObject go = Instantiate(prefab.gameObject, position, Quaternion.identity);
                ItemDrop drop = go.GetComponent<ItemDrop>();
#if _iMMOITEMRARITY
                // Debug.Log(">>" + itemSlot.item.name);
                if (gold == 0) { string rarity = itemSlot.item.data.rarity.ToString(); }
#endif
                if (gold == 0) drop.itemData = itemSlot.item.data;
                drop.amount = itemSlot.amount;
                drop.gold = (gold > 0) ? gold : 0;
                if (gold > 0) player.gold -= gold;
                if (gold == 0) drop.item = new Item(itemSlot.item.data);
#if _iMMOLOOTRULES && _iMMOITEMDROP
                drop.LiftRulesAfter = player.playerLootRules.lootRulesEntity.LiftRulesAfter;
                drop.LootVictorParty = player.playerLootRules.lootRulesEntity.LootVictorParty;
                drop.LootVictorGuild = player.playerLootRules.lootRulesEntity.LootVictorGuild;
#if _iMMOPVP
                drop.LootVictorRealm = player.playerLootRules.lootRulesEntity.LootVictorRealm;
                drop.hashRealm = player.hashRealm;
                drop.hashAlly = player.hashAlly;
#endif
                drop.LootEverybody = player.playerLootRules.lootRulesEntity.LootEverybody;
                drop.lastAggressor = (player.lastAggressor is Summonable summonable) ? summonable.owner : player.lastAggressor;
                drop.owner = player;
#endif
                drop.pickUpAuto = (gold > 0) ? itemDropConfiguration.autoPickupGold : ((itemDropConfiguration.overrideItemAutoPickup) ? itemDropConfiguration.itemAutoPickup : itemSlot.item.data.autoPickUp);

                NetworkServer.Spawn(go);
            }
        }
    }

#endif
}
