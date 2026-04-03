#if _SERVER
using System;

#if _MYSQL
using MySql.Data;
using MySql.Data.MySqlClient;
#elif _SQLITE
using SQLite;
#endif

// DATABASE (SQLite / mySQL Hybrid)

public partial class Database
{

#if _SQLITE
	// -----------------------------------------------------------------------------------
    // Character Timegates
    // -----------------------------------------------------------------------------------
    class character_timegates
    {
        public string character { get; set; }
        public string timegateName { get; set; }
        public int timegateCount { get; set; }
        public string timegateHours { get; set; }
    }
#endif
    private void Start_Tools_SimpleTimegate()
    {
        onConnected.AddListener(Connect_SimpleTimegate);
        onCharacterLoad.AddListener(CharacterLoad_SimpleTimegate);
        onCharacterSave.AddListener(CharacterSave_SimpleTimegate);
    }
    // -----------------------------------------------------------------------------------
    // Connect_SimpleTimegate
    // -----------------------------------------------------------------------------------
    private void Connect_SimpleTimegate()
    {
#if _MYSQL
		ExecuteNonQueryMySql(@"CREATE TABLE IF NOT EXISTS character_timegates (
			`character` VARCHAR(32) NOT NULL,
			timegateName TEXT NOT NULL,
			timegateCount INTEGER NOT NULL,
			timegateHours TEXT NOT NULL
              ) CHARACTER SET=utf8mb4");
#elif _SQLITE
        connection.CreateTable<character_timegates>();
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterLoad_SimpleTimegate
    // -----------------------------------------------------------------------------------
    private void CharacterLoad_SimpleTimegate(Player player)
    {
        if (player.playerAddonsConfigurator)
        {
            player.playerAddonsConfigurator.timegates.Clear();

#if _MYSQL
            var table = ExecuteReaderMySql("SELECT timegateName, timegateCount, timegateHours FROM character_timegates WHERE `character`=@name", new MySqlParameter("@name", player.name));
            foreach (var row in table)
            {
                Timegate timegate = new Timegate();
                timegate.name = (string)row[0];
                timegate.count = Convert.ToInt32((int)row[1]);
                timegate.hours = (string)row[2];
                timegate.valid = true;
                player.playerAddonsConfigurator.timegates.Add(timegate);
            }
#elif _SQLITE
        var table = connection.Query<character_timegates>("SELECT timegateName, timegateCount, timegateHours FROM character_timegates WHERE character=?", player.name);
        foreach (var row in table)
        {
            Timegate timegate = new Timegate();
            timegate.name = row.timegateName;
            timegate.count = row.timegateCount;
            timegate.hours = row.timegateHours;
            timegate.valid = true;
            player.playerAddonsConfigurator.timegates.Add(timegate);
        }
#endif
        }
    }

    // -----------------------------------------------------------------------------------
    // CharacterSave_SimpleTimegate
    // -----------------------------------------------------------------------------------
    private void CharacterSave_SimpleTimegate(Player player)
    {
        if (player.playerAddonsConfigurator)
        {
#if _MYSQL
            ExecuteNonQueryMySql("DELETE FROM character_timegates WHERE `character`=@character", new MySqlParameter("@character", player.name));
            for (int i = 0; i < player.playerAddonsConfigurator.timegates.Count; ++i)
            {
                ExecuteNonQueryMySql("INSERT INTO character_timegates VALUES (@character, @timegateName, @timegateCount, @timegateHours)",
                     new MySqlParameter("@character", player.name),
                     new MySqlParameter("@timegateName", player.playerAddonsConfigurator.timegates[i].name),
                     new MySqlParameter("@timegateCount", player.playerAddonsConfigurator.timegates[i].count),
                     new MySqlParameter("@timegateHours", player.playerAddonsConfigurator.timegates[i].hours));
            }
#elif _SQLITE
        connection.Execute("DELETE FROM character_timegates WHERE character=?", player.name);
        for (int i = 0; i < player.playerAddonsConfigurator.timegates.Count; ++i)
        {
            connection.Insert(new character_timegates
            {
                character = player.name,
                timegateName = player.playerAddonsConfigurator.timegates[i].name,
                timegateCount = player.playerAddonsConfigurator.timegates[i].count,
                timegateHours = player.playerAddonsConfigurator.timegates[i].hours
            });
        }
#endif
        }
    }
    // -----------------------------------------------------------------------------------
}
#endif