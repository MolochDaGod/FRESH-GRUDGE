using UnityEngine;

// PVPZONE_Settings

[System.Serializable]
public partial class PVPZONE_Settings
{
    [Header("[REQUIREMENTS]")]
    public Tools_Requirements requirements;

    [Header("[PVP SETTINGS]")]
    [Tooltip("OpenPvP - players can attack all other players (overrides all other settings)")]
    public bool OpenPvp = false;

    [Tooltip("RealmPvP - players can attack players of other realms, their allied realms or neutral realms - but not their own realm")]
    public bool RealmPvp = false;

    [Tooltip("GuildPvP - players can attack players of other guilds, but not their own guild (only checked if the player is in a guild)")]
    public bool GuildPvp = false;

    [Tooltip("PartyPvP - players can attack players in other parties, but not their own party members (only check if in a party)")]
    public bool PartyPvp = false;
}
