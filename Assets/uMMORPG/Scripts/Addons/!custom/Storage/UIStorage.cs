using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UIStorage : MonoBehaviour
{
    public GameObject panel;
    public UIStorageSlot slotPrefab;
    public Transform content;

    [Header("Durability Colors")]
    public Color brokenDurabilityColor = Color.red;
    public Color lowDurabilityColor = Color.magenta;
    [Range(0.01f, 0.99f)] public float lowDurabilityThreshold = 0.1f;

    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            if (player.storage != null && Vector3.Distance(player.transform.position, player.storage.transform.position) <= player.interactionRange)
            {
                panel.SetActive(true);

                Storage storage = player.storage;

                // instantiate/destroy enough slots
                UIUtils.BalancePrefabs(slotPrefab.gameObject, storage.slots.Count, content);

                // refresh all items
                for (int i = 0; i < storage.slots.Count; ++i)
                {
                    UIStorageSlot slot = content.GetChild(i).GetComponent<UIStorageSlot>();
                    slot.dragAndDropable.name = i.ToString(); // drag and drop index
                    ItemSlot itemSlot = storage.slots[i];

                    if (itemSlot.amount > 0)
                    {
                        slot.tooltip.enabled = true;
                        // only build tooltip while it's actually shown. this
                        // avoids MASSIVE amounts of StringBuilder allocations.
                        if (slot.tooltip.IsVisible())
                            slot.tooltip.text = itemSlot.ToolTip();
                        slot.dragAndDropable.dragable = true;
                        // use durability colors?
                        if (itemSlot.item.maxDurability > 0)
                        {
                            if (itemSlot.item.durability == 0)
                                slot.image.color = brokenDurabilityColor;
                            else if (itemSlot.item.DurabilityPercent() < lowDurabilityThreshold)
                                slot.image.color = lowDurabilityColor;
                            else
                                slot.image.color = Color.white;
                        }
                        else slot.image.color = Color.white; // reset for non-durability items
                        slot.image.sprite = itemSlot.item.image;
                        slot.amountOverlay.SetActive(itemSlot.amount > 1);
                        slot.amountText.text = itemSlot.amount.ToString();
                        int scopy = i;
                        slot.button.onClick.SetListener(() =>
                        {
                            int to = name.ToInt();
                            for (int i = 0; i < player.inventory.slots.Count; i++)
                            {
                                if (player.inventory.slots[i].amount == 0 ||
                                    (player.inventory.slots[i].item.data == storage.slots[scopy].item.data &&
                                     player.inventory.slots[i].amount < player.inventory.slots[i].item.maxStack))
                                {
                                    to = i;
                                    break;
                                }
                            }
                            int from = slot.transform.GetChild(0).name.ToInt();
                            int[] indices = new int[] { from, to };
                            player.storageUsage.OnDragAndDrop_StorageSlot_InventorySlot(indices);
                        });
                    }
                    else
                    {
                        // refresh invalid item
                        slot.tooltip.enabled = false;
                        slot.dragAndDropable.dragable = false;
                        slot.image.color = Color.clear;
                        slot.image.sprite = null;
                        slot.amountOverlay.SetActive(false);
                    }
                }
            }
            else
            {
                player.storage = null;
                panel.SetActive(false);
            }
        }
    }
}
