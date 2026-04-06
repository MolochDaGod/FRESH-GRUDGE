#if _iMMOTOOLS
using Mirror;
using UnityEngine;

// PLAYER

public partial class Player
{
    protected Tools_Popup popup;

    // -----------------------------------------------------------------------------------
    // Tools_ShowPrompt
    // Shows a popup, triggered on the server and sent to the client
    // @Server
    // -----------------------------------------------------------------------------------
    [ServerCallback]
    public void Tools_ShowPrompt(string message)
    {
        Target_Tools_ShowPrompt(connectionToClient, message);
    }

    // -----------------------------------------------------------------------------------
    // Target_Tools_ShowPrompt
    // @Client
    // @Server -> @Client
    // -----------------------------------------------------------------------------------
    [TargetRpc]
    protected void Target_Tools_ShowPrompt(NetworkConnection target, string message)
    {
        Tools_PopupShow(message);
    }

    // -----------------------------------------------------------------------------------
    // Tools_ShowPopup
    // Shows a popup, triggered on the server and sent to the client
    // @Server
    // -----------------------------------------------------------------------------------
    [ServerCallback]
    public void Tools_ShowPopup(string message, byte iconId = 0, byte soundId = 0)
    {
        Target_Tools_ShowPopup(connectionToClient, message, iconId, soundId);
    }

    // -----------------------------------------------------------------------------------
    // Target_Tools_ShowPopup
    // @Server -> @Client
    // -----------------------------------------------------------------------------------
    [TargetRpc]
    public void Target_Tools_ShowPopup(NetworkConnection target, string message, byte iconId, byte soundId)
    {
        if (popup == null)
            popup = GetComponent<Tools_Popup>();

        if (popup != null)
        {
            popup.Prepare(message, iconId, soundId);
            popup.Show();
            if (!popup.forceUseChat)
                Tools_AddMessage(message, 0, false);                                      // todo: add editable color
        }
    }

    // -----------------------------------------------------------------------------------
    // Tools_ClientShowPopup
    // Shows a popup, triggered on the client, shown on the client
    //  @Client
    // -----------------------------------------------------------------------------------
    public void Tools_ClientShowPopup(string message, byte iconId, byte soundId)
    {
        if (popup == null)
            popup = GetComponent<Tools_Popup>();

        if (popup != null)
        {
            popup.Prepare(message, iconId, soundId);
            popup.Show();
            if (!popup.forceUseChat)
                Tools_AddMessage(message, 0, false);                                      // todo: add editable color
        }
    }

    // -----------------------------------------------------------------------------------
}
#endif