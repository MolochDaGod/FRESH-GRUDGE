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
    // Character Currencies
    // -----------------------------------------------------------------------------------
    class character_currencies
    {
        public string character { get; set; }
        public string currency { get; set; }
        public long amount { get; set; }
        public long total { get; set; }
    }
#endif
    private void Start_Tools_HonorShop()
    {
        onConnected.AddListener(Connect_HonorShop);
        onCharacterLoad.AddListener(CharacterLoad_HonorShop);
        onCharacterSave.AddListener(CharacterSave_HonorShop);
    }
    // -----------------------------------------------------------------------------------
    // Connect_HonorShop
    // -----------------------------------------------------------------------------------
    private void Connect_HonorShop()
    {
#if _MYSQL
        MySqlHelper.ExecuteNonQuery(connectionString, @"CREATE TABLE IF NOT EXISTS character_currencies (
			`character` VARCHAR(32) NOT NULL,
			currency VARCHAR(32) NOT NULL,
			amount INTEGER(16) NOT NULL,
			total INTEGER(16) NOT NULL
		    )CHARACTER SET=utf8mb4");
#elif _SQLITE
        connection.CreateTable<character_currencies>();
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterLoad_HonorShop
    // -----------------------------------------------------------------------------------
    private void CharacterLoad_HonorShop(Player player)
    {
#if _MYSQL
        var table = ExecuteReaderMySql("SELECT currency, amount, total FROM character_currencies WHERE `character`=@name", new MySqlParameter("@name", player.name));
        foreach (var row in table)
        {
            string tmplName = (string)row[0];
            Tmpl_HonorCurrency tmplCurrency;

            if (Tmpl_HonorCurrency.All.TryGetValue(tmplName.GetStableHashCode(), out tmplCurrency))
            {
                HonorShopCurrency hsc = new HonorShopCurrency();
                hsc.honorCurrency = tmplCurrency;
                hsc.amount = (int)row[1];
                hsc.total = (int)row[2];
                player.playerHonorShop.honorCurrencies.Add(hsc);
            }
        }
#elif _SQLITE
        var table = connection.Query<character_currencies>("SELECT currency, amount, total FROM character_currencies WHERE character=?", player.name);
        foreach (var row in table)
        {
            string tmplName = row.currency;
            Tmpl_HonorCurrency tmplCurrency;

            if (Tmpl_HonorCurrency.All.TryGetValue(tmplName.GetStableHashCode(), out tmplCurrency))
            {
                HonorShopCurrency hsc = new HonorShopCurrency();
                hsc.honorCurrency = tmplCurrency;
                hsc.amount = row.amount;
                hsc.total = row.total;
                player.playerHonorShop.honorCurrencies.Add(hsc);
            }
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterSave_HonorShop
    // -----------------------------------------------------------------------------------
    private void CharacterSave_HonorShop(Player player)
    {
#if _MYSQL
        /* MySqlHelper.ExecuteNonQuery(connectionString, "DELETE FROM character_currencies WHERE `character`=@character", new MySqlParameter("@character", player.name));
         for (int i = 0; i < player.playerHonorShop.honorCurrencies.Count; ++i)
         {
             MySqlHelper.ExecuteNonQuery(connectionString, "INSERT INTO character_currencies VALUES (@character, @currency, @amount, @total)",
                  new MySqlParameter("@character", player.name),
                  new MySqlParameter("@currency", player.playerHonorShop.honorCurrencies[i].honorCurrency.name),
                  new MySqlParameter("@amount", player.playerHonorShop.honorCurrencies[i].amount),
                  new MySqlParameter("@total", player.playerHonorShop.honorCurrencies[i].total)
                  );
         }*/
        // Supprimer les entr�es existantes pour ce personnage
        MySqlHelper.ExecuteNonQuery(connectionString, "DELETE FROM character_currencies WHERE `character`=@character", new MySqlParameter("@character", player.name));

        // Construction de la requ�te INSERT group�e
        string insertQuery = "INSERT INTO character_currencies (`character`, `currency`, `amount`, `total`) VALUES ";

        List<MySqlParameter> parameters = new List<MySqlParameter>();
        int index = 0;

        for (int i = 0; i < player.playerHonorShop.honorCurrencies.Count; ++i)
        {
            if (index > 0)
                insertQuery += ",";

            insertQuery += $"(@character_{index}, @currency_{index}, @amount_{index}, @total_{index})";

            parameters.Add(new MySqlParameter($"@character_{index}", player.name));
            parameters.Add(new MySqlParameter($"@currency_{index}", player.playerHonorShop.honorCurrencies[i].honorCurrency.name));
            parameters.Add(new MySqlParameter($"@amount_{index}", player.playerHonorShop.honorCurrencies[i].amount));
            parameters.Add(new MySqlParameter($"@total_{index}", player.playerHonorShop.honorCurrencies[i].total));

            index++;
        }

        // Ex�cution de la requ�te si des param�tres ont �t� ajout�s
        if (parameters.Count > 0)
        {
            MySqlHelper.ExecuteNonQuery(connectionString, insertQuery, parameters.ToArray());
        }

#elif _SQLITE
        connection.Execute("DELETE FROM character_currencies WHERE character=?", player.name);
        for (int i = 0; i < player.playerHonorShop.honorCurrencies.Count; ++i)
            connection.InsertOrReplace(new character_currencies
            {
                character = player.name,
                currency = player.playerHonorShop.honorCurrencies[i].honorCurrency.name,
                amount = player.playerHonorShop.honorCurrencies[i].amount,
                total = player.playerHonorShop.honorCurrencies[i].total
            });
#endif
    }

    // -----------------------------------------------------------------------------------
}
#endif