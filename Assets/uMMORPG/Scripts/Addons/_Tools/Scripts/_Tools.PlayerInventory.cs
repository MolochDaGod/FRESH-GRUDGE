#if _iMMOTOOLS_LEGACY_CORE
using Mirror;
using UnityEngine.Events;

public partial class PlayerInventory
{

    public UnityEvent onInventoryChanged;


    void OnInventoryChanged(SyncList<ItemSlot>.Operation op, int index, ItemSlot oldData, ItemSlot newData)
    {
#if _SERVER && _iMMOWEIGHTSYSTEM
        player.playerWeightSystem.UpdateEvent();
#endif
        onInventoryChanged.Invoke();
    }

    public override void OnStartServer()
    {
        slots.Callback += OnInventoryChanged;
    }
}
#endif