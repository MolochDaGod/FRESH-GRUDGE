using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

#if _iMMOCHEST
// 	UI LOOTCRATE
public partial class UI_Lootcrate : MonoBehaviour
{
    public static UI_Lootcrate singleton;
    public GameObject panel;
    public GameObject goldSlot;
    public Text goldText;
    public GameObject coinSlot;
    public Text coinText;
    public UILootSlot itemSlotPrefab;
    public Transform content;

    // -----------------------------------------------------------------------------------
    // UI_Lootcrate
    // -----------------------------------------------------------------------------------
    public UI_Lootcrate()
    {
        // assign singleton only once (to work with DontDestroyOnLoad when
        // using Zones / switching scenes)
        if (singleton == null) singleton = this;
    }

    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    private void Update()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        if (panel.activeSelf &&
            player.playerAddonsConfigurator.LootcrateValidation()
             )
        {
            if (!player.playerAddonsConfigurator.selectedLootcrate.HasLoot())
            {
                Hide();
                return;
            }

            // -- gold slot
            if (player.playerAddonsConfigurator.selectedLootcrate.gold > 0)
            {
                goldSlot.SetActive(true);
                goldSlot.GetComponentInChildren<Button>().onClick.SetListener(() =>
                {
                    player.playerAddonsConfigurator.Cmd_TakeLootcrateGold();
                });
                goldText.text = player.playerAddonsConfigurator.selectedLootcrate.gold.ToString();
            }
            else goldSlot.SetActive(false);

            // -- coin slot
            if (player.playerAddonsConfigurator.selectedLootcrate.coins > 0)
            {
                coinSlot.SetActive(true);
                coinSlot.GetComponentInChildren<Button>().onClick.SetListener(() =>
                {
                    player.playerAddonsConfigurator.Cmd_TakeLootcrateCoins();
                });
                coinText.text = player.playerAddonsConfigurator.selectedLootcrate.coins.ToString();
            }
            else coinSlot.SetActive(false);

            // instantiate/destroy enough slots
            // (we only want to show the non-empty slots)
            List<ItemSlot> items = player.playerAddonsConfigurator.selectedLootcrate.inventory.Where(slot => slot.amount > 0).ToList();
            UIUtils.BalancePrefabs(itemSlotPrefab.gameObject, items.Count, content);

            // refresh all valid items
            for (int i = 0; i < items.Count; ++i)
            {
                UILootSlot slot = content.GetChild(i).GetComponent<UILootSlot>();
                slot.dragAndDropable.name = i.ToString(); // drag and drop index
                int itemIndex = player.playerAddonsConfigurator.selectedLootcrate.inventory.FindIndex(
                    // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                    itemSlot => itemSlot.amount > 0 && itemSlot.item.Equals(items[i].item)
                );

                // refresh
                slot.button.interactable = player.inventory.CanAdd(items[i].item, items[i].amount);
                slot.button.onClick.SetListener(() =>
                {
                    player.playerAddonsConfigurator.Cmd_TakeLootcrateItem(itemIndex);
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

        if (player.playerAddonsConfigurator.LootcrateValidation() && player.playerAddonsConfigurator.selectedLootcrate.HasLoot())
            panel.SetActive(true);
    }

    // -----------------------------------------------------------------------------------
    // Hide
    // -----------------------------------------------------------------------------------
    public void Hide(bool cancel = true)
    {
        Player player = Player.localPlayer;
        if (!player) return;

        if (cancel)
            player.playerAddonsConfigurator.CancelLootcrate();

        panel.SetActive(false);
    }
    // -----------------------------------------------------------------------------------
}
#endif