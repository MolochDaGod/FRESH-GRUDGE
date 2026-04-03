using Mirror;
using System.Collections.Generic;
//using System.Linq;
using UnityEngine;

// NetworkManagerMMO

public partial class NetworkManagerMMOWorldEvents : MonoBehaviour
{
    public NetworkManagerMMO networkManagerMMO;
#if _iMMOWORLDEVENTS
    public static List<WorldEvent> WorldEvents = new List<WorldEvent>();
    public static bool WorldEventsChanged = true;


    // -----------------------------------------------------------------------------------
    // OnStartServer_Tools_WorldEvents
    // @Server
    // -----------------------------------------------------------------------------------
#if _SERVER
    public void Start()
    {
        networkManagerMMO.onStartServer.AddListener(OnStartServer_WorldEvents);
        networkManagerMMO.onServerDisconnect.AddListener(OnServerDisconnect_WorldEvents);
    }

    private void OnStartServer_WorldEvents()
    {
        if (WorldEventTemplate.All.Count > 0)
        {
            foreach (WorldEventTemplate template in WorldEventTemplate.All.Values)
            {
                WorldEvent e = new WorldEvent()
                {
                    name = template.name,
                    count = 0
                };
                WorldEvents.Add(e);
            }
        }
        Database.singleton.Load_WorldEvents();
        WorldEventsChanged = true;
    }
    // -----------------------------------------------------------------------------------
    // OnStopServer_Tools_WorldEvents
    // @Server
    // we save all world events when the server stops
    // -----------------------------------------------------------------------------------
    public void OnStopServer()
    {
        SaveWorldEvents();
    }

    // -----------------------------------------------------------------------------------
    // OnServerDisconnect_Tools_WorldEvents
    // @Server
    // we also save all world events when a client disconnects (thats not too often but
    // frequently enough
    // -----------------------------------------------------------------------------------
    private void OnServerDisconnect_WorldEvents(NetworkConnection conn)
    {
        SaveWorldEvents();
    }

    // -----------------------------------------------------------------------------------
    // SaveWorldEvents
    // we only save the world events when its required
    // @Server
    // -----------------------------------------------------------------------------------
    public static void SaveWorldEvents()
    {
        if (WorldEventsChanged)
        {
            Database.singleton.Save_WorldEvents();
            WorldEventsChanged = false;
        }
    }

    // -----------------------------------------------------------------------------------
    // SetWorldEventCount
    // @Server
    // -----------------------------------------------------------------------------------
    public static void SetWorldEventCount(string name, int value)
    {
        int id = WorldEvents.FindIndex(x => x.name == name);

        if (id != -1)
        {
            WorldEvent worldEvent = WorldEvents[id];
            worldEvent.count = value;
            WorldEvents[id] = worldEvent;
            WorldEventsChanged = true;
        }
    }

    // -----------------------------------------------------------------------------------
    // GetWorldEventCount
    // @Server
    // -----------------------------------------------------------------------------------
    public static int GetWorldEventCount(WorldEventTemplate ev)
    {
        foreach (var item in WorldEvents)
        {
            if (item.template == ev)
                return item.count;
        }
        return 0;
        //? remove linq NetworManagerMMOForceDisconnect
        //return WorldEvents.FirstOrDefault(x => x.template == ev).count;
    }

    // -----------------------------------------------------------------------------------
    // CheckWorldEvent
    // @Server
    // -----------------------------------------------------------------------------------
    public static bool CheckWorldEvent(WorldEventTemplate ev, int minCount, int maxCount)
    {
        if (ev == null) return false;
        if (minCount == 0 && maxCount == 0) return true;

        int count = GetWorldEventCount(ev);
        bool result = ( (minCount <= 0 || count >= minCount) && (maxCount <= 0 || count <= maxCount) );
        return result;
    }
    
    // -----------------------------------------------------------------------------------
    // ModifyWorldEventCount
    // @Server
    // -----------------------------------------------------------------------------------
    public static void ModifyWorldEventCount(WorldEventTemplate ev, int value)
    {
        int id = WorldEvents.FindIndex(x => x.template == ev);

        if (id != -1)
        {
            WorldEvent e = WorldEvents[id];
            e.Modify(value);
            WorldEvents[id] = e;
            WorldEventsChanged = true;
        }
    }

    // -----------------------------------------------------------------------------------
    // BroadCastPopupToOnlinePlayers
    // @Server
    // -----------------------------------------------------------------------------------
    public static void BroadCastPopupToOnlinePlayers(WorldEventTemplate ev, bool participatedOnly, string message)
    {
        foreach (Player player in Player.onlinePlayers.Values)
        {
            foreach (var item in player.playerAddonsConfigurator.WorldEvents)
            {
                if (!participatedOnly || (participatedOnly && item.template == ev))
                    player.Tools_ShowPopup(message);
            }
        }
    }

#endif
    // -----------------------------------------------------------------------------------
#endif
}
