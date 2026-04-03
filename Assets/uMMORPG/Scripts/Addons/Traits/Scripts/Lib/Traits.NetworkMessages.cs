using Mirror;

// client to server
#pragma warning disable CS0282
public partial struct CharacterCreateMsg : NetworkMessage
{
    public int[] traits;
}
#pragma warning restore CS0282