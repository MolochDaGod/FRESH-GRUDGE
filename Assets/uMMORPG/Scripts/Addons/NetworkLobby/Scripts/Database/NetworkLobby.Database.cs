#if _SERVER
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
//using System.Data.SqlClient;

#if _MYSQL
using MySql.Data;
using MySql.Data.MySqlClient;
using SqlParameter = MySql.Data.MySqlClient.MySqlParameter;
#elif _SQLITE
using SQLite;
#endif

// DATABASE (SQLite / mySQL Hybrid)

public partial class Database
{

#if _SQLITE
	// -----------------------------------------------------------------------------------
    // Character Scene
    // -----------------------------------------------------------------------------------
    class character_scene
    {
        [PrimaryKey] // important for performance: O(log n) instead of O(n)
        public string character { get; set; }
        public string scene { get; set; }
    }

#endif
    private void Start_Tools_NetworkZone()
    {
        onConnected.AddListener(Connect_Tools_NetworkLobby);
    }
    // -----------------------------------------------------------------------------------
    // Connect_UCE_NetworkZone
    // -----------------------------------------------------------------------------------
    private void Connect_Tools_NetworkLobby()
    {
#if _MYSQL
		ExecuteNonQueryMySql(@"
        CREATE TABLE IF NOT EXISTS character_scene (
            `character` VARCHAR(32) NOT NULL,
            scene VARCHAR(64) NOT NULL,
            PRIMARY KEY(`character`)
            )");


#elif _SQLITE
        connection.CreateTable<character_scene>();
#endif
    }

    // -----------------------------------------------------------------------------------
    // IsCharacterOnlineAnywhere
    // a character is online on any of the servers if the online string is not
    // empty and if the TimeLogout difference is less than the save interval * 2
    // (* 2 to have some tolerance)
    // -----------------------------------------------------------------------------------
    public bool IsCharacterOnlineAnywhere(string characterName)
    {
        float saveInterval = ((NetworkManagerMMO)NetworkManager.singleton).saveInterval;

#if _MYSQL
		var obj = ExecuteScalarMySql("SELECT online FROM characters WHERE name=@name", new SqlParameter("@name", characterName));
		if (obj != null)
        {
            var time = (DateTime)obj;
            double elapsedSeconds = (DateTime.UtcNow - time).TotalSeconds;
            return elapsedSeconds < saveInterval * 2;
        }
#elif _SQLITE
        object obj = connection.FindWithQuery<characters>("SELECT online FROM characters WHERE name=?", characterName);
        if (obj != null)
        {
            string online = (string)obj;
            if (online != "")
            {
                DateTime time = DateTime.Parse(online);
                double elapsedSeconds = (DateTime.UtcNow - time).TotalSeconds;

                return elapsedSeconds < saveInterval * 2;
            }
        }
#endif
        return false;
    }

    // -----------------------------------------------------------------------------------
    // AnyAccountCharacterOnline
    // -----------------------------------------------------------------------------------
    public bool AnyAccountCharacterOnline(string account)
    {
        List<string> characters = CharactersForAccount(account);
        return characters.Any(IsCharacterOnlineAnywhere);
    }

    // -----------------------------------------------------------------------------------
    // GetCharacterScene
    // -----------------------------------------------------------------------------------
    public string GetCharacterScene(string characterName)
    {
#if _MYSQL
        object obj = ExecuteScalarMySql("SELECT scene FROM character_scene WHERE `character`=@character", new SqlParameter("@character", characterName));
        return obj != null ? (string)obj : "";
#elif _SQLITE
        character_scene characterScene = connection.FindWithQuery<character_scene>("SELECT scene FROM character_scene WHERE character=?", characterName);
        return (characterScene != null) ? characterScene.scene : "";
#else
        return "";
#endif
    }

    // -----------------------------------------------------------------------------------
    // SaveCharacterScene
    // -----------------------------------------------------------------------------------
    public void SaveCharacterScene(string characterName, string sceneName)
    {
#if _MYSQL
		var query = @"INSERT INTO character_scene SET `character`=@character, scene=@scene ON DUPLICATE KEY UPDATE scene=@scene";

        ExecuteNonQueryMySql(query,
                             new SqlParameter("@character", characterName),
                             new SqlParameter("@scene", sceneName));
#elif _SQLITE
        connection.InsertOrReplace(new character_scene
        {
            character = characterName,
            scene = sceneName
        });
#endif
    }
}
#endif