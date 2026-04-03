using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

// PLAYER COUNT AREA
#if _iMMO2D
[RequireComponent(typeof(CircleCollider2D))]
#else
[RequireComponent(typeof(SphereCollider))]
#endif
[RequireComponent(typeof(NetworkIdentity))]
public class LimitedZonesArea : NetworkBehaviour
{
    public LimitedZonesManager sharedInstanceManager;

    [Header("[REQUIREMENTS]")]
    public Tools_InteractionRequirements requirements;

    [Range(1, 500)] public int maxPlayersPerGroup = 5;
    public GroupType playerGroupType;
    public PlayerGroupLocations[] playerGroupLocation;

    protected List<Player> players = new List<Player>();

    [SyncVar, HideInInspector] public int playerCount = 0;
    public readonly SyncList<string> groupNames = new SyncList<string>();

    // -----------------------------------------------------------------------------------
    // Start
    // -----------------------------------------------------------------------------------
    private void Start()
    {
        if (!sharedInstanceManager)
            sharedInstanceManager = FindObjectOfType<LimitedZonesManager>();

        players = new List<Player>();
        players.Clear();
    }

    // =============================== TRIGGER FUNCTIONS =================================

    // -----------------------------------------------------------------------------------
    // OnTriggerEnter
    // @Server
    // -----------------------------------------------------------------------------------
    [ServerCallback]
#if _iMMO2D
    private void OnTriggerEnter2D(Collider2D co)
#else
    private void OnTriggerEnter(Collider co)
#endif
    {
        Player player = co.GetComponentInParent<Player>();

        if (player)
            addPlayer(player);
    }

    // -----------------------------------------------------------------------------------
    // OnTriggerExit
    // @Server
    // -----------------------------------------------------------------------------------
    [ServerCallback]
#if _iMMO2D
    private void OnTriggerExit2D(Collider2D co)
#else
    private void OnTriggerExit(Collider co)
#endif
    {
        Player player = co.GetComponentInParent<Player>();

        if (player)
            removePlayer(player);
    }

    // ================================ UPDATE FUNCTIONS =================================

    // -----------------------------------------------------------------------------------
    // addPlayer
    // Adds a player from the current instance and updates the group list
    // -----------------------------------------------------------------------------------
    protected void addPlayer(Player player)
    {
        if (playerGroupType == GroupType.Party)
        {
            updatePlayerParty(player, false);
        }
        else if (playerGroupType == GroupType.Guild)
        {
            updatePlayerGuild(player, false);
        }
 #if _iMMOPVP
        else if (playerGroupType == GroupType.Realm)
        {
            UpdatePlayerRealm(player, false);
        }
#endif
        players.Add(player);
        playerCount = players.Count;
    }

    // -----------------------------------------------------------------------------------
    // removePlayer
    // Removes a player from the current instance and updates the group list
    // -----------------------------------------------------------------------------------
    protected void removePlayer(Player player)
    {
        players.Remove(player);
        playerCount = players.Count;

        if (playerGroupType == GroupType.Party)
        {
            updatePlayerParty(player, true);
        }
        else if (playerGroupType == GroupType.Guild)
        {
            updatePlayerGuild(player, true);
        }
#if _iMMOPVP
        else if (playerGroupType == GroupType.Realm)
        {
            UpdatePlayerRealm(player, true);
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // updatePlayerParty
    // Checks if there are still players of <Party> around and updates the group list
    // -----------------------------------------------------------------------------------
    protected void updatePlayerParty(Player player, bool remove = false)
    {
        if (!player.party.InParty()) return;

        int count = 0;

        foreach (Player plyr in players)
        {
            if (plyr.party.InParty() && plyr.party.party.members[0] == player.party.party.members[0])
                count++;
        }

        if (count == 0)
        {
            if (remove)
                groupNames.Remove(player.party.party.members[0]);
            else
                groupNames.Add(player.party.party.members[0]);
        }
    }

    // -----------------------------------------------------------------------------------
    // updatePlayerGuild
    // Checks if there are still players of <Guild> around and updates the group list
    // -----------------------------------------------------------------------------------
    protected void updatePlayerGuild(Player player, bool remove = false)
    {
        if (!player.guild.InGuild()) return;

        int count = 0;

        foreach (Player plyr in players)
        {
            if (plyr.guild.InGuild() && plyr.guild.guild.name == player.guild.guild.name)
                count++;
        }

        if (count == 0)
        {
            if (remove)
                groupNames.Remove(player.guild.guild.name);
            else
                groupNames.Add(player.guild.guild.name);
        }
    }

    // -----------------------------------------------------------------------------------
    // UpdatePlayerRealm
    // Checks if there are still players of <Realm> around and updates the group list
    // -----------------------------------------------------------------------------------
#if _iMMOPVP
    protected void UpdatePlayerRealm(Player player, bool remove = false)
    {
        if (!player.Realm) return;

        int count = 0;

        foreach (Player plyr in players)
        {
            if (plyr.GetAlliedRealms(player))
                count++;
        }

        if (count == 0)
        {
            if (remove)
                groupNames.Remove(player.Realm.name);
            else
                groupNames.Add(player.Realm.name);
        }
    }
#endif
    // ================================ GETTER FUNCTIONS =================================

    // -----------------------------------------------------------------------------------
    // getPlayerCount
    // Returns the amount of players currently in this area
    // -----------------------------------------------------------------------------------
    public int getPlayerCount
    {
        get { return playerCount; }
    }

    // -----------------------------------------------------------------------------------
    // getMaxPlayerCount
    // Returns the maximum amount of players (per group) that can enter the area
    // -----------------------------------------------------------------------------------
    public int getMaxPlayerCount
    {
        get { return maxPlayersPerGroup; }
    }

    // -----------------------------------------------------------------------------------
    // getGroupCount
    // Returns the maximum amount of groups that can enter this area
    // -----------------------------------------------------------------------------------
    public int getGroupCount
    {
        get { return groupNames.Count; }
    }

    // -----------------------------------------------------------------------------------
    // getMaxGroupCount
    // Returns the maximum amount of groups that can enter this area
    // -----------------------------------------------------------------------------------
    public int getMaxGroupCount
    {
        get { return playerGroupLocation.Length; }
    }

    // -----------------------------------------------------------------------------------
    // getGroupIndex
    // The returns the index in the group array of the player
    // -----------------------------------------------------------------------------------
    public int getGroupIndex(Player player)
    {
        int index = -1;

        if (playerGroupType == GroupType.Party)
        {
            index = groupNames.IndexOf(player.party.party.members[0]);
        }
        else if (playerGroupType == GroupType.Guild)
        {
            index = groupNames.IndexOf(player.guild.guild.name);
        }
#if _iMMOPVP
        else if (playerGroupType == GroupType.Realm)
        {
            index = groupNames.IndexOf(player.Realm.name);
        }
#endif
        return index;
    }

    // ================================= CHECK FUNCTIONS =================================

    // -----------------------------------------------------------------------------------
    // canPlayerSee
    // Checks if the player can look at this instance in the menu
    // -----------------------------------------------------------------------------------
    public bool canPlayerSee(Player player)
    {
        return checkPlayerRequirements(player);
    }

    // -----------------------------------------------------------------------------------
    // canPlayerEnter
    // Checks <all> requirements, groups and limits of the player who wants to enter the area
    // -----------------------------------------------------------------------------------
    public bool canPlayerEnter(Player player)
    {
        return
                checkPlayerRequirements(player) &&
                checkPlayerGroup(player) &&
                checkGroupCount(player) &&
                (maxPlayersPerGroup == 0 || playerCount < maxPlayersPerGroup);
    }

    // -----------------------------------------------------------------------------------
    // checkPlayerRequirements
    // Checks if the player meets all the requirements in order to enter the area
    // -----------------------------------------------------------------------------------
    public bool checkPlayerRequirements(Player player)
    {
        return requirements.checkRequirements(player);
    }

    // -----------------------------------------------------------------------------------
    // checkPlayerGroup
    // Checks if the player meets the group requirements (Party, Guild or Realm)
    // -----------------------------------------------------------------------------------
    public bool checkPlayerGroup(Player player)
    {
        if (playerGroupType == GroupType.Party)
        {
            return player.party.InParty();
        }
        else if (playerGroupType == GroupType.Guild)
        {
            return player.guild.InGuild();
        }
#if _iMMOPVP
        else if (playerGroupType == GroupType.Realm)
        {
            return player.Realm != null;
        }
#endif
        return true;
    }

    // -----------------------------------------------------------------------------------
    // checkGroupCount
    // Checks if there is still space in the group (Party, Guild or Realm)
    // -----------------------------------------------------------------------------------
    public bool checkGroupCount(Player player)
    {
        if (playerGroupType == GroupType.Party)
        {
            return groupNames.Any(x => x == player.party.party.members[0]) || (getGroupCount < getMaxGroupCount);
        }
        else if (playerGroupType == GroupType.Guild)
        {
            return groupNames.Any(x => x == player.guild.guild.name) || (getGroupCount < getMaxGroupCount);
        }
#if _iMMOPVP
        else if (playerGroupType == GroupType.Realm)
        {
            return groupNames.Any(x => x == player.Realm.name) || (getGroupCount < getMaxGroupCount);
        }
#endif
        return true;
    }

    // -----------------------------------------------------------------------------------
}