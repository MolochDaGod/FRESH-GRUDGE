#if _SERVER && _iMMOWORLDEVENTS
#if _MYSQL
using MySql.Data.MySqlClient;
using UnityEngine;
#elif _SQLITE
using SQLite;
#endif

// DATABASE (SQLite / mySQL Hybrid)
public partial class Database
{

#if _SQLITE
	// -----------------------------------------------------------------------------------
    // World Events
    // -----------------------------------------------------------------------------------
    class server_worldevents
    {
        [PrimaryKey] // important for performance: O(log n) instead of O(n)
        public string name { get; set; }
        public int count { get; set; }
    }
#endif
    private void Start_Tools_WorldEvents()
    {
        onConnected.AddListener(Connect_WorldEvents);
        onCharacterLoad.AddListener(CharacterLoad_WorldEvents);
        onCharacterSave.AddListener(CharacterSave_WorldEvents);
        
    }
    // -----------------------------------------------------------------------------------
    // Connect_WorldEvents
    // -----------------------------------------------------------------------------------
    public void Connect_WorldEvents()
    {
#if _MYSQL
        ExecuteNonQueryMySql(@"CREATE TABLE IF NOT EXISTS server_worldevents (`name` VARCHAR(64) NOT NULL, `count` INTEGER NOT NULL) CHARACTER SET=utf8mb4");
#elif _SQLITE
        connection.CreateTable<server_worldevents>();
#endif
    }

    // -----------------------------------------------------------------------------------
    // Load_WorldEvents
    // -----------------------------------------------------------------------------------
    public void Load_WorldEvents()
    {
#if _MYSQL
		var table = ExecuteReaderMySql("SELECT `name`, `count` FROM server_worldevents");
		foreach (var row in table) {
			string name = (string)row[0];
			int count 	= (int)row[1];

			if (!string.IsNullOrWhiteSpace(name) && count != 0)
			{
				NetworkManagerMMOWorldEvents.SetWorldEventCount(name, count);
			}
		}
#elif _SQLITE
        var table = connection.Query<server_worldevents>("SELECT `name`, `count` FROM server_worldevents");
        foreach (var row in table)
        {
            string name = row.name;
            int count = row.count;

            if (!string.IsNullOrWhiteSpace(name) && count != 0)
            {
                NetworkManagerMMOWorldEvents.SetWorldEventCount(name, count);
            }
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // Save_WorldEvents
    // -----------------------------------------------------------------------------------
    public void Save_WorldEvents()
    {
#if _MYSQL
		ExecuteNonQueryMySql("DELETE FROM server_worldevents");
        foreach (WorldEvent worldEvent in NetworkManagerMMOWorldEvents.WorldEvents)
        {
            ExecuteNonQueryMySql("INSERT INTO server_worldevents VALUES (@name, @count)",
                 new MySqlParameter("@name", worldEvent.name),
                 new MySqlParameter("@count", worldEvent.count)
                 );
        }
#elif _SQLITE
        connection.Execute("DELETE FROM server_worldevents");
        foreach (WorldEvent worldEvent in NetworkManagerMMOWorldEvents.WorldEvents)
            connection.Insert(new server_worldevents
            {
                name = worldEvent.name,
                count = worldEvent.count
            });
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterLoad_Tools_WorldEvents
    // refresh the world event list once a character is loaded to populate it with data
    // -----------------------------------------------------------------------------------
    private void CharacterLoad_WorldEvents(Player player)
    {
        player.playerAddonsConfigurator.WorldEvents.Clear();
        foreach (WorldEvent worldEvent in NetworkManagerMMOWorldEvents.WorldEvents)
        {
            WorldEvent playerWorldEvent = new WorldEvent
            {
                name = worldEvent.name,
                count = worldEvent.count
            };
            player.playerAddonsConfigurator.WorldEvents.Add(playerWorldEvent);
        }
    }

    // -----------------------------------------------------------------------------------
    // CharacterSave_WorldEvents
    // refresh the world event list every TimeLogout a character is saved to keep it in sync
    // -----------------------------------------------------------------------------------
    private void CharacterSave_WorldEvents(Player player)
    {
        foreach (WorldEvent worldEvent in NetworkManagerMMOWorldEvents.WorldEvents)
        {
            int id = player.playerAddonsConfigurator.WorldEvents.FindIndex(x => x.template == worldEvent.template);

            if (id != -1)
            {
                WorldEvent playerWorldEvent = player.playerAddonsConfigurator.WorldEvents[id];
                playerWorldEvent.count = worldEvent.count;
                player.playerAddonsConfigurator.WorldEvents[id] = playerWorldEvent;
            }
        }

        // -- we save the world events as well here, but only if they changed and only once (not for every player)
        NetworkManagerMMOWorldEvents.SaveWorldEvents();
    }

    // -----------------------------------------------------------------------------------
}
#endif
