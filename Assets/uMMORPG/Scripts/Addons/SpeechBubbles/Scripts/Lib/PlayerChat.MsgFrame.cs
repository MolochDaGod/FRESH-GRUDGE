using Mirror;
using UnityEngine;
#if _iMMOCOMPLETECHAT
public partial class PlayerCompleteChat
#else
public partial class PlayerChat
#endif
{
    private MsgFrame msgFrame;
    private bool fakeMsgsFinished;

    // -----------------------------------------------------------------------------------
    // Awake
    // -----------------------------------------------------------------------------------
    public void Awake()
    {
        msgFrame = Instantiate(Resources.Load<GameObject>("MsgFrame"), this.transform).GetComponent<MsgFrame>();
    }

    // -----------------------------------------------------------------------------------
    // OnStartLocalPlayer_MsgFrame
    // -----------------------------------------------------------------------------------
#if _CLIENT
    public override void OnStartClient()
    {
        fakeMsgsFinished = true;
        onSubmit.AddListener(OnSubmit_MsgFrame);
    }

    // -----------------------------------------------------------------------------------
    // OnSubmit_MsgFrame
    // -----------------------------------------------------------------------------------
    private void OnSubmit_MsgFrame(string text)
    {
        if (!fakeMsgsFinished) return;
        //local chat
#if _iMMOCOMPLETECHAT

        if (!text.StartsWith("/") && validSend == true)
#else
        if (!text.StartsWith("/"))
#endif
            {
                // find the space that separates the name and the message
                int i = text.IndexOf(": ");
            if (i >= 0)
            {
                text = text.Substring(i + 1);
            }
            if (text != "")
            {
                msgFrame.ShowMessage(text);

                Cmd_RpcMsgLocal(text);
            }
        }
    }
#endif

    [Command]
    private void Cmd_RpcMsgLocal(string message)
    {
        RpcMsgLocal_MsgFrame(name, message);
    }

    // -----------------------------------------------------------------------------------
    // RpcMsgLocal_MsgFrame
    // -----------------------------------------------------------------------------------
    [ClientRpc]
    public void RpcMsgLocal_MsgFrame(string sender, string msg)
    {
        Player p = Player.onlinePlayers[sender];
        if (p)
#if _iMMOCOMPLETECHAT
            p.GetComponent<PlayerCompleteChat>().msgFrame.ShowMessage(msg);
#else
            p.GetComponent<PlayerChat>().msgFrame.ShowMessage(msg);
#endif
    }

    // -----------------------------------------------------------------------------------
}