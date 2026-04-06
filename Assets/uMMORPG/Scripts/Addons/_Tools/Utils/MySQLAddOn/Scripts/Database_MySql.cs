#if _MYSQL

using UnityEngine;
using Mirror;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;                   // From MySql.Data.dll in Plugins folder
using UnityEngine.Events;

using SqlParameter = MySql.Data.MySqlClient.MySqlParameter;

// Database (mySQL)
public partial class Database : MonoBehaviour
{
    public static Database singleton;
    private string connectionString = null;

    public string RuntimeDbHost => string.IsNullOrWhiteSpace(dbHost) ? "127.0.0.1" : dbHost;
    public string RuntimeDbName => string.IsNullOrWhiteSpace(dbName) ? "database" : dbName;
    public string RuntimeDbUser => string.IsNullOrWhiteSpace(dbUser) ? "user" : dbUser;

    public bool RuntimeDbUsesPlaceholderHost()
    {
        return string.IsNullOrWhiteSpace(RuntimeDbHost) || RuntimeDbHost == "127.0.0.1" || RuntimeDbHost.Equals("localhost", StringComparison.OrdinalIgnoreCase);
    }

    public bool RuntimeDbUsesPlaceholderName()
    {
        return string.IsNullOrWhiteSpace(RuntimeDbName) || RuntimeDbName.Equals("database", StringComparison.OrdinalIgnoreCase);
    }

    public bool RuntimeDbUsesPlaceholderUser()
    {
        return string.IsNullOrWhiteSpace(RuntimeDbUser) || RuntimeDbUser.Equals("user", StringComparison.OrdinalIgnoreCase);
    }

    public bool RuntimeDbUsesPlaceholderPassword()
    {
        return string.IsNullOrWhiteSpace(dbPassword) || dbPassword.Equals("password", StringComparison.OrdinalIgnoreCase);
    }
    // -----------------------------------------------------------------------------------
    // ConnectionString
    // -----------------------------------------------------------------------------------
    private string ConnectionString
    {
        get
        {
#if _SERVER
            if (connectionString == null)
            {
                MySqlConnectionStringBuilder connectionStringBuilder = new MySqlConnectionStringBuilder
                {
                    Server = string.IsNullOrWhiteSpace(dbHost) ? "127.0.0.1" : dbHost,
                    Database = string.IsNullOrWhiteSpace(dbName) ? "database" : dbName,
                    UserID = string.IsNullOrWhiteSpace(dbUser) ? "user" : dbUser,
                    Password = string.IsNullOrWhiteSpace(dbPassword) ? "password" : dbPassword,
                    Port = dbPort,
                    CharacterSet = string.IsNullOrWhiteSpace(dbCharacterSet) ? "utf8mb4" : dbCharacterSet
                };
                connectionString = connectionStringBuilder.ConnectionString;
            }
#endif
            return connectionString;
        }
    }
 
    [Header("Events")]
    // use onConnected to create an extra table for your addon
    public UnityEvent onConnected;
    public UnityEventPlayer onCharacterLoad;
    public UnityEventPlayer onCharacterSave;



    #region MYSQL FUNCTION HELPER 

    // -----------------------------------------------------------------------------------
    // Transaction
    // -----------------------------------------------------------------------------------
    private void Transaction(Action<MySqlCommand> action)
    {
#if _SERVER
        using MySqlConnection connection = new MySqlConnection(ConnectionString);
        connection.Open();
        MySqlTransaction transaction = null;

        try
        {
            transaction = connection.BeginTransaction();

            MySqlCommand command = new MySqlCommand();
            command.Connection = connection;
            command.Transaction = transaction;

            action(command);

            transaction.Commit();
        }
        catch (Exception ex)
        {
            if (transaction != null)
                transaction.Rollback();
            throw ex;
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // ExecuteNonQueryMySql
    // -----------------------------------------------------------------------------------
    public void ExecuteNonQueryMySql(string sql, params SqlParameter[] args)
    {
#if _SERVER
        try
        {
            MySqlHelper.ExecuteNonQuery(ConnectionString, sql, args);
        }
        catch (Exception ex)
        {
            throw ex;
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // ExecuteNonQueryMySql
    // -----------------------------------------------------------------------------------
    public void ExecuteNonQueryMySql(MySqlCommand command, string sql, params SqlParameter[] args)
    {
#if _SERVER
        try
        {
            command.CommandText = sql;
            command.Parameters.Clear();

            foreach (SqlParameter arg in args)
            {
                command.Parameters.Add(arg);
            }

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw ex;
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // ExecuteScalarMySql
    // -----------------------------------------------------------------------------------
    public object ExecuteScalarMySql(string sql, params SqlParameter[] args)
    {
#if _SERVER
        try
        {
            return MySqlHelper.ExecuteScalar(ConnectionString, sql, args);
        }
        catch (Exception ex)
        {
            throw ex;
        }
#else
        return null;
#endif
    }

    // -----------------------------------------------------------------------------------
    // ExecuteDataRowMySql
    // -----------------------------------------------------------------------------------
    public DataRow ExecuteDataRowMySql(string sql, params SqlParameter[] args)
    {
#if _SERVER
        try
        {
            return MySqlHelper.ExecuteDataRow(ConnectionString, sql, args);
        }
        catch (Exception ex)
        {
            throw ex;
        }
#else
        return null;
#endif
    }

    // -----------------------------------------------------------------------------------
    // ExecuteDataSetMySql
    // -----------------------------------------------------------------------------------
    public DataSet ExecuteDataSetMySql(string sql, params SqlParameter[] args)
    {
#if _SERVER
        try
        {
            return MySqlHelper.ExecuteDataset(ConnectionString, sql, args);
        }
        catch (Exception ex)
        {
            throw ex;
        }
#else
        return null;
#endif
    }

    // -----------------------------------------------------------------------------------
    // ExecuteReaderMySql
    // -----------------------------------------------------------------------------------
    public List<List<object>> ExecuteReaderMySql(string sql, params SqlParameter[] args)
    {
#if _SERVER
        List<List<object>> result = new List<List<object>>();

        try
        {
            using (var reader = MySqlHelper.ExecuteReader(ConnectionString, sql, args))
            {
                while (reader.Read())
                {
                    var buf = new object[reader.FieldCount];
                    reader.GetValues(buf);
                    result.Add(buf.ToList());
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            throw ex;
        }
#else
        return null;
#endif
    }

    // -----------------------------------------------------------------------------------
    // GetReader
    // -----------------------------------------------------------------------------------
    public MySqlDataReader GetReader(string sql, params SqlParameter[] args)
    {
#if _SERVER
        try
        {
            return MySqlHelper.ExecuteReader(ConnectionString, sql, args);
        }
        catch (Exception ex)
        {
            throw ex;
        }
#else
        return null;
#endif
    }


    private string debugShowQuery(string sql, string[] args)
    {

        return "";
    }
    // -----------------------------------------------------------------------------------
    // GetEnv
    // -----------------------------------------------------------------------------------
    private string GetEnv(String name)
    {
        return Environment.GetEnvironmentVariable(name);
    }

    // -----------------------------------------------------------------------------------
    // GetUIntEnv
    // -----------------------------------------------------------------------------------
    private uint GetUIntEnv(String name, uint defaultValue = 0)
    {
        var value = Environment.GetEnvironmentVariable(name);

        if (value == null)
            return defaultValue;

        uint result;

        if (uint.TryParse(value, out result))
            return result;

        return defaultValue;
    }

    #endregion MYSQL FUNCTION HELPER 

    // -----------------------------------------------------------------------------------
    // Awake
    // -----------------------------------------------------------------------------------
    void Awake()
    {
        if (singleton == null) singleton = this;
        Utils.InvokeMany(typeof(Database), this, "Start_Tools_");
    }

    // -----------------------------------------------------------------------------------
    // Connect
    // -----------------------------------------------------------------------------------
    public void Connect()
    {
#if _SERVER
        // -- accounts
        ExecuteNonQueryMySql(@"
        CREATE TABLE IF NOT EXISTS accounts (
            name VARCHAR(32) NOT NULL,
            password VARCHAR(64) NOT NULL,
            created DATETIME NOT NULL,
            lastlogin DATETIME NOT NULL,
            banned BOOLEAN NOT NULL DEFAULT 0,
            PRIMARY KEY(name)
        ) CHARACTER SET=utf8mb4");

        // -- characters
#if !_iMMO2D
        ExecuteNonQueryMySql(@"
        CREATE TABLE IF NOT EXISTS characters(
            name VARCHAR(32) NOT NULL,
            account VARCHAR(32) NOT NULL,
            `class` VARCHAR(32) NOT NULL,
            x FLOAT NOT NULL,
        	y FLOAT NOT NULL,
            z FLOAT NOT NULL,
        	level INT NOT NULL DEFAULT 1,
            health INT NOT NULL,
        	mana INT NOT NULL,
            strength INT NOT NULL DEFAULT 0,
        	intelligence INT NOT NULL DEFAULT 0,
            experience BIGINT NOT NULL DEFAULT 0,
        	skillExperience BIGINT NOT NULL DEFAULT 0,
            gold BIGINT NOT NULL DEFAULT 0,
        	coins BIGINT NOT NULL DEFAULT 0,
            gamemaster BOOLEAN NOT NULL DEFAULT 0, 
            online INT NOT NULL,
            lastsaved DATETIME NOT NULL,
            deleted BOOLEAN NOT NULL,

        	PRIMARY KEY (name),
            INDEX(account),
        	FOREIGN KEY(account)
                REFERENCES accounts(name)
                ON DELETE CASCADE ON UPDATE CASCADE

        ) CHARACTER SET=utf8mb4");
#else
        ExecuteNonQueryMySql(@"
        CREATE TABLE IF NOT EXISTS characters(
            name VARCHAR(32) NOT NULL,
            account VARCHAR(32) NOT NULL,
            `class` VARCHAR(32) NOT NULL,
            x FLOAT NOT NULL,
        	y FLOAT NOT NULL,
        	level INT NOT NULL DEFAULT 1,
            health INT NOT NULL,
        	mana INT NOT NULL,
            strength INT NOT NULL DEFAULT 0,
        	intelligence INT NOT NULL DEFAULT 0,
            experience BIGINT NOT NULL DEFAULT 0,
        	skillExperience BIGINT NOT NULL DEFAULT 0,
            gold BIGINT NOT NULL DEFAULT 0,
        	coins BIGINT NOT NULL DEFAULT 0,
            online INT NOT NULL,
            lastsaved DATETIME NOT NULL,
            deleted BOOLEAN NOT NULL,

        	PRIMARY KEY (name),
            INDEX(account),
        	FOREIGN KEY(account)
                REFERENCES accounts(name)
                ON DELETE CASCADE ON UPDATE CASCADE

        ) CHARACTER SET=utf8mb4");
#endif
        // -- character_inventory
#if !_iMMO2D
        ExecuteNonQueryMySql(@"
        CREATE TABLE IF NOT EXISTS character_inventory(
            `character` VARCHAR(32) NOT NULL,
            slot INT NOT NULL,
        	name VARCHAR(64) NOT NULL,
            amount INT NOT NULL,
            durability INT NOT NULL,
        	summonedHealth INT NOT NULL,
            summonedLevel INT NOT NULL,
            summonedExperience BIGINT NOT NULL,

            primary key(`character`, slot),
        	FOREIGN KEY(`character`)
                REFERENCES characters(name)
                ON DELETE CASCADE ON UPDATE CASCADE
        ) CHARACTER SET=utf8mb4");
#else
        ExecuteNonQueryMySql(@"
        CREATE TABLE IF NOT EXISTS character_inventory(
            `character` VARCHAR(32) NOT NULL,
            slot INT NOT NULL,
        	name VARCHAR(64) NOT NULL,
            amount INT NOT NULL,
        	summonedHealth INT NOT NULL,
            summonedLevel INT NOT NULL,
            summonedExperience BIGINT NOT NULL,

            primary key(`character`, slot),
        	FOREIGN KEY(`character`)
                REFERENCES characters(name)
                ON DELETE CASCADE ON UPDATE CASCADE
        ) CHARACTER SET=utf8mb4");
#endif

        // -- character_equipment
#if !_iMMO2D
        ExecuteNonQueryMySql(@"
        CREATE TABLE IF NOT EXISTS character_equipment(
           `character` VARCHAR(32) NOT NULL,
            slot INT NOT NULL,
        	name VARCHAR(64) NOT NULL,
            amount INT NOT NULL,
            durability INT NOT NULL,
        	summonedHealth INT NOT NULL,
            summonedLevel INT NOT NULL,
            summonedExperience BIGINT NOT NULL,

            primary key(`character`, slot),
        	FOREIGN KEY(`character`)
                REFERENCES characters(name)
                ON DELETE CASCADE ON UPDATE CASCADE
        ) CHARACTER SET=utf8mb4");
#else
        ExecuteNonQueryMySql(@"
        CREATE TABLE IF NOT EXISTS character_equipment(
           `character` VARCHAR(32) NOT NULL,
            slot INT NOT NULL,
        	name VARCHAR(64) NOT NULL,
            amount INT NOT NULL,
            summonedHealth INT NOT NULL,
            summonedLevel INT NOT NULL,
            summonedExperience BIGINT NOT NULL,

            primary key(`character`, slot),
        	FOREIGN KEY(`character`)
                REFERENCES characters(name)
                ON DELETE CASCADE ON UPDATE CASCADE
        ) CHARACTER SET=utf8mb4");
#endif
        // -- character_itemcooldowns
        ExecuteNonQueryMySql(@"
        CREATE TABLE IF NOT EXISTS character_itemcooldowns(
            `character` VARCHAR(32) NOT NULL,
            category VARCHAR(50) NOT NULL,
            cooldownEnd FLOAT NOT NULL,

            PRIMARY KEY (`character`),
            FOREIGN KEY(`character`)
                REFERENCES characters(name)
                ON DELETE CASCADE ON UPDATE CASCADE
        ) CHARACTER SET=utf8mb4");

        // -- character_skills
        ExecuteNonQueryMySql(@"
        CREATE TABLE IF NOT EXISTS character_skills(
            `character` VARCHAR(32) NOT NULL,
            name VARCHAR(50) NOT NULL,
            level INT NOT NULL,
        	castTimeEnd FLOAT NOT NULL,
            cooldownEnd FLOAT NOT NULL,

            PRIMARY KEY (`character`, name),
            FOREIGN KEY(`character`)
                REFERENCES characters(name)
                ON DELETE CASCADE ON UPDATE CASCADE
        ) CHARACTER SET=utf8mb4");

        // -- character_buffs
        ExecuteNonQueryMySql(@"
        CREATE TABLE IF NOT EXISTS character_buffs (
            `character` VARCHAR(32) NOT NULL,
            name VARCHAR(64) NOT NULL,
            level INT NOT NULL,
            buffTimeEnd FLOAT NOT NULL,

            PRIMARY KEY (`character`, name),
            FOREIGN KEY(`character`)
                REFERENCES characters(name)
                ON DELETE CASCADE ON UPDATE CASCADE
        ) CHARACTER SET=utf8mb4");

#if !_iMMOQUESTS
        // -- character_quests
        ExecuteNonQueryMySql(@"
        CREATE TABLE IF NOT EXISTS character_quests(
            `character` VARCHAR(32) NOT NULL,
            name VARCHAR(64) NOT NULL,
            progress INT NOT NULL,
        	completed BOOLEAN NOT NULL,

            PRIMARY KEY(`character`, name),
        	FOREIGN KEY(`character`)
                REFERENCES characters(name)
                ON DELETE CASCADE ON UPDATE CASCADE
        ) CHARACTER SET=utf8mb4");
#endif

        // -- character_orders
        ExecuteNonQueryMySql(@"
        CREATE TABLE IF NOT EXISTS character_orders(
            orderid BIGINT NOT NULL AUTO_INCREMENT,
            `character` VARCHAR(32) NOT NULL,
            coins BIGINT NOT NULL,
            processed BIGINT NOT NULL,

            PRIMARY KEY(orderid),
            INDEX(`character`),
        	FOREIGN KEY(`character`)
                REFERENCES characters(name)
                ON DELETE CASCADE ON UPDATE CASCADE
        ) CHARACTER SET=utf8mb4");

        // -- character_guild
        ExecuteNonQueryMySql(@"
        CREATE TABLE IF NOT EXISTS character_guild(
            `character` VARCHAR(32) NOT NULL,
            guild VARCHAR(64) NOT NULL,
            `rank`INT NOT NULL,
            PRIMARY KEY(`character`)
        ) CHARACTER SET=utf8mb4");

        // -- guild_info
        ExecuteNonQueryMySql(@"
        CREATE TABLE IF NOT EXISTS guild_info(
            name VARCHAR(32) NOT NULL,
            notice TEXT NOT NULL,
            PRIMARY KEY(name)
        ) CHARACTER SET=utf8mb4");

        // addon system hooks
        onConnected.Invoke();

#endif
    }

    // -----------------------------------------------------------------------------------
    // TryLogin
    // -----------------------------------------------------------------------------------
    public bool TryLogin(string account, string password)
    {
#if _SERVER
        if (!string.IsNullOrWhiteSpace(account) && !string.IsNullOrWhiteSpace(password))
        {
            // demo feature: create account if it doesn't exist yet.
            ExecuteNonQueryMySql("INSERT IGNORE INTO accounts VALUES (@name, @password, @created, @lastlogin, 0)", new SqlParameter("@name", account), new SqlParameter("@password", password), new SqlParameter("@created", DateTime.UtcNow), new SqlParameter("@lastlogin", DateTime.UtcNow));

            // check account name, password, banned status
            bool valid = ((long)ExecuteScalarMySql("SELECT Count(*) FROM accounts WHERE name=@name AND password=@password AND banned=0", new SqlParameter("@name", account), new SqlParameter("@password", password))) == 1;
            if (valid)
            {
                // save last login time and return true
                ExecuteNonQueryMySql("UPDATE accounts SET lastlogin=@lastlogin WHERE name=@name", new SqlParameter("@name", account), new SqlParameter("@lastlogin", DateTime.UtcNow));
                return true;
            }
        }
#endif
        return false;
    }

    // -----------------------------------------------------------------------------------
    // CharacterExists
    // -----------------------------------------------------------------------------------
    public bool CharacterExists(string characterName)
    {
#if _SERVER
        return ((long)ExecuteScalarMySql("SELECT Count(*) FROM characters WHERE name=@name", new SqlParameter("@name", characterName))) == 1;
#else
        return false;
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterDelete
    // -----------------------------------------------------------------------------------
    public void CharacterDelete(string characterName)
    {
#if _SERVER
        ExecuteNonQueryMySql("UPDATE characters SET deleted=1 WHERE name=@character", new SqlParameter("@character", characterName));
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharactersForAccount
    // -----------------------------------------------------------------------------------
    public List<string> CharactersForAccount(string account)
    {
        List<String> result = new List<String>();
#if _SERVER
        var table = ExecuteReaderMySql("SELECT name FROM characters WHERE account=@account AND deleted=0", new SqlParameter("@account", account));
        foreach (var row in table)
            result.Add((string)row[0]);
#endif
        return result;
    }

    // -----------------------------------------------------------------------------------
    // LoadInventory Mysql Version
    // -----------------------------------------------------------------------------------
    private void LoadInventory(PlayerInventory inventory)
    {
#if _SERVER
        for (int i = 0; i < inventory.size; ++i)
            inventory.slots.Add(new ItemSlot());
#if !_iMMO2D
        using (var reader = GetReader(@"SELECT name, slot, amount,durability, summonedHealth, summonedLevel, summonedExperience FROM character_inventory WHERE `character`=@character;", new SqlParameter("@character", inventory.name)))
#else
        using (var reader = GetReader(@"SELECT name, slot, amount, summonedHealth, summonedLevel, summonedExperience FROM character_inventory WHERE `character`=@character;", new SqlParameter("@character", inventory.name)))
#endif
        {
            while (reader.Read())
            {
                string itemName = (string)reader["name"];
                int slot = (int)reader["slot"];

                ScriptableItem itemData;
                if (slot < inventory.size && ScriptableItem.All.TryGetValue(itemName.GetStableHashCode(), out itemData))
                {
                    Item item = new Item(itemData);
                    int amount = (int)reader["amount"];
#if !_iMMO2D
                    item.durability = (int)reader["durability"];
#endif
                    item.summonedHealth = (int)reader["summonedHealth"];
                    item.summonedLevel = (int)reader["summonedLevel"];
                    item.summonedExperience = (long)reader["summonedExperience"];
                    inventory.slots[slot] = new ItemSlot(item, amount); ;
                }
            }
        }
#endif
    }

     // -----------------------------------------------------------------------------------
     // LoadEquipment
     // -----------------------------------------------------------------------------------
    private void LoadEquipment(PlayerEquipment equipment)
    {
#if _SERVER
        for (int i = 0; i < equipment.slotInfo.Length; ++i)
            equipment.slots.Add(new ItemSlot());

        using (var reader = GetReader(@"SELECT * FROM character_equipment WHERE `character`=@character;",
                                           new SqlParameter("@character", equipment.name)))
        {
            while (reader.Read())
            {
                string itemName = (string)reader["name"];
                int slot = (int)reader["slot"];

                ScriptableItem itemData;
                if (slot < equipment.slotInfo.Length && ScriptableItem.All.TryGetValue(itemName.GetStableHashCode(), out itemData))
                {
                    Item item = new Item(itemData);
                    int amount = (int)reader["amount"];
#if !_iMMO2D
                    item.durability = (int)reader["durability"];
#endif
                    item.summonedHealth = (int)reader["summonedHealth"];
                    item.summonedLevel = (int)reader["summonedLevel"];
                    item.summonedExperience = (long)reader["summonedExperience"];
                    equipment.slots[slot] = new ItemSlot(item, amount);
                }
            }
        }
#endif
    }

    void LoadItemCooldowns(Player player)
    {
        // then load cooldowns
        // (one big query is A LOT faster than querying each slot separately)

        using (var reader = GetReader(
             "SELECT * FROM character_itemcooldowns WHERE `character`=@character ",
             new SqlParameter("@character", player.name)))
        {
            while (reader.Read())
            {
                string categoryName = (string)reader["category"];
                float coldownEnd = (float)reader["cooldownEnd"];
                player.itemCooldowns.Add(categoryName, coldownEnd + NetworkTime.time);
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // LoadSkills
    // -----------------------------------------------------------------------------------
    private void LoadSkills(PlayerSkills skills)
    {
#if _SERVER
        foreach (ScriptableSkill skillData in skills.skillTemplates)
            skills.skills.Add(new Skill(skillData));

        using (var reader = GetReader(
            "SELECT name, level, castTimeEnd, cooldownEnd FROM character_skills WHERE `character`=@character ",
            new SqlParameter("@character", skills.name)))
        {
            while (reader.Read())
            {
                string skillName = (string)reader["name"];

                int index = skills.GetSkillIndexByName(skillName);
                if (index != -1)
                {
                    Skill skill = skills.skills[index];
                    skill.level = Mathf.Clamp((int)reader["level"], 1, skill.maxLevel);
                    skill.castTimeEnd = (float)reader["castTimeEnd"] + Time.time;
                    skill.cooldownEnd = (float)reader["cooldownEnd"] + Time.time;
                    skills.skills[index] = skill;
                }
            }
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // LoadBuffs
    // -----------------------------------------------------------------------------------
    private void LoadBuffs(PlayerSkills skills)
    {
#if _SERVER
        using (var reader = GetReader(
            "SELECT name, level, buffTimeEnd FROM character_buffs WHERE `character` = @character ",
            new SqlParameter("@character", skills.name)))
        {
            while (reader.Read())
            {
                string buffName = (string)reader["name"];
                ScriptableSkill skillData;
                if (ScriptableSkill.All.TryGetValue(buffName.GetStableHashCode(), out skillData))
                {
                    int level = Mathf.Clamp((int)reader["level"], 1, skillData.maxLevel);
                    Buff buff = new Buff((BuffSkill)skillData, level);
                    buff.buffTimeEnd = (float)reader["buffTimeEnd"] + Time.time;
                    skills.buffs.Add(buff);
                }
            }
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // LoadQuests
    // -----------------------------------------------------------------------------------
    private void LoadQuests(PlayerQuests quests)
    {
#if _SERVER
        using (var reader = GetReader("SELECT name, progress, completed FROM character_quests WHERE `character`=@character",
                                           new SqlParameter("@character", quests.name)))
        {
            while (reader.Read())
            {
                string questName = (string)reader["name"];
                ScriptableQuest questData;
                if (ScriptableQuest.All.TryGetValue(questName.GetStableHashCode(), out questData))
                {
                    Quest quest = new Quest(questData);
                    quest.progress = (int)reader["progress"];
                    quest.completed = (bool)reader["completed"];
                    quests.quests.Add(quest);
                }
                else Debug.LogWarning("MYSQL LoadQuests: skipped quest " + questName + " for " + quests.name + " because it doesn't exist anymore. If it wasn't removed intentionally then make sure it's in the Resources folder.");
            }
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // LoadGuild
    // -----------------------------------------------------------------------------------
    Guild LoadGuild(string guildName)
    {
        Guild guild = new Guild();
#if _SERVER

        guild.name = guildName;

        List<List<object>> table = ExecuteReaderMySql("SELECT notice FROM guild_info WHERE name=@guild", new SqlParameter("@guild", guildName));
        if (table.Count == 1)
        {
            List<object> row = table[0];
            guild.notice = (string)row[0];
        }

        List<GuildMember> members = new List<GuildMember>();
        table = ExecuteReaderMySql("SELECT `character`, `rank` FROM character_guild WHERE guild=@guild", new SqlParameter("@guild", guildName));

        foreach (List<object> row in table)
        {
            GuildMember member = new GuildMember();
            member.name = (string)row[0];
            member.rank = (GuildRank)((int)row[1]);

            // is this player online right now? then use runtime data
            if (Player.onlinePlayers.TryGetValue(member.name, out Player player))
            {
                member.online = true;
                member.level = player.level.current;
            }
            else
            {
                member.online = false;
                object scalar = ExecuteScalarMySql("SELECT level FROM characters WHERE name=@character", new SqlParameter("@character", member.name));
                member.level = scalar != null ? ((int)scalar) : 1;
            }
            members.Add(member);
        }
        guild.members = members.ToArray();
#endif
        return guild;
    }

    // -----------------------------------------------------------------------------------
    // LoadGuildOnDemand
    // -----------------------------------------------------------------------------------
    void LoadGuildOnDemand(PlayerGuild playerGuild)
    {
#if _SERVER
        string guildName = (string)ExecuteScalarMySql("SELECT guild FROM character_guild WHERE `character`=@character", new SqlParameter("@character", playerGuild.name));
        if (guildName != null)
        {
            // load guild on demand when the first player of that guild logs in
            // (= if it's not in GuildSystem.guilds yet)
            if (!GuildSystem.guilds.ContainsKey(guildName))
            {
                Guild guild = LoadGuild(guildName);
                GuildSystem.guilds[guild.name] = guild;
                playerGuild.guild = guild;
            }
            // assign from already loaded guild
            else playerGuild.guild = GuildSystem.guilds[guildName];
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterLoad
    // -----------------------------------------------------------------------------------
    public GameObject CharacterLoad(string characterName, List<Player> prefabs, bool isPreview)
    {
#if _SERVER
        var row = ExecuteDataRowMySql("SELECT * FROM characters WHERE name=@name AND deleted=0", new SqlParameter("@name", characterName));

        if (row != null)
        {
            string className = (string)row["class"];
            var prefab = prefabs.Find(p => p.name == className);
            if (prefab != null)
            {
                GameObject go = GameObject.Instantiate(prefab.gameObject);
                Player player = go.GetComponent<Player>();

                player.name = (string)row["name"];
                player.account = (string)row["account"];
                player.className = (string)row["class"];
                float x = (float)row["x"];
                float y = (float)row["y"];
#if !_iMMO2D
                float z = (float)row["z"];
                Vector3 position = new Vector3(x, y, z);
#else
                Vector2 position = new Vector2(x, y);
#endif
                player.level.current = Mathf.Min((int)row["level"], player.level.max);
                int health = (int)row["health"];
                int mana = (int)row["mana"];
                player.strength = (int)row["strength"];
                player.intelligence = (int)row["intelligence"];
                player.experience.current = (long)row["experience"];
                ((PlayerSkills)player.skills).skillExperience = (long)row["skillExperience"];
                player.gold = (long)row["gold"];
#if !_iMMO2D
                player.isGameMaster = (bool)row["gamemaster"];
#endif
                //player.isGameMaster = row.gamemaster;
                player.itemMall.coins = (long)row["coins"];

                if (player.movement.IsValidSpawnPoint(position))
                {
                    // agent.warp is recommended over transform.position and
                    // avoids all kinds of weird bugs
                    player.movement.Warp(position);
                }
                // otherwise warp to start position
                else
                {
                    Transform start = NetworkManagerMMO.GetNearestStartPosition(position);
                    player.movement.Warp(start.position);
                    // no need to show the message all the time. it would spam
                    // the server logs too much.
                    //Debug.Log(player.name + " spawn position reset because it's not on a NavMesh anymore. This can happen if the player previously logged out in an instance or if the Terrain was changed.");
                }

                LoadEquipment((PlayerEquipment)player.equipment);
                LoadInventory(player.inventory);
                LoadItemCooldowns(player);
                LoadSkills((PlayerSkills)player.skills);
                LoadBuffs((PlayerSkills)player.skills);
#if !_iMMOQUESTS
                LoadQuests(player.quests);
#endif
                LoadGuildOnDemand(player.guild);

                // addon system hooks
                onCharacterLoad.Invoke(player);
                if (!isPreview)
                    ExecuteNonQueryMySql("UPDATE characters SET online=1, lastsaved=@lastsaved WHERE name=@name", new SqlParameter("@name", characterName), new SqlParameter("@lastsaved", DateTime.UtcNow));

                player.health.current = health;
                player.mana.current = mana;



                return go;
            }
        }
        else Debug.LogError("no prefab found for class: " + row["class"]);
#endif
                return null;
    }

    // -----------------------------------------------------------------------------------
    // SaveInventory
    // -----------------------------------------------------------------------------------
    void SaveInventory(PlayerInventory inventory, MySqlCommand command)
    {
#if _SERVER
        ExecuteNonQueryMySql(command, "DELETE FROM character_inventory WHERE `character`=@character", new SqlParameter("@character", inventory.name));
        for (int i = 0; i < inventory.slots.Count; ++i)
        {
            ItemSlot slot = inventory.slots[i];
            if (slot.amount > 0)
#if !_iMMO2D
                ExecuteNonQueryMySql(command, "INSERT INTO character_inventory VALUES (@character, @slot, @name, @amount, @durability, @summonedHealth, @summonedLevel, @summonedExperience)",
#else
                ExecuteNonQueryMySql(command, "INSERT INTO character_inventory VALUES (@character, @slot, @name, @amount, @summonedHealth, @summonedLevel, @summonedExperience)",
#endif
                new SqlParameter("@character", inventory.name),
                        new SqlParameter("@slot", i),
                        new SqlParameter("@name", slot.item.name),
                        new SqlParameter("@amount", slot.amount),
#if !_iMMO2D
                        new SqlParameter("@durability", slot.item.durability),
#endif
                        new SqlParameter("@summonedHealth", slot.item.summonedHealth),
                        new SqlParameter("@summonedLevel", slot.item.summonedLevel),
                        new SqlParameter("@summonedExperience", slot.item.summonedExperience));
        }
#endif
        }


    void SaveItemCooldowns(Player player, MySqlCommand command)
    {
        // equipment: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        ExecuteNonQueryMySql(command, "DELETE FROM character_itemcooldowns WHERE `character`=@character", new SqlParameter("@character", player.name));
        foreach (KeyValuePair<string, double> kvp in player.itemCooldowns)
        {
            float cooldown = player.GetItemCooldown(kvp.Key);
            if (cooldown > 0)
            {
                ExecuteNonQueryMySql(command, @"
                    INSERT INTO character_itemcooldowns
                    SET
                        `character` = @character,
                        category = @category,
                        cooldownEnd = @cooldownEnd",
                                    new SqlParameter("@character", player.name),
                                    new SqlParameter("@category", kvp.Key),
                                    new SqlParameter("@cooldownEnd", cooldown));
            }
        }
    }


    // -----------------------------------------------------------------------------------
    // SaveEquipment
    // -----------------------------------------------------------------------------------
    void SaveEquipment(PlayerEquipment equipment, MySqlCommand command)
    {
#if _SERVER
        ExecuteNonQueryMySql(command, "DELETE FROM character_equipment WHERE `character`=@character", new SqlParameter("@character", equipment.name));
        for (int i = 0; i < equipment.slots.Count; ++i)
        {
            ItemSlot slot = equipment.slots[i];
            if (slot.amount > 0)
#if !_iMMO2D
                ExecuteNonQueryMySql(command, "INSERT INTO character_equipment VALUES (@character, @slot, @name, @amount, @durability, @summonedHealth, @summonedLevel, @summonedExperience)",
#else
                ExecuteNonQueryMySql(command, "INSERT INTO character_equipment VALUES (@character, @slot, @name, @amount, @summonedHealth, @summonedLevel, @summonedExperience)",
#endif
                new SqlParameter("@character", equipment.name),
                            new SqlParameter("@slot", i),
                            new SqlParameter("@name", slot.item.name),
                            new SqlParameter("@amount", slot.amount),
#if !_iMMO2D
                            new SqlParameter("@durability", slot.item.durability),
#endif
                            new SqlParameter("@summonedHealth", slot.item.summonedHealth),
                            new SqlParameter("@summonedLevel", slot.item.summonedLevel),
                            new SqlParameter("@summonedExperience", slot.item.summonedExperience));
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // SaveSkills
    // -----------------------------------------------------------------------------------
    void SaveSkills(PlayerSkills skills, MySqlCommand command)
    {
#if _SERVER
        ExecuteNonQueryMySql(command, "DELETE FROM character_skills WHERE `character`=@character", new SqlParameter("@character", skills.name));
        foreach (Skill skill in skills.skills)
        {
            if (skill.level > 0)
            {
                ExecuteNonQueryMySql(command, @"
                    INSERT INTO character_skills
                    SET
                        `character` = @character,
                        name = @name,
                        level = @level,
                        castTimeEnd = @castTimeEnd,
                        cooldownEnd = @cooldownEnd",
                                    new SqlParameter("@character", skills.name),
                                    new SqlParameter("@name", skill.name),
                                    new SqlParameter("@level", skill.level),
                                    new SqlParameter("@castTimeEnd", skill.CastTimeRemaining()),
                                    new SqlParameter("@cooldownEnd", skill.CooldownRemaining()));
            }
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // SaveBuffs
    // -----------------------------------------------------------------------------------
    void SaveBuffs(PlayerSkills skills, MySqlCommand command)
    {
#if _SERVER
        ExecuteNonQueryMySql(command, "DELETE FROM character_buffs WHERE `character`=@character", new SqlParameter("@character", skills.name));
        foreach (Buff buff in skills.buffs)
        {
            ExecuteNonQueryMySql(command, "INSERT INTO character_buffs VALUES (@character, @name, @level, @buffTimeEnd)",
                            new SqlParameter("@character", skills.name),
                            new SqlParameter("@name", buff.name),
                            new SqlParameter("@level", buff.level),
                            new SqlParameter("@buffTimeEnd", buff.BuffTimeRemaining())); //(float)buff.BuffTimeRemaining()
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // SaveQuests
    // -----------------------------------------------------------------------------------
    void SaveQuests(PlayerQuests quests, MySqlCommand command)
    {
#if _SERVER
        ExecuteNonQueryMySql(command, "DELETE FROM character_quests WHERE `character`=@character", new SqlParameter("@character", quests.name));
        foreach (Quest quest in quests.quests)
        {
            ExecuteNonQueryMySql(command, "INSERT INTO character_quests VALUES (@character, @name, @progress, @completed)",
                            new SqlParameter("@character", quests.name),
                            new SqlParameter("@name", quest.name),
                            new SqlParameter("@progress", quest.progress),
                            new SqlParameter("@completed", quest.completed));
        }
#endif
    }


    // -----------------------------------------------------------------------------------
    // CharacterSave
    // -----------------------------------------------------------------------------------
    void CharacterSave(Player player, bool online, MySqlCommand command)
    {
#if _SERVER
        DateTime? onlineTimestamp = null;

        if (!online)
            onlineTimestamp = DateTime.Now;
#if _iMMO2D
        var query = @"
            INSERT INTO characters
            SET
                name=@name,
                account=@account,
                class = @class,
                x = @x,
                y = @y,
                level = @level,
                health = @health,
                mana = @mana,
                strength = @strength,
                intelligence = @intelligence,
                experience = @experience,
                skillExperience = @skillExperience,
                gold = @gold,
                coins = @coins,
                online = @online,
                lastsaved = @lastsaved,
                deleted = 0

            ON DUPLICATE KEY UPDATE
                account=@account,
                class = @class,
                x = @x,
                y = @y,
                level = @level,
                health = @health,
                mana = @mana,
                strength = @strength,
                intelligence = @intelligence,
                experience = @experience,
                skillExperience = @skillExperience,
                gold = @gold,
                coins = @coins,
                online = @online,
                lastsaved = @lastsaved,
                deleted = 0
            ";
#else
        var query = @"
            INSERT INTO characters
            SET
                name=@name,
                account=@account,
                class = @class,
                x = @x,
                y = @y,
                z = @z,
                level = @level,
                health = @health,
                mana = @mana,
                strength = @strength,
                intelligence = @intelligence,
                experience = @experience,
                skillExperience = @skillExperience,
                gold = @gold,
                coins = @coins,
                gamemaster = @gamemaster,
                online = @online,
                lastsaved = @lastsaved,
                deleted = 0

            ON DUPLICATE KEY UPDATE
                account=@account,
                class = @class,
                x = @x,
                y = @y,
                z = @z,
                level = @level,
                health = @health,
                mana = @mana,
                strength = @strength,
                intelligence = @intelligence,
                experience = @experience,
                skillExperience = @skillExperience,
                gold = @gold,
                coins = @coins,
                gamemaster = @gamemaster,
                online = @online,
                lastsaved = @lastsaved,
                deleted = 0
            ";
#endif
        ExecuteNonQueryMySql(command, query,
                    new SqlParameter("@name", player.name),
                    new SqlParameter("@account", player.account),
                    new SqlParameter("@class", player.className),
                    new SqlParameter("@x", player.transform.position.x),
                    new SqlParameter("@y", player.transform.position.y),
#if !_iMMO2D
                    new SqlParameter("@z", player.transform.position.z),
#endif
                    //new SqlParameter("@ry", player.transform.rotation.x),
                    new SqlParameter("@level", player.level.current),
                    new SqlParameter("@health", player.health.current),
                    new SqlParameter("@mana", player.mana.current),
                    new SqlParameter("@strength", player.strength),
                    new SqlParameter("@intelligence", player.intelligence),
                    new SqlParameter("@experience", player.experience.current),
                    new SqlParameter("@skillExperience", ((PlayerSkills)player.skills).skillExperience),
                    new SqlParameter("@gold", player.gold),
                    new SqlParameter("@coins", player.itemMall.coins),
#if !_iMMO2D
                    new SqlParameter("@gamemaster", player.isGameMaster),
#endif
                    new SqlParameter("@online", online ? 1 : 0),
                    new SqlParameter("@lastsaved", DateTime.UtcNow)
                );

        SaveInventory(player.inventory, command);
        SaveEquipment((PlayerEquipment)player.equipment, command);
        SaveItemCooldowns(player, command);
        SaveSkills((PlayerSkills)player.skills, command);
        SaveBuffs((PlayerSkills)player.skills, command);
#if !_iMMOQUESTS
        SaveQuests(player.quests, command);
#endif
        if (player.guild.InGuild())
            SaveGuild(player.guild.guild, false); // TODO only if needs saving? but would be complicated

        // addon system hooks
        onCharacterSave.Invoke(player);

#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterSave
    // -----------------------------------------------------------------------------------
    public void CharacterSave(Player player, bool online, bool useTransaction = true)
    {
#if _SERVER
        Transaction(command =>
        {
            CharacterSave(player, online, command);
        });
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterSaveMany
    // -----------------------------------------------------------------------------------

    public void CharacterSaveMany(IEnumerable<Player> players, bool online = true)
    {
#if _SERVER && !_iMMOTHREADDB
        Transaction(command =>
        {
            foreach (Player player in players)
                CharacterSave(player, online, command);
        });
#endif
    }

    // -----------------------------------------------------------------------------------
    // SaveGuild
    // -----------------------------------------------------------------------------------
    public void SaveGuild(Guild guild, bool useTransaction = true)
    {
#if _SERVER
        Transaction(command =>
        {
            var query = @"INSERT INTO guild_info SET `name` = @guild, `notice` = @notice ON DUPLICATE KEY UPDATE `notice` = @notice";

            ExecuteNonQueryMySql(command, query,
                                new SqlParameter("@guild", guild.name),
                                new SqlParameter("@notice", guild.notice));

            ExecuteNonQueryMySql(command, "DELETE FROM character_guild WHERE `guild` = @guild", new SqlParameter("@guild", guild.name));

            var query2 = @"INSERT INTO character_guild SET `guild` = @guild, `rank`= @rank, `character`= @character
                            ON DUPLICATE KEY UPDATE `guild` = @guild, `rank`= @rank, `character`= @character";

            foreach (GuildMember member in guild.members)
            {
                ExecuteNonQueryMySql(command, query2,
                                new SqlParameter("@guild", guild.name),
                                new SqlParameter("@rank", member.rank),
                                new SqlParameter("@character", member.name));
            }
        });
#endif
    }

    // -----------------------------------------------------------------------------------
    // GuildExists
    // -----------------------------------------------------------------------------------
    public bool GuildExists(string guild)
    {
#if _SERVER
        return ((long)ExecuteScalarMySql("SELECT Count(*) FROM guild_info WHERE `name`=@name", new SqlParameter("@name", guild))) == 1;
#else
        return false;
#endif
    }

    // -----------------------------------------------------------------------------------
    // RemoveGuild
    // -----------------------------------------------------------------------------------
    public void RemoveGuild(string guild)
    {
#if _SERVER
        ExecuteNonQueryMySql("DELETE FROM guild_info WHERE `name`=@name", new SqlParameter("@name", guild));
        ExecuteNonQueryMySql("DELETE FROM character_guild WHERE guild=@guild", new SqlParameter("@guild", guild));
#endif
    }

    // -----------------------------------------------------------------------------------
    // GrabCharacterOrders
    // -----------------------------------------------------------------------------------
    public List<long> GrabCharacterOrders(string characterName)
    {
        var result = new List<long>();
#if _SERVER
        var table = ExecuteReaderMySql("SELECT orderid, coins FROM character_orders WHERE `character`=@character AND processed=0", new SqlParameter("@character", characterName));

        foreach (var row in table)
        {
            result.Add((long)row[1]);
            ExecuteNonQueryMySql("UPDATE character_orders SET processed=1 WHERE orderid=@orderid", new SqlParameter("@orderid", (long)row[0]));
        }
#endif
        return result;
    }

    // -----------------------------------------------------------------------------------
}
#endif