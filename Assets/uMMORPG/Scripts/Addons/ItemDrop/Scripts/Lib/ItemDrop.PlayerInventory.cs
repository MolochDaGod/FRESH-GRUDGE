using Mirror;
using UnityEngine;

public partial class PlayerInventory
{

    void OnDragAndClear_InventorySlot(int slotIndex)
    {
        if(player.playerAddonsConfigurator.itemDropConfiguration!= null && player.playerAddonsConfigurator.itemDropConfiguration.enableDropOnGround)
            CmdDropItem(slotIndex);
    }

    [Command]
    public void CmdDropItem(int index)
    {
#if _SERVER
        // validate
        if (player.health.current > 0 && 0 <= index && index < slots.Count && slots[index].amount > 0)
            DropItemAndClearSlot(index);
#endif
    }

#if _SERVER
    [Server]
    public void DropItemAndClearSlot(int index)
    {
        // drop and remove from inventory
        ItemSlot slot = slots[index];
        if (slot.item.data.dropPrefab || player.playerAddonsConfigurator.itemDropConfiguration.defaultPrefabItemDrop != null)
        {
            DropItemOnGround(slot.item, slot.amount, (slot.item.data.dropPrefab) ? slot.item.data.dropPrefab : player.playerAddonsConfigurator.itemDropConfiguration.defaultPrefabItemDrop);
            slot.amount = 0;
            slots[index] = slot;
        }
        else
        {
            Debug.Log("Scriptable : " + slot.item.name + " does not contain drop prefab please select one");
        }
    }

    [Server]
    public void DropItemOnGround(Item item, int amount, ItemDrop prefab)
    {
#if _iMMO2D
        Vector2 position = UMMO_Tools.ReachableRandomUnitCircleOnNavMesh(transform.position, player.playerAddonsConfigurator.itemDropConfiguration.playerItemDropRadius, 1);
#else
        Vector3 position = UMMO_Tools.ReachableRandomUnitCircleOnNavMesh(transform.position, player.playerAddonsConfigurator.itemDropConfiguration.playerItemDropRadius, 1);
#endif
        // drop
        GameObject go = Instantiate(prefab.gameObject, position, Quaternion.identity);
        ItemDrop drop = go.GetComponent<ItemDrop>();
        drop.item = item;
        drop.amount = amount;
        drop.itemData = item.data;
        NetworkServer.Spawn(go);
    }
#endif

}
