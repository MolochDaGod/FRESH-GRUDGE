using UnityEngine;

// SURPRISE BOX ITEM

[CreateAssetMenu(menuName = "ADDON/Item/Pvp Zone and Realm/Change Realm", order = 999)]
public class Item_ChangeRealm : UsableItem
{
    [Header("[-=-[ Change Realm ]-=-]")]
    public Tmpl_Realm realm;

    [Tooltip("Decrease amount by how many each use (can be 0)?")]
    public int decreaseAmount = 1;

    [Tooltip("Message changed Realm.")]
    public string failMessage = "this is already your Realm!";

#if _iMMOZONES 
    [Tooltip("if this not Empty and Network Lobby is installed, player are teleported to this scene!")]
    public UnityScene sceneLocation;
#endif
    // -----------------------------------------------------------------------------------
    // Use
    // -----------------------------------------------------------------------------------
    public override void Use(Player player, int inventoryIndex)
    {
        ItemSlot slot = player.inventory.slots[inventoryIndex];

        // -- Only activate if enough charges left and can fit all items
        if (decreaseAmount == 0 || slot.amount >= decreaseAmount)
        {
            if (player.isAlive && player.Realm  != realm)
            {
                // always call base function too
                base.Use(player, inventoryIndex);

                // -- change realm !
                player.Realm = realm;

                // decrease amount
                slot.DecreaseAmount(decreaseAmount);
                player.inventory.slots[inventoryIndex] = slot;
            }
            else
            {
                player.Tools_TargetAddMessage(failMessage);
            }
        }
    }

    // -----------------------------------------------------------------------------------
}