using TMPro;
using UnityEngine;
using UnityEngine.UI;

// UI PLAYER WAREHOUSE
public class UI_PlayerWarehouse : MonoBehaviour
{
    public GameObject MainPanel;
    public GameObject panel;
    public UI_PlayerWarehouseUpgradePanel upgradePanel;
    public UI_UniversalSlot slotPrefab;
    public Transform content;
    public GameObject goldInOutPanel;
    public Button buttonUpgrade;
    public Button buttonDeposit;
    public Button buttonWithdrawal;
    public TMP_InputField goldInputPanel;
    public TMP_Text goldTextPlaceholder;
    public Button buttonAction;
    public TMP_Text levelText;
    public TMP_Text goldText;
    public TMP_Text goldInventoryText;

    [Header("[LABELS]")]
    public string depositLabel = "Deposit gold:";
    public string withdrawLabel = "Withdraw gold:";
    public string levelLabel = "L";

    private int bdc = 0;
    private int bwc = 0;

    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    private void UpdateEvent()
    {
#if _iMMOWAREHOUSE
        Player player = Player.localPlayer;

        if (!player) return;

        if (panel.activeSelf && !player.playerAddonsConfigurator.warehouseConfiguration.forceUseNpc || (player.target != null && player.target is Npc && Utils.ClosestDistance(player, player.target) <= player.interactionRange))
        {

            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.playerAddonsConfigurator.playerWarehouseItemSlot.Count, content);

            for (int i = 0; i < player.playerAddonsConfigurator.playerWarehouseItemSlot.Count; ++i)
            {
                UI_UniversalSlot slot = content.GetChild(i).GetComponent<UI_UniversalSlot>();
                slot.dragAndDropable.name = i.ToString(); // drag and drop index
                ItemSlot itemSlot = player.playerAddonsConfigurator.playerWarehouseItemSlot[i];

                if (itemSlot.amount > 0)
                {
#if _iMMOITEMRARITY
                    ScriptableItem itemData = player.playerAddonsConfigurator.playerWarehouseItemSlot[i].item.data;
                    slot.rarityOutline.color = RarityColor.SetRarityColorResult(player.playerAddonsConfigurator.playerWarehouseItemSlot[i].item);
#endif
                    slot.tooltip.enabled = true;
                    slot.tooltip.text = itemSlot.ToolTip();
                    slot.dragAndDropable.dragable = true;
                    slot.image.color = Color.white;
                    slot.image.sprite = itemSlot.item.image;
                    slot.amountOverlay.SetActive(itemSlot.amount > 1);
                    slot.amountText.text = itemSlot.amount.ToString();
                    slot.button.enabled = true;
                    slot.button.onClick.SetListener(() =>
                    {
                        int to = name.ToInt();
                        for (int i = 0; i < player.inventory.slots.Count; i++)
                        {
                            if (player.inventory.slots[i].amount == 0)
                            {
                                to = UIInventory.singleton.content.GetChild(i).GetChild(0).name.ToInt();
                                break;
                            }
                        }
                        int from = slot.transform.GetChild(0).name.ToInt();
                        int[] indices = new int[] { from, to };
                        player.playerAddonsConfigurator.OnDragAndDrop_WarehouseSlot_InventorySlot(indices);
                    });
#if _iMMOITEMLEVELUP
                    if (itemSlot.item.data is EquipmentItem equipmentItem && itemSlot.item.equipmentLevel > 0)
                    {
                        slot.equipmentLevelUpText.gameObject.SetActive(true);
                        slot.equipmentLevelUpText.text = equipmentItem ? "+" + itemSlot.item.equipmentLevel.ToString() : "";
                    }
#endif
                }
                else
                {
#if _iMMOITEMRARITY
                    slot.rarityOutline.color = Color.clear;
#endif
                    slot.button.onClick.RemoveAllListeners();
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                    slot.amountOverlay.SetActive(false);
#if _iMMOITEMLEVELUP
                    slot.equipmentLevelUpText.gameObject.SetActive(false); ;
#endif
                }
            }

            // ----- level
            levelText.text = levelLabel + (player.playerAddonsConfigurator.playerWarehouseLevel).ToString() + " / " + player.playerAddonsConfigurator.warehouseConfiguration.warehouseUpgradeCost.Length;

            // ----- gold
            goldText.text = player.playerAddonsConfigurator.playerWarehouseGold.ToString();

            goldInputPanel.interactable = player.isAlive && (player.gold > 0 || player.playerAddonsConfigurator.playerWarehouseGold > 0);

            buttonUpgrade.interactable = player.playerAddonsConfigurator.CanUpgradePlayerWarehouse();
            buttonUpgrade.onClick.SetListener(() =>
            {
                upgradePanel.Show();
            });

            buttonDeposit.interactable = player.isAlive && player.gold > 0 && player.playerAddonsConfigurator.HasEnoughPlayerWarehouseGoldSpace();
            buttonDeposit.onClick.SetListener(() =>
            {
                if (goldInOutPanel.activeInHierarchy)
                {
                    goldInOutPanel.SetActive(false);
                }

                bdc = (bdc > 0 ? 0 : 1);
                bwc = 0;

                if (bdc == 1)
                {
                    goldTextPlaceholder.text = depositLabel;
                    goldInOutPanel.SetActive(true);
                    goldInputPanel.ActivateInputField();
                }
            });

            buttonWithdrawal.interactable = player.isAlive && player.playerAddonsConfigurator.playerWarehouseGold > 0;
            buttonWithdrawal.onClick.SetListener(() =>
            {
                if (goldInOutPanel.activeInHierarchy)
                {
                    goldInOutPanel.SetActive(false);
                }

                bwc = (bwc > 0 ? 0 : 1);
                bdc = 0;

                if (bwc == 1)
                {
                    goldTextPlaceholder.text = withdrawLabel;
                    goldInOutPanel.SetActive(true);
                    goldInputPanel.ActivateInputField();
                }
            });

            buttonAction.onClick.SetListener(() =>
            {
                var amountValue = goldInputPanel.text;

                if (!string.IsNullOrWhiteSpace(amountValue))
                {
                    int amount = int.Parse(amountValue);

                    if ((bdc != 1 || bwc != 1) && amount < 1) return;

                    if (bdc == 1)
                    {
                        player.playerAddonsConfigurator.CmdDepositGold(amount);
                        player.playerAddonsConfigurator.warehouseConfiguration.uiEventPlayerWarehouse.TriggerEvent();
                    }

                    if (bwc == 1)
                    {
                        player.playerAddonsConfigurator.CmdWithdrawGold(amount);
                        player.playerAddonsConfigurator.warehouseConfiguration.uiEventPlayerWarehouse.TriggerEvent();
                    }

                    goldText.text = player.playerAddonsConfigurator.playerWarehouseGold.ToString();
                    //goldInventoryText.text = player.gold.ToString();

                    bdc = 0;
                    bwc = 0;
                }

                goldInputPanel.text = "";

                goldInOutPanel.SetActive(false);
            });
        }
        else
        {
            panel.SetActive(false);
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // Show
    // -----------------------------------------------------------------------------------
    public void Show()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        if (!panel.activeSelf)
        {
            panel.SetActive(true);
            MainPanel.SetActive(true);
        }
        UpdateEvent();
    }

    // -----------------------------------------------------------------------------------
    // Hide
    // -----------------------------------------------------------------------------------
    public void Hide()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        if (panel.activeSelf)
        {
            panel.SetActive(false);
            content.RemoveAllChildren();
        }
    }

    // -----------------------------------------------------------------------------------
}