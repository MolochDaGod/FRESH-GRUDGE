using Mirror;
using System.Collections.Generic;
using UnityEngine;

// NETWORK MANAGER MMO
public class NetworkManagerMMOUnlockableClasses : MonoBehaviour
{
    public NetworkManagerMMO networkManagerMMO;

    [Header("[-=-[ Default Unlocked Classes ]-=-]")]
    public Tmpl_UnlockableClasses defaultUnlockedClasses;
#if _iMMOUNLOCKABLECLASSES

    [HideInInspector] public List<string> availableClasses = new List<string>();



    public void Start()
    {
#if _SERVER
        networkManagerMMO.onServerConnect.AddListener(OnServerConnect_UnlockableClasses);
#endif
#if _CLIENT
        networkManagerMMO.onClientConnect.AddListener(OnClientConnect_UnlockableClasses);
#endif
    }

#if _CLIENT
    // -----------------------------------------------------------------------------------
    // OnClientConnect_Tools_UnlockableClasses
    // -----------------------------------------------------------------------------------
    private void OnClientConnect_UnlockableClasses(NetworkConnection conn)
    {
        NetworkClient.RegisterHandler<ClassesAvailableMsg>(OnClientReceiveClassesAvailable);
    }

    // -----------------------------------------------------------------------------------
    // OnClientReceiveClassesAvailable
    // @Client
    // -----------------------------------------------------------------------------------
    public void OnClientReceiveClassesAvailable(ClassesAvailableMsg message)
    {

        if (!defaultUnlockedClasses) return;
        availableClasses = new List<string>();

        foreach (Player player in defaultUnlockedClasses.defaultClasses)
        {
            availableClasses.Add(player.name);
        }

        availableClasses.AddRange(message.unlockedClasses);
    }

    // -----------------------------------------------------------------------------------
    // HasUnlockedClass
    // -----------------------------------------------------------------------------------
    public bool HasUnlockedClass(Player player)
    {
        if (player == null || player.name == "") return false;
        return availableClasses.Contains(player.name);
    }


#endif

#if _SERVER
    // -----------------------------------------------------------------------------------
    // OnServerConnect_Tools_UnlockableClasses
    // @Server
    // -----------------------------------------------------------------------------------
    private void OnServerConnect_UnlockableClasses(NetworkConnection conn)
    {
        string account = networkManagerMMO.lobby[conn];
        if (!defaultUnlockedClasses) return;

        List<string> unlockedClasses = Database.singleton.GetUnlockedClasses(account);
        ClassesAvailableMsg message = new ClassesAvailableMsg { unlockedClasses = unlockedClasses.ToArray() };

        conn.Send(message);
    }
#endif
#endif
    // -----------------------------------------------------------------------------------
}
