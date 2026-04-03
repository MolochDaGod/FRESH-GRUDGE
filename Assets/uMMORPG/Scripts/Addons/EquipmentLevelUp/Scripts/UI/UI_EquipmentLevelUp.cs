using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class UI_EquipmentLevelUp : MonoBehaviour
{
    public GameObject panel;
    public UI_UniversalSlot itemSlot;
    public Tools_UI_ToolsCostRequirementSlot requireSlot;
    public TMP_Text itemName;
    public TMP_Text itemLevel;
    public TMP_Text uprgadeChance;
    public TMP_Text destroyChance;
    public Button equipmentLevelUpButton;
    public Transform requierementContent;
    public int itemIndex = -1;


    private void OnEnable()
    {
        itemIndex = -1;
        UpdateEvent();
    }

    public void UpdateItemIndex(int slotIndex = -1)
    {
        itemIndex = slotIndex;
        if(slotIndex == -1)
            requierementContent.RemoveAllChildren();
        UpdateEvent();
    }
    public void UpdateEvent()
    {
        Player player = Player.localPlayer;

        // use collider point(s) to also work with big entities
        if (player != null) //&&
            //player.target != null &&
            //player.target is Npc &&
            //Utils.ClosestDistance(player, player.target) <= player.interactionRange)
        {

            // Equipment level up
            if (itemIndex != -1 && itemIndex < player.inventory.slots.Count && player.inventory.slots[itemIndex].amount > 0 && player.inventory.slots[itemIndex].item.data is EquipmentItem itemData)
            {
                ItemSlot slot = player.inventory.slots[itemIndex];
                
#if _iMMOITEMRARITY
                //    ScriptableItem itemData = player.playerAddonsConfigurator.playerWarehouseItemSlot[i].item.data;
                itemSlot.rarityOutline.color = RarityColor.SetRarityColorResult(slot.item);
#endif
                itemSlot.tooltip.enabled = true;
                itemSlot.tooltip.text = slot.ToolTip();
                itemSlot.dragAndDropable.dragable = true;
                itemSlot.image.color = Color.white;
                itemSlot.image.sprite = slot.item.image;
                itemSlot.amountOverlay.SetActive(slot.amount > 1);
                itemSlot.amountText.text = slot.amount.ToString();
                itemSlot.button.enabled = true;
#if _iMMOITEMLEVELUP
                if (slot.item.data is EquipmentItem equipmentItem && slot.item.equipmentLevel > 0)
                {
                    itemSlot.equipmentLevelUpText.gameObject.SetActive(true);
                    itemSlot.equipmentLevelUpText.text = equipmentItem ? "+" + slot.item.equipmentLevel.ToString() : "";
                }
#endif

                // Afficher le nom de l'objet et son niveau d'équipement (s'il est supérieur à 0).
                itemName.text = slot.item.name + ((slot.item.equipmentLevel > 0) ? "+" + slot.item.equipmentLevel : "");

                // Afficher le niveau d'équipement de l'objet.
                itemLevel.text = slot.item.equipmentLevel.ToString();

                // Afficher la chance de mise à niveau en fonction des conditions.
                if (itemData.LevelUpParameters.Length > 0)
                {
                    if (slot.item.equipmentLevel < itemData.LevelUpParameters.Length )
                    {
                        // L'équipement a encore des paramètres de mise à niveau disponibles.
                        int successRate = itemData.LevelUpParameters[slot.item.equipmentLevel].successRate;
                        uprgadeChance.text = successRate.ToString() + "% success";

                        // Si l'objet a une chance de destruction activée, afficher la chance de destruction.
                        if (itemData.LevelUpParameters[slot.item.equipmentLevel].enableDestroyChance)
                        {
                            int destroyRate = itemData.LevelUpParameters[slot.item.equipmentLevel].destroyRate;
                            destroyChance.text = destroyRate.ToString() + "% destroy";
                        }
                        else
                        {
                            // L'objet n'est pas destructible.
                            destroyChance.text = "not destructible";
                        }


                        List<ListCostValue> costListValue = itemData.LevelUpParameters[slot.item.equipmentLevel].levelUpCost.ListCost(player);
                        bool costValiv = itemData.LevelUpParameters[slot.item.equipmentLevel].levelUpCost.CheckCost(player);
                        UIUtils.BalancePrefabs(requireSlot.gameObject, costListValue.Count, requierementContent);
                        for(int i = 0; i < costListValue.Count; i++)
                        {
                            Tools_UI_ToolsCostRequirementSlot slotRequierement = requierementContent.GetChild(i).GetComponent<Tools_UI_ToolsCostRequirementSlot>();
                            slotRequierement.completed.text = costListValue[i].completed ? "Completed" : "Not Completed";
                            slotRequierement.completed.color = costListValue[i].completed ?Color.green: Color.red;
                            slotRequierement.requireAndAmount.text = costListValue[i].requireName + " (*"+ costListValue[i].amout + ")";
                        }
                        //GameLog.LogMessage("<color=red> Valide :"+ costValiv + "</color>");

                        equipmentLevelUpButton.interactable = costValiv && itemData.enableLevelUp;
                        equipmentLevelUpButton.onClick.SetListener(() => {
                            player.Cmd_updrageEquipmentItem(itemIndex);
                            UpdateEvent();
                        });
                    }
                    else
                    {
                        // L'équipement est au niveau maximum.
                        requierementContent.RemoveAllChildren();
                        uprgadeChance.text = "Level Max";
                        destroyChance.text = "Level Max";
                    }

                }
                else
                {
                    // L'objet n'a pas de niveau d'équipement ou de paramètres de mise à niveau.
                    requierementContent.RemoveAllChildren();
                    uprgadeChance.text = "No Upgradable";
                    destroyChance.text = "No Upgradable";
                }
            }
            else
            {
#if _iMMOITEMRARITY
                itemSlot.rarityOutline.color = Color.clear;
#endif
                itemSlot.button.onClick.RemoveAllListeners();
                itemSlot.tooltip.enabled = false;
                itemSlot.dragAndDropable.dragable = false;
                itemSlot.image.color = Color.clear;
                itemSlot.image.sprite = null;
                itemSlot.amountOverlay.SetActive(false);
#if _iMMOITEMLEVELUP
                itemSlot.equipmentLevelUpText.gameObject.SetActive(false);
#endif

                itemName.text = "No item Selected";
                itemLevel.text = "0";
                uprgadeChance.text = "0";
                destroyChance.text = "0";
                equipmentLevelUpButton.interactable = false;
            }
        }
        else panel.SetActive(false);
    }
}