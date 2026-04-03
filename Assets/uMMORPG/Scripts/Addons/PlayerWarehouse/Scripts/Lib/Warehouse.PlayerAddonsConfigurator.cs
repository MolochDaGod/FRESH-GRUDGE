using UnityEngine;
using Mirror;
using System;
using System.Linq;

// Component PlayerWarehouse
public partial class PlayerAddonsConfigurator
{

    [Header("[-=-[ Warehouse ]-=-]")]
    public Tmpl_WarehouseConfiguration warehouseConfiguration;
#if _iMMOWAREHOUSE
    [SyncVar] private long _playerWarehouseGold = 0;
    [SyncVar] private int _playerWarehouseLevel = 0;
    public readonly SyncList<ItemSlot> playerWarehouseItemSlot = new SyncList<ItemSlot>();

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public long playerWarehouseGold
    {
        get { return _playerWarehouseGold; }
        set { _playerWarehouseGold = Math.Max(value, 0); }
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public int playerWarehouseLevel
    {
        get { return _playerWarehouseLevel; }
        set { _playerWarehouseLevel = Math.Max(value, 0); }
    }

    // -----------------------------------------------------------------------------------
    // playerWarehouseStorageItems
    // -----------------------------------------------------------------------------------
    public int playerWarehouseStorageItems
    {
        get {
        	if (warehouseConfiguration)
        	{
        		return warehouseConfiguration.warehouseStorageItems.Get(_playerWarehouseLevel + 1);
        	} else{ 
        		Debug.LogWarning("You forgot to assign a warehouse template to one of your player prefabs");
        		return 0;
        	}
         }
    }

    // -----------------------------------------------------------------------------------
    // playerWarehouseStorageGold
    // -----------------------------------------------------------------------------------
    public long playerWarehouseStorageGold
    {
        get {
        	if (warehouseConfiguration)
        	{
        		return warehouseConfiguration.warehouseStorageGold.Get(playerWarehouseLevel + 1);
        	} else {
        		Debug.LogWarning("You forgot to assign a warehouse template to one of your player prefabs");
        		return 0;
         	}
         }
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public int GetWarehouseIndexByName(string itemName)
    {
        return playerWarehouseItemSlot.FindIndex(slot => slot.amount > 0 && slot.item.name == itemName);
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public int WarehouseSlotsFree()
    {
        return playerWarehouseItemSlot.Count(slot => slot.amount == 0);
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public int WarehouseCountAmount(Item item)
    {
        return (from slot in playerWarehouseItemSlot
                where slot.amount > 0 && slot.item.Equals(item)
                select slot.amount).Sum();
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public bool WarehouseRemoveAmount(Item item, int amount)
    {
        for (int i = 0; i < playerWarehouseItemSlot.Count; ++i)
        {
            ItemSlot slot = playerWarehouseItemSlot[i];
            if (slot.amount > 0 && slot.item.Equals(item))
            {
                amount -= slot.DecreaseAmount(amount);
                playerWarehouseItemSlot[i] = slot;
                if (amount == 0) return true;
            }
        }
        return false;
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public bool WarehouseCanAddAmount(Item item, int amount)
    {
        for (int i = 0; i < playerWarehouseItemSlot.Count; ++i)
        {
            if (playerWarehouseItemSlot[i].amount == 0)
                amount -= item.maxStack;
            else if (playerWarehouseItemSlot[i].item.Equals(item))
                amount -= (playerWarehouseItemSlot[i].item.maxStack - playerWarehouseItemSlot[i].amount);
            if (amount <= 0) return true;
        }
        return false;
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public bool WarehouseAddAmount(Item item, int amount)
    {
        // we only want to add them if there is enough space for all of them, so
        // let's double check
        if (item.tradable && WarehouseCanAddAmount(item, amount))
        {
            // go through each slot
            for (int i = 0; i < playerWarehouseItemSlot.Count; ++i)
            {
                // empty? then fill slot with as many as possible
                if (playerWarehouseItemSlot[i].amount == 0)
                {
                    int add = Mathf.Min(amount, item.maxStack);
                    playerWarehouseItemSlot[i] = new ItemSlot(item, add);
                    amount -= add;
                }
                // not empty and same type? then add free amount (max-amount)
                else if (playerWarehouseItemSlot[i].item.Equals(item))
                {
                    ItemSlot temp = playerWarehouseItemSlot[i];
                    amount -= temp.IncreaseAmount(amount);
                    playerWarehouseItemSlot[i] = temp;
                }

                // were we able to fit the whole amount already?
                if (amount <= 0) return true;
            }
            // we should have been able to add all of them
            if (amount != 0) Debug.LogError("warehouse add failed: " + item.name + " " + amount);
        }
        return false;
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public bool CheckForceNpc()
    {
        bool valid = true; // if force use npc is uncheck valid = true
                           
        // if force use npc is check and player is close to npc and npc is warehouse defined and npc offers player warehouse is true
        if (player.playerAddonsConfigurator.warehouseConfiguration.forceUseNpc)
        {
            valid = (player.target != null && player.target is Npc npc && Utils.ClosestDistance(player, npc) <= player.interactionRange && npc.npcWarehouse && npc.npcWarehouse.offersPlayerWarehouse);
        }
        return valid;
    }
    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    [Command]
    public void CmdSwapInventoryWarehouse(int fromIndex, int toIndex)
    {
#if _SERVER
        ItemSlot slot = player.inventory.slots[fromIndex];

        if (CheckForceNpc() && (player.state == "IDLE" || player.state == "MOVING" || player.state == "CASTING") &&
            (warehouseConfiguration.storeTradable || (!warehouseConfiguration.storeTradable && !slot.item.tradable)) &&
            (warehouseConfiguration.storeSellable || (!warehouseConfiguration.storeSellable && !slot.item.sellable)) &&
            (warehouseConfiguration.storeDestroyable || (!warehouseConfiguration.storeDestroyable && !slot.item.destroyable)) &&
            WarehouseSlotsFree() >= 1 &&
            0 <= fromIndex && fromIndex < player.inventory.slots.Count &&
            0 <= toIndex && toIndex < playerWarehouseItemSlot.Count)
        {
            // don't allow player to add items which has zero amount or if it's summoned pet item
            if (slot.amount > 0 && !slot.item.summoned)
            {
                // swap them
                player.inventory.slots[fromIndex] = playerWarehouseItemSlot[toIndex];
                playerWarehouseItemSlot[toIndex] = slot;
                UI_UpdatePlayerWarehouse(player.connectionToClient);
            }
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    [Command]
    public void CmdSwapWarehouseInventory(int fromIndex, int toIndex)
    {
#if _SERVER
        if (CheckForceNpc())
        {
            if ((player.state == "IDLE" || player.state == "MOVING" || player.state == "CASTING") &&
                0 <= fromIndex && fromIndex < playerWarehouseItemSlot.Count &&
                0 <= toIndex && toIndex < player.inventory.slots.Count)
            {
                // swap them
                ItemSlot temp = playerWarehouseItemSlot[fromIndex];
                playerWarehouseItemSlot[fromIndex] = player.inventory.slots[toIndex];
                player.inventory.slots[toIndex] = temp;
                UI_UpdatePlayerWarehouse(player.connectionToClient);
            }
        }
        else
        {
            player.Tools_TargetAddMessage(warehouseConfiguration.distanceNpcTooGreat);
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    [Command]
    public void CmdSwapWarehouseWarehouse(int fromIndex, int toIndex)
    {
#if _SERVER
        if (CheckForceNpc()) {
            if ((player.state == "IDLE" || player.state == "MOVING" || player.state == "CASTING") &&
                0 <= fromIndex && fromIndex < playerWarehouseItemSlot.Count &&
                0 <= toIndex && toIndex < playerWarehouseItemSlot.Count &&
                fromIndex != toIndex)
            {
                // swap them
                ItemSlot temp = playerWarehouseItemSlot[fromIndex];
                playerWarehouseItemSlot[fromIndex] = playerWarehouseItemSlot[toIndex];
                playerWarehouseItemSlot[toIndex] = temp;
            }

            UI_UpdatePlayerWarehouse(player.connectionToClient);
        }
        else
        {
            player.Tools_TargetAddMessage(warehouseConfiguration.distanceNpcTooGreat);
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    [Command]
    public void CmdWarehouseSplit(int fromIndex, int toIndex)
    {
#if _SERVER
        if (CheckForceNpc())
        {
            if ((player.state == "IDLE" || player.state == "MOVING" || player.state == "CASTING") &&
                0 <= fromIndex && fromIndex < playerWarehouseItemSlot.Count &&
                0 <= toIndex && toIndex < playerWarehouseItemSlot.Count &&
                fromIndex != toIndex)
            {
                // slotFrom has to have an entry, slotTo has to be empty
                ItemSlot slotFrom = playerWarehouseItemSlot[fromIndex];
                ItemSlot slotTo = playerWarehouseItemSlot[toIndex];

                // from entry needs at least amount of 2
                if (slotFrom.amount >= 2 && slotTo.amount == 0)
                {
                    // split them serversided (has to work for even and odd)
                    slotTo = slotFrom;

                    slotTo.amount = slotFrom.amount / 2;
                    slotFrom.amount -= slotTo.amount; // works for odd too

                    // put back into the list
                    playerWarehouseItemSlot[fromIndex] = slotFrom;
                    playerWarehouseItemSlot[toIndex] = slotTo;
                }
                UI_UpdatePlayerWarehouse(player.connectionToClient);
            }
        }
        else
        {
            player.Tools_TargetAddMessage(warehouseConfiguration.distanceNpcTooGreat);
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    [Command]
    public void CmdWarehouseMerge(int fromIndex, int toIndex)
    {
#if _SERVER
        if (CheckForceNpc()) {
            if ((player.state == "IDLE" || player.state == "MOVING" || player.state == "CASTING") &&
                0 <= fromIndex && fromIndex < playerWarehouseItemSlot.Count &&
                0 <= toIndex && toIndex < playerWarehouseItemSlot.Count &&
                fromIndex != toIndex)
            {
                // both items have to be valid
                ItemSlot slotFrom = playerWarehouseItemSlot[fromIndex];
                ItemSlot slotTo = playerWarehouseItemSlot[toIndex];

                if (slotFrom.amount > 0 && slotTo.amount > 0)
                {
                    // check if the both items are the same type
                    if (slotFrom.item.Equals(slotTo.item))
                    {
                        int put = slotTo.IncreaseAmount(slotFrom.amount);
                        slotFrom.DecreaseAmount(put);

                        // put back into the list
                        playerWarehouseItemSlot[fromIndex] = slotFrom;
                        playerWarehouseItemSlot[toIndex] = slotTo;
                    }
                }
                UI_UpdatePlayerWarehouse(player.connectionToClient);
            }
        }
        else
        {
            player.Tools_TargetAddMessage(warehouseConfiguration.distanceNpcTooGreat);
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    [Command]
    public void CmdDepositGold(int amount)
    {
#if _SERVER
        if (CheckForceNpc())
        {
            WarehouseAddGold(amount);
            UI_UpdatePlayerWarehouse(player.connectionToClient);
        }
        else
            player.Tools_TargetAddMessage(warehouseConfiguration.distanceNpcTooGreat);
#endif
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    [Command]
    public void CmdWithdrawGold(int amount)
    {
#if _SERVER
        if (CheckForceNpc())
        {
            WarehouseRemoveGold(amount);
            UI_UpdatePlayerWarehouse(player.connectionToClient);
        }
        else
            player.Tools_TargetAddMessage(warehouseConfiguration.distanceNpcTooGreat);
#endif
    }

    // -----------------------------------------------------------------------------------
    // Cmd_UpgradePlayerWarehouse
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_UpgradePlayerWarehouse()
    {
#if _SERVER
        if (CheckForceNpc())
        {
            UpgradePlayerWarehouse();
            UI_UpdatePlayerWarehouse(player.connectionToClient);
        }
        else
            player.Tools_TargetAddMessage(warehouseConfiguration.distanceNpcTooGreat);
#endif
    }


    // -----------------------------------------------------------------------------------
    // HasEnoughGoldSpace
    // -----------------------------------------------------------------------------------
    public bool HasEnoughPlayerWarehouseGoldSpace(long amount = 1)
    {
        return amount > 0 && playerWarehouseStorageGold >= playerWarehouseGold + amount;
    }

    // -----------------------------------------------------------------------------------
    // CanUpgradePlayerWarehouse
    // @Client OR @Server
    // -----------------------------------------------------------------------------------
   /* public bool CanUpgradePlayerWarehouse()
    {
        if (!player.warehouseConfiguration.warehouseConfiguration.forceUseNpc || (player.target != null && player.target is Npc && Utils.ClosestDistance(player, player.target) <= player.interactionRange))
        {

        }
        else
        {

        }
            return (warehouseConfiguration.guildWarehouseUpgradeCost.Length > 0 && warehouseConfiguration.guildWarehouseUpgradeCost.Length > playerWarehouseLevel && warehouseConfiguration.guildWarehouseUpgradeCost[playerWarehouseLevel].CheckCost(player));
    }*/

    public bool CanUpgradePlayerWarehouse()
    {
        return CheckForceNpc() && warehouseConfiguration.warehouseUpgradeCost.Length > 0 && warehouseConfiguration.warehouseUpgradeCost.Length > playerWarehouseLevel && warehouseConfiguration.warehouseUpgradeCost[playerWarehouseLevel].CheckCost(player);
    }

#if _SERVER

    // -----------------------------------------------------------------------------------
    // UpgradePlayerWarehouse
    // @Server
    // -----------------------------------------------------------------------------------
    [Server]
    public void UpgradePlayerWarehouse()
    {
        if (CanUpgradePlayerWarehouse())
        {
            Debug.Log(" ||| > " + warehouseConfiguration.warehouseUpgradeCost[playerWarehouseLevel] + " , je paie : " + warehouseConfiguration.warehouseUpgradeCost[playerWarehouseLevel].goldCost);
            warehouseConfiguration.warehouseUpgradeCost[playerWarehouseLevel].PayCost(player);

            int oldSize = playerWarehouseStorageItems;
            playerWarehouseLevel++;

            int sizeDifference = playerWarehouseStorageItems - oldSize;

            for (int i = 0; i < sizeDifference; ++i)
            {
                playerWarehouseItemSlot.Add(new ItemSlot());
            }

            player.Tools_ShowPopup(warehouseConfiguration.upgradeLabel);
        }
    }

    // -----------------------------------------------------------------------------------
    // HasEnoughGoldOnInventory
    // -----------------------------------------------------------------------------------
    [Server]
    public bool HasEnoughGoldOnInventory(long amount)
    {
        return amount > 0 && player.gold >= amount;
    }

    // -----------------------------------------------------------------------------------
    // HasEnoughGoldOnWarehouse
    // -----------------------------------------------------------------------------------
    [Server]
    public bool HasEnoughGoldOnWarehouse(long amount)
    {
        return amount > 0 && playerWarehouseGold >= amount;
    }
 
    // -----------------------------------------------------------------------------------
    // WarehouseAddGold
    // -----------------------------------------------------------------------------------
    [Server]
    public void WarehouseAddGold(long amount)
    {
        if (HasEnoughGoldOnInventory(amount) && HasEnoughPlayerWarehouseGoldSpace(amount))
        {
            player.gold -= amount;
            playerWarehouseGold += amount;
        }
    }

    // -----------------------------------------------------------------------------------
    // WarehouseRemoveGold
    // -----------------------------------------------------------------------------------
    [Server]
    public void WarehouseRemoveGold(long amount)
    {
        if (HasEnoughGoldOnWarehouse(amount))
        {
            playerWarehouseGold -= amount;
            player.gold += amount;
        }
    }

#endif

    [TargetRpc]
    public void UI_UpdatePlayerWarehouse(NetworkConnection target)
    {
        warehouseConfiguration.uiEventPlayerWarehouse.TriggerEvent();
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    private void OnDragAndDrop_WarehouseSlot_WarehouseSlot(int[] slotIndices)
    {
        // merge? (just check the name, rest is done server sided)
        if (playerWarehouseItemSlot[slotIndices[0]].amount > 0 && playerWarehouseItemSlot[slotIndices[1]].amount > 0 && playerWarehouseItemSlot[slotIndices[0]].item.Equals(playerWarehouseItemSlot[slotIndices[1]].item))
        {
            CmdWarehouseMerge(slotIndices[0], slotIndices[1]);
            // split?
        }
        else if (Utils.AnyKeyPressed(player.inventory.splitKeys))
        {
            CmdWarehouseSplit(slotIndices[0], slotIndices[1]);
            // swap?
        }
        else
        {
            CmdSwapWarehouseWarehouse(slotIndices[0], slotIndices[1]);
        }
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public void OnDragAndDrop_WarehouseSlot_InventorySlot(int[] slotIndices)
    {
        if (playerWarehouseItemSlot[slotIndices[0]].amount > 0)
        {
            CmdSwapWarehouseInventory(slotIndices[0], slotIndices[1]);
        }
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    private void OnDragAndDrop_InventorySlot_WarehouseSlot(int[] slotIndices)
    {
        if (player.inventory.slots[slotIndices[0]].amount > 0)
        {
            CmdSwapInventoryWarehouse(slotIndices[0], slotIndices[1]);
        }
    }

    // -----------------------------------------------------------------------------------
#endif
}