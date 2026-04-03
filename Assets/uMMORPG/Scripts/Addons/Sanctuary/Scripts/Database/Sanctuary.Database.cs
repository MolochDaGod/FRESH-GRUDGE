#if _SERVER
using System;
#if _MYSQL
using MySql.Data.MySqlClient;
#elif _SQLITE
using SQLite;
#endif

// DATABASE (SQLite / mySQL Hybrid)
public partial class Database
{
#if _SQLITE
    // -----------------------------------------------------------------------------------
    // Character Last Online
    // -----------------------------------------------------------------------------------
    class character_lastonline
    {
        [PrimaryKey] // important for performance: O(log n) instead of O(n)
        public string character { get; set; }
        public string lastOnline { get; set; }
    }
#endif

    private void Start_Tools_Sanctuary()
    {
        onConnected.AddListener(Connect_Sanctuary);
        onCharacterLoad.AddListener(CharacterLoad_Sanctuary);
        onCharacterSave.AddListener(CharacterSave_Sanctuary);
    }
    // -----------------------------------------------------------------------------------
    // Connect_Sanctuary
    // -----------------------------------------------------------------------------------
    private void Connect_Sanctuary()
    {
#if _MYSQL
		ExecuteNonQueryMySql(@"CREATE TABLE IF NOT EXISTS character_lastonline (
				`character` VARCHAR(32) NOT NULL,
				lastOnline VARCHAR(64) NOT NULL,
                    PRIMARY KEY(`character`)
                ) CHARACTER SET=utf8mb4");
#elif _SQLITE
        connection.CreateTable<character_lastonline>();
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterLoad_Sanctuary
    // -----------------------------------------------------------------------------------
    private void CharacterLoad_Sanctuary(Player player)
    {
#if _MYSQL
		var row = (string)ExecuteScalarMySql("SELECT lastOnline FROM character_lastonline WHERE `character`=@name", new MySqlParameter("@name", player.name));
		if (!string.IsNullOrWhiteSpace(row)) {
			DateTime time 				= DateTime.Parse(row);
			player.SecondsPassed 	= (DateTime.UtcNow - time).TotalSeconds;
		} else {
			player.SecondsPassed 	= 0;
		}
#elif _SQLITE
        var results = connection.FindWithQuery<character_lastonline>("SELECT lastOnline FROM character_lastonline WHERE character=?", player.name);
        string row = (results != null) ? results.lastOnline : "";
        if (!string.IsNullOrWhiteSpace(row))
        {
            DateTime time = DateTime.Parse(row);
            player.SecondsPassed = (DateTime.UtcNow - time).TotalSeconds;
        }
        else
        {
            player.SecondsPassed = 0;
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterSave_Sanctuary
    // -----------------------------------------------------------------------------------
    private void CharacterSave_Sanctuary(Player player)
    {
#if _MYSQL
		ExecuteNonQueryMySql("DELETE FROM character_lastonline WHERE `character`=@character", new MySqlParameter("@character", player.name));
        ExecuteNonQueryMySql("INSERT INTO character_lastonline VALUES (@character, @lastOnline)",
				new MySqlParameter("@lastOnline", DateTime.UtcNow.ToString("s")),
				new MySqlParameter("@character", player.name));
#elif _SQLITE
        connection.Execute("DELETE FROM character_lastonline WHERE character=?", player.name);
        connection.Insert(new character_lastonline
        {
            character = player.name,
            lastOnline = DateTime.UtcNow.ToString("s")
        });
#endif
    }

    // -----------------------------------------------------------------------------------
}
#endif