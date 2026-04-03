using UnityEngine;

// Sets our new hotkeys for item mall.
public partial class UIItemMall : MonoBehaviour
{
#if _iMMOSETTINGS
    // Set our hotkey based on the players selection.
    public void SetHotkey(KeyCode newHotkey)
    {
        hotKey = newHotkey;
    }
#endif
}