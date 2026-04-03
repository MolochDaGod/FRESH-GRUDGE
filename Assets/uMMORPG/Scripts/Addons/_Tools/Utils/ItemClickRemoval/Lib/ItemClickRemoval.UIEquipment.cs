using UnityEngine.UI;

public partial class UIEquipment
{
    // Adds a listener to the equipment item button to allow click removal.
    private void RemoveEquipmentItem(int currentIndex, UIEquipmentSlot slot, Player player)
    {
        Button button = slot.transform.GetChild(0).GetComponent<Button>();
        button.onClick.SetListener(() =>
        {
            player.CmdRemoveEquipmentItem(currentIndex); // added to move equipment into open inventory slot
        });
    }
}