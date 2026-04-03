#if _SERVER
#if _MYSQL
using MySql.Data.MySqlClient;
using System.Collections.Generic;
#elif _SQLITE
using SQLite;
#endif

// DATABASE (SQLite / mySQL Hybrid)
public partial class Database
{

#if _SQLITE
	// -----------------------------------------------------------------------------------
    // Character Travel Routes
    // -----------------------------------------------------------------------------------
    class character_travelroutes
    {
        public string character { get; set; }
        public string travelroute { get; set; }
    }
#endif
    private void Start_Tools_Travelroutes()
    {
        onConnected.AddListener(Connect_Travelroutes);
        onCharacterLoad.AddListener(CharacterLoad_Travelroutes);
        onCharacterSave.AddListener(CharacterSave_Travelroutes);
    }
    // -----------------------------------------------------------------------------------
    // Connect_Travelroutes
    // -----------------------------------------------------------------------------------
    private void Connect_Travelroutes()
    {
#if _MYSQL
        MySqlHelper.ExecuteNonQuery(connectionString, @"CREATE TABLE IF NOT EXISTS character_travelroutes (
				`character` VARCHAR(32) NOT NULL,
				travelroute VARCHAR(32) NOT NULL
				) CHARACTER SET=utf8mb4 ");
#elif _SQLITE
        connection.CreateTable<character_travelroutes>();
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterLoad_Travelroutes
    // -----------------------------------------------------------------------------------
    private void CharacterLoad_Travelroutes(Player player)
    {
#if _MYSQL
        var table = ExecuteReaderMySql("SELECT travelroute FROM character_travelroutes WHERE `character`=@name", new MySqlParameter("@name", player.name));
		foreach (var row in table) {
			TravelrouteClass tRoute = new TravelrouteClass((string)row[0]);
			player.playerTravelroute.travelroutes.Add(tRoute);
		}
#elif _SQLITE
        var table = connection.Query<character_travelroutes>("SELECT travelroute FROM character_travelroutes WHERE character=?", player.name);
        foreach (var row in table)
        {
            TravelrouteClass tRoute = new TravelrouteClass(row.travelroute);
            player.playerTravelroute.travelroutes.Add(tRoute);
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterSave_Travelroutes
    // -----------------------------------------------------------------------------------
    private void CharacterSave_Travelroutes(Player player)
    {
#if _MYSQL
        /*MySqlHelper.ExecuteNonQuery(connectionString, "DELETE FROM character_travelroutes WHERE `character`=@character", new MySqlParameter("@character", player.name));
		for (int i = 0; i < player.playerTravelroute.travelroutes.Count; ++i) {
            MySqlHelper.ExecuteNonQuery(connectionString, "INSERT INTO character_travelroutes VALUES (@character, @travelroute)",
 				new MySqlParameter("@character", player.name),
 				new MySqlParameter("@travelroute", player.playerTravelroute.travelroutes[i].name));
 		}*/
        // Supprimer les entr�es existantes pour ce personnage
        MySqlHelper.ExecuteNonQuery(connectionString, "DELETE FROM character_travelroutes WHERE `character`=@character", new MySqlParameter("@character", player.name));

        // Construction de la requ�te INSERT group�e
        string insertQuery = "INSERT INTO character_travelroutes (`character`, `travelroute`) VALUES ";

        List<MySqlParameter> parameters = new List<MySqlParameter>();
        int index = 0;

        for (int i = 0; i < player.playerTravelroute.travelroutes.Count; ++i)
        {
            if (index > 0)
                insertQuery += ",";

            insertQuery += $"(@character_{index}, @travelroute_{index})";

            parameters.Add(new MySqlParameter($"@character_{index}", player.name));
            parameters.Add(new MySqlParameter($"@travelroute_{index}", player.playerTravelroute.travelroutes[i].name));

            index++;
        }

        // Ex�cution de la requ�te si des param�tres ont �t� ajout�s
        if (parameters.Count > 0)
        {
            MySqlHelper.ExecuteNonQuery(connectionString, insertQuery, parameters.ToArray());
        }

#elif _SQLITE
        connection.Execute("DELETE FROM character_travelroutes WHERE character=?", player.name);
        for (int i = 0; i < player.playerTravelroute.travelroutes.Count; ++i)
        {
            connection.Insert(new character_travelroutes
            {
                character = player.name,
                travelroute = player.playerTravelroute.travelroutes[i].name
            });
        }
#endif
    }

    // -----------------------------------------------------------------------------------
}

#endif