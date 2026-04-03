using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class UI_PlayerWarehouseUpgradePanel : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text warehouseNextLevelText;
    public Button acceptButton;
    public Button declineButton;
    public Tools_UI_ToolsCostRequirementSlot requireSlot;
    public Transform requierementContent;

    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    private void OnEnable()
    {
#if _iMMOWAREHOUSE
        Player player = Player.localPlayer;
        if (!player) return;

        // use collider point(s) to also work with big entities
        if (player.playerAddonsConfigurator.playerWarehouseLevel < player.playerAddonsConfigurator.warehouseConfiguration.warehouseUpgradeCost.Length && !player.playerAddonsConfigurator.warehouseConfiguration.forceUseNpc || (player.target != null && player.target is Npc && Utils.ClosestDistance(player, player.target) <= player.interactionRange))
        {
            //long goldPrice = player.warehouseConfiguration.warehouseConfiguration.guildWarehouseUpgradeCost[player.warehouseConfiguration.playerWarehouseLevel].goldCost;
            //if (goldPrice > 0)
            //    guildWarehouseNextLevelText.text = goldPrice + " Gold";
            warehouseNextLevelText.text = (player.playerAddonsConfigurator.playerWarehouseLevel+1).ToString();


            List<ListCostValue> costListValue = player.playerAddonsConfigurator.warehouseConfiguration.warehouseUpgradeCost[player.playerAddonsConfigurator.playerWarehouseLevel].ListCost(player);
            bool costValiv = player.playerAddonsConfigurator.warehouseConfiguration.warehouseUpgradeCost[player.playerAddonsConfigurator.playerWarehouseLevel].CheckCost(player);
            UIUtils.BalancePrefabs(requireSlot.gameObject, costListValue.Count, requierementContent);
            for (int i = 0; i < costListValue.Count; i++)
            {
                Tools_UI_ToolsCostRequirementSlot slotRequierement = requierementContent.GetChild(i).GetComponent<Tools_UI_ToolsCostRequirementSlot>();
                slotRequierement.completed.text = costListValue[i].completed ? "Completed" : "Not Completed";
                slotRequierement.completed.color = costListValue[i].completed ? Color.green : Color.red;
                slotRequierement.requireAndAmount.text = costListValue[i].requireName + " (*" + costListValue[i].amout + ")";
            }

            acceptButton.interactable = player.playerAddonsConfigurator.CanUpgradePlayerWarehouse();
            acceptButton.onClick.SetListener(() =>
            {
                player.playerAddonsConfigurator.Cmd_UpgradePlayerWarehouse();
                player.playerAddonsConfigurator.warehouseConfiguration.uiEventPlayerWarehouse.TriggerEvent();
                panel.SetActive(false);
            });

            declineButton.onClick.SetListener(() =>
            {
                panel.SetActive(false);
            });
        }
        else panel.SetActive(false);
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
        }
    }

    // -----------------------------------------------------------------------------------
    // Show
    // -----------------------------------------------------------------------------------
    public void Hide()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        if (panel.activeSelf)
        {
            panel.SetActive(false);
        }
    }

    // -----------------------------------------------------------------------------------
}