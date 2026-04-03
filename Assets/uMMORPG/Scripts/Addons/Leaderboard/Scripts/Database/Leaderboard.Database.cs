#if _SERVER

#if _MYSQL
using MySql.Data.MySqlClient;
#endif
using UnityEngine;

public partial class Database
{

#if _SQLITE
    // -----------------------------------------------------------------------------------
    // Character Currencies
    // -----------------------------------------------------------------------------------
    class leaderboard
    {
        public string character { get; set; }
        public long death { get; set; }
        public long playerkill { get; set; }
        public long monsterkill { get; set; }
    }
#endif

    private void Start_Tools_Leaderboard()
    {
        onConnected.AddListener(Connect_Leaderboard);
        onCharacterLoad.AddListener(CharacterLoad_Leaderboard);
        onCharacterSave.AddListener(CharacterSave_Leaderboard);
    }
    // -----------------------------------------------------------------------------------
    // Connect_Tools_Leaderboard
    // -----------------------------------------------------------------------------------
    private void Connect_Leaderboard()
    {
#if _MYSQL
		ExecuteNonQueryMySql(@"CREATE TABLE IF NOT EXISTS `leaderboard` (
            `character` varchar(60) NOT NULL,
            `death` int(11) NOT NULL DEFAULT 0,
            `playerkill` int(11) NOT NULL DEFAULT 0,
            `monsterkill` int(11) NOT NULL DEFAULT 0,
            UNIQUE KEY `character` (`character`)
        )");
#elif _SQLITE
        Tools_connection.CreateTable<leaderboard>();
#endif
    }


    // -----------------------------------------------------------------------------------
    // CharacterLoad_Tools_Leaderboard
    // -----------------------------------------------------------------------------------
    private void CharacterLoad_Leaderboard(Player player)
    {
#if _MYSQL
		var table = ExecuteReaderMySql("SELECT death, playerkill, monsterkill FROM leaderboard WHERE `character`=@name", new MySqlParameter("@name", player.name));
        foreach (var row in table)
        {
            player.playerAddonsConfigurator.death = (int)row[0];
            player.playerAddonsConfigurator.playerKill = (int)row[1];
            player.playerAddonsConfigurator.monsterkill = (int)row[2];
        }
#elif _SQLITE
        var table = Tools_connection.Query<leaderboard>("SELECT death, playerkill, monsterkill FROM leaderboard WHERE character=?", player.name);
        foreach (var row in table)
        {
            player.playerAddonsConfigurator.death = (int)row.death;
            player.playerAddonsConfigurator.playerKill = (int)row.playerkill;
            player.playerAddonsConfigurator.monsterkill = (int)row.monsterkill;
        }
#endif
    }

    // enregistré les modification quand on save tout le jeu
    // -----------------------------------------------------------------------------------
    // CharacterSave_Tools_Statistics
    // -----------------------------------------------------------------------------------
    private void CharacterSave_Leaderboard(Player player)
    {
#if _MYSQL
		ExecuteNonQueryMySql("DELETE FROM leaderboard WHERE `character`=@character", new MySqlParameter("@character", player.name));
        ExecuteNonQueryMySql("INSERT INTO leaderboard VALUES (@character, @death, @playerkill, @monsterkill)",
                 new MySqlParameter("@character",   player.name),
                 new MySqlParameter("@death",       player.playerAddonsConfigurator.death),
                 new MySqlParameter("@playerkill",  player.playerAddonsConfigurator.playerKill),
                 new MySqlParameter("@monsterkill", player.playerAddonsConfigurator.monsterkill)
                 );
#elif _SQLITE
        Tools_connection.Execute("DELETE FROM leaderboard WHERE character=?", player.name);
        Tools_connection.InsertOrReplace(new leaderboard
        {
            character = player.name,
            death = player.playerAddonsConfigurator.death,
            playerkill = player.playerAddonsConfigurator.playerKill,
            monsterkill = player.playerAddonsConfigurator.monsterkill
        });
#endif
        // faire des recherches pour mettre à jour la liste ?
    }
}
#endif