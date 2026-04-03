// Contains all the network messages that we need.
using System.Collections.Generic;
using System.Linq;
using Mirror;

// client to server ////////////////////////////////////////////////////////////
public partial struct CharacterCreateMsg : NetworkMessage
{
    public string name;
    public int classIndex;
#if !_iMMO2D
    //public bool gameMaster; // only allowed if host connection!
#endif
}

public struct CharacterSelectMsg : NetworkMessage
{
    public int index;
}

public struct CharacterDeleteMsg : NetworkMessage
{
    public int index;
}

// server to client ////////////////////////////////////////////////////////////
// we need an error msg packet because we can't use TargetRpc with the Network-
// Manager, since it's not a MonoBehaviour.
public struct ErrorMsg : NetworkMessage
{
    public string text;
    public bool causesDisconnect;
}

public struct LoginSuccessMsg : NetworkMessage
{
}