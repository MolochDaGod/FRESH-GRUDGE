using UnityEngine;
using Mirror;
using UnityEngine.Events;

using System.Text;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerGuild))]
[RequireComponent(typeof(PlayerParty))]
[DisallowMultipleComponent]
public partial class PlayerCompleteChat : NetworkBehaviour
{
#if _iMMOCOMPLETECHAT
    public enum ChatErrorType
    {
        None,
        CantFindPlayer,
        TooSoon,
        LevelTooLow,
        MissingItem
    }

    [Header("Components")] // to be assigned in inspector
   // public Player player; 
    public PlayerGuild guild;
    public PlayerParty party;

    [Header("[-=-[ Chat Configuration ]-=-]")]
    public Tmpl_CompleteChat chatConfiguration;

#if _SERVER || UNITY_EDITOR
    [Header("[-=-[ Server Chat Configuration ]-=-]")]
    [Tooltip("this is accessible only in server build")]
    public Tmpl_ServerChatAPI serverChatAPI;
#endif

    public float localNextChatTime/*, partyNextChatTime, guildNextChatTime*/ = 0f;
    [HideInInspector] public bool validSend = false;
    [Header("Events")]
    public UnityEventString onSubmit;

    public override void OnStartLocalPlayer()
    {
        if (chatConfiguration != null)
        {
            foreach (string message in chatConfiguration.messageDisplayDefault)
            {
                UICompleteChat.singleton.AddMessage(new ChatMessage("", chatConfiguration.infoChannel.identifierIn, message, "", chatConfiguration.infoChannel.textPrefab));
            }
        }
    }
    /*
  //  private string listMessage = "";
    public override void OnStartServer()
    {
        StartCoroutine(getRequest("http://localhost/remoteComand/chat.php", "apikey"));

    }



    IEnumerator getRequest(string uri, string apikey)
    {

        WWWForm form = new WWWForm();

        form.AddField("apikey", apikey);

        UnityWebRequest www = UnityWebRequest.Post(uri, form);
        //{
        //Debug.Log("WWW lancé");
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Erreur :" + www.error);
        }
        else
        {
            string listMessage = www.downloadHandler.text.ToString();
            Debug.Log(listMessage);
            List<ChatServiceMessage> chatMessage = JsonConvert.DeserializeObject<List<ChatServiceMessage>>( listMessage );


            foreach (ChatServiceMessage message in chatMessage)
            {
                //UICompleteChat.singleton.AddMessage(new ChatMessage("", infoChannel.identifierIn, message.chat_command, "", infoChannel.textPrefab));
                OnResume(message.player, message.chat_command, message.guild, message.party);
            }
        }
        www.Dispose();
    }*/

    [TargetRpc]
    public void OnResume(string playerSend, string textSend, string guildSend, string partySend)
    {
        // not empty and not only spaces?
#if _iMMOCOMPLETECHAT
        if (IsAllowedChatText(textSend))
#else
        if (!string.IsNullOrWhiteSpace(text))
#endif
        {
            // command in the commands list?
            // note: we don't do 'break' so that one message could potentially
            //       be sent to multiple channels (see mmorpg local chat)
#pragma warning disable CS0219
            string lastCommand = "";
#pragma warning restore
            if (textSend.StartsWith(chatConfiguration.whisperChannel.command))
            {
                // whisper
                string[] parsed = ParsePM(chatConfiguration.whisperChannel.command, textSend);
                string user = parsed[0];
                string msg = parsed[1];
                if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(msg))
                {
                    if (playerSend != name)
                    {
                        // add message with identifierIn
                        string identifier = chatConfiguration.whisperChannel.identifierIn;
                        string reply = chatConfiguration.whisperChannel.command + " " + playerSend + " "; // whisper
                        UICompleteChat.singleton.AddMessage(new ChatMessage(playerSend, identifier, msg, reply, chatConfiguration.whisperChannel.textPrefab));
                    }
                    else
                    {
                        // add message with identifierOut
                        string identifier = chatConfiguration.whisperChannel.identifierOut;
                        string reply = chatConfiguration.whisperChannel.command + " " + user + " "; // whisper
                        UICompleteChat.singleton.AddMessage(new ChatMessage(user, identifier, msg, reply, chatConfiguration.whisperChannel.textPrefab));
                    }
                }
                else print("invalid whisper format: " + user + "/" + msg);
            }
            else if (!textSend.StartsWith("/"))
            {
                //check for an error message with chat request
                ChatErrorType localChatError = LocalChatCheckForError();
                //and the user can use the global chat
                if (localChatError == ChatErrorType.None)
                {

                    validSend = true;
                    // local chat is special: it has no command
                    //lastCommand = "";
                    //CmdMsgLocal(textSend);
                    string identifier = playerSend != name ? chatConfiguration.localChannel.identifierIn : chatConfiguration.localChannel.identifierOut;
                    string reply = chatConfiguration.whisperChannel.command + " " + playerSend + " "; // whisper
                    UICompleteChat.singleton.AddMessage(new ChatMessage(playerSend, identifier, textSend, reply, chatConfiguration.localChannel.textPrefab));

                    //set next TimeLogout the player can chat
                }
                else
                {
                    validSend = false;
                    string errorMessage = "";

                    switch (localChatError)
                    {
                        case ChatErrorType.LevelTooLow:
                            errorMessage = BuildErrorMessage(chatConfiguration.errorLevelTooLow, chatConfiguration.localChatLevel.ToString());
                            break;

                        case ChatErrorType.MissingItem:
                            errorMessage = BuildErrorMessage(chatConfiguration.errorMissingItem, null, chatConfiguration.localChatItem);
                            break;

                        case ChatErrorType.TooSoon:
                            errorMessage = BuildErrorMessage(chatConfiguration.errorTooSoon, chatConfiguration.localRateLimit.ToString("0.#"));
                            break;
                    }

                    //if the error message is not empty
                    if (!string.IsNullOrWhiteSpace(errorMessage))
                    {
                        UICompleteChat.singleton.AddMessage(new ChatMessage(playerSend, chatConfiguration.infoChannel.identifierIn, errorMessage, "", chatConfiguration.localChannel.textPrefab));
                    }
                }
                // local chat is special: it has no command
                //lastCommand = "";
                //CmdMsgLocal(text);
            }
            else if (textSend.StartsWith(chatConfiguration.partyChannel.command) && party.InParty() && party.party.partyId.ToString() == partySend)
            {
                // party
                string msg = ParseGeneral(chatConfiguration.partyChannel.command, textSend);
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    //lastCommand = partyChannel.command + " ";
                    string reply = chatConfiguration.whisperChannel.command + " " + playerSend + " "; // whisper
                    UICompleteChat.singleton.AddMessage(new ChatMessage(playerSend, chatConfiguration.partyChannel.identifierIn, msg, reply, chatConfiguration.partyChannel.textPrefab));
                }
            }
            else if (textSend.StartsWith(chatConfiguration.guildChannel.command) && guild.guild.name == guildSend)
            {
                // guild
                string msg = ParseGeneral(chatConfiguration.guildChannel.command, textSend);
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    //lastCommand = guildChannel.command + " ";
                    //CmdMsgGuild(msg);
                    string reply = chatConfiguration.whisperChannel.command + " " + playerSend + " "; // whisper
                    UICompleteChat.singleton.AddMessage(new ChatMessage(playerSend, chatConfiguration.guildChannel.identifierIn, msg, reply, chatConfiguration.guildChannel.textPrefab));
                }
            }

            // addon system hooks
            //onSubmit.Invoke(textSend);

            // input text should be set to lastcommand
            //return lastCommand;
        }

        // input text should be cleared
        //return "";
    }

    [ClientRpc]
    public void UIAutoScroll()
    {
        UICompleteChat.singleton.AutoScroll();
    }
 
    // submit tries to send the string and then returns the new input text
    [Client]
    public string OnSubmit(string text)
    {
        // not empty and not only spaces?
#if _iMMOCOMPLETECHAT
        if (IsAllowedChatText(text))
#else
        if (!string.IsNullOrWhiteSpace(text))
#endif
        {
            // command in the commands list?
            // note: we don't do 'break' so that one message could potentially
            //       be sent to multiple channels (see mmorpg local chat)
            string lastCommand = "";
            if (text.StartsWith(chatConfiguration.whisperChannel.command))
            {
                // whisper
                string[] parsed = ParsePM(chatConfiguration.whisperChannel.command, text);
                string user = parsed[0];
                string msg = parsed[1];
                if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(msg))
                {
                    if (user != name)
                    {
                        lastCommand = chatConfiguration.whisperChannel.command + " " + user + " ";
                        if (chatConfiguration.useChaExternalService)
                            Cmd_AddLog(text);
                        else
                            CmdMsgWhisper(user, msg);
                    }
                    else
                    {
                        UICompleteChat.singleton.AddMessage(new ChatMessage("", chatConfiguration.infoChannel.identifierIn, chatConfiguration.errorNotWhisperSelf, "", chatConfiguration.infoChannel.textPrefab));
                    }
                }
                else print("invalid whisper format: " + user + "/" + msg);
            }
            else if (!text.StartsWith("/"))
            {
                //check for an error message with chat request
                ChatErrorType localChatError = LocalChatCheckForError();
                //and the user can use the global chat
                if (localChatError == ChatErrorType.None)
                {
                    validSend = true;
                    // local chat is special: it has no command
                    //lastCommand = "";
                    if (chatConfiguration.useChaExternalService)
                        Cmd_AddLog(text);
                    else
                        CmdMsgLocal(text);

                    //set next TimeLogout the player can chat
                    localNextChatTime = Time.time + chatConfiguration.localRateLimit;
                }
                else
                {
                    validSend = false;
                    string errorMessage = "";

                    switch (localChatError)
                    {
                        case ChatErrorType.LevelTooLow:
                            errorMessage = BuildErrorMessage(chatConfiguration.errorLevelTooLow, chatConfiguration.localChatLevel.ToString());
                            break;

                        case ChatErrorType.MissingItem:
                            errorMessage = BuildErrorMessage(chatConfiguration.errorMissingItem, null, chatConfiguration.localChatItem);
                            break;

                        case ChatErrorType.TooSoon:
                            errorMessage = BuildErrorMessage(chatConfiguration.errorTooSoon, chatConfiguration.localRateLimit.ToString("0.#"));
                            break;
                    }

                    //if the error message is not empty
                    if (!string.IsNullOrWhiteSpace(errorMessage))
                    {
                        UICompleteChat.singleton.AddMessage(new ChatMessage("", chatConfiguration.infoChannel.identifierIn, errorMessage, "", chatConfiguration.localChannel.textPrefab));
                    }
                }
                // local chat is special: it has no command
                //lastCommand = "";
                //CmdMsgLocal(text);
            }
            else if (text.StartsWith(chatConfiguration.partyChannel.command))
            {
                // party
                string msg = ParseGeneral(chatConfiguration.partyChannel.command, text);
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    //lastCommand = partyChannel.command + " ";
                    if (chatConfiguration.useChaExternalService)
                        Cmd_AddLog(text);
                    else
                        CmdMsgParty(msg);
                }
            }
            else if (text.StartsWith(chatConfiguration.guildChannel.command))
            {
                // guild
                string msg = ParseGeneral(chatConfiguration.guildChannel.command, text);
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    //lastCommand = guildChannel.command + " ";
                    if (chatConfiguration.useChaExternalService)
                        Cmd_AddLog(text);
                    else
                        CmdMsgGuild(msg);
                }
            }

            // addon system hooks
            onSubmit.Invoke(text);

            // input text should be set to lastcommand
            return lastCommand;
        }

        // input text should be cleared
        return "";
    }

    // parse a message of form "/command message"
    static string ParseGeneral(string command, string msg)
    {
        // return message without command prefix (if any)
        return msg.StartsWith(command + " ") ? msg.Substring(command.Length + 1) : "";
    }

    public static string[] ParsePM(string command, string pm)
    {
        // parse to /w content
        string content = ParseGeneral(command, pm);

        // now split the content in "user msg"
        if (content != "")
        {
            // find the first space that separates the name and the message
            int i = content.IndexOf(" ");
            if (i >= 0)
            {
                string user = content.Substring(0, i);
                string msg = content.Substring(i+1);
                return new string[] {user, msg};
            }
        }
        return new string[] {"", ""};
    }

    // networking //////////////////////////////////////////////////////////////
    [Command]
    void CmdMsgLocal(string message)
    {
        if (message.Length > chatConfiguration.maxLengthMessage) return;

        // it's local chat, so let's send it to all observers via ClientRpc
        RpcMsgLocal(name, message);
    }

    [Command]
    void CmdMsgParty(string message)
    {
        if (message.Length > chatConfiguration.maxLengthMessage) return;

        // send message to all online party members
        if (party.InParty())
        {
            foreach (string member in party.party.members)
            {
                if (Player.onlinePlayers.TryGetValue(member, out Player onlinePlayer))
                {
                    // call TargetRpc on that GameObject for that connection
                    onlinePlayer.chat.TargetMsgParty(name, message);
                }
            }
        }
    }

    [Command]
    void CmdMsgGuild(string message)
    {
#if _SERVER
        if (message.Length > chatConfiguration.maxLengthMessage) return;

        // send message to all online guild members
        if (guild.InGuild())
        {
            foreach (GuildMember member in guild.guild.members)
            {
                if (Player.onlinePlayers.TryGetValue(member.name, out Player onlinePlayer))
                {
                    // call TargetRpc on that GameObject for that connection
                    onlinePlayer.chat.TargetMsgGuild(name, message);
                }
            }
        }
#endif
    }

    [Command]
    void CmdMsgWhisper(string playerName, string message)
    {
        if (message.Length > chatConfiguration.maxLengthMessage) return;

        // find the player with that name
        if (Player.onlinePlayers.TryGetValue(playerName, out Player onlinePlayer))
        {
            // receiver gets a 'from' message, sender gets a 'to' message
            // (call TargetRpc on that GameObject for that connection)
            onlinePlayer.chat.TargetMsgWhisperFrom(name, message);
            TargetMsgWhisperTo(playerName, message);
        }
    }

    // send a global info message to everyone
    [Server]
    public void SendGlobalMessage(string message)
    {
        foreach (Player player in Player.onlinePlayers.Values)
            player.chat.TargetMsgInfo(message);
    }

    // message handlers ////////////////////////////////////////////////////////
    [TargetRpc]
    public void TargetMsgWhisperFrom(string sender, string message)
    {
        // add message with identifierIn
        string identifier = chatConfiguration.whisperChannel.identifierIn;
        string reply = chatConfiguration.whisperChannel.command + " " + sender + " "; // whisper
        UICompleteChat.singleton.AddMessage(new ChatMessage(sender, identifier, message, reply, chatConfiguration.whisperChannel.textPrefab));
    }

    [TargetRpc]
    public void TargetMsgWhisperTo(string receiver, string message)
    {
        // add message with identifierOut
        string identifier = chatConfiguration.whisperChannel.identifierOut;
        string reply = chatConfiguration.whisperChannel.command + " " + receiver + " "; // whisper
        UICompleteChat.singleton.AddMessage(new ChatMessage(receiver, identifier, message, reply, chatConfiguration.whisperChannel.textPrefab));
    }

    [ClientRpc]
    public void RpcMsgLocal(string sender, string message)
    {
        // add message with identifierIn or Out depending on who sent it
        string identifier = sender != name ? chatConfiguration.localChannel.identifierIn : chatConfiguration.localChannel.identifierOut;
        string reply = chatConfiguration.whisperChannel.command + " " + sender + " "; // whisper
        UICompleteChat.singleton.AddMessage(new ChatMessage(sender, identifier, message, reply, chatConfiguration.localChannel.textPrefab));
    }

    [TargetRpc]
    public void TargetMsgGuild(string sender, string message)
    {
        string reply = chatConfiguration.whisperChannel.command + " " + sender + " "; // whisper
        UICompleteChat.singleton.AddMessage(new ChatMessage(sender, chatConfiguration.guildChannel.identifierIn, message, reply, chatConfiguration.guildChannel.textPrefab));
    }

    [TargetRpc]
    public void TargetMsgParty(string sender, string message)
    {
        string reply = chatConfiguration.whisperChannel.command + " " + sender + " "; // whisper
        UICompleteChat.singleton.AddMessage(new ChatMessage(sender, chatConfiguration.partyChannel.identifierIn, message, reply, chatConfiguration.partyChannel.textPrefab));
    }

    [TargetRpc]
    public void TargetMsgInfo(string message)
    {
        AddMsgInfo(message);
    }

    // info message can be added from client too
    public void AddMsgInfo(string message)
    {
        UICompleteChat.singleton.AddMessage(new ChatMessage("", chatConfiguration.infoChannel.identifierIn, message, "", chatConfiguration.infoChannel.textPrefab));
    }

//#if _iMMOCOMPLETECHAT
    public bool IsAllowedChatText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
#if _iMMOWORDFILTER
        if (!chatConfiguration.wordFilter || chatConfiguration.wordFilter.badwords.Length == 0) return true;

        return WordFilter(text.ToLower());
#else
        return true;
#endif
    }


#if _iMMOWORDFILTER
    // -----------------------------------------------------------------------------------
    // WordFilter
    // -----------------------------------------------------------------------------------
    public bool WordFilter(string text)
    {
        foreach (string badword in chatConfiguration.wordFilter.badwords)
        {
            if (text.Contains(" "+ badword.ToLower()+ " "))
            {
                AddMessage();
                return false;
            }
        }
        return true;
    }

    private void AddMessage()
    {
        UICompleteChat.singleton.AddMessage(new ChatMessage("", chatConfiguration.infoChannel.identifierIn, chatConfiguration.messageError, "", chatConfiguration.infoChannel.textPrefab));
    }
#endif

    // -----------------------------------------------------------------------------------
    // LocalChatCheckForError
    // -----------------------------------------------------------------------------------
    public ChatErrorType LocalChatCheckForError()
    {
        Player player = GetComponent<Player>();

        //was the player found
        if (player != null)
        {
            //is the player at or above the required level
            if (player.level.current >= chatConfiguration.localChatLevel)
            {
                //if enoug TimeLogout has passed since last message
                if (Time.time >= localNextChatTime)
                {
                    //is the item null (not required) or found in the player inventory
                    if (chatConfiguration.localChatItem == null || player.inventory.Count(new Item(chatConfiguration.localChatItem)) > 0)
                    {
                        return ChatErrorType.None;
                    }
                    else
                    {
                        return ChatErrorType.MissingItem;
                    }
                }
                else
                {
                    return ChatErrorType.TooSoon;
                }
            }
            else
            {
                return ChatErrorType.LevelTooLow;
            }
        }
        else
        {
            return ChatErrorType.CantFindPlayer;
        }
    }


    // -----------------------------------------------------------------------------------
    // BuildErrorMessage
    // -----------------------------------------------------------------------------------
    public string BuildErrorMessage(string text, string require, ScriptableItem item = null)
    {
        StringBuilder errorText = new StringBuilder(text);

        errorText.Replace("{NAME}", name);
        errorText.Replace("{RATELIMIT}", require);
        errorText.Replace("{MINLEVEL}", require);

        if (item != null)
            errorText.Replace("{CHATITEM}", item.name);

        return errorText.ToString();
    }

    /*
    void Start_ExternalChat()
    {
        if (player.chat.chatConfiguration.useChaExternalService)
        {
            player.chat.onSubmit.AddListener(AddLogChat);
        }
    }*/

   /* private void AddLogChat(string chat)
    {

        // on doit ajouter le message dans les log du control center
        Cmd_AddLog(chat);

    }*/

    [Command]
    public void Cmd_AddLog(string chat)
    {
#if _SERVER
        if (serverChatAPI != null && chatConfiguration != null && chatConfiguration.useChaExternalService)
            StartCoroutine(getRequest(serverChatAPI.uriServiceChat, serverChatAPI.apiKey.ToString(), chat));
        else
            Debug.Log("In player prefab, PlayerCompleteChat component, serverChatAPI is empty !");
#endif
    }

#if _SERVER
    IEnumerator getRequest(string uri, string apikey, string texte)
    {

        WWWForm form = new WWWForm();

        form.AddField("apikey", apikey);
        form.AddField("player", name);
        form.AddField("guild", (guild.InGuild() ? guild.guild.name : "no"));
        form.AddField("party", (party.InParty() ? party.party.partyId.ToString() : "no"));
        form.AddField("text", texte);
        form.AddField("server", SceneManager.GetActiveScene().name);

        UnityWebRequest www = UnityWebRequest.Post(uri, form);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Erreur :" + www.error);
        }
        else
        {
            Debug.Log(">" + www.downloadHandler.text + "<");
        }
        www.Dispose();
    }
#endif
#endif
}

[System.Serializable]
public class ChatServiceMessageList
{
    public List<ChatServiceMessage> message;
}

[System.Serializable]
public struct ChatServiceMessage
{
    public string chat_id;
    public string player;
    public string chat_command;
    public string guild;
    public string party;
    public string server;
    public string date_send;
}