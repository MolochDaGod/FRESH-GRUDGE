using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class NetworkManagerMMOCompleteChat : MonoBehaviour
{

#if _SERVER || UNITY_EDITOR
    public NetworkManagerMMO networkManagerMMO;
#if _iMMOCOMPLETECHAT
    [Header("[-=-[ Complete Chat Services ]-=-]")]
    public Tmpl_ServerChatAPI serverChatAPI;

    private bool messageCacheInit = false;
    private List<ChatServiceMessage> chatMessages = new List<ChatServiceMessage>();
    private HashSet<string> sentMessageIds = new HashSet<string>();

    // Start is called before the first frame update
    void Start()
    {        
#if _iMMOLOBBY
        if(networkManagerMMO.networkManagerMMONetworkZones != null && networkManagerMMO.networkManagerMMONetworkZones.enabled && networkManagerMMO.networkManagerMMONetworkZones.listSceneScriptable != null)
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene != null && (scene.name == networkManagerMMO.networkManagerMMONetworkZones.listSceneScriptable.LobbyScene.SceneName ||
                scene.name == networkManagerMMO.networkManagerMMONetworkZones.listSceneScriptable.CharacterManager.unityScenes.SceneName) ){ return; }
        }
#endif
        if (serverChatAPI != null)
            networkManagerMMO.onStartServer.AddListener(Start_SaasChat);
            
    }

    private void Start_SaasChat()
    {
        InvokeRepeating(nameof(SaasChat), serverChatAPI.refreshDelai, serverChatAPI.refreshDelai);
    }

    private void SaasChat()
    {
        StartCoroutine(ChatSync(serverChatAPI.uriServiceChat, serverChatAPI.apiKey));
    }
    private void SyncChatUser(string chat)
    {
        Debug.Log(chat);
        ChatServiceMessageList chatMessageList = JsonUtility.FromJson<ChatServiceMessageList>("{\"message\":" + chat + "}");

        // Ajouter les nouveaux messages à la liste
        foreach (ChatServiceMessage message in chatMessageList.message)
        {
            chatMessages.Add(message);
            if (chatMessages.Count > 100)
            {
                chatMessages.RemoveAt(0);
            }

        }

        // Mise en cache du serveur
        if (!messageCacheInit)
        {
            foreach (ChatServiceMessage message in chatMessageList.message)
            {
                sentMessageIds.Add(message.chat_id);
            }
            messageCacheInit = true;
        }

            
        //Debug.Log("Message pouf : " + player.name);
        foreach (ChatServiceMessage message in chatMessages)
        {
            // si le message n'a pas été sent à tous les joueurs
            if (!sentMessageIds.Contains(message.chat_id))
            {
                // Envoyer les nouveaux messages aux utilisateurs
                foreach (Player player in Player.onlinePlayers.Values)
                {
                    //Debug.Log("Nouveau message : " + message.chat_command);
                    // Check if player guild == message guild 
                    if (message.chat_command.StartsWith(player.chat.chatConfiguration.guildChannel.command))
                    {
                        if (player.guild.InGuild() && player.guild.guild.name == message.guild)
                        {
                            Debug.Log("Guild Message");
                            player.chat.OnResume(message.player, message.chat_command, message.guild, message.party);
                        }
                    }
                    else if (message.chat_command.StartsWith(player.chat.chatConfiguration.partyChannel.command))
                    {
                        if (player.party.InParty() && player.party.party.partyId.ToString() == message.party)
                        {
                            if (player.party.InParty() && player.party.party.partyId.ToString() == message.party)
                            {
                                Debug.Log("Party Message");
                                player.chat.OnResume(message.player, message.chat_command, message.guild, message.party);
                            }
                        }
                    }
                    else if (message.chat_command.StartsWith(player.chat.chatConfiguration.whisperChannel.command))
                    {
                        // whisper
                        string[] parsed = ParsePM(player.chat.chatConfiguration.whisperChannel.command, message.chat_command);
                        string user = parsed[0];
                        string msg = parsed[1];
                        if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(msg))
                        {
                            Debug.Log("Whisper Message");
                            //if (user != name)
                            //{
                            if (user == player.name || message.player == player.name)
                                player.chat.OnResume(message.player, message.chat_command, message.guild, message.party);

                            //}
                        }
                    }
                    else
                    {
                        Debug.Log("Classic message");
                        player.chat.OnResume(message.player, message.chat_command, message.guild, message.party);
                    }

                    player.chat.UIAutoScroll();
                }
            }
        }

        // Marquer tous les messages comme envoyés
        foreach (ChatServiceMessage message in chatMessages)
        {
            if (!sentMessageIds.Contains(message.chat_id))
                sentMessageIds.Add(message.chat_id);
        }

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
                string msg = content.Substring(i + 1);
                return new string[] { user, msg };
            }
        }
        return new string[] { "", "" };
    }
    static string ParseGeneral(string command, string msg)
    {
        // return message without command prefix (if any)
        return msg.StartsWith(command + " ") ? msg.Substring(command.Length + 1) : "";
    }

    IEnumerator ChatSync(string uri, string apikey)
    {

        WWWForm form = new WWWForm();

        form.AddField("apikey", apikey); ;

        UnityWebRequest www = UnityWebRequest.Post(uri, form);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Erreur :" + www.error);
        }
        else if (www.downloadHandler.text != "")
        {
            SyncChatUser(www.downloadHandler.text);
        }
        www.Dispose();
    }
#endif
#endif
}
