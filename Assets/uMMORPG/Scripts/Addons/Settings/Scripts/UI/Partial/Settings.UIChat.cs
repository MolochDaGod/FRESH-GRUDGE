    using UnityEngine;

// Grabs our dagger settings variables for chat to know if its visible or not.
#if _iMMOCOMPLETECHAT
public partial class UICompleteChat : MonoBehaviour
#else
public partial class UIChat : MonoBehaviour

#endif
{
#if _iMMOSETTINGS
    /// Chat not use hotkey
    // Set our hotkey based on the players selection.
    /*public void SetHotkey(KeyCode newHotkey)
    {
        hotKey = newHotkey;
    }*/
#endif
}