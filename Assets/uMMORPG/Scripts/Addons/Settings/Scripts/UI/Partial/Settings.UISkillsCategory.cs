#if _iMMOSKILLCATEGORY
using UnityEngine;

// Sets our new hotkeys for skills.
public partial class UI_Skills : MonoBehaviour
{
#if _iMMOSETTINGS
    // Set our hotkey based on the players selection.
    public void SetHotkey(KeyCode newHotkey)
    {
        hotKey = newHotkey;
    }
#endif
}
#endif