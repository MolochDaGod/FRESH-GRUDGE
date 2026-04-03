#if _SERVER

#if _MYSQL
using MySql.Data.MySqlClient;
using System;

#elif _SQLITE
using SQLite;

#endif

// DATABASE (SQLite / mySQL Hybrid)
public partial class Database
{

#if _SQLITE
	// -----------------------------------------------------------------------------------
    // Character Factions
    // -----------------------------------------------------------------------------------
    class character_factions
    {
        public string character { get; set; }
        public string faction { get; set; }
        public int rating { get; set; }
    }
#endif

    private void Start_Tools_Factions()
    {
        onConnected.AddListener(Connect_Factions);
        onCharacterLoad.AddListener(CharacterLoad_Factions);
        onCharacterSave.AddListener(CharacterSave_Factions);
    }
    // -----------------------------------------------------------------------------------
    // Connect_Factions
    // -----------------------------------------------------------------------------------
    private void Connect_Factions()
    {
#if _MYSQL
		ExecuteNonQueryMySql(@"CREATE TABLE IF NOT EXISTS character_factions (
        				`character` VARCHAR(32) NOT NULL,
        				faction VARCHAR(32)  NOT NULL,
        				rating INTEGER(15),
                        primary key(`character`, faction)
        				) CHARACTER SET=utf8mb4 ");
#elif _SQLITE
        connection.CreateTable<character_factions>();
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterLoad_Factions
    // -----------------------------------------------------------------------------------
    private void CharacterLoad_Factions(Player player)
    {
        if (player.playerFactions)
        {
#if _MYSQL
            var table = ExecuteReaderMySql("SELECT faction, rating FROM character_factions WHERE `character`=@character", new MySqlParameter("@character", player.name));

            foreach (var row in table)
            {
                Faction faction = new Faction
                {
                    name = (string)row[0],
                    rating = Convert.ToInt32(row[1])
                };
                player.playerFactions.Factions.Add(faction);
            }
#elif _SQLITE
            var table = connection.Query<character_factions>("SELECT faction, rating FROM character_factions WHERE character=?", player.name);

            foreach (var row in table)
            {
                Faction faction = new Faction();
                faction.name = row.faction;
                faction.rating = row.rating;
                player.playerFactions.Factions.Add(faction);
            }
#endif
        }
    }

    // -----------------------------------------------------------------------------------
    // CharacterSave_Factions
    // -----------------------------------------------------------------------------------
    private void CharacterSave_Factions(Player player)
    {
        if (player.playerFactions)
        {
#if _MYSQL
            ExecuteNonQueryMySql("DELETE FROM character_factions WHERE `character` = @character", new MySqlParameter("@character", player.name));
            foreach (Faction faction in player.playerFactions.Factions)
                ExecuteNonQueryMySql("INSERT INTO character_factions VALUES (@character, @faction, @rating)",
                                new MySqlParameter("@character", player.name),
                                new MySqlParameter("@faction", faction.name),
                                new MySqlParameter("@rating", faction.rating));
#elif _SQLITE
            connection.Execute("DELETE FROM character_factions WHERE character=?", player.name);
            foreach (Faction faction in player.playerFactions.Factions)
                connection.Insert(new character_factions
                {
                    character = player.name,
                    faction = faction.name,
                    rating = faction.rating
                });
#endif
        }
    }

    // -----------------------------------------------------------------------------------
}
#endif
