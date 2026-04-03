#if _SERVER && _iMMOPRESTIGECLASSES

#if _MYSQL
using MySql.Data.MySqlClient;
#endif

// DATABASE (SQLite / mySQL Hybrid)
public partial class Database
{

#if _SQLITE
	// -----------------------------------------------------------------------------------
    // Character Prestige Classes
    // -----------------------------------------------------------------------------------
    class character_prestigeclasses
    {
        public string character { get; set; }
        public string class1 { get; set; }
        public string class2 { get; set; }
    }
#endif
    private void Start_Tools_PrestigeClasses()
    {
        onConnected.AddListener(Connect_PrestigeClasses);
        onCharacterLoad.AddListener(CharacterLoad_PrestigeClasses);
        onCharacterSave.AddListener(CharacterSave_PrestigeClasses);
    }
    // -----------------------------------------------------------------------------------
    // Connect_PrestigeClasses
    // -----------------------------------------------------------------------------------
    private void Connect_PrestigeClasses()
    {
#if _MYSQL
		ExecuteReaderMySql(@"CREATE TABLE IF NOT EXISTS character_prestigeclasses (
			`character` VARCHAR(32) NOT NULL,
			class1 VARCHAR(32) NOT NULL,
			class2 VARCHAR(32) NOT NULL
		) CHARACTER SET=utf8mb4");
#elif _SQLITE
        connection.CreateTable<character_prestigeclasses>();
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterLoad_PrestigeClasses
    // -----------------------------------------------------------------------------------
    private void CharacterLoad_PrestigeClasses(Player player)
    {
//#if _MYSQL
//		var table = ExecuteReaderMySql("SELECT class1, class2 FROM character_prestigeclasses WHERE `character`=@name", new MySqlParameter("@name", player.name));
//#elif _SQLITE
//        var table = connection.Query<character_prestigeclasses>("SELECT class1, class2 FROM character_prestigeclasses WHERE character=?", player.name);
//#endif
//        if (table.Count == 1)
//        {
//            var row = table[0];
//#if _MYSQL
//            string class1 = (string)row[0];
//            string class2 = (string)row[1];
//#elif _SQLITE
//            string class1 = row.class1;
//            string class2 = row.class2;
//#endif
//            PrestigeClassTemplate prestigeClass1 = n```ull;
//            if (PrestigeClassTemplate.All.TryGetValue(class1.GetStableHashCode(), out prestigeClass1))
//                player.playerAddonsConfigurator.prestigeClass = prestigeClass1;
//        }
    }

    // -----------------------------------------------------------------------------------
    // CharacterSave_PrestigeClasses
    // -----------------------------------------------------------------------------------
    private void CharacterSave_PrestigeClasses(Player player)
    {
#if _MYSQL
		ExecuteNonQueryMySql("DELETE FROM character_prestigeclasses WHERE `character`=@character", new MySqlParameter("@character", player.name));
        ExecuteNonQueryMySql("INSERT INTO character_prestigeclasses VALUES (@character, @class1, @class2)",
				new MySqlParameter("@character", 	player.name),
				new MySqlParameter("@class1", 		(player.playerAddonsConfigurator.prestigeClass != null) ? player.playerAddonsConfigurator.prestigeClass.name : ""),
				new MySqlParameter("@class2", 		""));
#elif _SQLITE
        connection.Execute("DELETE FROM character_prestigeclasses WHERE character=?", player.name);
        connection.Insert(new character_prestigeclasses
        {
            character = player.name,
            class1 = (player.playerAddonsConfigurator.prestigeClass != null) ? player.playerAddonsConfigurator.prestigeClass.name : "",
            class2 = ""
        });
#endif
    }

    // -----------------------------------------------------------------------------------
}
#endif 
