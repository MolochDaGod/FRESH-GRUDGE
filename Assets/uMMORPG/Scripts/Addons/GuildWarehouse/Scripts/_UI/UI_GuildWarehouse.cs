using TMPro;
using UnityEngine;
using UnityEngine.UI;

// UI GUILD WAREHOUSE
public class UI_GuildWarehouse : MonoBehaviour
{
    public GameObject MainPanel;
    public GameObject panel;
    public UI_GuildWarehouseUpgradePanel upgradePanel;
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
    public Button logPanel;

    [Header("[LABELS]")]
    public string logsLabel = "Logs";
    public string depositLabel = "Deposit gold:";
    public string withdrawLabel = "Withdraw gold:";
    public string levelLabel = "Leve: ";

    private int bdc = 0;
    private int bwc = 0;


    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    private void UpdateEvent()
    {
#if _iMMOGUILDWAREHOUSE
        Player player = Player.localPlayer;
        if (!player) return;

        if (!player.guild.InGuild())
            Hide();

        //player.playerAddonsConfigurator.Cmd_LoadGuildWarehouse(); // Force refresh inventory warehouse

        // only update the panel if it's active
        if (panel.activeSelf)
        {
            // instantiate/destroy enough slots
            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.playerAddonsConfigurator.guildWarehouseItemSlot.Count, content);
            for (int i = 0; i < player.playerAddonsConfigurator.guildWarehouseItemSlot.Count; ++i)
            {
                UI_UniversalSlot slot = content.GetChild(i).GetComponent<UI_UniversalSlot>();
                slot.dragAndDropable.name = i.ToString(); // drag and drop index
                ItemSlot itemSlot = player.playerAddonsConfigurator.guildWarehouseItemSlot[i];

                if (itemSlot.amount > 0)
                {
#if _iMMOITEMRARITY
                    ScriptableItem itemData = player.playerAddonsConfigurator.guildWarehouseItemSlot[i].item.data;
                    slot.rarityOutline.color = RarityColor.SetRarityColorResult(player.playerAddonsConfigurator.guildWarehouseItemSlot[i].item);
#endif
                    slot.tooltip.enabled = true;
                    slot.tooltip.text = itemSlot.ToolTip();
                    slot.dragAndDropable.dragable = true;
                    slot.image.color = Color.white;
                    slot.image.sprite = itemSlot.item.image;
                    slot.amountOverlay.SetActive(itemSlot.amount > 1);
                    slot.amountText.text = itemSlot.amount.ToString();
                    slot.button.enabled = true;
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
            levelText.text = levelLabel + player.playerAddonsConfigurator.guildWarehouseLevel.ToString();

            // ----- gold
            goldText.text = player.playerAddonsConfigurator.guildWarehouseGold.ToString() + "/" + player.playerAddonsConfigurator.guildWarehouseStorageGold;

            
            
            goldInputPanel.interactable = player.playerAddonsConfigurator.CanAccessGuildWarehouse() && (player.gold > 0 || player.playerAddonsConfigurator.guildWarehouseGold > 0);

            buttonUpgrade.interactable = player.playerAddonsConfigurator.CanUpgradeGuildWarehouse();
            buttonUpgrade.onClick.SetListener(() =>
            {
                upgradePanel.Show();
            });

            buttonDeposit.interactable = player.playerAddonsConfigurator.CanAccessGuildWarehouse() && player.gold > 0 && player.playerAddonsConfigurator.HasEnoughGoldSpace();
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

            buttonWithdrawal.interactable = player.playerAddonsConfigurator.CanAccessGuildWarehouse() && player.playerAddonsConfigurator.guildWarehouseGold > 0;
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
                string amountValue = goldInputPanel.text;

                if (!string.IsNullOrWhiteSpace(amountValue))
                {
                    int amount = int.Parse(amountValue);

                    if ((bdc != 1 || bwc != 1) && amount < 1) return;

                    if (bdc == 1)
                    {
                        player.playerAddonsConfigurator.Cmd_GuildWarehouseDepositGold(amount);
                        //UpdateEvent();
                    }

                    if (bwc == 1)
                    {
                        player.playerAddonsConfigurator.Cmd_GuildWarehouseWithdrawGold(amount);
                        //UpdateEvent();
                    }

                    goldText.text = player.playerAddonsConfigurator.guildWarehouseGold.ToString();
                    goldInventoryText.text = player.gold.ToString();

                    bdc = 0;
                    bwc = 0;
                }

                goldInputPanel.text = "";

                goldInOutPanel.SetActive(false);
            });
        }
        else
        {
            Hide();
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
            player.playerAddonsConfigurator.Cmd_LoadGuildWarehouse();
            panel.SetActive(true);
            MainPanel.SetActive(true);
            UpdateEvent();
        }
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
#if _iMMOGUILDWAREHOUSE
            player.playerAddonsConfigurator.Cmd_CloseWarehouse();
#endif
            content.RemoveAllChildren();
        }
    }

    public void refreshUI(bool value)
    {
        Player player = Player.localPlayer;
        if (!player) return;
        if (value && panel.activeSelf)
        {
            player.playerAddonsConfigurator.Cmd_LoadGuildWarehouse();
            UpdateEvent();
        }
    }
    // -----------------------------------------------------------------------------------
}