#if _SERVER && _iMMOGUILDWAREHOUSE
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection.Emit;

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
    // Guild Warehouse
    // -----------------------------------------------------------------------------------
    class guild_warehouse
    {
        [PrimaryKey] // important for performance: O(log n) instead of O(n)
        public string guild { get; set; }
        public int gold { get; set; }
        public int level { get; set; }
        public int locked { get; set; }
        public int busy { get; set; }
    }
    
    // -----------------------------------------------------------------------------------
    // Guild Warehouse Items
    // -----------------------------------------------------------------------------------
    class guild_warehouse_items
    {
        public string guild { get; set; }
        public int slot { get; set; }
        public string name { get; set; }
        public int amount { get; set; }
        public int summonedHealth { get; set; }
        public int summonedLevel { get; set; }
        public long summonedExperience { get; set; }
        public int equipmentLevel { get; set; }
        public string equipmentGems { get; set; }
    }

    class guild_warehouse_logs
    {
        public string guild { get; set; }
        public string player { get; set; }
        public int slot { get; set; }
        public long gold { get; set; }
        public int levelup { get; set; }
        public string name { get; set; }
        public int amount { get; set; }
        public int added { get; set; }
        public string actionDate { get; set; }
                  /*`player` varchar(32) CHARACTER SET utf8 NOT NULL,
                  `slot` int (4) DEFAULT NULL,
                   `gold` int (16) DEFAULT NULL,
                    `levelup` int (16) DEFAULT NULL,
                     `name` varchar(255) CHARACTER SET utf8 DEFAULT NULL,
                  `amount` int (6) DEFAULT NULL,
                   `added` int (1) NOT NULL DEFAULT 0,*/
    }
#endif
    private void Start_Tools_GuildWarehouse()
    {
        onConnected.AddListener(Connect_GuildWareHouse);
    }

    // -----------------------------------------------------------------------------------
    // Connect_GuildWareHouse
    // -----------------------------------------------------------------------------------
    private void Connect_GuildWareHouse()
    {
#if _MYSQL
		ExecuteNonQueryMySql(@"
            CREATE TABLE IF NOT EXISTS guild_warehouse(
			        guild VARCHAR(32) NOT NULL,
					gold INTEGER(16) NOT NULL DEFAULT 0,
					level INTEGER(16) NOT NULL DEFAULT 0,
					locked INTEGER(1) NOT NULL DEFAULT 0,
					busy INTEGER(1) NOT NULL DEFAULT 0,
                    PRIMARY KEY(guild)
                  ) CHARACTER SET=utf8mb4");

        ExecuteNonQueryMySql(@"
           CREATE TABLE IF NOT EXISTS guild_warehouse_items(
                    guild VARCHAR(32) NOT NULL,
                    slot INTEGER(16) NOT NULL,
                    name TEXT(16) NOT NULL,
                    amount INTEGER(16) NOT NULL,
                    summonedHealth INTEGER NOT NULL,
                    summonedLevel INTEGER NOT NULL,
                    summonedExperience INTEGER NOT NULL,
                    equipmentLevel INTEGER NOT NULL DEFAULT 0,
                    equipmentGems VARCHAR(256) NOT NULL,
                    UNIQUE KEY `guild_key` (`guild`,`slot`)
                  ) CHARACTER SET=utf8mb4");
        ExecuteNonQueryMySql(@"
            CREATE TABLE IF NOT EXISTS `guild_warehouse_logs` (
              `id` int(11) NOT NULL AUTO_INCREMENT,
              `actionDate` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
              `guild` varchar(32) CHARACTER SET utf8 NOT NULL,
              `player` varchar(32) CHARACTER SET utf8 NOT NULL,
              `slot` int(4) DEFAULT NULL,
              `gold` int(16) DEFAULT NULL,
              `levelup` int(16) DEFAULT NULL,
              `name` varchar(255) CHARACTER SET utf8 DEFAULT NULL,
              `amount` int(6) DEFAULT NULL,
              `added` int(1) NOT NULL DEFAULT 0,
              PRIMARY KEY (`id`)
            ) ENGINE=MyISAM DEFAULT CHARSET=latin1;");

#elif _SQLITE
        connection.CreateTable<guild_warehouse>();
        connection.CreateTable<guild_warehouse_items>();
        connection.CreateTable<guild_warehouse_logs>();
#endif
    }

    // -----------------------------------------------------------------------------------
    // LoadGuildWarehouse
    // -----------------------------------------------------------------------------------
    public bool LoadGuildWarehouse(Player player)
    {
        if (player.playerAddonsConfigurator.guildWarehouseConfiguration != null)
        {
            player.playerAddonsConfigurator.ResetGuildWarehouse();
            if (!player.guild.InGuild()) return false;

#if _MYSQL

		    var warehouseData = ExecuteReaderMySql("SELECT gold, level, locked FROM guild_warehouse WHERE guild=@guild", new MySqlParameter("@guild", player.guild.guild.name));

            if (warehouseData.Count == 0)
            {
                ExecuteNonQueryMySql("INSERT INTO guild_warehouse (guild, gold, level, locked, busy) VALUES(@guild, 0, 0, 0, 0)", new MySqlParameter("@guild", player.guild.guild.name));
                player.playerAddonsConfigurator.guildWarehouseGold 	= 0;
                player.playerAddonsConfigurator.guildWarehouseLevel 	= 0;
            }
            else
            {
                player.playerAddonsConfigurator.guildWarehouseGold = Convert.ToInt32(warehouseData[0][0]);
                player.playerAddonsConfigurator.guildWarehouseLevel = Convert.ToInt32(warehouseData[0][1]);
            }

            // récupération des data des donnée de warehouse guild
		    var table = ExecuteReaderMySql("SELECT name, slot, amount, summonedHealth, summonedLevel, summonedExperience, equipmentLevel, equipmentGems FROM guild_warehouse_items WHERE guild=@guild", new MySqlParameter("@guild", player.guild.guild.name));


            int toalSlot = player.playerAddonsConfigurator.guildWarehouseStorageItems;
            
            for (int i = 0; i < player.playerAddonsConfigurator.guildWarehouseStorageItems; ++i) {
			    player.playerAddonsConfigurator.guildWarehouseItemSlot.Add(new ItemSlot());
		    }

            if (table.Count > 0)
            {
                foreach (var row in table)
                {
                    string itemName 	= (string)row[0];
                    int slot 			= Convert.ToInt32(row[1]);
                    ScriptableItem template;
                    if (slot < player.playerAddonsConfigurator.guildWarehouseStorageItems && ScriptableItem.All.TryGetValue(itemName.GetStableHashCode(), out template))
                    {
                        Item item 					= new Item(template);
                        int amount 					= Convert.ToInt32(row[2]);
                        item.summonedHealth 		= Convert.ToInt32(row[3]);
                        item.summonedLevel 			= Convert.ToInt32(row[4]);
                        item.summonedExperience 	= Convert.ToInt32(row[5]);
                        item.equipmentLevel         = Convert.ToInt32(row[6]);
                        item.equipmentGems          = row[7].ToString();
                        player.playerAddonsConfigurator.guildWarehouseItemSlot[slot] = new ItemSlot(item, amount);
                    }
                }
            }

#elif _SQLITE

            var warehouseData = connection.FindWithQuery<guild_warehouse>("SELECT gold, level, locked FROM guild_warehouse WHERE guild=?", player.guild.guild.name);

            // -- exists already? load to player and set busy

            if (warehouseData != null)
            {
                player.playerAddonsConfigurator.guildWarehouseGold = warehouseData.gold;
                player.playerAddonsConfigurator.guildWarehouseLevel = warehouseData.level;
                //player.guildWarehouseLock = (warehouseData.locked == 1) ? true : false;
            }
            else
            {
                // -- does not exist? create new and set busy

                connection.InsertOrReplace(new guild_warehouse
                {
                    guild = player.guild.guild.name,
                    gold = 0,
                    level = 0,
                    locked = 0,
                    busy = 0
                });
                player.playerAddonsConfigurator.guildWarehouseGold = 0;
                player.playerAddonsConfigurator.guildWarehouseLevel = 0;
                //player.playerGuildWarehouse.guildWarehouseLock = false;
            }

            for (int i = 0; i < player.playerAddonsConfigurator.guildWarehouseStorageItems; ++i)
            {
                player.playerAddonsConfigurator.guildWarehouseItemSlot.Add(new ItemSlot());
            }

            var table = connection.Query<guild_warehouse_items>("SELECT name, slot, amount, summonedHealth, summonedLevel, summonedExperience, equipmentLevel, equipmentGems FROM guild_warehouse_items WHERE guild=?", player.guild.guild.name);

            if (table.Count > 0)
            {
                foreach (var row in table)
                {
                    string itemName = row.name;
                    int slot = row.slot;
                    ScriptableItem template;

                    if (slot < player.playerAddonsConfigurator.guildWarehouseStorageItems && ScriptableItem.All.TryGetValue(itemName.GetStableHashCode(), out template))
                    {
                        Item item = new Item(template);
                        int amount = row.amount;
                        item.summonedHealth = row.summonedHealth;
                        item.summonedLevel = row.summonedLevel;
                        item.summonedExperience = row.summonedExperience;
                        item.equipmentLevel = row.equipmentLevel;
                        item.equipmentGems = row.equipmentGems;
                        player.playerAddonsConfigurator.guildWarehouseItemSlot[slot] = new ItemSlot(item, amount);
                    }
                }
            }

#endif
        }
        else
            Debug.LogWarning("PlayerGuildWarehouse is not registered on component player in " + player.className + " prefab !");
        //player.playerGuildWarehouse.guildWarehouseActionDone = false;

        return true;
    }

    // -----------------------------------------------------------------------------------
    // SaveGuildWarehouse
    // -----------------------------------------------------------------------------------
    public void SaveGuildWarehouse(Player player)
    {
        if (player.playerAddonsConfigurator) {
            if (!player.guild.InGuild()) player.playerAddonsConfigurator.ResetGuildWarehouse();
            //if (!player.guild.InGuild() || !player.playerGuildWarehouse.guildWarehouseActionDone) return;

#if _MYSQL
		    long EntryExistsOrBusy = Convert.ToInt32(ExecuteScalarMySql("SELECT busy FROM guild_warehouse WHERE guild=@guild", new MySqlParameter("@guild", player.guild.guild.name)));

		    // -- check if exists, only delete entries when it does and is not busy
            ExecuteNonQueryMySql("DELETE FROM guild_warehouse_items WHERE guild=@guild", new MySqlParameter("@guild", player.guild.guild.name));

            // Créez une liste de paramètres pour chaque slot non vide
            List<MySqlParameter> parameters = new List<MySqlParameter>();

            string insertQuery = "INSERT INTO guild_warehouse_items VALUES ";

            // Creation d'une requêtes préparé avec toutes les donnée (si on à 100 slots on ne fait plus 100 requêtes mais une seule)
            for (int i = 0; i < player.playerAddonsConfigurator.guildWarehouseItemSlot.Count; ++i)
            {
                ItemSlot slot = player.playerAddonsConfigurator.guildWarehouseItemSlot[i];

                if (slot.amount > 0)
                {
                    if (parameters.Count > 0)
                        insertQuery += ",";

                    insertQuery += "(@guild" + i + ", @slot_" + i + ", @name_" + i + ", @amount_" + i + ", @petHealth_" + i + ", @petLevel_" + i + ", @petExperience_" + i + ", @equipmentLevel_" + i + ", @equipmentGems_" + i + ")";

                    parameters.Add(new MySqlParameter("@guild" + i, player.guild.guild.name));
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


            ExecuteNonQueryMySql("UPDATE guild_warehouse SET gold=@gold, level=@level, busy=0 WHERE guild=@guild",
			new MySqlParameter("@gold", 	player.playerAddonsConfigurator.guildWarehouseGold),
			new MySqlParameter("@level", 	player.playerAddonsConfigurator.guildWarehouseLevel),
			//new MySqlParameter("@locked", 	(player.playerGuildWarehouse.guildWarehouseLock) ? 1 : 0),
			new MySqlParameter("@guild", 	player.guild.guild.name));

#elif _SQLITE

            /* var results = connection.FindWithQuery<guild_warehouse>("SELECT busy FROM guild_warehouse WHERE guild=?", player.guild.guild.name);
             long EntryExistsOrBusy = results.busy;
            */
            // -- check if exists, only delete entries when it does and is not busy

            //if (EntryExistsOrBusy != 1)
            connection.Execute("DELETE FROM guild_warehouse_items WHERE guild=?", player.guild.guild.name);

            for (int i = 0; i < player.playerAddonsConfigurator.guildWarehouseItemSlot.Count; ++i)
            {
                ItemSlot slot = player.playerAddonsConfigurator.guildWarehouseItemSlot[i];

                if (slot.amount > 0)
                {
                    connection.Insert(new guild_warehouse_items
                    {
                        guild = player.guild.guild.name,
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

            connection.Execute("UPDATE guild_warehouse SET gold=?, level=?, busy=0 WHERE guild=?", player.playerAddonsConfigurator.guildWarehouseGold, player.playerAddonsConfigurator.guildWarehouseLevel, player.guild.guild.name);
#endif
        }
    }

    /**
     * Log function
     *              guild VARCHAR(32) NOT NULL,
                    slot INTEGER(16) NOT NULL,
                    gold INTEGER(16) NOT NULL,
                    levelup INTEGER(16) NOT NULL,
                    name TEXT(16) NOT NULL,
                    amount INTEGER(16) NOT NULL,
                    added INTEGER NOT NULL,
     */

    //Add Log AddGold
    public void SetLogGuildWarehouseGold(Player player, long amount, bool insert )
    {
#if _MYSQL
        ExecuteNonQueryMySql("INSERT INTO guild_warehouse_logs (guild, player, gold, added) VALUES(@guild, @player, @gold, @added)",
            new MySqlParameter("@player", player.name),
            new MySqlParameter("@gold", (int)amount),
            new MySqlParameter("@added",  insert),
            new MySqlParameter("@guild", player.guild.guild.name));
#elif _SQLITE
        connection.Insert(new guild_warehouse_logs
        {
            guild = player.guild.guild.name,
            player = player.name,
            slot = -1,
            gold = amount,
            levelup = 0,
            name = "",
            amount = 0,
            added = insert ? 1 : 0,
            actionDate = DateTime.Now.ToString()

        });
       
#endif
    }

    public void SetLogGuildWarehouseUpgrade(Player player, int level)
    {
#if _MYSQL
        ExecuteNonQueryMySql("INSERT INTO guild_warehouse_logs (guild, player, levelup) VALUES(@guild, @player, @level)",
            new MySqlParameter("@player", player.name),
           new MySqlParameter("@level", level),
            new MySqlParameter("@guild", player.guild.guild.name));
#elif _SQLITE
        connection.Insert(new guild_warehouse_logs
        {
            guild = player.guild.guild.name,
            player = player.name,
            //slot = -1,
            //gold = 0,
            levelup = level,
            name = "",
            //amount = 0,
            //added = insert ? 1 : 0
            actionDate = DateTime.Now.ToString()
        });
#endif
    }


    public void SetLogGuildWarehouseSlotMove(Player player, int slot, string name, int amount, int insert)
    {
#if _MYSQL
        ExecuteNonQueryMySql("INSERT INTO guild_warehouse_logs (guild, player, slot, name, amount, added) VALUES(@guild, @player, @slot, @name, @amount, @added)",
            new MySqlParameter("@player", player.name),
            new MySqlParameter("@guild", player.guild.guild.name),
            new MySqlParameter("@slot", slot),
            new MySqlParameter("@name", name),
            new MySqlParameter("@amount", amount),
            new MySqlParameter("@added", insert));
#elif _SQLITE
        connection.Insert(new guild_warehouse_logs
        {
            guild = player.guild.guild.name,
            player = player.name,
            slot = slot,
            gold = 0,
            levelup = 0,
            name = name,
            amount = amount,
            added = insert,
            actionDate = DateTime.Now.ToString()
        }); ;
#endif
    }




    public void LoadGuildWarehouseLog(Player player)
    {
#if _MYSQL

        var logs = ExecuteReaderMySql("Select actionDate, player, levelup, gold, added, name, slot, amount FROM guild_warehouse_logs WHERE guild = @guild ORDER BY id DESC LIMIT 100", new MySqlParameter("@guild", player.guild.guild.name));
        if(logs.Count >= 1)
        {
            player.playerAddonsConfigurator.listLogGuildWarehouse.Clear();
            foreach (var row in logs)
            {
                GuildWarehouseLog log = new()
                {
                    logActionDate = row[0].ToString(),
                    logPlayerName = row[1].ToString(),
                    logLevelUp = row[2].ToString(),
                    logGold = row[3].ToString(),
                    logAdded = row[4].ToString(),
                    logItemName = row[5].ToString(),
                    logSlot = row[6].ToString(),
                    logAmout = row[7].ToString()
                };

                player.playerAddonsConfigurator.listLogGuildWarehouse.Add(log);
            }
        }
#elif _SQLITE
        var logs = connection.Query<guild_warehouse_logs>("Select actionDate, player, levelup, gold, added, name, slot, amount FROM guild_warehouse_logs WHERE guild=? order by actionDate DESC LIMIT 100", player.guild.guild.name);
        Debug.Log("ok.. > "+ logs.Count);
        if (logs.Count > 0)
        {
            Debug.Log("ok");
            player.playerAddonsConfigurator.listLogGuildWarehouse.Clear();
            foreach (var row in logs)
            {
                GuildWarehouseLog log = new GuildWarehouseLog()
                {
                    logActionDate = row.actionDate.ToString(),
                    logPlayerName = row.player.ToString(),
                    logLevelUp = row.levelup.ToString(),
                    logGold = row.gold.ToString(),
                    logAdded = row.added.ToString(),
                    logItemName = row.name.ToString(),
                    logSlot = row.slot.ToString(),
                    logAmout = row.amount.ToString()
                };

                player.playerAddonsConfigurator.listLogGuildWarehouse.Add(log);
            }
        
        }
#endif
    }
    // -----------------------------------------------------------------------------------
}
#endif