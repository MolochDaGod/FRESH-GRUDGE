using Mirror;
using System;
using System.Linq;
using UnityEngine;

public partial class PlayerAddonsConfigurator
{

    [Header("[-=-[ Guild Warehouse ]-=-]")]
    public Tmpl_GuildWarehouseConfiguration guildWarehouseConfiguration;

    [SyncVar] private long _guildWarehouseGold = 0;
    [SyncVar] private int _guildWarehouseLevel = 0;
    [SyncVar] private bool lookingGuildWarehouse = false;

    public readonly SyncList<ItemSlot> guildWarehouseItemSlot = new SyncList<ItemSlot>();
    public readonly SyncList<GuildWarehouseLog> listLogGuildWarehouse = new SyncList<GuildWarehouseLog>();
    public void OnStartClient_GuildWarehouse()
    {
        listLogGuildWarehouse.Callback += OnGuildWarehouseLogUpdated;
    }
    void OnGuildWarehouseLogUpdated(SyncList<GuildWarehouseLog>.Operation op, int index, GuildWarehouseLog oldlog, GuildWarehouseLog newLog)
    {
        Debug.Log("Changer");
        guildWarehouseConfiguration.uiEventGuildWarehouse.TriggerEvent();
    }

    // -----------------------------------------------------------------------------------
    // ResetGuildWarehouse
    // -----------------------------------------------------------------------------------
    public void ResetGuildWarehouse()
    {
        guildWarehouseItemSlot.Clear();
        _guildWarehouseGold = 0;
        _guildWarehouseLevel = 0;
        listLogGuildWarehouse.Clear();
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public long guildWarehouseGold
    {
        get { return _guildWarehouseGold; }
        set { _guildWarehouseGold = Math.Max(value, 0); }
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public int guildWarehouseLevel
    {
        get { return _guildWarehouseLevel; }
        set { _guildWarehouseLevel = Math.Max(value, 0); }
    }

    // -----------------------------------------------------------------------------------
    // guildWarehouseStorageItems
    // -----------------------------------------------------------------------------------
    public int guildWarehouseStorageItems
    {
        get
        {
            if (guildWarehouseConfiguration)
            {
                return guildWarehouseConfiguration.guildWarehouseStorageItems.Get(guildWarehouseLevel);
            }
            else
            {
                Debug.LogWarning("You forgot to assign a guild warehouse template to one of your player prefabs");
                return 0;
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // guildWarehouseStorageGold
    // -----------------------------------------------------------------------------------
    public long guildWarehouseStorageGold
    {
        get
        {
            if (guildWarehouseConfiguration)
            {
                return guildWarehouseConfiguration.guildWarehouseStorageGold.Get(guildWarehouseLevel);
            }
            else
            {
                Debug.LogWarning("You forgot to assign a guild warehouse template to one of your player prefabs");
                return 0;
            }
        }
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public int Get_GuildWarehouseIndexByName(string itemName)
    {
        return guildWarehouseItemSlot.FindIndex(slot => slot.amount > 0 && slot.item.name == itemName);
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public int GuildWarehouseSlotsFree()
    {
        return guildWarehouseItemSlot.Count(slot => slot.amount == 0);
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public int GuildWarehouseCountAmount(Item item)
    {
        return (from slot in guildWarehouseItemSlot where slot.amount > 0 && slot.item.Equals(item) select slot.amount).Sum();
    }


    [Command]
    public void Cmd_LoadGuildWarehouseLogs()
    {
#if _SERVER
        Database.singleton.LoadGuildWarehouseLog(player);
        UI_UpdateGuildWarehouse(player.connectionToClient);
#endif
    }
    // -----------------------------------------------------------------------------------
    // Cmd_LoadGuildWarehouse
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_LoadGuildWarehouse()
    {
#if _SERVER
        if (player.guild.guild.name == "") return;
        bool loaded = Database.singleton.LoadGuildWarehouse(player);
        player.playerAddonsConfigurator.lookingGuildWarehouse = true;
        if (loaded)
            UI_UpdateGuildWarehouse(player.connectionToClient);
#endif
    }
    [Server]
    public void Cmd_LoadGuildWarehouseServer()
    {
#if _SERVER
        if (player.guild.guild.name == "") return;
        bool loaded = Database.singleton.LoadGuildWarehouse(player);
        player.playerAddonsConfigurator.lookingGuildWarehouse = true;
        if (loaded)
            UI_UpdateGuildWarehouse(player.connectionToClient);
#endif
    }


    [Command]
    public void Cmd_CloseWarehouse()
    {
        player.playerAddonsConfigurator.lookingGuildWarehouse = false;
        guildWarehouseItemSlot.Clear();
        UI_UpdateGuildWarehouse(player.connectionToClient);
    }
    // -----------------------------------------------------------------------------------
    // Cmd_SaveGuildWarehouse
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_SaveGuildWarehouse()
    {
#if _SERVER
        if (player.guild.guild.name == "") return;
        Database.singleton.SaveGuildWarehouse(player);
        UI_UpdateGuildWarehouse(player.connectionToClient);
#endif
    }

    // -----------------------------------------------------------------------------------
    //  Swap inventory to warehouse
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_Swap_InventoryToWarehouse(int fromIndex, int toIndex)
    {
#if _SERVER
        ItemSlot slot = player.inventory.slots[fromIndex];

        if ((player.state == "IDLE" || player.state == "MOVING" || player.state == "CASTING") &&
            (guildWarehouseConfiguration.storeTradable || (!guildWarehouseConfiguration.storeTradable && !slot.item.tradable)) &&
            (guildWarehouseConfiguration.storeSellable || (!guildWarehouseConfiguration.storeSellable && !slot.item.sellable)) &&
            (guildWarehouseConfiguration.storeDestroyable || (!guildWarehouseConfiguration.storeDestroyable && !slot.item.destroyable)) &&
            0 <= fromIndex && fromIndex < player.inventory.slots.Count &&
            0 <= toIndex && toIndex < guildWarehouseItemSlot.Count)
        {
            Debug.Log("Warehouse reçois un ti truc");
            // don't allow player to add items which has zero amount or if it's summoned pet item
            if (slot.amount > 0 && !slot.item.summoned)
            {
                // swap them
                player.inventory.slots[fromIndex] = guildWarehouseItemSlot[toIndex];
                guildWarehouseItemSlot[toIndex] = slot;

                Debug.Log("de inventaire index "+ fromIndex + " à slot "+ toIndex + " , et il y a "+ slot.amount + " item nomé "+ slot.item.name);
                Database.singleton.SetLogGuildWarehouseSlotMove(player, toIndex, slot.item.name, slot.amount, 1);
                Database.singleton.SaveGuildWarehouse(player);

                //player.playerAddonsConfigurator.Cmd_LoadGuildWarehouse(); // Force refresh inventory warehouse
                UI_UpdateGuildWarehouse(player.connectionToClient);

                ForceReloadGuilWarehouseData();
            }
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    //  Swap Warehouse to inventory
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_Swap_WarehouseToInventory(int fromIndex, int toIndex)
    {
#if _SERVER
        if ((player.state == "IDLE" || player.state == "MOVING" || player.state == "CASTING") && 0 <= fromIndex && fromIndex < guildWarehouseItemSlot.Count && 0 <= toIndex && toIndex < player.inventory.slots.Count)
        {
            // swap them
            ItemSlot temp = guildWarehouseItemSlot[fromIndex];
            guildWarehouseItemSlot[fromIndex] = player.inventory.slots[toIndex];
            player.inventory.slots[toIndex] = temp;

            Database.singleton.SaveGuildWarehouse(player);
            Database.singleton.SetLogGuildWarehouseSlotMove(player, fromIndex, temp.item.name, temp.amount, 0);
            ForceReloadGuilWarehouseData();
            UI_UpdateGuildWarehouse(player.connectionToClient);
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    //  Swap Warehouse to Warehouse
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_Swap_WarehouseToWarehouse(int fromIndex, int toIndex)
    {
#if _SERVER
        if ((player.state == "IDLE" || player.state == "MOVING" || player.state == "CASTING") &&
            0 <= fromIndex && fromIndex < guildWarehouseItemSlot.Count &&
            0 <= toIndex && toIndex < guildWarehouseItemSlot.Count &&
            fromIndex != toIndex)
        {
            // swap them
            ItemSlot temp = guildWarehouseItemSlot[fromIndex];
            guildWarehouseItemSlot[fromIndex] = guildWarehouseItemSlot[toIndex];
            guildWarehouseItemSlot[toIndex] = temp;

            Database.singleton.SaveGuildWarehouse(player);
            Database.singleton.SetLogGuildWarehouseSlotMove(player, fromIndex, temp.item.name, temp.amount, 2);
            ForceReloadGuilWarehouseData();
            UI_UpdateGuildWarehouse(player.connectionToClient);
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_WarehouseSplit(int fromIndex, int toIndex)
    {
#if _SERVER
        if ((player.state == "IDLE" || player.state == "MOVING" || player.state == "CASTING") &&
            0 <= fromIndex && fromIndex < guildWarehouseItemSlot.Count &&
            0 <= toIndex && toIndex < guildWarehouseItemSlot.Count &&
            fromIndex != toIndex)
        {
            // slotFrom has to have an entry, slotTo has to be empty
            ItemSlot slotFrom = guildWarehouseItemSlot[fromIndex];
            ItemSlot slotTo = guildWarehouseItemSlot[toIndex];

            // from entry needs at least amount of 2
            if (slotFrom.amount >= 2 && slotTo.amount == 0)
            {
                // split them serversided (has to work for even and odd)
                slotTo = slotFrom;

                slotTo.amount = slotFrom.amount / 2;
                slotFrom.amount -= slotTo.amount; // works for odd too

                // put back into the list
                guildWarehouseItemSlot[fromIndex] = slotFrom;
                guildWarehouseItemSlot[toIndex] = slotTo;

                Database.singleton.SaveGuildWarehouse(player);
                Database.singleton.SetLogGuildWarehouseSlotMove(player, fromIndex, slotFrom.item.name, slotFrom.amount, 3);
                ForceReloadGuilWarehouseData();
                UI_UpdateGuildWarehouse(player.connectionToClient);
            }
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_WarehouseMerge(int fromIndex, int toIndex)
    {
#if _SERVER
        if ((player.state == "IDLE" || player.state == "MOVING" || player.state == "CASTING") &&
            0 <= fromIndex && fromIndex < guildWarehouseItemSlot.Count &&
            0 <= toIndex && toIndex < guildWarehouseItemSlot.Count &&
            fromIndex != toIndex)
        {
            // both items have to be valid
            ItemSlot slotFrom = guildWarehouseItemSlot[fromIndex];
            ItemSlot slotTo = guildWarehouseItemSlot[toIndex];

            if (slotFrom.amount > 0 && slotTo.amount > 0)
            {
                // check if the both items are the same type
                if (slotFrom.item.Equals(slotTo.item))
                {
                    int put = slotTo.IncreaseAmount(slotFrom.amount);
                    slotFrom.DecreaseAmount(put);

                    // put back into the list
                    guildWarehouseItemSlot[fromIndex] = slotFrom;
                    guildWarehouseItemSlot[toIndex] = slotTo;

                    Database.singleton.SaveGuildWarehouse(player);
                    Database.singleton.SetLogGuildWarehouseSlotMove(player, fromIndex, slotFrom.item.name, slotFrom.amount, 4);
                    ForceReloadGuilWarehouseData();
                    UI_UpdateGuildWarehouse(player.connectionToClient);
                }
            }
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_GuildWarehouseDepositGold(int amount)
    {
#if _SERVER
        GuildWarehouseAddGold(amount);
        UI_UpdateGuildWarehouse(player.connectionToClient);
#endif
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_GuildWarehouseWithdrawGold(int amount)
    {
#if _SERVER
        GuildWarehouseRemoveGold(amount);
        UI_UpdateGuildWarehouse(player.connectionToClient);
#endif
    }

    // -----------------------------------------------------------------------------------
    // Cmd_UpgradeGuildWarehouse
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_UpgradeGuildWarehouse()
    {
#if _SERVER
        UpgradeGuildWarehouse();
        UI_UpdateGuildWarehouse(player.connectionToClient);
#endif
    }


    // -----------------------------------------------------------------------------------
    // HasEnoughGoldSpace
    // -----------------------------------------------------------------------------------
    public bool HasEnoughGoldSpace(long amount = 1)
    {
        return amount > 0 && guildWarehouseStorageGold >= guildWarehouseGold + amount;
    }

    // -----------------------------------------------------------------------------------
    // CanUpgradeGuildWarehouse
    // @Client OR @Server
    // -----------------------------------------------------------------------------------
    public bool CanUpgradeGuildWarehouse()
    {
        return (guildWarehouseConfiguration.guildWarehouseUpgradeCost.Length > 0 && guildWarehouseConfiguration.guildWarehouseUpgradeCost.Length > guildWarehouseLevel && guildWarehouseConfiguration.guildWarehouseUpgradeCost[guildWarehouseLevel].CheckCost(player));
    }

    // -----------------------------------------------------------------------------------
    // CanAccessGuildWarehouse
    // @Client OR @Server
    // -----------------------------------------------------------------------------------
    public bool CanAccessGuildWarehouse()
    {
        return (player.isAlive && (player.guild.InGuild() /*&& player.guild.guild.CanNotify(name)*/)
                );
    }





    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    private void OnDragAndDrop_GuildWarehouseSlot_GuildWarehouseSlot(int[] slotIndices)
    {
        // merge? (just check the name, rest is done server sided)
        if (guildWarehouseItemSlot[slotIndices[0]].amount > 0 && guildWarehouseItemSlot[slotIndices[1]].amount > 0 &&
            guildWarehouseItemSlot[slotIndices[0]].item.Equals(guildWarehouseItemSlot[slotIndices[1]].item))
        {
            Cmd_WarehouseMerge(slotIndices[0], slotIndices[1]);
            // split?
        }
        else if (Utils.AnyKeyPressed(player.inventory.splitKeys))
        {
            Cmd_WarehouseSplit(slotIndices[0], slotIndices[1]);
            // swap?
        }
        else
        {
            Cmd_Swap_WarehouseToWarehouse(slotIndices[0], slotIndices[1]);
        }
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    private void OnDragAndDrop_GuildWarehouseSlot_InventorySlot(int[] slotIndices)
    {
        if (guildWarehouseItemSlot[slotIndices[0]].amount > 0)
        {
            Cmd_Swap_WarehouseToInventory(slotIndices[0], slotIndices[1]);
        }
    }

    // -----------------------------------------------------------------------------------
    // 
    // -----------------------------------------------------------------------------------
    private void OnDragAndDrop_InventorySlot_GuildWarehouseSlot(int[] slotIndices)
    {
        if (player.inventory.slots[slotIndices[0]].amount > 0)
        {
            Cmd_Swap_InventoryToWarehouse(slotIndices[0], slotIndices[1]);
        }
    }

    [TargetRpc]
    private void UI_UpdateGuildWarehouse(NetworkConnection target)
    {
        guildWarehouseConfiguration.uiEventGuildWarehouse.TriggerEventBool(true);
    }

#if _SERVER
    // -----------------------------------------------------------------------------------

    // -----------------------------------------------------------------------------------
    // WarehouseAddGold
    // @Server
    // -----------------------------------------------------------------------------------
    [Server]
    public void GuildWarehouseAddGold(long amount)
    {
        if (GuildHasEnoughGoldOnInventory(amount) && HasEnoughGoldSpace(amount))
        {
            player.gold -= amount;
            guildWarehouseGold += amount;
            Database.singleton.SaveGuildWarehouse(player);
            ForceReloadGuilWarehouseData();
            Database.singleton.SetLogGuildWarehouseGold(player, amount, true);
        }
    }
    // -----------------------------------------------------------------------------------
    // UpgradePlayerWarehouse
    // @Server
    // -----------------------------------------------------------------------------------
    [Server]
    public void UpgradeGuildWarehouse()
    {
        if (CanUpgradeGuildWarehouse())
        {
            guildWarehouseConfiguration.guildWarehouseUpgradeCost[guildWarehouseLevel].PayCost(player);

            int oldSize = guildWarehouseStorageItems;
            guildWarehouseLevel++;

            int sizeDifference = guildWarehouseStorageItems - oldSize;

            for (int i = 0; i < sizeDifference; ++i)
            {
                guildWarehouseItemSlot.Add(new ItemSlot());
            }

            Database.singleton.SaveGuildWarehouse(player);
            Database.singleton.SetLogGuildWarehouseUpgrade(player, guildWarehouseLevel);
            ForceReloadGuilWarehouseData();

            player.Tools_ShowPopup(guildWarehouseConfiguration.guildWarehouseUpgradeLabel);
        }
    }

    // -----------------------------------------------------------------------------------
    // HasEnoughGoldOnInventory
    // -----------------------------------------------------------------------------------
    [Server]
    public bool GuildHasEnoughGoldOnInventory(long amount)
    {
        return amount > 0 && player.gold >= amount;
    }

    // -----------------------------------------------------------------------------------
    // HasEnoughGoldOnWarehouse
    // -----------------------------------------------------------------------------------
    [Server]
    public bool HasEnoughGoldOnGuildWarehouse(long amount)
    {
        return amount > 0 && guildWarehouseGold >= amount;
    }

    // -----------------------------------------------------------------------------------
    // WarehouseRemoveGold
    // @Server
    // -----------------------------------------------------------------------------------
    [Server]
    public void GuildWarehouseRemoveGold(long amount)
    {
        if (HasEnoughGoldOnGuildWarehouse(amount))
        {
            guildWarehouseGold -= amount;
            player.gold += amount;
            Database.singleton.SaveGuildWarehouse(player);
            Database.singleton.SetLogGuildWarehouseGold(player, amount, false);
            ForceReloadGuilWarehouseData();
        }
    }

    [Server]
    private void ForceReloadGuilWarehouseData()
    {
        foreach (GuildMember plyer in player.guild.guild.members)
        {
            if (plyer.online)
            {
                Player.onlinePlayers.TryGetValue(plyer.name, out Player other);
                 
                // transmition à tout les connecté de la guilde, et qui regarde le Guild Warehouse des information sur les dernière mise à jour
                if (player.name != other.name)
                {

                    other.playerAddonsConfigurator.Cmd_LoadGuildWarehouseServer();
                    //other.playerAddonsConfigurator.ResetGuildWarehouse();
                    /*other.playerAddonsConfigurator.guildWarehouseGold = player.playerAddonsConfigurator.guildWarehouseGold;
                    other.playerAddonsConfigurator.guildWarehouseLevel = player.playerAddonsConfigurator.guildWarehouseLevel;
                    other.playerAddonsConfigurator.guildWarehouseItemSlot.Clear();
                    
                    for (int i = 0; i < player.playerAddonsConfigurator.guildWarehouseStorageItems; ++i)
                    {
                        other.playerAddonsConfigurator.guildWarehouseItemSlot.Add(player.playerAddonsConfigurator.guildWarehouseItemSlot[i]);
                    }
                    other.playerAddonsConfigurator.UI_UpdateGuildWarehouse(other.connectionToClient);
                    */
                }
            }
        }
    }

#endif
}
