#if _SERVER
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
    // Character Pvp Zones
    // -----------------------------------------------------------------------------------
    class character_realm
    {
        public string character { get; set; }
        public string realm { get; set; }
        public string alliedrealm { get; set; }
    }
#endif
    private void Start_Tools_PVPZone()
    {
        onConnected.AddListener(Connect_PVPZone);
        onCharacterLoad.AddListener(CharacterLoad_PVPZone);
        onCharacterSave.AddListener(CharacterSave_PVPZone);
    }
    // -----------------------------------------------------------------------------------
    // Connect_PVPZone
    // -----------------------------------------------------------------------------------
    private void Connect_PVPZone()
    {
#if _MYSQL
        MySqlHelper.ExecuteNonQuery(connectionString, @"CREATE TABLE IF NOT EXISTS character_realm (
			`character` VARCHAR(32) NOT NULL,
			realm VARCHAR(32) NOT NULL,
			alliedrealm VARCHAR(32) NOT NULL
		) CHARACTER SET=utf8mb4");
#elif _SQLITE
        connection.CreateTable<character_realm>();
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterLoad_PVPZone
    // -----------------------------------------------------------------------------------
    private void CharacterLoad_PVPZone(Player player)
    {
#if _MYSQL
        var table = ExecuteReaderMySql("SELECT realm, alliedrealm FROM character_realm WHERE `character`=@name", new MySqlParameter("@name", player.name));
#elif _SQLITE
        var table = connection.Query<character_realm>("SELECT realm, alliedrealm FROM character_realm WHERE character=?", player.name);
#endif
        if (table.Count == 1)
        {
            var row = table[0];
#if _MYSQL
            string realm = (string)row[0];
            string ally = (string)row[1];
            player.SetRealm(realm.GetStableHashCode(), ally.GetStableHashCode());
#elif _SQLITE
            string realm = row.realm;
            string ally = row.alliedrealm;
            player.SetRealm(realm.GetStableHashCode(), ally.GetStableHashCode());
#endif 
        }
    }

    // -----------------------------------------------------------------------------------
    // CharacterSave_PVPZone
    // -----------------------------------------------------------------------------------
    private void CharacterSave_PVPZone(Player player)
    {
#if _MYSQL
        MySqlHelper.ExecuteNonQuery(connectionString, "DELETE FROM character_realm WHERE `character`=@character", new MySqlParameter("@character", player.name));
        MySqlHelper.ExecuteNonQuery(connectionString, "INSERT INTO character_realm VALUES (@character, @realm, @alliedrealm)",
				new MySqlParameter("@character", 	player.name),
				new MySqlParameter("@realm", 		(player.Realm != null) ? player.Realm.name : ""),
				new MySqlParameter("@alliedrealm", 	(player.Ally != null) ? player.Ally.name : ""));
#elif _SQLITE
        connection.Execute("DELETE FROM character_realm WHERE character=?", player.name);
        connection.Insert(new character_realm
        {
            character = player.name,
            realm = (player.Realm != null) ? player.Realm.name : "",
            alliedrealm = (player.Ally != null) ? player.Ally.name : ""
        });
#endif
    }

    // -----------------------------------------------------------------------------------
}
#endif