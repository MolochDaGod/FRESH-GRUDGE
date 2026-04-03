#if _SERVER

#if _MYSQL
using MySql.Data.MySqlClient;

#elif _SQLITE
using SQLite;
#endif

#if _iMMOHARVESTING

// DATABASE (SQLite / mySQL Hybrid)
public partial class Database
{

#if _SQLITE
	// -----------------------------------------------------------------------------------
    // Character Professions
    // -----------------------------------------------------------------------------------
    class character_professions
    {
        public string character { get; set; }
        public string profession { get; set; }
        public long experience { get; set; }
    }
#endif
    private void Start_Tools_Harvesting()
    {
        onConnected.AddListener(Connect_Harvesting);
        onCharacterLoad.AddListener(CharacterLoad_Harvesting);
        onCharacterSave.AddListener(CharacterSave_Harvesting);
    }
    // -----------------------------------------------------------------------------------
    // Connect_Harvesting
    // -----------------------------------------------------------------------------------
    private void Connect_Harvesting()
    {
#if _MYSQL
		ExecuteNonQueryMySql(@"CREATE TABLE IF NOT EXISTS character_professions (
                    `character` VARCHAR(32) NOT NULL,
                    profession VARCHAR(32) NOT NULL,
                    experience BIGINT,
                     PRIMARY KEY(`character`, profession)
                    ) CHARACTER SET=utf8mb4");
#elif _SQLITE
        connection.CreateTable<character_professions>();
        connection.CreateIndex(nameof(character_professions), new[] { "character", "profession" });
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterLoad_Harvesting
    // -----------------------------------------------------------------------------------
    private void CharacterLoad_Harvesting(Player player)
    {
#if _MYSQL
		var table = ExecuteReaderMySql("SELECT profession, experience FROM character_professions WHERE `character`=@character", new MySqlParameter("@character", player.name));
        foreach (var row in table)
        {
            HarvestingProfession profession = new HarvestingProfession((string)row[0]);
            profession.experience = (long)row[1];
            player.playerHarvesting.Professions.Add(profession);
        }
#elif _SQLITE
        var table = connection.Query<character_professions>("SELECT profession, experience FROM character_professions WHERE character=?", player.name);
        foreach (var row in table)
        {
            HarvestingProfession profession = new HarvestingProfession(row.profession);
            profession.experience = row.experience;
            player.playerHarvesting.Professions.Add(profession);
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterSave_Harvesting
    // -----------------------------------------------------------------------------------
    private void CharacterSave_Harvesting(Player player)
    {
#if _MYSQL
		var query2 = @"INSERT INTO character_professions SET `character`=@character, profession=@profession, experience = @experience ON DUPLICATE KEY UPDATE `character`=@character, profession=@profession, experience = @experience";

        foreach (var profession in player.playerHarvesting.Professions)
            ExecuteNonQueryMySql(query2,
           new MySqlParameter("@character", player.name),
           new MySqlParameter("@profession", profession.templateName),
           new MySqlParameter("@experience", profession.experience));
#elif _SQLITE
        connection.Execute("DELETE FROM character_professions WHERE character=?", player.name);
        foreach (var profession in player.playerHarvesting.Professions)
            connection.InsertOrReplace(new character_professions
            {
                character = player.name,
                profession = profession.templateName,
                experience = profession.experience
            });
#endif
    }
    // -----------------------------------------------------------------------------------
}
#endif
#endif