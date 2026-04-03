
//(c8

using Mirror;

public partial class NetworkManagerMMO : NetworkManager
{
    public void OnServerDisconnectVehicle(NetworkConnection conn)
    {
        if (conn.identity != null)
        {
            Player player = conn.identity.GetComponent<Player>();

            if (player.playerVehicle.IsInsideVehicle())
            {
                player.playerVehicle.enterVehicle.ExitVehicleServer(player);
            }
        }
    }
}
