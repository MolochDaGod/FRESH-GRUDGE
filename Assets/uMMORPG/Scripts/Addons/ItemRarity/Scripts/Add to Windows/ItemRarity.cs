using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemRarity : MonoBehaviour
{
    public enum SlotType { Equipment, Inventory, Loot, PlayerTrading, NpcTrading, ItemMall, Crafting }

    public SlotType slotType;

    //private int selfIndex, targetIndex;
    private Player player;
    private ItemSlot itemSlot;
    private ScriptableItem scriptItem;
    private RaritySlot raritySlot;
    private UIShowToolTip tooltip;


    // Assign our componenet based on the slot type.
    private void LateUpdate()
    {
        player = Player.localPlayer;
        if (player != null)
            switch (slotType)
            {
                case SlotType.Equipment:
                    // refresh all
                    int lastECount = 0;
                    if (lastECount != player.equipment.slots.Count)
                        for (int i = 0; i < player.equipment.slots.Count; ++i)
                        {
                            lastECount = player.equipment.slots.Count;
                            if (player.equipment.slots[i].amount > 0)
                            {
                                UIEquipment equipmentContents = gameObject.GetComponentInParent<UIEquipment>();
                                UIUtils.BalancePrefabs(equipmentContents.slotPrefab.gameObject, player.equipment.slots.Count, equipmentContents.content);
                                if (equipmentContents.panel.activeSelf)
                                {
                                    UIEquipmentSlot slot = equipmentContents.content.transform.GetChild(i).GetComponent<UIEquipmentSlot>();
                                    raritySlot = slot.GetComponent<RaritySlot>();
                                    tooltip = slot.tooltip;
                                    slot.dragAndDropable.name = i.ToString(); // drag and drop slot
                                    itemSlot = player.equipment.slots[i];
                                    raritySlot.rarityOutline.color = RarityColor.SetRarityColorResult(itemSlot.item);
                                }
                            }
                            else
                            {
                                UIEquipment equipmentContents = gameObject.GetComponentInParent<UIEquipment>();
                                if (equipmentContents.panel.activeSelf)
                                {
                                    UIEquipmentSlot slot = equipmentContents.content.transform.GetChild(i).GetComponent<UIEquipmentSlot>();
                                    raritySlot = slot.GetComponent<RaritySlot>();
                                    raritySlot.rarityOutline.color = Color.clear;
                                }
                            }
                        }
                    break;

                case SlotType.Inventory:
                    // refresh all
                    int lastICount = 0;
                    if (lastICount != player.inventory.slots.Count)
                        for (int i = 0; i < player.inventory.slots.Count; ++i)
                        {
                            lastICount = player.inventory.slots.Count;
                            if (player.inventory.slots[i].amount > 0)
                            {
                                UIInventory inventoryContents = GetComponentInParent<UIInventory>();
                                UIUtils.BalancePrefabs(inventoryContents.slotPrefab.gameObject, player.inventory.slots.Count, inventoryContents.content);
                                if (inventoryContents.panel.activeSelf)
                                {
                                    UIInventorySlot slot = inventoryContents.content.transform.GetChild(i).GetComponent<UIInventorySlot>();
                                    raritySlot = slot.GetComponent<RaritySlot>();
                                    tooltip = slot.tooltip;
                                    slot.dragAndDropable.name = i.ToString(); // drag and drop slot
                                    itemSlot = player.inventory.slots[i];
                                    raritySlot.rarityOutline.color = RarityColor.SetRarityColorResult(itemSlot.item);
                                }
                            }
                            else
                            {
                                UIInventory inventoryContents = gameObject.GetComponentInParent<UIInventory>();
                                if (inventoryContents.panel.activeSelf)
                                {
                                    UIInventorySlot slot = inventoryContents.content.transform.GetChild(i).GetComponent<UIInventorySlot>();
                                    raritySlot = slot.GetComponent<RaritySlot>();
                                    raritySlot.rarityOutline.color = Color.clear;
                                }
                            }
                        }
                    break;

                case SlotType.Loot:
                    if (player.target != null && player.target.health.current <= 0)
                    {
                        UILoot lootContent = GetComponentInParent<UILoot>();
                        List<ItemSlot> items = ((Monster)player.target).inventory.slots.Where(slot => slot.amount > 0).ToList();
                        UIUtils.BalancePrefabs(lootContent.itemSlotPrefab.gameObject, items.Count, lootContent.content);

                        // refresh all valid items
                        for (int i = 0; i < items.Count; ++i)
                        {
                            UILootSlot slot = lootContent.content.GetChild(i).GetComponent<UILootSlot>();
                            slot.dragAndDropable.name = i.ToString(); // drag and drop index
                            int itemIndex = ((Monster)player.target).inventory.slots.FindIndex(
                                // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                                itemSlot => itemSlot.amount > 0 && itemSlot.item.Equals(items[i].item)
                            );

                            // refresh
                            raritySlot = slot.GetComponent<RaritySlot>();
                            tooltip = slot.tooltip;
                            slot.dragAndDropable.name = i.ToString(); // drag and drop slot
                            itemSlot = items[i];
                            raritySlot.rarityOutline.color = RarityColor.SetRarityColorResult(itemSlot.item.data);
                        }
                    }
                    break;

                case SlotType.PlayerTrading:
                    if (player.state == "TRADING")
                    {
                        Player other = (Player)player.target;
                        int lastPTYCount = 0;
                        if (lastPTYCount != player.trading.offerItems.Count)
                            for (int i = 0; i < player.trading.offerItems.Count; ++i)
                            {
                                lastPTYCount = player.trading.offerItems.Count;
                                UIPlayerTrading tradeContents = GetComponentInParent<UIPlayerTrading>();
                                UIUtils.BalancePrefabs(tradeContents.slotPrefab.gameObject, player.trading.offerItems.Count, tradeContents.myContent);
                                if (tradeContents.panel.activeSelf)
                                {
                                    UIPlayerTradingSlot slot = tradeContents.myContent.transform.GetChild(i).GetComponent<UIPlayerTradingSlot>();
                                    if (slot.amountText.text != "0")
                                    {
                                        raritySlot = slot.GetComponent<RaritySlot>();
                                        tooltip = slot.tooltip;
                                        slot.dragAndDropable.name = i.ToString(); // drag and drop slot
                                        int inventoryIndex = player.trading.offerItems[i];
                                        itemSlot = player.inventory.slots[inventoryIndex];
                                        raritySlot.rarityOutline.color = RarityColor.SetRarityColorResult(itemSlot.item.data);
                                    }
                                    else
                                    {
                                        raritySlot = slot.GetComponent<RaritySlot>();
                                        raritySlot.rarityOutline.color = Color.clear;
                                    }
                                }
                            }

                        int lastPTOCount = 0;
                        if (lastPTOCount != other.trading.offerItems.Count)
                            for (int i = 0; i < other.trading.offerItems.Count; ++i)
                            {
                                lastPTOCount = other.trading.offerItems.Count;
                                UIPlayerTrading tradeContents = GetComponentInParent<UIPlayerTrading>();
                                UIUtils.BalancePrefabs(tradeContents.slotPrefab.gameObject, other.trading.offerItems.Count, tradeContents.otherContent);
                                if (tradeContents.panel.activeSelf)
                                {
                                    UIPlayerTradingSlot slot = tradeContents.otherContent.transform.GetChild(i).GetComponent<UIPlayerTradingSlot>();
                                    if (slot.amountText.text != "0")
                                    {
                                        raritySlot = slot.GetComponent<RaritySlot>();
                                        tooltip = slot.tooltip;
                                        slot.dragAndDropable.name = i.ToString(); // drag and drop slot
                                        int inventoryIndex = other.trading.offerItems[i];
                                        itemSlot = other.inventory.slots[inventoryIndex];
                                        raritySlot.rarityOutline.color = RarityColor.SetRarityColorResult(itemSlot.item.data);
                                    }
                                    else
                                    {
                                        raritySlot = slot.GetComponent<RaritySlot>();
                                        raritySlot.rarityOutline.color = Color.clear;
                                    }
                                }
                            }
                    }
                    break;

                case SlotType.NpcTrading:
                    if (player.target is Npc)
                    {
                        Npc npTradingc = (Npc)player.target;

                        int lastNTCount = 0;
                        if (lastNTCount != npTradingc.trading.saleItems.Length)
                            for (int i = 0; i < npTradingc.trading.saleItems.Length; ++i)
                            {
                                lastNTCount = npTradingc.trading.saleItems.Length;
                                UINpcTrading npcContents = GetComponent<UINpcTrading>();
                                UIUtils.BalancePrefabs(npcContents.slotPrefab.gameObject, npTradingc.trading.saleItems.Length, npcContents.content);
                                if (npcContents.panel.activeSelf)
                                {
                                    UINpcTradingSlot slot = npcContents.content.transform.GetChild(i).GetComponent<UINpcTradingSlot>();
                                    raritySlot = slot.GetComponent<RaritySlot>();
                                    tooltip = slot.tooltip;
                                    scriptItem = npTradingc.trading.saleItems[i];
                                    raritySlot.rarityOutline.color = RarityColor.SetRarityColorResult(scriptItem);
                                }
                            }
                    }
                    break;

                case SlotType.ItemMall:
                    UIItemMall mallContents = GetComponentInParent<UIItemMall>();
                    if (mallContents.panel.activeSelf)
                    {
                        ScriptableItem[] items = player.itemMall.config.categories[mallContents.currentCategory].items;
                        UIUtils.BalancePrefabs(mallContents.itemSlotPrefab.gameObject, items.Length, mallContents.itemContent);

                        int lastIMCount = 0;
                        int currentPage = 0;
                        if (lastIMCount != items.Length || currentPage != mallContents.currentCategory)
                            for (int i = 0; i < items.Length; ++i)
                            {
                                lastIMCount = items.Length;
                                currentPage = mallContents.currentCategory;
                                UIItemMallSlot slot = mallContents.itemContent.GetChild(i).GetComponent<UIItemMallSlot>();
                                raritySlot = slot.GetComponent<RaritySlot>();
                                tooltip = slot.tooltip;
                                scriptItem = items[i];
                                raritySlot.rarityOutline.color = RarityColor.SetRarityColorResult(scriptItem);
                            }
                    }
                    break;

                case SlotType.Crafting:
                    UICrafting craftContents = GetComponentInParent<UICrafting>();
                    UIUtils.BalancePrefabs(craftContents.ingredientSlotPrefab.gameObject, player.crafting.indices.Count, craftContents.ingredientContent);
                    if (craftContents.panel.activeSelf)
                    {
                        int lastCCount = 0;
                        if (lastCCount != player.crafting.indices.Count)
                            for (int i = 0; i < player.crafting.indices.Count; ++i)
                            {
                                lastCCount = player.crafting.indices.Count;
                                UICraftingIngredientSlot slot = craftContents.ingredientContent.GetChild(i).GetComponent<UICraftingIngredientSlot>();
                                if (player.crafting.indices[i] != -1)
                                {
                                    int itemIndex = player.crafting.indices[i];
                                    raritySlot = slot.GetComponent<RaritySlot>();
                                    tooltip = slot.tooltip;
                                    itemSlot = player.inventory.slots[itemIndex];
                                    raritySlot.rarityOutline.color = RarityColor.SetRarityColorResult(itemSlot.item.data);
                                }
                                else
                                {
                                    raritySlot = slot.GetComponent<RaritySlot>();
                                    raritySlot.rarityOutline.color = Color.clear;
                                }
                            }
                    }
                    break;
            }
    }
}