using UnityEngine;

// ADMINISTRATION - CLASSES

// ---------------------------------------------------------------------------------------
// AdminCommandArgument
// ---------------------------------------------------------------------------------------
[System.Serializable]
public class AdminCommandArgument
{
    public enum AdminCommandArgumentType
    {
        TargetType,
        AnyName,
        PlayerName,
        ItemName,
        Integer,
        AnyText,
        MonsterName,
        NpcName
    }

    public AdminCommandArgumentType argumentType;
}

// ---------------------------------------------------------------------------------------
// AdminCommandList
// ---------------------------------------------------------------------------------------
[System.Serializable]
public class AdminCommandList
{
    [Header("[-=-[Commands]-=-]")]
    public Tmpl_AdminCommand[] commands;

    [Header("[-=-[Target Tags]-=-]")]
    public string tagTargetPlayer = "t";

    public string tagTargetParty = "p";
    public string tagTargetGuild = "g";
    public string tagTargetRealm = "r";
    public string tagTargetAll = "a";
}