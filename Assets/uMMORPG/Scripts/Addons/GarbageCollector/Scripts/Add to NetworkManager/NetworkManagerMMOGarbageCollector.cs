using UnityEngine;
using Mirror;

// NetworkManagerMMO
public partial class NetworkManagerMMOGarbageCollector : MonoBehaviour
{
    public NetworkManagerMMO networkManagerMMO;
    // -----------------------------------------------------------------------------------
    // OnStartServer_GarbageCollector
    // -----------------------------------------------------------------------------------
#if _SERVER
    public void Start()
    {
        Invoke("GarbageCollection", 3);
        networkManagerMMO.onServerDisconnect.AddListener(OnServerDisconnect_GarbageCollector);
    }

    // -----------------------------------------------------------------------------------
    // OnServerDisconnect_GarbageCollector
    // -----------------------------------------------------------------------------------
    private void OnServerDisconnect_GarbageCollector(NetworkConnection conn)
    {
        if (Player.onlinePlayers.Count <= 1)
            GarbageCollection();
    }

    // -----------------------------------------------------------------------------------
    // GarbageCollection
    // -----------------------------------------------------------------------------------
    protected void GarbageCollection()
    {
        System.GC.Collect();
        Debug.Log("System Garbage Collection called...");
    }
#endif
    // -----------------------------------------------------------------------------------
}