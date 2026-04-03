#if _SERVER

#if _MYSQL
using MySql.Data;
using MySql.Data.MySqlClient;
using UnityEngine;

#elif _SQLITE
using SQLite;

#endif

// DATABASE (SQLite / mySQL Hybrid)
public partial class Database
{

#if _SQLITE
    // -----------------------------------------------------------------------------------
    // character_traits
    // -----------------------------------------------------------------------------------
    class character_traits
    {
        public string character { get; set; }
        public string name { get; set; }
    }
#endif
    private void Start_Tools_Traits()
    {
        onConnected.AddListener(Connect_Traits);
        onCharacterLoad.AddListener(CharacterLoad_Traits);
        onCharacterSave.AddListener(CharacterSave_Traits);
    }
    // -----------------------------------------------------------------------------------
    // Connect_Traits
    // -----------------------------------------------------------------------------------
    private void Connect_Traits()
    {
#if _MYSQL
		ExecuteNonQueryMySql(@"CREATE TABLE IF NOT EXISTS character_traits (`character` VARCHAR(32) NOT NULL, name VARCHAR(32) NOT NULL)");
#elif _SQLITE
        connection.CreateTable<character_traits>();
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterLoad_Traits
    // -----------------------------------------------------------------------------------
    private void CharacterLoad_Traits(Player player)
    {
#if _MYSQL
		var table = ExecuteReaderMySql("SELECT name FROM character_traits WHERE `character`=@character", new MySqlParameter("@character", player.name));
#elif _SQLITE
        var table = connection.Query<character_traits>("SELECT name FROM character_traits WHERE character=?", player.name);
#endif
        foreach (var row in table)
        {
#if _MYSQL
            TraitTemplate tmpl = TraitTemplate.dict[((string)row[0]).GetStableHashCode()];
#elif _SQLITE
            TraitTemplate tmpl = TraitTemplate.dict[row.name.GetStableHashCode()];
#endif
            player.playerTraits.Traits.Add(new Trait(tmpl));
        }
    }

    // -----------------------------------------------------------------------------------
    // CharacterSave_Traits
    // -----------------------------------------------------------------------------------
    private void CharacterSave_Traits(Player player)
    {
#if _MYSQL
		ExecuteNonQueryMySql("DELETE FROM character_traits WHERE `character`=@character", new MySqlParameter("@character", player.name));
        for (int i = 0; i < player.playerTraits.Traits.Count; ++i)
        {
            Trait trait = player.playerTraits.Traits[i];
            ExecuteNonQueryMySql("INSERT INTO character_traits VALUES (@character, @name)",
                    new MySqlParameter("@character", player.name),
                    new MySqlParameter("@name", trait.name)
                    );
        }
#elif _SQLITE
        connection.Execute("DELETE FROM character_traits WHERE character=?", player.name);
        for (int i = 0; i < player.playerTraits.Traits.Count; ++i)
        {
            Trait trait = player.playerTraits.Traits[i];
            connection.Insert(new character_traits
            {
                character = player.name,
                name = trait.name
            });
        }
#endif
    }

    // -----------------------------------------------------------------------------------
}
#endif