using UnityEngine;

// PLAYER MAIL SETTINGS
[CreateAssetMenu(fileName = "Mail Settings", menuName = "ADDON/Templates/New Mail Settings", order = 999)]
public class Tmpl_MailSettings : ScriptableObject
{
    [Header("[EXPIRATION]")]
    [Range(1, 999)] public int expiresAmount = 30;

    public DateInterval expiresPart = DateInterval.Days;

    [Header("[SEND]")]
    public bool mailSendFromAnywhere = true;

    [Range(0, 99)] public int mailWaitSeconds = 3;
    public Tools_Cost costPerMail;

    [Header("[RECEIVE]")]
    [Range(1, 999)] public int mailCheckSeconds = 30;

    [Header("[Mail Subject Length]")]
    [Range(1, 255)] public int subjectLength = 50;

    [Header("[Mail Body Length]")]
    [Range(1, 2048)] public int bodyLength = 1024;

    [Header("[ Force use Npc ]")]
    public bool forceUseNpc = false;

    [Header("[LABELS]")]
    public string labelRecipient = "Recipient missing!";
    public string labelSubject = "Subject missing!";
    public string labelBody = "Mail Body missing!";
    public string labelCost = "Cost not met!";
    public string labelSubjectTooLong = "Subject too long";
    public string labelBodyTooLong = "Body too long";
}