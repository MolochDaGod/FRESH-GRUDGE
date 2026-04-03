using UnityEngine;

#if _iMMOATTRIBUTES

// Sets our new hotkeys for character info.
public partial class UI_CharacterInfoAttributes : MonoBehaviour
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

// Sets our new hotkeys for character info.
public partial class UICharacterInfo : MonoBehaviour
{
#if _iMMOSETTINGS
    // Set our hotkey based on the players selection.
    public void SetHotkey(KeyCode newHotkey)
    {
        hotKey = newHotkey;
    }
#endif
}

