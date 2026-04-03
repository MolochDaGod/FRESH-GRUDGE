using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResearchPanel : MonoBehaviour
{
    public GameObject panel;
    public UIDragAndDropable itemSlot;
    public TextMeshProUGUI costDescriptionText;
    public Button confirmResearchButton;

    [HideInInspector]
    public int itemIndex = -1;

    public static ResearchPanel instance;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (panel.activeSelf)
        {
            Player player = Player.localPlayer;

            if (itemIndex != -1 && itemIndex < player.inventory.slots.Count &&
                player.inventory.slots[itemIndex].amount > 0)
            {
                ItemSlot slot = player.inventory.slots[itemIndex];
                ScriptableItem itemData = slot.item.data;

                itemSlot.GetComponent<Image>().color = Color.white;
                itemSlot.GetComponent<Image>().sprite = slot.item.image;
                itemSlot.GetComponent<UIShowToolTip>().enabled = true;

                // Only build tooltip while it's actually shown to avoid allocations.
                if (itemSlot.GetComponent<UIShowToolTip>().IsVisible())
                    itemSlot.GetComponent<UIShowToolTip>().text = slot.ToolTip();

                itemSlot.dragable = true;

                // Loop through all costs
                costDescriptionText.text = "";
                bool hasGoldCost = itemData.researchCosts.gold >= 1; // Check if gold cost is 1 or more

                for (int i2 = 0; i2 < itemData.researchCosts.requiredItems.Length; i2++)
                {
                    costDescriptionText.text += itemData.researchCosts.requiredItems[i2].amount + " " + itemData.researchCosts.requiredItems[i2].item.name + " \n";
                }

                if (itemData.researchCosts.gold > 0)
                {
                    costDescriptionText.text += itemData.researchCosts.gold + " Gold \n";
                }

                // Disable the research button if gold cost is less than 1
                confirmResearchButton.interactable = hasGoldCost;

                confirmResearchButton.onClick.SetListener(() =>
                {
                    player.research.CmdResearchItem(itemIndex);

                    // Clear the item slot after research
                    itemSlot.GetComponent<Image>().sprite = null; // Remove sprite
                    itemSlot.GetComponent<Image>().color = Color.clear; // Optionally make it transparent
                    itemSlot.GetComponent<UIShowToolTip>().enabled = false; // Disable tooltip
                    itemSlot.dragable = false; // Disable dragging
                    costDescriptionText.text = ""; // Clear cost description
                    itemIndex = -1; // Reset item index
                });
            }
        }
        else
        {
            itemIndex = -1;
        }
    }
}