using Mirror;
using System;
using UnityEngine;

public partial class PlayerAddonsConfigurator 
{

    public readonly SyncList<Timegate> timegates = new SyncList<Timegate>();
    [SyncVar, HideInInspector] public Area_Timegate myTimegate;

    [Header("[-=-[ Timegate ]-=-]")]
    public GameEvent timegateGameEvent;
    // -----------------------------------------------------------------------------------
    // Cmd_SimpleTimegate
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_SimpleTimegate()
    {
#if _SERVER
        if (player.state == "IDLE" && ValidateTimegate())
        {
            SetSimpleTimegate(myTimegate);
            myTimegate.teleportationTarget.OnTeleport(player);
        }
#endif
    }

    [ClientRpc]
    public void EnteredInTimegate(Area_Timegate timegate)
    {
        myTimegate = timegate;
        timegateGameEvent.TriggerEventBool(true);
    }

    [ClientRpc]
    public void ExitTimegate()
    {
        myTimegate = null;
        timegateGameEvent.TriggerEventBool(false);
    }

    // -----------------------------------------------------------------------------------
    // validateTimegateTime
    // -----------------------------------------------------------------------------------
    public bool ValidateTimegateTime(string timestamp, int Hours)
    {
        if (Hours > 0)
        {
            DateTime time = DateTime.Parse(timestamp);
            double HoursPassed = (DateTime.UtcNow - time).TotalHours;
            return (HoursPassed >= Hours) ? true : false;
        }
        return true;
    }

    // -----------------------------------------------------------------------------------
    // GetTimegateIndexByName
    // -----------------------------------------------------------------------------------
    public int GetTimegateIndexByName(string gateName)
    {
        return timegates.FindIndex(t => t.name == gateName);
    }

#if _SERVER
    // -----------------------------------------------------------------------------------
    // validateTimegate
    // -----------------------------------------------------------------------------------
    private bool ValidateTimegate()
    {
        bool valid = false;
        if (myTimegate)
        {
            valid = (myTimegate.teleportationTarget.Valid);
            valid = (player.isAlive) ? true : false;
            valid = (myTimegate.dayStart == 0 || myTimegate.dayStart >= DateTime.UtcNow.Day) ? true : false;
            valid = (myTimegate.dayEnd == 0 || myTimegate.dayEnd <= DateTime.UtcNow.Day) ? true : false;
            valid = (myTimegate.activeMonth == 0 || myTimegate.activeMonth == DateTime.UtcNow.Month) ? true : false;

            int idx = GetTimegateIndexByName(myTimegate.name);

            if (idx > -1 && timegates[idx].valid)
            {
                valid = ((myTimegate.maxVisits == 0 || myTimegate.hoursBetweenVisits == 0) || timegates[idx].count < myTimegate.maxVisits && ValidateTimegateTime(timegates[idx].hours, myTimegate.hoursBetweenVisits));
            }
        }
        else
        {
            Debug.Log("myTimegate est vide ?");
        }
        return valid;
    }

    // -----------------------------------------------------------------------------------
    // SetSimpleTimegate
    // -----------------------------------------------------------------------------------
    private void SetSimpleTimegate(Area_Timegate targetTimegate)
    {
        // ---------- Update only if either Visits or Hours is set
        if (targetTimegate.maxVisits != 0 || targetTimegate.hoursBetweenVisits != 0)
        {
            bool done = false;
            int idx = GetTimegateIndexByName(targetTimegate.name);

            // -- Update existing Timegate entry
            if (idx > -1 && timegates[idx].valid && timegates[idx].name == targetTimegate.name)
            {
                Timegate myTimegate = new Timegate();
                myTimegate.name = targetTimegate.name;
                myTimegate.count = timegates[idx].count + 1;
                myTimegate.hours = DateTime.UtcNow.ToString("s");
                myTimegate.valid = true;
                timegates[idx] = myTimegate;
                done = true;
            }
            // -- Add new Timegate if it does not exist
            if (!done)
            {
                Timegate myTimegate = new Timegate();
                myTimegate.name = targetTimegate.name;
                myTimegate.count = 1;
                myTimegate.hours = DateTime.UtcNow.ToString("s");
                myTimegate.valid = true;
                timegates.Add(myTimegate);
            }
        }
    }

#endif
    // -----------------------------------------------------------------------------------
}
