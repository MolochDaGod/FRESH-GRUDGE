#if !DISABLESTEAMWORKS
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;

public class SkinsPanel : MonoBehaviour
{
    public GameObject panel;
    public GameObject skinItemPrefab; // Prefab for displaying skin options
    public Transform skinListContainer; // Container to hold the skin items

    void Update()
    {
        if (panel.activeSelf)
        {
            // Check for any input or other functionalities here if needed
        }
    }

    public void OnItemDropped(ScriptableItem droppedItem)
    {
        if (droppedItem.itemSkins.Length > 0)
        {
            DisplayAvailableSkins(droppedItem);
        }
    }

    private void DisplayAvailableSkins(ScriptableItem droppedItem)
    {
        // Clear previous skins
        foreach (Transform child in skinListContainer)
        {
            Destroy(child.gameObject);
        }

        // Fetch the player's inventory from Steamworks
        GetPlayerSteamInventory(droppedItem);
    }

    private void GetPlayerSteamInventory(ScriptableItem droppedItem)
    {
        // Create a result handle
        SteamInventoryResult_t inventoryResult;

        // Call to fetch the inventory
        if (SteamInventory.GetAllItems(out inventoryResult))
        {
            // This will hold the items fetched
            uint itemCount = 0;
            SteamItemDetails_t[] items = new SteamItemDetails_t[100]; // Adjust size as needed

            // Get the items
            if (SteamInventory.GetResultItems(inventoryResult, items, ref itemCount))
            {
                List<string> playerSteamInventory = new List<string>();

                for (uint i = 0; i < itemCount; i++)
                {
                    playerSteamInventory.Add(items[i].m_itemId.ToString()); // Use the correct property
                }

                // Now display the skins based on the fetched inventory
                foreach (ItemSkinWithID itemSkin in droppedItem.itemSkins)
                {
                    if (playerSteamInventory.Contains(itemSkin.steamProductId))
                    {
                        // Instantiate and configure a UI element for the available skin
                        GameObject skinItem = Instantiate(skinItemPrefab, skinListContainer);
                        skinItem.GetComponentInChildren<TextMeshProUGUI>().text = itemSkin.skin.name; // Use .name to get the item's name

                        // Add listener to handle skin selection on clicking this item
                        skinItem.GetComponent<Button>().onClick.AddListener(() => SelectSkin(itemSkin));
                    }
                }
            }
            else
            {
                Debug.LogError("Failed to get items from inventory result.");
            }

            // Destroy the inventory result handle to release resources
            SteamInventory.DestroyResult(inventoryResult);
        }
        else
        {
            Debug.LogError("Failed to fetch inventory items.");
        }
    }

    private void SelectSkin(ItemSkinWithID selectedSkin)
    {
        // Logic to apply the selected skin to the item
        Debug.Log($"Selected skin: {selectedSkin.skin.name}"); // Display the selected skin name
        // Add your skin application logic here
    }
}
#endif