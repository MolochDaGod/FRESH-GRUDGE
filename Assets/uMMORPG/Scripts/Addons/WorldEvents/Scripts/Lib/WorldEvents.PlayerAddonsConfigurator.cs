using Mirror;
using System.Linq;

// PLAYER
public partial class PlayerAddonsConfigurator
{
#if _iMMOWORLDEVENTS
    public readonly SyncList<WorldEvent> WorldEvents = new SyncList<WorldEvent>();

    // -----------------------------------------------------------------------------------
    // CheckWorldEvent
    // -----------------------------------------------------------------------------------
    public bool CheckWorldEvent(WorldEventTemplate events, int minCount, int maxCount)
    {
        if (events == null || (minCount == 0 && maxCount == 0)) return true;

        int count = GetWorldEventCount(events);

        if (count == 0) return false;

        return ((minCount == 0 || count >= minCount) && (maxCount == 0 || count <= maxCount));
    }

    // -----------------------------------------------------------------------------------
    // GetWorldEventCount
    // -----------------------------------------------------------------------------------
    public int GetWorldEventCount(WorldEventTemplate events)
    {
        return WorldEvents.FirstOrDefault(x => x.template == events).count;
    }

#if _SERVER
    // -----------------------------------------------------------------------------------
    // ModifyWorldEventCount
    // @server
    // -----------------------------------------------------------------------------------
    public void ModifyWorldEventCount(WorldEventTemplate events, int value)
    {

        // -- update the players event list

        int id = WorldEvents.FindIndex(x => x.template == events);

        if (id != -1)
        {
            WorldEvent currentEvent = WorldEvents[id];
            currentEvent.Modify(value, false);
            WorldEvents[id] = currentEvent;
        }

        // -- update the global event list as well
        NetworkManagerMMOWorldEvents.ModifyWorldEventCount(events, value);

    }
#endif
#endif
    // -----------------------------------------------------------------------------------
}