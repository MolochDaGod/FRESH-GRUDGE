#if _SERVER && _iMMOBINDPOINT
using UnityEngine;

#if _MYSQL
//using MySql.Data;
using MySql.Data.MySqlClient;
#elif _SQLITE
using SQLite;
#endif

// DATABASE (SQLite / mySQL Hybrid)

public partial class Database
{

#if _SQLITE
	// -----------------------------------------------------------------------------------
    // Character Bindpoint
    // -----------------------------------------------------------------------------------
    class character_bindpoint
    {
        [PrimaryKey] // important for performance: O(log n) instead of O(n)
        public string character { get; set; }
        public string name { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public string sceneName { get; set; }
    }
#endif
    private void Start_Tools_Bindpoint()
    {
        onConnected.AddListener(Connect_Bindpoint);
        onCharacterLoad.AddListener(CharacterLoad_Bindpoint);
        onCharacterSave.AddListener(CharacterSave_Bindpoint);
    }

    // -----------------------------------------------------------------------------------
    // Connect_Bindpoint
    // -----------------------------------------------------------------------------------
    private void Connect_Bindpoint()
    {
#if _MYSQL
		ExecuteNonQueryMySql(@"CREATE TABLE IF NOT EXISTS character_bindpoint (
					 `character` VARCHAR(32) NOT NULL,
					 `name` VARCHAR(255) NOT NULL,
					x FLOAT NOT NULL,
            		y FLOAT NOT NULL,
            		z FLOAT NOT NULL,
            		sceneName VARCHAR(64) NOT NULL,
                    PRIMARY KEY(`character`)
                ) CHARACTER SET=utf8mb4");
#elif _SQLITE
        connection.CreateTable<character_bindpoint>();
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterSave_Bindpoint
    // -----------------------------------------------------------------------------------
    private void CharacterSave_Bindpoint(Player player)
    {
        if (!player.playerAddonsConfigurator.MyBindpoint.Valid) return;

#if _MYSQL
        ExecuteNonQueryMySql("DELETE FROM character_bindpoint WHERE `character`=@character", new MySqlParameter("@character", player.name));
        ExecuteNonQueryMySql("INSERT INTO character_bindpoint VALUES (@character, @name, @x, @y, @z, @sceneName)",
				new MySqlParameter("@character", 	player.name),
				new MySqlParameter("@name", 		player.playerAddonsConfigurator.MyBindpoint.name),
				new MySqlParameter("@x", 			player.playerAddonsConfigurator.MyBindpoint.position.x),
				new MySqlParameter("@y", 			player.playerAddonsConfigurator.MyBindpoint.position.y),
				new MySqlParameter("@z", 			player.playerAddonsConfigurator.MyBindpoint.position.z),
				new MySqlParameter("@sceneName", 	player.playerAddonsConfigurator.MyBindpoint.SceneName)
				);
#elif _SQLITE
        connection.Execute("DELETE FROM character_bindpoint WHERE character=?", player.name);
        connection.Insert(new character_bindpoint
        {
            character = player.name,
            name = player.playerAddonsConfigurator.MyBindpoint.name,
            x = player.playerAddonsConfigurator.MyBindpoint.position.x,
            y = player.playerAddonsConfigurator.MyBindpoint.position.y,
            z = player.playerAddonsConfigurator.MyBindpoint.position.z,
            sceneName = player.playerAddonsConfigurator.MyBindpoint.SceneName
        });
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterLoad_Bindpoint
    // -----------------------------------------------------------------------------------
    private void CharacterLoad_Bindpoint(Player player)
    {
        player.playerAddonsConfigurator.MyBindpoint = new BindPoint();
		
#if _MYSQL
		var table = ExecuteReaderMySql("SELECT name, x, y, z, sceneName FROM character_bindpoint WHERE `character`=@name", new MySqlParameter("@name", player.name));
#elif _SQLITE
        var table = connection.Query<character_bindpoint>("SELECT name, x, y, z, sceneName FROM character_bindpoint WHERE character=?", player.name);
#endif

        if (table.Count == 1)
        {
            var row = table[0];

#if _MYSQL
            Vector3 p = new Vector3((float)row[1], (float)row[2], (float)row[3]);
            string sceneName = (string)row[4];
#elif _SQLITE
            Vector3 p = new Vector3(row.x, row.y, row.z);
            string sceneName = row.sceneName;
#endif
            if (p != Vector3.zero && !string.IsNullOrEmpty(sceneName))
            {
#if _MYSQL
                player.playerAddonsConfigurator.MyBindpoint.name = (string)row[0];
#elif _SQLITE
                player.playerAddonsConfigurator.MyBindpoint.name = row.name;
#endif
                player.playerAddonsConfigurator.MyBindpoint.position = p;
                player.playerAddonsConfigurator.MyBindpoint.SceneName = sceneName;
            }
        }

    }

    // -----------------------------------------------------------------------------------
}
#endif