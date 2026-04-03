using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NETWORK MANAGER MMO
public class NetworkManagerMMOMail : MonoBehaviour
{
    public NetworkManagerMMO networkManagerMMO;
    [Header("[-=-[ Mail Settings ]-=-]")]
    public Tmpl_MailSettings mailSettings;

    // -----------------------------------------------------------------------------------
    // OnStartServer_MailChecker
    // -----------------------------------------------------------------------------------
#if _SERVER
    public  void Start()
    {
        if (mailSettings)
        {
            //Database.singleton.Connect_Mail();
            networkManagerMMO.onStartServer.AddListener(startUpdateMail);
        }
        else
            Debug.LogWarning("You forgot to assign Mail Settings to NetworkManager!");
    }
    private void startUpdateMail()
    {
        StartCoroutine(UpdateMailStatus());
    }
    // -----------------------------------------------------------------------------------
    // UpdateMailStatus
    // -----------------------------------------------------------------------------------
    private IEnumerator UpdateMailStatus()
    {
        yield return null;
        //get the last known ID known on server startup
        //new messages are considered to be any after this point so we can notify people of new messages

        long maxID = Database.singleton.Mail_FindMaxID();

        while (true)
        {
            yield return new WaitForSeconds(mailSettings.mailCheckSeconds);

            //check for new messages
            List<MailMessage> newMessages = Database.singleton.Mail_CheckForNewMessages(maxID);

            foreach (MailMessage message in newMessages)
            {
                //if the player is online, add to their synclist
                if (Player.onlinePlayers.ContainsKey(message.to))
                {
                    Player.onlinePlayers[message.to].playerAddonsConfigurator.mailMessages.Add(message);
                }

                if (message.id > maxID)
                {
                    maxID = message.id;
                }
            }
        }
    }
#endif
    // -----------------------------------------------------------------------------------
}