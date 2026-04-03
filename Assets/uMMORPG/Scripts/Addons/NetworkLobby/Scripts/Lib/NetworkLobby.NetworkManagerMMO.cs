using UnityEngine;
using Mirror;
public enum NetworkSwitch { Yes, No }
public struct SwitchServerMsg : NetworkMessage
{
    public string scenePath;
    public string characterName;
}

// NETWORK MANAGER MMO

public partial class NetworkManagerMMO
{

    //public static new NetworkManager singleton { get; set; }
    [Header("[Component: Network Manager MMO Network Zones")]
    public NetworkManagerMMONetworkLobby networkManagerMMONetworkZones;

    // current network manager state on client
    [HideInInspector] public NetworkSwitch switching = NetworkSwitch.No;
}
