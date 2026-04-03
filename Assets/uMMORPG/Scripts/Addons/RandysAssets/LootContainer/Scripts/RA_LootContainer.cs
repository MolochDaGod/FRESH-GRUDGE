using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "uMMORPG Item/Randy's Assets/Loot Container", order = 999)]
public class RA_LootContainer : UsableItem {
    [Header("Randy's Assets - Loot Container")]
    public List<RA_ContainerItem> lootContainer = new List<RA_ContainerItem>();

    [TextArea] public string Notes = "Do not set stack sizes higher than items max sack size or items will be lost if there is not enough inventory space.";

    public override void Use(Player player, int inventoryIndex) {
        base.Use(player, inventoryIndex);

        if (lootContainer.Count > 0) {
            List<RA_ContainerItem> containerItems = new List<RA_ContainerItem>();

            foreach(RA_ContainerItem containerItem in lootContainer) {
                if(Random.value <= containerItem.Probability) {
                    switch(containerItem.ItemType) {
                        case RA_ContainerItemType.Item:
                            containerItems.Add(new RA_ContainerItem(containerItem.ItemType, containerItem.ScriptableItem, Random.Range(containerItem.StackSizeMin, (containerItem.StackSizeMax + 1))));
                            break;
                        case RA_ContainerItemType.Gold:
                            containerItems.Add(new RA_ContainerItem(containerItem.ItemType, Random.Range(containerItem.GoldAmountMin, (containerItem.GoldAmountMax + 1))));
                            break;
                        case RA_ContainerItemType.Coins:
                            containerItems.Add(new RA_ContainerItem(containerItem.ItemType, Random.Range(containerItem.CoinsAmountMin, (containerItem.CoinsAmountMax + 1))));
                            break;
                    }
                }
            }

            if(containerItems.Count > 0) {
                if(player.inventory.SlotsFree() >= containerItems.Count) {

                    foreach(RA_ContainerItem containerItem in containerItems) {
                        switch(containerItem.ItemType) {
                            case RA_ContainerItemType.Item:
                                player.inventory.Add(new Item(containerItem.ScriptableItem), containerItem.Amount);
                                player.chat.TargetMsgInfo("You receive: " + containerItem.Amount + "x " + containerItem.ScriptableItem.name);
                                break;
                            case RA_ContainerItemType.Gold:
                                player.gold += containerItem.Amount;
                                player.chat.TargetMsgInfo("You receive: " + containerItem.Amount + " Gold");
                                break;
                            case RA_ContainerItemType.Coins:
                                player.itemMall.coins += containerItem.Amount;
                                player.chat.TargetMsgInfo("You receive: " + containerItem.Amount + " Coins");
                                break;
                        }
                    }

                    ItemSlot slot = player.inventory.slots[inventoryIndex];
                    slot.DecreaseAmount(1);
                    player.inventory.slots[inventoryIndex] = slot;
                }
            }
        }
    }

    public override string ToolTip() {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        string containerTip = "";

        for(int cnt = 0; cnt < lootContainer.Count; cnt++) {
            if(lootContainer[cnt].ItemType == RA_ContainerItemType.Item) {
                if(lootContainer[cnt].StackSizeMin == lootContainer[cnt].StackSizeMax) {
                    containerTip += lootContainer[cnt].StackSizeMax + "x " + lootContainer[cnt].ScriptableItem.name + " (" + (lootContainer[cnt].Probability * 100) + "%)";
                } else {
                    containerTip += "[" + lootContainer[cnt].StackSizeMin + " - " + lootContainer[cnt].StackSizeMax + "]x " + lootContainer[cnt].ScriptableItem.name + " (" + (lootContainer[cnt].Probability * 100) + "%)";
                }
            } else if(lootContainer[cnt].ItemType == RA_ContainerItemType.Coins) {
                if(lootContainer[cnt].CoinsAmountMin == lootContainer[cnt].CoinsAmountMax) {
                    containerTip += lootContainer[cnt].CoinsAmountMax + "x Coins (" + (lootContainer[cnt].Probability * 100) + "%)";
                } else {
                    containerTip += "[" + lootContainer[cnt].CoinsAmountMin + " - " + lootContainer[cnt].CoinsAmountMax + "]x Coins (" + (lootContainer[cnt].Probability * 100) + "%)";
                }
            } else if(lootContainer[cnt].ItemType == RA_ContainerItemType.Gold) {
                if (lootContainer[cnt].GoldAmountMin == lootContainer[cnt].GoldAmountMax) {
                    containerTip += lootContainer[cnt].GoldAmountMax + "x Gold (" + (lootContainer[cnt].Probability * 100) + "%)";
                } else {
                    containerTip += "[" + lootContainer[cnt].GoldAmountMin + " - " + lootContainer[cnt].GoldAmountMax + "]x Gold (" + (lootContainer[cnt].Probability * 100) + "%)";
                }
            }

            if(cnt + 1 != lootContainer.Count) containerTip += "\n";
        }

        tip.Replace("{REWARDS}", containerTip);

        return tip.ToString();
    }
}
