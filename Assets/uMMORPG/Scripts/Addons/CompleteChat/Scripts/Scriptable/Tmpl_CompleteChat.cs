using UnityEngine;

// COMPLETE CHAT - TEMPLATE
[CreateAssetMenu(fileName = "Tmpl_CompleteChat", menuName = "ADDON/Templates/Complete Chat/New Complete Chat", order = 999)]
public class Tmpl_CompleteChat : ScriptableObject
{

    public bool useChaExternalService = false;

    [Header("[-=-[ Configuration ]-=-]")]
    [Tooltip("Optional. If the player has this item, they can use global chat. If this is null, then it is not required.")]
    public ScriptableItem localChatItem;
    [Tooltip("Player must be at least this level to use the global chat.")]
    [Min(1)] public int localChatLevel = 1;
    [Tooltip("Number of seconds before the player can use global chat again. (0 for disable)")]
    [Min(0)] public float localRateLimit = 1f;
    public int maxLengthMessage = 150;


    [Header("[-=-[ Channels ]-=-]")]
    public ChannelInfo localChannel = new ChannelInfo("", "", "", null);
    public ChannelInfo whisperChannel = new ChannelInfo("/w", "(TO)", "(FROM)", null);
    public ChannelInfo partyChannel = new ChannelInfo("/p", "(Party)", "(Party)", null);
    public ChannelInfo guildChannel = new ChannelInfo("/g", "(Guild)", "(Guild)", null);
    public ChannelInfo infoChannel = new ChannelInfo("", "(Info)", "(Info)", null);

    [Header("[-=-[ Default Message Info ]-=-]")]
    public string[] messageDisplayDefault = {
        "Welcone to yourGameName",
        "thank you for staying calm and courteous!", 
        "In the event of abuse on the chat, sanctions can be taken!" 
    };


    [Header("[-=-[ Error Message ]-=-]")]
    public string errorNotWhisperSelf = "cant whisper to self";
    public string errorTooSoon = "Must wait at least {RATELIMIT} seconds between messages.";
    public string errorLevelTooLow = "Must be at least level {MINLEVEL} before using global chat.";
    public string errorMissingItem = "You must have purchased {CHATITEM} before you can use global chat.";

    [Header("Emote list")]
    public string[] emoteList = { };

#if _iMMOWORDFILTER
    [Header("[-=-[ WORD FILTER ]-=-]")]
    public Tmpl_WordFilter wordFilter;
    public string messageError = "[Sys] World filter detect bad word...";
#endif

    [HideInInspector] public bool validSend = false;
}

