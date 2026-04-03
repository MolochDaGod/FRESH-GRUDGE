using Mirror;
using System;
using UnityEngine;

public partial class PlayerAddonsConfigurator
{
    [Header("[-=-[ Report Configuration ]-=-]")]
    public Tmpl_ReportsConfiguration reportsConfiguration;

    public readonly SyncList<ReportsMember> reports = new SyncList<ReportsMember>();

    //Sends the command to the server database telling about the report and informs the player.
    [Command]
    public void CmdSendBugReport(string _title, string _message)
    {
#if _SERVER
        if (!reportsConfiguration.enableReport)
        {
            TargetHelpResponse(player.netIdentity, reportsConfiguration.messageInfo.identifierIn, "", reportsConfiguration.messageReportDisabled, "");
            return;
        }

        var lastReportTime = (reports.Count > 0) ? DateTime.Parse(reports[reports.Count - 1].time) : DateTime.Today.AddDays(-1);
        var timeSinceLastReport = DateTime.UtcNow.Subtract(lastReportTime);

        if (reports.Count > 0 && timeSinceLastReport.TotalMinutes <= reportsConfiguration.timeToReport)
        {
            var remaining = reportsConfiguration.timeToReport - (int)timeSinceLastReport.TotalMinutes;
            TargetHelpResponse(player.netIdentity, reportsConfiguration.messageInfo.identifierIn, "", string.Format(reportsConfiguration.messageCantReport, reportsConfiguration.timeToReport.ToString(), remaining), "");
        }
        else
        {
            SaveReports(_title, _message); // Envoyer les informations à la base de données du serveur
        }
#endif
    }


#if _SERVER
    [Server]
    private void SaveReports(string title, string message)
    {
        ReportsMember report = new ReportsMember()
        {
            readBefore = false,
            senderAcc = player.account,
            senderCharacter = player.name,
            title = title,
            message = message,
            solved = false,
            time = DateTime.UtcNow.ToString(),
            position = player.transform.position.ToString()
        };

        Database.singleton.SaveReports(report);
        reports.Clear();
        reports.Add(report);
        TargetHelpResponse(player.netIdentity, reportsConfiguration.messageInfo.identifierIn, "", reportsConfiguration.messageSent, "");
    }
#endif

    // Sends the response back to the player reporting the issue.
    [TargetRpc]
    private void TargetHelpResponse(NetworkIdentity identity, string sender, string identifier, string message, string replyPrefix)
    {
#if _CLIENT
        if (reportsConfiguration.messageInfo.textPrefab)
        {
#if _iMMOCOMPLETECHAT
            UICompleteChat.singleton.AddMessage(new ChatMessage(sender, identifier, message, replyPrefix, reportsConfiguration.messageInfo.textPrefab));
#else
            UIChat.singleton.AddMessage(new ChatMessage(sender, identifier, message, replyPrefix, reportsConfiguration.messageInfo.textPrefab));
#endif
        }
#if UNITY_EDITOR
        else
        {
            Debug.LogWarning("Please note that you have not defined the chat prefab in the PlayerReports component (Prefab "+ player.className + ")!");
        }
#endif
#endif
    }

}