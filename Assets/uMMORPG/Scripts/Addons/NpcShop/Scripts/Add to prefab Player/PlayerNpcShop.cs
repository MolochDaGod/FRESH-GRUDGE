using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerNpcShop : NetworkBehaviour
{

    [Header("Components")]
    public Player player;
    public PlayerInventory inventory;

    public List<string> shopItemsSold = new List<string>();

    // -----------------------------------------------------------------------------------
    // Cmd_NpcBuyItem
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_NpcBuyItem(int index, string currentCategory, int amount, int totalItems)
    {
#if _SERVER
        // le joueur à un target, le target et un npc et le target à un npcshop définie, le joueur et en IDLE et en vie et le NPC et dans le range d'intéraction
        if(player.target != null && player.target is Npc npc && npc.npcShop.shopTrading != null && player.isAlive && Utils.ClosestDistance(player, npc) <= player.interactionRange)
        {
            List<ScriptableItem> saleItems = new List<ScriptableItem>();

            ScriptableItem[] items = npc.npcShop.shopTrading.saleItems.Where(x => (x.itemCategory == currentCategory || currentCategory == "" || currentCategory == "All")).ToArray();
            saleItems.AddRange(items);
            
            // Si j'ai des item unique
            if (npc.npcShop.shopTrading.saleUniqueItems.Length > 0)
            {
                foreach (var uniqueItem in npc.npcShop.shopTrading.saleUniqueItems)
                {
                    // Si l'article unique appartient à la catégorie actuelle
                    if (uniqueItem.saleUniqueItems.itemCategory == currentCategory || currentCategory == "" || currentCategory == "All")
                    {
                        bool shouldAddUniqueItem = true;

                        // Vérifier si l'article unique est déjà vendu
                        if (uniqueItem.isUniqueWorldItem == true && npc.npcShop.shopItemsSold.Any(soldItem => soldItem.itemName == uniqueItem.saleUniqueItems.name))
                        {
                            // L'article unique est déjà vendu
                            shouldAddUniqueItem = false;
                        }
                        else if (uniqueItem.isUniqueWorldItem == false)
                        {
                            // L'article unique peut être vendu plusieurs fois, tant qu'il n'a pas déjà été acheté par le joueur
                            if (npc.npcShop.shopItemsSold.Any(soldItem => soldItem.itemName == uniqueItem.saleUniqueItems.name && soldItem.character == player.name))
                            {
                                // Le joueur a déjà acheté l'article unique
                                shouldAddUniqueItem = false;
                            }
                        }

                        // Ajouter l'article unique à la liste si nécessaire
                        if (shouldAddUniqueItem)
                        {
                            saleItems.Add(uniqueItem.saleUniqueItems);
                        }
                    }
                }
            }

            if (saleItems.Count <= totalItems)
            {

                if (0 <= index && index < saleItems.Count)
                {
                    Item npcItem = new Item(saleItems[index]);

                    if (1 <= amount && amount <= npcItem.maxStack)
                    {
                        long price = npcItem.buyPrice * amount;

                        // enough gold and enough space in inventory?
                        if (player.gold >= price && inventory.CanAdd(npcItem, amount))
                        {
                            // On est dans un vente unique
                            if (index > (items.Length - 1)) // -1 because index start 0
                            {
                                npc.npcShop.AddShopItemSold(new SoldUniqueItems { itemName = npcItem.name, character = player.name });
                                AddShopItemSold(npc.npcShop.shopTrading.name, npcItem.name);
                                Invoke(nameof(Rpc_RefreshList), 0.1f);
                            }
                            player.gold -= price;
                            inventory.Add(npcItem, amount);
                        }
                        else
                        {
                            if (player.gold < price)
                                player.chat.AddMsgInfo("you do not have enough gold on you to make this purchase.");
                            if (inventory.CanAdd(npcItem, amount))
                                player.chat.AddMsgInfo("you do not have enough slots in your inventory to make this purchase.");
                        }
                    }
                    else
                    {
                        Invoke(nameof(Rpc_RefreshList), 0.1f);
                    }
                }
            }
            else
            {
                Invoke(nameof(Rpc_RefreshList), 0.1f);
                player.chat.AddMsgInfo("A unique item has been sold in the meantime, please try again, but hurry it may no longer be available!");
                //player.Tools_AddMessage("A unique item has been sold in the meantime, please try again, but hurry it may no longer be available!");
            }
        }
        else
        {
            // Refresh list items
            Invoke(nameof(Rpc_RefreshList), 0.1f); // Refresh list 
        }
#endif
    }


    [Command]
    public void Cmd_SellItem(int index, int amount)
    {
#if _SERVER
        // validate: close enough, npc alive and valid index and valid item?
        // use collider point(s) to also work with big entities
        if (player.target != null && player.target.health.current > 0 && player.target is Npc npc && npc.npcShop != null && Utils.ClosestDistance(player, player.target) <= player.interactionRange && 0 <= index && index < inventory.slots.Count)
        {
            // sellable?
            ItemSlot slot = inventory.slots[index];
            if (slot.amount > 0 && slot.item.sellable && !slot.item.summoned)
            {
                // valid amount?
                if (1 <= amount && amount <= slot.amount)
                {
                    // sell the amount
                    long price = slot.item.sellPrice * amount;
                    player.gold += price;
                    slot.DecreaseAmount(amount);
                    inventory.slots[index] = slot;
                    Invoke(nameof(Rpc_RefreshList), 0.1f); // pour refresh chez le client une foi les opération faite (avec un delai pour amortir le temps réseau)
                }
            }
            else
            {
                player.chat.AddMsgInfo("you cannot sell an item that has been moved, an unsellable item, or a currently mounted mount");
            }
        }
#endif
    }

#if !_iMMO2D
    [Command]
    public void Cmd_RepairAllItems()
    {
#if _SERVER
        // validate: close enough, npc alive and valid index and valid item?
        // use collider point(s) to also work with big entities
        if (player.target != null &&
            player.target.health.current > 0 &&
            player.target is Npc npc &&
            npc.npcShop != null && // only if Npc offers trading

            npc.npcShop.shopTrading.offersRepair && // and repairs

            Utils.ClosestDistance(player, player.target) <= player.interactionRange)
        {
            // calculate missing durability from inventory + equipment
            int costInventory = ((npc.npcShop.shopTrading.repairInventory) ? player.inventory.GetTotalMissingDurability() : 0);
            int costEquipment = ((npc.npcShop.shopTrading.repairEquipment) ? player.equipment.GetTotalMissingDurability() : 0);
            int missing = costInventory + costEquipment;
            int price = missing * npc.npcShop.shopTrading.repairGoldCostPerDurabilityPoint;
            // calculate costs based on npc repair costs

            // don't allow negative price ever. just in case a calculation is
            // wrong, we don't want the player to get money back when repairing.
            if (price > 0)
            {
                // check if player has enough gold
                if (player.gold >= price)
                {
                    // repair all items
                    if (npc.npcShop.shopTrading.repairInventory)
                        player.inventory.RepairAllItems();
                    if (npc.npcShop.shopTrading.repairEquipment)
                        player.equipment.RepairAllItems();

                    // reduce gold
                    player.gold -= price;
                }
            }
        }
#endif
    }
#endif

    [TargetRpc]
    public void Rpc_RefreshList()
    {
        if (player.target is Npc npc)
        {
            GameEvent npcShopGameEvent = npc.npcShop.shopTrading.uiNpcShop;
            npcShopGameEvent.TriggerEvent();
        }
    }

#if _SERVER
    [Server]
    private void AddShopItemSold(string shopName, string itemName)
    {
        Database.singleton.AddSelledItem(shopName, player.name, itemName);
        //FindSelledItems(); // on fait la mise à jour de la synclist
    }

#endif
}
