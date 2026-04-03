using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if _iMMOHARVESTING

// 	UI HARVESTING

public partial class UI_HarvestingLoot : MonoBehaviour
{
    //public static UI_HarvestingLoot singleton;
    public GameObject panel;
    public UILootSlot itemSlotPrefab;
    public Transform content;

    // -----------------------------------------------------------------------------------
    // UI_HarvestingLoot
    // -----------------------------------------------------------------------------------
   /* public UI_HarvestingLoot()
    {
        // assign singleton only once (to work with DontDestroyOnLoad when
        // using Zones / switching scenes)
        if (singleton == null) singleton = this;
    }*/

    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    private void UpdateEvent()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        if (player.playerHarvesting.ResourceNodeValidation())
        {
            if (player.playerHarvesting.selectedNode == null || !player.playerHarvesting.ResourceNodeValidation() || !player.playerHarvesting.selectedNode.HasResources())
            {
                Hide();
                return;
            }

            List<ItemSlot> items = player.playerHarvesting.selectedNode.inventory.Where(slot => slot.amount > 0).ToList();
            UIUtils.BalancePrefabs(itemSlotPrefab.gameObject, items.Count, content);

            for (int i = 0; i < items.Count; ++i)
            {
                UILootSlot slot = content.GetChild(i).GetComponent<UILootSlot>();
                slot.dragAndDropable.name = i.ToString(); // drag and drop index
                int itemIndex = player.playerHarvesting.selectedNode.inventory.FindIndex(
                    itemSlot => itemSlot.amount > 0 && itemSlot.item.Equals(items[i].item)
                );

                slot.button.interactable = player.inventory.CanAdd(items[i].item, items[i].amount);
                slot.button.onClick.SetListener(() =>
                {
                    player.playerHarvesting.Cmd_TakeHarvestingResources(itemIndex);
                    UpdateEvent();
                });
                slot.tooltip.text = items[i].ToolTip();
                slot.image.color = Color.white;
                slot.image.sprite = items[i].item.image;
                slot.nameText.text = items[i].item.name;
                slot.amountOverlay.SetActive(items[i].amount > 1);
                slot.amountText.text = items[i].amount.ToString();
            }
        }
        else
        {
            panel.SetActive(false);
        }
    }

    // -----------------------------------------------------------------------------------
    // Show
    // -----------------------------------------------------------------------------------
    public void Show()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        if (player.playerHarvesting.ResourceNodeValidation())
        {
            panel.SetActive(true);
            UpdateEvent();
        }
    }

    // -----------------------------------------------------------------------------------
    // Hide
    // -----------------------------------------------------------------------------------
    public void Hide(bool cancel = true)
    {
        Player player = Player.localPlayer;
        if (!player) return;

        if (cancel)
            player.playerHarvesting.CancelResourceNode();

        panel.SetActive(false);
    }
    // -----------------------------------------------------------------------------------
}
#endif