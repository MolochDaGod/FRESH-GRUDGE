using UnityEngine.UI;
using UnityEngine;

public partial class Loot_All : MonoBehaviour
{
    public Button lootAllBtn;
    private UILoot loot;
    private int invFull;

    private void Start()
    {
        loot = GetComponent<UILoot>();
    }

    public void LootAll()
    {
        Player player = Player.localPlayer;
        var items = ((Monster)player.target).inventory.slots;
         //  var items = player.target.inventory.Where(item => item.amount > 0).ToList();
        invFull = 0;
        // refresh all valid items
        for (int i = 0; i < items.Count; ++i)
        {
            if (loot.content.childCount > 0)
            {
                UILootSlot slot = loot.content.GetChild(i).GetComponent<UILootSlot>();
                slot.dragAndDropable.name = i.ToString(); // drag and drop index
                                                          // int itemMailIndex = player.target.inventory.FindIndex(item => item.amount > 0 && item.item.name == items[i].item.name);
                                                          // add a check for each item (we cannot loot all if we dont have enough space in our inventory
                if (player.inventory.CanAdd(items[i].item, items[i].amount)) { invFull++; }
                else { invFull--; }

                if (invFull == items.Count) { lootAllBtn.interactable = true; }
                else { lootAllBtn.interactable = false; }

                lootAllBtn.onClick.RemoveAllListeners();
                lootAllBtn.onClick.SetListener(() => { player.TakeAllLootItem(); });
            }
        }
    }
}