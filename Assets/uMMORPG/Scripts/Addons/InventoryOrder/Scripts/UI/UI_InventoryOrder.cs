using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_InventoryOrder : MonoBehaviour
{
    public Dropdown inventoryOrderSelect;
    public SortOption enumToUse;
    private void Start()
    {
        //inventoryOrderSelect = GetComponent<Dropdown>();
        inventoryOrderSelect.ClearOptions();

        // Get all values in the enum
        var enumValues = System.Enum.GetValues(typeof(SortOption));

        // Create a list to hold the dropdown options
        var dropdownOptions = new List<Dropdown.OptionData>();

        // Loop through the enum values and add each as a dropdown option
        foreach (SortOption option in enumValues)
        {
            dropdownOptions.Add(new Dropdown.OptionData(option.ToString()));
        }

        // Add the options to the dropdown
        inventoryOrderSelect.AddOptions(dropdownOptions);

        // Set the dropdown's value to the first option (None)
        inventoryOrderSelect.value = (int)enumToUse;
    }

    public void BtnInventoryOrder()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            //Debug.Log("lol" + inventoryOrderSelect.value);
            player.CmdSortInventory((SortOption)inventoryOrderSelect.value);
        }
    }
}