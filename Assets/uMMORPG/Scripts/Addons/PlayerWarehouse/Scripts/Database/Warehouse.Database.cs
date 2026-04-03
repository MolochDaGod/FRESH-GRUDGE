#if _SERVER && _iMMOWAREHOUSE

#if _MYSQL
using System;
using System.Collections.Generic;

using MySql.Data.MySqlClient;
#elif _SQLITE
using SQLite;
#endif

// DATABASE (SQLite / mySQL Hybrid)

public partial class Database
{

#if _SQLITE
    // -----------------------------------------------------------------------------------
    // Warehouse
    // -----------------------------------------------------------------------------------
    class character_warehouse
    {
        [PrimaryKey] // important for performance: O(log n) instead of O(n)
        public string character { get; set; }
        public int gold { get; set; }
        public int level { get; set; }
    }

    // -----------------------------------------------------------------------------------
    // Warehouse Items
    // -----------------------------------------------------------------------------------
    class character_warehouse_items
    {
        public string character { get; set; }
        public int slot { get; set; }
        public string name { get; set; }
        public int amount { get; set; }
        public int summonedHealth { get; set; }
        public int summonedLevel { get; set; }
        public long summonedExperience { get; set; }
        public int equipmentLevel { get; set; }
        public string equipmentGems { get; set; }
    }
#endif
    private void Start_Tools__Warehouse()
    {
        onConnected.AddListener(Connect_Warehouse);
        onCharacterLoad.AddListener(CharacterLoad_Warehouse);
        onCharacterSave.AddListener(CharacterSave_Warehouse);
    }
    // -----------------------------------------------------------------------------------
    // Connect_Warehouse
    // -----------------------------------------------------------------------------------
    private void Connect_Warehouse()
    {

#if _MYSQL
		ExecuteNonQueryMySql(@"CREATE TABLE IF NOT EXISTS character_warehouse (
							`character` VARCHAR(32) NOT NULL PRIMARY KEY,
							gold INTEGER(16) NOT NULL DEFAULT 0,
							level INTEGER(16) NOT NULL DEFAULT 0
							) CHARACTER SET=utf8mb4");

		ExecuteNonQueryMySql(@"CREATE TABLE IF NOT EXISTS character_warehouse_items (
                           `character` VARCHAR(32) NOT NULL,
                           slot INTEGER(16) NOT NULL,
                           `name` VARCHAR(32) NOT NULL,
                           amount INTEGER(16) NOT NULL,
                           summonedHealth INTEGER NOT NULL,
                           summonedLevel INTEGER NOT NULL,
                           summonedExperience INTEGER NOT NULL,
                           equipmentLevel INTEGER NOT NULL DEFAULT 0,
                           equipmentGems VARCHAR(256) NOT NULL,
                           UNIQUE KEY `character_2` (`character`,`slot`)
                           ) CHARACTER SET=utf8mb4");
#elif _SQLITE
        connection.CreateTable<character_warehouse>();
        connection.CreateTable<character_warehouse_items>();
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterLoad_Warehouse
    // -----------------------------------------------------------------------------------
    private void CharacterLoad_Warehouse(Player player)
    {
        //player.warehouseConfiguration.warehouseActionDone = false;

#if _MYSQL

		var warehouseData = ExecuteReaderMySql("SELECT gold, level FROM character_warehouse WHERE `character`=@character", new MySqlParameter("@character", player.name));

		if (warehouseData.Count == 1) {
            player.playerAddonsConfigurator.playerWarehouseGold 		= Convert.ToInt32(warehouseData[0][0]);
            player.playerAddonsConfigurator.playerWarehouseLevel 	= Convert.ToInt32(warehouseData[0][1]);
		} else {
			ExecuteNonQueryMySql("INSERT INTO character_warehouse (`character`, gold, level) VALUES(@character, 0, 0)", new MySqlParameter("@character", player.name));
            player.playerAddonsConfigurator.playerWarehouseGold 		= 0;
            player.playerAddonsConfigurator.playerWarehouseLevel 	= 0;
		}

		for (int i = 0; i < player.playerAddonsConfigurator.playerWarehouseStorageItems; ++i)
			player.playerAddonsConfigurator.playerWarehouseItemSlot.Add(new ItemSlot());

		List<List<object>> table = ExecuteReaderMySql("SELECT `name`, slot, amount, summonedHealth, summonedLevel, summonedExperience, equipmentLevel, equipmentGems FROM character_warehouse_items WHERE `character`=@character", new MySqlParameter("@character", player.name));
		if (table.Count > 0) {
			foreach (List<object> row in table) {
				string itemName 	= (string)row[0];
				int slot 			= Convert.ToInt32(row[1]);
				ScriptableItem template;

				if (slot < player.playerAddonsConfigurator.playerWarehouseStorageItems && ScriptableItem.All.TryGetValue(itemName.GetStableHashCode(), out template)) {
					Item item 					= new Item(template);
					int amount 					= Convert.ToInt32(row[2]);
					item.summonedHealth 		= Convert.ToInt32(row[3]);
					item.summonedLevel 			= Convert.ToInt32(row[4]);
					item.summonedExperience 	= Convert.ToInt32(row[5]);
                    item.equipmentLevel         = Convert.ToInt32(row[6]);
                    item.equipmentGems          = row[7].ToString();
					player.playerAddonsConfigurator.playerWarehouseItemSlot[slot] = new ItemSlot(item, amount);
				}
			}
		}

#elif _SQLITE

        var warehouseData = connection.FindWithQuery<character_warehouse>("SELECT gold, level FROM character_warehouse WHERE character=?", player.name);
        if (warehouseData != null)
        {
            player.playerAddonsConfigurator.playerWarehouseGold = warehouseData.gold;
            player.playerAddonsConfigurator.playerWarehouseLevel = warehouseData.level;
        }
        else
        {
            connection.InsertOrReplace(new character_warehouse
            {
                character = player.name,
                gold = 0,
                level = 0
            });
            player.playerAddonsConfigurator.playerWarehouseGold = 0;
            player.playerAddonsConfigurator.playerWarehouseLevel = 0;
        }

        for (int i = 0; i < player.playerAddonsConfigurator.playerWarehouseStorageItems; ++i)
            player.playerAddonsConfigurator.playerWarehouseItemSlot.Add(new ItemSlot());

        var table = connection.Query<character_warehouse_items>("SELECT name, slot, amount, summonedHealth, summonedLevel, summonedExperience, equipmentLevel, equipmentGems FROM character_warehouse_items WHERE character=?", player.name);
        if (table.Count > 0)
        {
            foreach (var row in table)
            {
                string itemName = row.name;
                int slot = row.slot;
                ScriptableItem template;

                if (slot < player.playerAddonsConfigurator.playerWarehouseStorageItems && ScriptableItem.All.TryGetValue(itemName.GetStableHashCode(), out template))
                {
                    Item item = new Item(template);
                    int amount = row.amount;
                    item.summonedHealth = row.summonedHealth;
                    item.summonedLevel = row.summonedLevel;
                    item.summonedExperience = row.summonedExperience;
                    item.equipmentLevel = row.equipmentLevel;
                    item.equipmentGems = row.equipmentGems;
                    player.playerAddonsConfigurator.playerWarehouseItemSlot[slot] = new ItemSlot(item, amount);
                }
            }
        }

#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterSave_Warehouse
    // -----------------------------------------------------------------------------------
    private void CharacterSave_Warehouse(Player player)
    {
#if _MYSQL

		var characterWarehouseExists = ExecuteReaderMySql("SELECT 1 FROM character_warehouse WHERE `character`=@character", new MySqlParameter("@character", player.name));
        if (characterWarehouseExists.Count == 1)
        {
            ExecuteNonQueryMySql("UPDATE character_warehouse SET gold=@gold, level=@level WHERE `character`=@character",
                new MySqlParameter("@gold", player.playerAddonsConfigurator.playerWarehouseGold),
                new MySqlParameter("@level", player.playerAddonsConfigurator.playerWarehouseLevel),
                new MySqlParameter("@character", player.name));
        }
		

        // On clear la liste (pour évité les doublon)
        ExecuteNonQueryMySql("DELETE FROM character_warehouse_items WHERE `character`=@character", new MySqlParameter("@character", player.name));

        // Créez une liste de paramètres pour chaque slot non vide
        List<MySqlParameter> parameters = new List<MySqlParameter>();

        string insertQuery = "INSERT INTO character_warehouse_items VALUES ";

        // Creation d'une requêtes préparé avec toutes les donnée (si on à 100 slots on ne fait plus 100 requêtes mais une seule)
        for (int i = 0; i < player.playerAddonsConfigurator.playerWarehouseItemSlot.Count; ++i)
        {
            ItemSlot slot = player.playerAddonsConfigurator.playerWarehouseItemSlot[i];

            if (slot.amount > 0)
            {
                if (parameters.Count > 0)
                    insertQuery += ",";

                insertQuery += "(@character_"+i+", @slot_"+i+", @name_"+i+", @amount_"+i+", @petHealth_"+i+", @petLevel_"+i+", @petExperience_"+i+", @equipmentLevel_"+i+", @equipmentGems_"+i+")";

                parameters.Add(new MySqlParameter("@character_" + i, player.name));
                parameters.Add(new MySqlParameter("@slot_" + i, i));
                parameters.Add(new MySqlParameter("@name_" + i, slot.item.name));
                parameters.Add(new MySqlParameter("@amount_" + i, slot.amount));
                parameters.Add(new MySqlParameter("@petHealth_" + i, slot.item.summonedHealth));
                parameters.Add(new MySqlParameter("@petLevel_" + i, slot.item.summonedLevel));
                parameters.Add(new MySqlParameter("@petExperience_" + i, slot.item.summonedExperience));
                parameters.Add(new MySqlParameter("@equipmentLevel_" + i, slot.item.equipmentLevel));
                parameters.Add(new MySqlParameter("@equipmentGems_" + i, slot.item.equipmentGems));
            }
        }

        // Exécutez la requête SQL avec tous les paramètres
        if (parameters.Count > 0)
        {
            ExecuteNonQueryMySql(insertQuery, parameters.ToArray());
        }


#elif _SQLITE

        var characterWarehouseExists = connection.FindWithQuery<character_warehouse>("SELECT 1 FROM character_warehouse WHERE character=?", player.name);
        if (characterWarehouseExists != null)
        {
            connection.Execute("UPDATE character_warehouse SET gold=?, level=? WHERE character=?", player.playerAddonsConfigurator.playerWarehouseGold, player.playerAddonsConfigurator.playerWarehouseLevel, player.name);
        }

        //if (player.playerWarehouse.warehouseActionDone) {
            connection.Execute("DELETE FROM character_warehouse_items WHERE character=?", player.name);

            for (int i = 0; i < player.playerAddonsConfigurator.playerWarehouseItemSlot.Count; ++i)
            {
                ItemSlot slot = player.playerAddonsConfigurator.playerWarehouseItemSlot[i];

                if (slot.amount > 0)
                {
                    connection.Insert(new character_warehouse_items
                    {
                        character = player.name,
                        slot = i,
                        name = slot.item.name,
                        amount = slot.amount,
                        summonedHealth = slot.item.summonedHealth,
                        summonedLevel = slot.item.summonedLevel,
                        summonedExperience = slot.item.summonedExperience,
                        equipmentLevel = slot.item.equipmentLevel,
                        equipmentGems = slot.item.equipmentGems
                    });
                }
            }
        //}

#endif
    }

    // -----------------------------------------------------------------------------------
}
#endif