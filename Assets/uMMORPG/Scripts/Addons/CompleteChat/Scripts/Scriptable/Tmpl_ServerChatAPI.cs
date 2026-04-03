using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// COMPLETE CHAT - TEMPLATE
[CreateAssetMenu(fileName = "Tmpl_CompleteChatAPI", menuName = "ADDON/Templates/Complete Chat/New Chat API", order = 999)]
public class Tmpl_ServerChatAPI : ScriptableObject
{
#if _SERVER || UNITY_EDITOR
    [Header("[-=-[ Complete Chat Services ]-=-]")]
    public string uriServiceChat = "https://localhost/remoteCommand/API/chat.php";
    public string apiKey = "aRevfD!zefF;FezrczeMlreA.fFVE";
    [Range(0.1f, 1f)] public float refreshDelai = 0.2f;
#endif
}
