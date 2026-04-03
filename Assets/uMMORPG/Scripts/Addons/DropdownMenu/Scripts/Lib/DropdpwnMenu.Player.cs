using Mirror;
using UnityEngine;

public partial class Player
{
    [HideInInspector] public int clickedPartyIndex;

    [Command]
    public void CmdPartyPromote(int index)
    {
        string[] members = party.party.members;

        for (int k = 1; k < members.Length; k++)
            PartySystem.KickFromParty(party.party.partyId, name, members[k]);

        PartySystem.LeaveParty(party.party.partyId, name);
        PartySystem.FormParty(members[index], name);

        Player invitor = onlinePlayers[members[index]];
        for (int a = 1; a < members.Length; a++)
            if (a != index)
                PartySystem.AddToParty(invitor.party.party.partyId, members[a]);
    }
}