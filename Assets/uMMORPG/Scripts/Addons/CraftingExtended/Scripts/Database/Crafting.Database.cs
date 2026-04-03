#if _SERVER && _iMMOCRAFTING

#if _MYSQL
using MySql.Data.MySqlClient;
#endif

// DATABASE (SQLite / mySQL Hybrid)
public partial class Database
{

#if _SQLITE
	// -----------------------------------------------------------------------------------
    // Character Crafts
    // -----------------------------------------------------------------------------------
    class character_crafts
    {
        public string character { get; set; }
        public string profession { get; set; }
        public long experience { get; set; }
    }

    // -----------------------------------------------------------------------------------
    // Character Recipes
    // -----------------------------------------------------------------------------------
    class character_recipes
    {
        public string character { get; set; }
        public string recipe { get; set; }
    }
#endif
    private void Start_Tools_Crafting()
    {
        onConnected.AddListener(Connect_Crafting);
        onCharacterLoad.AddListener(CharacterLoad_Crafting);
        onCharacterSave.AddListener(CharacterSave_Crafting);
    }
    // -----------------------------------------------------------------------------------
    // Connect_Crafting
    // -----------------------------------------------------------------------------------
    private void Connect_Crafting()
    {
#if _MYSQL
		ExecuteNonQueryMySql(@"CREATE TABLE IF NOT EXISTS character_crafts (
			`character` VARCHAR(32) NOT NULL,
			profession VARCHAR(32) NOT NULL,
			experience BIGINT
            ) CHARACTER SET=utf8mb4");

        ExecuteNonQueryMySql(@"CREATE TABLE IF NOT EXISTS character_recipes (
			`character` VARCHAR(32) NOT NULL,
			recipe VARCHAR(32) NOT NULL
             ) CHARACTER SET=utf8mb4");
#elif _SQLITE
        connection.CreateTable<character_crafts>();
        connection.CreateIndex(nameof(character_crafts), new[] { "character", "profession" });
        connection.CreateTable<character_recipes>();
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterLoad_Crafting
    // -----------------------------------------------------------------------------------
    private void CharacterLoad_Crafting(Player player)
    {
        if (player.playerCraftingExtended)
        {
#if _MYSQL
            var table = ExecuteReaderMySql("SELECT profession, experience FROM character_crafts WHERE `character`=@character",
                        new MySqlParameter("@character", player.name));

            foreach (var row in table)
            {
                CraftingProfession profession = new CraftingProfession((string)row[0]);
                profession.experience = (long)row[1];
                player.playerCraftingExtended.Crafts.Add(profession);
            }

            var table2 = ExecuteReaderMySql("SELECT recipe FROM character_recipes WHERE `character`=@name", new MySqlParameter("@name", player.name));
            foreach (var row in table2)
            {
                player.playerCraftingExtended._recipes.Add((string)row[0]);
            }
#elif _SQLITE
        var table = connection.Query<character_crafts>("SELECT profession, experience FROM character_crafts WHERE character=?", player.name);
        foreach (var row in table)
        {
            CraftingProfession profession = new CraftingProfession(row.profession);
            profession.experience = row.experience;
            player.playerCraftingExtended.Crafts.Add(profession);
        }

        var table2 = connection.Query<character_recipes>("SELECT recipe FROM character_recipes WHERE character=?", player.name);
        foreach (var row in table2)
            player.playerCraftingExtended._recipes.Add(row.recipe);
#endif
        }
    }

    // -----------------------------------------------------------------------------------
    // CharacterSave_Crafting
    // -----------------------------------------------------------------------------------
    private void CharacterSave_Crafting(Player player)
    {
        if (player.playerCraftingExtended)
        {
#if _MYSQL
            ExecuteNonQueryMySql("DELETE FROM character_crafts WHERE `character`=@character", new MySqlParameter("@character", player.name));
            foreach (var profession in player.playerCraftingExtended.Crafts)
                ExecuteNonQueryMySql("INSERT INTO character_crafts VALUES (@character, @profession, @experience)",
                                new MySqlParameter("@character", player.name),
                                new MySqlParameter("@profession", profession.templateName),
                                new MySqlParameter("@experience", profession.experience));

            ExecuteNonQueryMySql("DELETE FROM character_recipes WHERE `character`=@character", new MySqlParameter("@character", player.name));
            for (int i = 0; i < player.playerCraftingExtended._recipes.Count; ++i)
            {
                ExecuteNonQueryMySql("INSERT INTO character_recipes VALUES (@character, @recipe)",
                     new MySqlParameter("@character", player.name),
                     new MySqlParameter("@recipe", player.playerCraftingExtended._recipes[i]));
            }
#elif _SQLITE
        connection.Execute("DELETE FROM character_crafts WHERE character=?", player.name);
        foreach (var profession in player.playerCraftingExtended.Crafts)
            connection.InsertOrReplace(new character_crafts
            {
                character = player.name,
                profession = profession.templateName,
                experience = profession.experience
            });

        connection.Execute("DELETE FROM character_recipes WHERE character=?", player.name);
        for (int i = 0; i < player.playerCraftingExtended._recipes.Count; ++i)
        {
            connection.InsertOrReplace(new character_recipes
            {
                character = player.name,
                recipe = player.playerCraftingExtended._recipes[i]
            });
        }
#endif
        }
    }
    // -----------------------------------------------------------------------------------
}

#endif