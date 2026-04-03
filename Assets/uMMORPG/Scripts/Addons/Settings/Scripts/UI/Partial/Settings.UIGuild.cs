using UnityEngine;

// Sets our new hotkeys for guild.
public partial class UIGuild : MonoBehaviour
{
#if _iMMOSETTINGS
    // Set our hotkey based on the players selection.
    public void SetHotkey(KeyCode newHotkey)
    {
        hotKey = newHotkey;
    }
#endif
}