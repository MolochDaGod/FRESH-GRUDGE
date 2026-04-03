using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(PlayerInventory))]
[DisallowMultipleComponent]
public partial class PlayerAddonsConfigurator
{



    [Header("[-=-[ Mail Setting ]-=-]")]
    public Tmpl_MailSettings mailSettings;

    private UI_SendMail mail;

    public readonly SyncList<MailMessage> mailMessages = new SyncList<MailMessage>();
    // -----------------------------------------------------------------------------------
    // UnreadMailCount
    // -----------------------------------------------------------------------------------
    public int UnreadMailCount()
    {
        int count = 0;
        foreach (MailMessage message in mailMessages)
        {
            if (message.read == 0)
                count++;
        }
        return count;
    }

    // -----------------------------------------------------------------------------------
    // CmdMail_Send
    // -----------------------------------------------------------------------------------
    [Command]
    public void CmdMail_Send(string messageTo, string subject, string body, int itemIndex)
    {
#if _SERVER
        string errors = "";

        // ----- check if mail can be sent

        if (string.IsNullOrEmpty(messageTo))
        {
            errors += mailSettings.labelRecipient;
        }
        if (string.IsNullOrEmpty(subject))
        {
            errors += mailSettings.labelSubject;
        }
        if (string.IsNullOrEmpty(body))
        {
            errors += mailSettings.labelBody;
        }
        if (!string.IsNullOrEmpty(subject) && subject.Length >= mailSettings.subjectLength)
        {
            errors += mailSettings.labelSubjectTooLong;
        }
        if (!string.IsNullOrEmpty(body) && body.Length >= mailSettings.bodyLength)
        {
            errors += mailSettings.labelBodyTooLong;
        }
        if (!mailSettings.costPerMail.CheckCost(player))
        {
            errors += mailSettings.labelCost;
        }
        if (itemIndex != -1)
        {
            if (inventory.slots[itemIndex].amount < 1)
                errors += "Missing item to send!\n";
        }

        // ----- begin send mail

        //if no errors yet, perform more complicated checks
        if (string.IsNullOrEmpty(errors))
        {
            long expiration = Mail_CalculateExpiration();

            mailSettings.costPerMail.PayCost(player);

            string itemName = "";

            if (itemIndex != -1)
            {
                itemName = inventory.slots[itemIndex].item.data.name;
                Item item = new Item(inventory.slots[itemIndex].item.data);
                player.inventory.Remove(item, 1);
                Debug.Log(player.name + " : Suppression de l'item " + inventory.slots[itemIndex].item.data.name + " dans le slot ( s" + inventory.slots[itemIndex].ToString() + " )");
            }

            Database.singleton.Mail_CreateMessage(name, messageTo, subject, body, itemName, expiration);

            //commit player immediately so if server went offline, any items/gold are not recovered
            Database.singleton.CharacterSave(player, true);

            TargetMail_SendResults(connectionToClient, "Mail Sent", true);
        }
        else
        {
            TargetMail_SendResults(connectionToClient, errors, false);
        }

        player.nextRiskyActionTime = NetworkTime.time + mailSettings.mailWaitSeconds;
#endif
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public long Mail_CalculateExpiration()
    {
        long expiration = 0;

        switch (mailSettings.expiresPart)
        {
            case DateInterval.Seconds:
                expiration = mailSettings.expiresAmount;
                break;

            case DateInterval.Minutes:
                expiration = mailSettings.expiresAmount * 60;
                break;

            case DateInterval.Hours:
                expiration = mailSettings.expiresAmount * 3600;
                break;

            case DateInterval.Days:
                expiration = mailSettings.expiresAmount * 86400;
                break;

            case DateInterval.Months:
                expiration = mailSettings.expiresAmount * 86400 * 30;
                break;

            case DateInterval.Years:
                expiration = mailSettings.expiresAmount * 86400 * 365;
                break;
        }

        return expiration * 1000; //convert to milliseconds
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    [TargetRpc]
    public void TargetMail_SendResults(NetworkConnection target, string status, bool success)
    {
        if (mail == null)
            mail = FindObjectOfType<UI_SendMail>();

        if (mail != null)
        {
            mail.MailMessageSent(status);
        }
    }

    // -----------------------------------------------------------------------------------
    //  This is a Server function
    // -----------------------------------------------------------------------------------
    [Command]
    public void CmdMail_Search(string searchString)
    {
#if _SERVER
        List<MailSearch> result = Database.singleton.Mail_SearchForCharacter(searchString, name);
        //serialize the result to string (easiest)
        string[] serialized = new string[result.Count];
        for (int i = 0; i < result.Count; i++)
        {
            serialized[i] = result[i].name + "|" + result[i].level + "|";
            if (result[i].guild != null)
            {
                serialized[i] += result[i].guild;
            }
        }

        TargetMail_SearchResults(connectionToClient, String.Join("&", serialized));

        player.nextRiskyActionTime = NetworkTime.time + mailSettings.mailWaitSeconds;
#endif
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    [TargetRpc]
    public void TargetMail_SearchResults(NetworkConnection target, string searchResults)
    {
        if (mail == null)
            mail = FindObjectOfType<UI_SendMail>();

        List<MailSearch> results = new List<MailSearch>();

        if (!string.IsNullOrEmpty(searchResults))
        {
            string[] tmp = searchResults.Split('&');

            for (int i = 0; i < tmp.Length; i++)
            {
                string[] tmp2 = tmp[i].Split('|');

                MailSearch res = new MailSearch();
                res.name = tmp2[0];
                int.TryParse(tmp2[1], out res.level);
                res.guild = tmp2[2];
                results.Add(res);
            }
        }

        if (mail != null)
        {
            mail.UpdateSearchResults(results);
        }
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    [Command]
    public void CmdMail_ReadMessage(int index)
    {
#if _SERVER
        if (index >= 0 && index < mailMessages.Count)
        {
            MailMessage message = mailMessages[index];
            message.read = Epoch.Current();
            mailMessages[index] = message;
            Database.singleton.Mail_UpdateMessage(message);
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    [Command]
    public void CmdMail_TakeItem(int index)
    {
#if _SERVER
        if (index >= 0 && index < mailMessages.Count)
        {
            MailMessage message = mailMessages[index];
            if (inventory.CanAdd(new Item(message.item), 1))
            {
                inventory.Add(new Item(message.item), 1);
                message.read = Epoch.Current();
                message.item = null;
                mailMessages[index] = message;
                Database.singleton.Mail_UpdateMessage(message);
            }
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    [Command]
    public void CmdMail_DeleteMessage(int index)
    {
#if _SERVER
        if (index >= 0 && index < mailMessages.Count)
        {
            MailMessage message = mailMessages[index];
            message.deleted = Epoch.Current();
            mailMessages.RemoveAt(index);
            Database.singleton.Mail_UpdateMessage(message);
        }

        player.nextRiskyActionTime = NetworkTime.time + mailSettings.mailWaitSeconds;
#endif
    }


    // drag & drop /////////////////////////////////////////////////////////////
    void OnDragAndDrop_InventorySlot_MailItemSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        ItemSlot slot = inventory.slots[slotIndices[0]];
        if (slot.item.tradable && slot.item.sellable && !slot.item.summoned)
        {
            mail.itemMailIndex = slotIndices[0];
            //mail.body.text = " bon l'indice est : " + slotIndices[0];
            //Debug.Log("nouvel indice slot la -> " + slotIndices[0]);
        }
    }

    void OnDragAndClear_MailItemSlot(int slotIndex)
    {
        if (slotIndex >= -1)
        {
            Debug.Log("clear = itemMailIndex -1 ");
            mail.itemMailIndex = -1;
        }
    }
}