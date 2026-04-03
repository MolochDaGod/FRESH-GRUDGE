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

// Database (mySQL)
public partial class Database : MonoBehaviour
{
    public static Database singleton;
    private string connectionString = null;

#if _SERVER
    private const string EnvHost = "UMMORPG_MYSQL_HOST";
    private const string EnvDatabase = "UMMORPG_MYSQL_DATABASE";
    private const string EnvUser = "UMMORPG_MYSQL_USER";
    private const string EnvPassword = "UMMORPG_MYSQL_PASSWORD";
    private const string EnvPort = "UMMORPG_MYSQL_PORT";
    private const string EnvCharacterSet = "UMMORPG_MYSQL_CHARSET";
    private const string EnvConnectionTimeout = "UMMORPG_MYSQL_CONNECTION_TIMEOUT";
#endif

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
                    Server = EffectiveDbHost,
                    Database = EffectiveDbName,
                    UserID = EffectiveDbUser,
                    Password = EffectiveDbPassword,
                    Port = EffectiveDbPort,
                    CharacterSet = EffectiveDbCharacterSet,
                    ConnectionTimeout = EffectiveConnectionTimeout
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

#if _SERVER
    private static readonly string[] HostEnvKeys = { EnvHost, "MYSQL_HOST", "DB_HOST" };
    private static readonly string[] DatabaseEnvKeys = { EnvDatabase, "MYSQL_DATABASE", "MYSQL_DB", "DB_NAME" };
    private static readonly string[] UserEnvKeys = { EnvUser, "MYSQL_USER", "MYSQL_USERNAME", "DB_USER" };
    private static readonly string[] PasswordEnvKeys = { EnvPassword, "MYSQL_PASSWORD", "DB_PASSWORD" };
    private static readonly string[] PortEnvKeys = { EnvPort, "MYSQL_PORT", "DB_PORT" };
    private static readonly string[] CharsetEnvKeys = { EnvCharacterSet, "MYSQL_CHARSET", "DB_CHARSET" };
    private static readonly string[] ConnectionTimeoutEnvKeys = { EnvConnectionTimeout, "MYSQL_CONNECTION_TIMEOUT", "DB_CONNECTION_TIMEOUT" };

    public string RuntimeDbHost => EffectiveDbHost;
    public string RuntimeDbName => EffectiveDbName;
    public string RuntimeDbUser => EffectiveDbUser;
    public uint RuntimeDbPort => EffectiveDbPort;

    private string EffectiveDbHost => FirstNonEmpty(GetEnv(HostEnvKeys), dbHost, "127.0.0.1");
    private string EffectiveDbName => FirstNonEmpty(GetEnv(DatabaseEnvKeys), dbName, "database");
    private string EffectiveDbUser => FirstNonEmpty(GetEnv(UserEnvKeys), dbUser, "user");
    private string EffectiveDbPassword => FirstNonEmpty(GetEnv(PasswordEnvKeys), dbPassword, "password");
    private uint EffectiveDbPort => GetUIntEnv(PortEnvKeys, dbPort == 0 ? 3306u : dbPort);
    private string EffectiveDbCharacterSet => FirstNonEmpty(GetEnv(CharsetEnvKeys), dbCharacterSet, "utf8mb4");
    private uint EffectiveConnectionTimeout => GetUIntEnv(ConnectionTimeoutEnvKeys, 8);

    public bool RuntimeDbUsesPlaceholderHost()
    {
        return IsPlaceholderValue(EffectiveDbHost, "localhost", "127.0.0.1");
    }

    public bool RuntimeDbUsesPlaceholderName()
    {
        return IsPlaceholderValue(EffectiveDbName, "database", "dbName");
    }

    public bool RuntimeDbUsesPlaceholderUser()
    {
        return IsPlaceholderValue(EffectiveDbUser, "user", "dbUser");
    }

    public bool RuntimeDbUsesPlaceholderPassword()
    {
        return IsPlaceholderValue(EffectiveDbPassword, "password", "dbPassword");
    }

    private string FirstNonEmpty(params string[] values)
    {
        foreach (string value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        return string.Empty;
    }

    private string GetEnv(params string[] names)
    {
        for (int i = 0; i < names.Length; ++i)
        {
            string value = Environment.GetEnvironmentVariable(names[i]);
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return null;
    }

    private bool IsPlaceholderValue(string value, params string[] placeholders)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        foreach (string placeholder in placeholders)
        {
            if (string.Equals(value.Trim(), placeholder, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private void ValidateMySqlConfiguration()
    {
        List<string> issues = new List<string>();

        if (IsPlaceholderValue(EffectiveDbName, "database", "dbName"))
            issues.Add($"database name is not configured ({string.Join(", ", DatabaseEnvKeys)} or Database.dbName)");

        if (IsPlaceholderValue(EffectiveDbUser, "user", "dbUser"))
            issues.Add($"database user is not configured ({string.Join(", ", UserEnvKeys)} or Database.dbUser)");

        if (IsPlaceholderValue(EffectiveDbPassword, "password", "dbPassword"))
            issues.Add($"database password is not configured ({string.Join(", ", PasswordEnvKeys)} or Database.dbPassword)");

        if (EffectiveDbPort == 0)
            issues.Add($"database port is invalid ({string.Join(", ", PortEnvKeys)} or Database.dbPort)");

        if (issues.Count > 0)
        {
            throw new InvalidOperationException(
                "MySQL startup configuration is incomplete: " + string.Join("; ", issues));
        }
    }

    private void EnsureMySqlConnectionAvailable()
    {
        ValidateMySqlConfiguration();

        try
        {
            using MySqlConnection connection = new MySqlConnection(ConnectionString);
            connection.Open();
        }
        catch (Exception ex) when (ex is MySqlException || ex is TimeoutException)
        {
            throw new InvalidOperationException(
                $"Unable to connect to MySQL during server startup. Host={EffectiveDbHost}, Port={EffectiveDbPort}, Database={EffectiveDbName}, User={EffectiveDbUser}. " +
                $"Update the Database component or set one of these env var groups before starting the server: [{string.Join(", ", HostEnvKeys)}], [{string.Join(", ", DatabaseEnvKeys)}], [{string.Join(", ", UserEnvKeys)}], [{string.Join(", ", PasswordEnvKeys)}], [{string.Join(", ", PortEnvKeys)}].",
                ex);
        }
    }
#endif

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
    public void ExecuteNonQueryMySql(string sql, params MySqlParameter[] args)
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
    public void ExecuteNonQueryMySql(MySqlCommand command, string sql, params MySqlParameter[] args)
    {
#if _SERVER
        try
        {
            command.CommandText = sql;
            command.Parameters.Clear();

            foreach (MySqlParameter arg in args)
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
    public object ExecuteScalarMySql(string sql, params MySqlParameter[] args)
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
    public DataRow ExecuteDataRowMySql(string sql, params MySqlParameter[] args)
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
    public DataSet ExecuteDataSetMySql(string sql, params MySqlParameter[] args)
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
    public List<List<object>> ExecuteReaderMySql(string sql, params MySqlParameter[] args)
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
    public MySqlDataReader GetReader(string sql, params MySqlParameter[] args)
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

    private uint GetUIntEnv(string[] names, uint defaultValue = 0)
    {
        for (int i = 0; i < names.Length; ++i)
        {
            uint result = GetUIntEnv(names[i], uint.MaxValue);
            if (result != uint.MaxValue)
                return result;
        }

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
        EnsureMySqlConnectionAvailable();

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
        	stamina INT NOT NULL DEFAULT 0,
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
        //ExecuteNonQueryMySql("ALTER TABLE characters DROP PRIMARY KEY, ADD INDEX (name), ADD UNIQUE (name)");
        //ExecuteNonQueryMySql("ALTER TABLE characters ADD charcters_id INT(11) NOT NULL AUTO_INCREMENT FIRST, ADD PRIMARY KEY (charcters_id); ");
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
        	stamina INT NOT NULL DEFAULT 0,
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
            equipmentLevel INT NOT NULL DEFAULT 0,
            equipmentGems VARCHAR(256),

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
            equipmentLevel INT NOT NULL DEFAULT 0,
            equipmentGems VARCHAR(256),

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
            equipmentLevel INT NOT NULL,
            equipmentGems VARCHAR(256),

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
            equipmentLevel INT NOT NULL,
            equipmentGems VARCHAR(256),

            primary key(`character`, slot),
        	FOREIGN KEY(`character`)
                REFERENCES characters(name)
                ON DELETE CASCADE ON UPDATE CASCADE
        ) CHARACTER SET=utf8mb4");
#endif
        // -- character_itemcooldowns
        ExecuteNonQueryMySql(@"
        CREATE TABLE IF NOT EXISTS `character_itemcooldowns` (
            `character` varchar(32) NOT NULL,
            `category` varchar(50) NOT NULL,
            `cooldownEnd` float NOT NULL,
            UNIQUE KEY `character` (`character`,`category`),
            FOREIGN KEY(`character`)
                REFERENCES characters(name)
                ON DELETE CASCADE ON UPDATE CASCADE
        ) ENGINE=MyISAM DEFAULT CHARSET=utf8mb4;");

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
            MySqlParameter accountParameter = new("@name", MySqlDbType.VarChar) { Value = account };
            MySqlParameter passwordParameter = new("@password", MySqlDbType.VarChar) { Value = password };
            MySqlParameter createdParameter = new("@created", MySqlDbType.DateTime) { Value = DateTime.UtcNow };
            MySqlParameter lastloginParameter = new("@lastlogin", MySqlDbType.DateTime) { Value = DateTime.UtcNow };
            // Execute query
            ExecuteNonQueryMySql("INSERT IGNORE INTO accounts VALUES (@name, @password, @created, @lastlogin, 0)", accountParameter, passwordParameter, createdParameter, lastloginParameter);

            // check account name, password, banned status
            bool valid = ((long)ExecuteScalarMySql("SELECT Count(*) FROM accounts WHERE name=@name AND password=@password AND banned=0", accountParameter, passwordParameter)) == 1;
            if (valid)
            {
                // save last login time and return true
                ExecuteNonQueryMySql("UPDATE accounts SET lastlogin=@lastlogin WHERE name=@name", accountParameter, lastloginParameter);
                return true;
            }
        }
#endif
        return false;
    }

    public bool CreateAccount(string account, string password)
    {
#if _SERVER
        if (!string.IsNullOrWhiteSpace(account) && !string.IsNullOrWhiteSpace(password))
        {
            MySqlParameter accountParameter = new("@name", MySqlDbType.VarChar) { Value = account };
            MySqlParameter passwordParameter = new("@password", MySqlDbType.VarChar) { Value = password };
            bool valid = ((long)ExecuteScalarMySql("SELECT Count(*) FROM accounts WHERE name=@name AND password=@password AND banned=0", accountParameter, passwordParameter)) == 1;
            if (!valid)
            {
                MySqlParameter createdParameter = new("@created", MySqlDbType.DateTime) { Value = DateTime.UtcNow };
                MySqlParameter lastloginParameter = new("@lastlogin", MySqlDbType.DateTime) { Value = DateTime.UtcNow };
                // Execute query
                ExecuteNonQueryMySql("INSERT IGNORE INTO accounts VALUES (@name, @password, @created, @lastlogin, 0)", accountParameter, passwordParameter, createdParameter, lastloginParameter);
                return true;
            }
            else
            {
                return false;
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
        MySqlParameter characterNameParameter = new("@character", MySqlDbType.VarChar) { Value = characterName };
        return ((long)ExecuteScalarMySql("SELECT Count(*) FROM characters WHERE name=@character", characterNameParameter)) == 1;
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
        MySqlParameter characterNameParameter = new("@character", MySqlDbType.VarChar) { Value = characterName };
        ExecuteNonQueryMySql("UPDATE characters SET deleted=1 WHERE name=@character", characterNameParameter);
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharactersForAccount
    // -----------------------------------------------------------------------------------
    public List<string> CharactersForAccount(string account)
    {
        List<String> result = new List<String>();
#if _SERVER
        MySqlParameter accountParameter = new("@account", MySqlDbType.VarChar) { Value = account };
        var table = ExecuteReaderMySql("SELECT name FROM characters WHERE account=@account AND deleted=0", accountParameter);
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

        MySqlParameter characterNameParameter = new("@character", MySqlDbType.VarChar) { Value = inventory.name };
#if !_iMMO2D
        using (var reader = GetReader(@"SELECT name, slot, amount,durability, summonedHealth, summonedLevel, summonedExperience, equipmentLevel, equipmentGems FROM character_inventory WHERE `character`=@character;", characterNameParameter))
#else
        using (var reader = GetReader(@"SELECT name, slot, amount, summonedHealth, summonedLevel, summonedExperience, equipmentLevel, equipmentGems FROM character_inventory WHERE `character`=@character;", characterNameParameter))
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
                    item.equipmentLevel = (int)reader["equipmentLevel"];
                    item.equipmentGems = (string)reader["equipmentGems"];
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


        MySqlParameter characterNameParameter = new("@character", MySqlDbType.VarChar) { Value = equipment.name };
        using (var reader = GetReader(@"SELECT * FROM character_equipment WHERE `character`=@character;", characterNameParameter))
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
                    item.equipmentLevel = (int)reader["equipmentLevel"];
                    item.equipmentGems = (string)reader["equipmentGems"];
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

        MySqlParameter characterNameParameter = new("@character", MySqlDbType.VarChar) { Value = player.name };
        using (var reader = GetReader("SELECT * FROM character_itemcooldowns WHERE `character`=@character ", characterNameParameter))
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

        MySqlParameter characterNameParameter = new("@character", MySqlDbType.VarChar) { Value = skills.name };
        using (var reader = GetReader("SELECT name, level, castTimeEnd, cooldownEnd FROM character_skills WHERE `character`=@character ", characterNameParameter))
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

        MySqlParameter characterNameParameter = new("@character", MySqlDbType.VarChar) { Value = skills.name };
        using (var reader = GetReader("SELECT name, level, buffTimeEnd FROM character_buffs WHERE `character` = @character ", characterNameParameter))
        {
            while (reader.Read())
            {
                string buffName = (string)reader["name"];
                if (ScriptableSkill.All.TryGetValue(buffName.GetStableHashCode(), out ScriptableSkill skillData))
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

        MySqlParameter characterNameParameter = new("@character", MySqlDbType.VarChar) { Value = quests.name };
        using var reader = GetReader("SELECT name, progress, completed FROM character_quests WHERE `character`=@character", characterNameParameter);
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

        MySqlParameter guildNameParameter = new("@guild", MySqlDbType.VarChar) { Value = guildName };

        List<List<object>> table = ExecuteReaderMySql("SELECT notice FROM guild_info WHERE name=@guild", guildNameParameter);
        if (table.Count == 1)
        {
            List<object> row = table[0];
            guild.notice = (string)row[0];
        }

        List<GuildMember> members = new List<GuildMember>();
        /**
        table = ExecuteReaderMySql("SELECT `character`, `rank` FROM character_guild WHERE guild=@guild", guildNameParameter);
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
                object scalar = ExecuteScalarMySql("SELECT level FROM characters WHERE name=@character", new MySqlParameter("@character", member.name));
                member.level = scalar != null ? ((int)scalar) : 1;
            }
            members.Add(member);
        }*/

        using var reader = GetReader("SELECT cg.character as guild_member_name, cg.rank, c.level, c.online FROM character_guild cg LEFT JOIN characters c ON cg.character = c.name WHERE cg.guild = @guild ", guildNameParameter);
        while (reader.Read())
        {

            GuildMember member = new()
            {
                name = reader["guild_member_name"].ToString(),
                rank = (GuildRank)(int)reader["rank"],
                level = reader["level"] != null ? (int)reader["level"] : 1,
                online = (int)reader["online"] == 1
            };
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

        MySqlParameter characterNameParameter = new("@character", MySqlDbType.VarChar) { Value = playerGuild.name };
        string guildName = (string)ExecuteScalarMySql("SELECT guild FROM character_guild WHERE `character`=@character", characterNameParameter);
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
        var row = ExecuteDataRowMySql("SELECT * FROM characters WHERE name=@name AND deleted=0", new MySqlParameter("@name", characterName));

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
#if _iMMOSTAMINA
                int stamina = (int)row["stamina"];
#endif
                player.experience.current = (long)row["experience"];
                ((PlayerSkills)player.skills).skillExperience = (long)row["skillExperience"];
                player.gold = (long)row["gold"];
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
                    ExecuteNonQueryMySql("UPDATE characters SET online=1, lastsaved=@lastsaved WHERE name=@name", new MySqlParameter("@name", characterName), new MySqlParameter("@lastsaved", DateTime.UtcNow));

                player.health.current = health;
                player.mana.current = mana;
#if _iMMOSTAMINA
                //Debug.Log(" -> "+stamina);
                player.stamina.current = stamina;
#endif


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
        ExecuteNonQueryMySql(command, "DELETE FROM character_inventory WHERE `character`=@character", new MySqlParameter("@character", inventory.name));
        for (int i = 0; i < inventory.slots.Count; ++i)
        {
            ItemSlot slot = inventory.slots[i];
            if (slot.amount > 0)
#if !_iMMO2D
                ExecuteNonQueryMySql(command, "INSERT INTO character_inventory VALUES (@character, @slot, @name, @amount, @durability, @summonedHealth, @summonedLevel, @summonedExperience, @equipementLevel, @equipmentGems)",
#else
                ExecuteNonQueryMySql(command, "INSERT INTO character_inventory VALUES (@character, @slot, @name, @amount, @summonedHealth, @summonedLevel, @summonedExperience, @equipementLevel, @equipmentGems)",
#endif
                    new MySqlParameter("@character", inventory.name),
                    new MySqlParameter("@slot", i),
                    new MySqlParameter("@name", slot.item.name),
                    new MySqlParameter("@amount", slot.amount),
#if !_iMMO2D
                    new MySqlParameter("@durability", slot.item.durability),
#endif
                    new MySqlParameter("@summonedHealth", slot.item.summonedHealth),
                    new MySqlParameter("@summonedLevel", slot.item.summonedLevel),
                    new MySqlParameter("@summonedExperience", slot.item.summonedExperience),
                    new MySqlParameter("@equipementLevel", slot.item.equipmentLevel),
                    new MySqlParameter("@equipmentGems", slot.item.summonedExperience));
    }
#endif
        }


    void SaveItemCooldowns(Player player, MySqlCommand command)
    {
        // equipment: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        ExecuteNonQueryMySql(command, "DELETE FROM character_itemcooldowns WHERE `character`=@character", new MySqlParameter("@character", player.name));
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
                                    new MySqlParameter("@character", player.name),
                                    new MySqlParameter("@category", kvp.Key),
                                    new MySqlParameter("@cooldownEnd", cooldown));
            }
        }
    }


    // -----------------------------------------------------------------------------------
    // SaveEquipment
    // -----------------------------------------------------------------------------------
    void SaveEquipment(PlayerEquipment equipment, MySqlCommand command)
    {
#if _SERVER
        ExecuteNonQueryMySql(command, "DELETE FROM character_equipment WHERE `character`=@character", new MySqlParameter("@character", equipment.name));
        for (int i = 0; i < equipment.slots.Count; ++i)
        {
            ItemSlot slot = equipment.slots[i];
            if (slot.amount > 0)
#if !_iMMO2D
                ExecuteNonQueryMySql(command, "INSERT INTO character_equipment VALUES (@character, @slot, @name, @amount, @durability, @summonedHealth, @summonedLevel, @summonedExperience, @equipementLevel, @equipmentGems)",
#else
                ExecuteNonQueryMySql(command, "INSERT INTO character_equipment VALUES (@character, @slot, @name, @amount, @summonedHealth, @summonedLevel, @summonedExperience, @equipementLevel, @equipmentGems)",
#endif
                    new MySqlParameter("@character", equipment.name),
                    new MySqlParameter("@slot", i),
                    new MySqlParameter("@name", slot.item.name),
                    new MySqlParameter("@amount", slot.amount),
#if !_iMMO2D
                    new MySqlParameter("@durability", slot.item.durability),
#endif
                    new MySqlParameter("@summonedHealth", slot.item.summonedHealth),
                    new MySqlParameter("@summonedLevel", slot.item.summonedLevel),
                    new MySqlParameter("@summonedExperience", slot.item.summonedExperience),
                    new MySqlParameter("@equipementLevel", slot.item.equipmentLevel),
                    new MySqlParameter("@equipmentGems", slot.item.summonedExperience));
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // SaveSkills
    // -----------------------------------------------------------------------------------
    void SaveSkills(PlayerSkills skills, MySqlCommand command)
    {
#if _SERVER
        ExecuteNonQueryMySql(command, "DELETE FROM character_skills WHERE `character`=@character", new MySqlParameter("@character", skills.name));
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
                                    new MySqlParameter("@character", skills.name),
                                    new MySqlParameter("@name", skill.name),
                                    new MySqlParameter("@level", skill.level),
                                    new MySqlParameter("@castTimeEnd", skill.CastTimeRemaining()),
                                    new MySqlParameter("@cooldownEnd", skill.CooldownRemaining()));
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
        ExecuteNonQueryMySql(command, "DELETE FROM character_buffs WHERE `character`=@character", new MySqlParameter("@character", skills.name));
        foreach (Buff buff in skills.buffs)
        {
            ExecuteNonQueryMySql(command, "INSERT INTO character_buffs VALUES (@character, @name, @level, @buffTimeEnd)",
                            new MySqlParameter("@character", skills.name),
                            new MySqlParameter("@name", buff.name),
                            new MySqlParameter("@level", buff.level),
                            new MySqlParameter("@buffTimeEnd", buff.BuffTimeRemaining())); //(float)buff.BuffTimeRemaining()
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // SaveQuests
    // -----------------------------------------------------------------------------------
    void SaveQuests(PlayerQuests quests, MySqlCommand command)
    {
#if _SERVER
        ExecuteNonQueryMySql(command, "DELETE FROM character_quests WHERE `character`=@character", new MySqlParameter("@character", quests.name));
        foreach (Quest quest in quests.quests)
        {
            ExecuteNonQueryMySql(command, "INSERT INTO character_quests VALUES (@character, @name, @progress, @completed)",
                            new MySqlParameter("@character", quests.name),
                            new MySqlParameter("@name", quest.name),
                            new MySqlParameter("@progress", quest.progress),
                            new MySqlParameter("@completed", quest.completed));
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
                stamina = @stamina,
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
                stamina = @stamina,
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
                stamina = @stamina,
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
                z = @z,
                level = @level,
                health = @health,
                mana = @mana,
                stamina = @stamina,
                experience = @experience,
                skillExperience = @skillExperience,
                gold = @gold,
                coins = @coins,
                online = @online,
                lastsaved = @lastsaved,
                deleted = 0
            ";
#endif
        ExecuteNonQueryMySql(command, query,
                    new MySqlParameter("@name", player.name),
                    new MySqlParameter("@account", player.account),
                    new MySqlParameter("@class", player.className),
                    new MySqlParameter("@x", player.transform.position.x),
                    new MySqlParameter("@y", player.transform.position.y),
#if !_iMMO2D
                    new MySqlParameter("@z", player.transform.position.z),
#endif
                    //new MySqlParameter("@ry", player.transform.rotation.x),
                    new MySqlParameter("@level", player.level.current),
                    new MySqlParameter("@health", player.health.current),
                    new MySqlParameter("@mana", player.mana.current),
                    new MySqlParameter("@stamina",
#if _iMMOSTAMINA
                    player.stamina.current
#else
                    "0"
#endif
                    ),
                    new MySqlParameter("@experience", player.experience.current),
                    new MySqlParameter("@skillExperience", ((PlayerSkills)player.skills).skillExperience),
                    new MySqlParameter("@gold", player.gold),
                    new MySqlParameter("@coins", player.itemMall.coins),
                    new MySqlParameter("@online", online ? 1 : 0),
                    new MySqlParameter("@lastsaved", DateTime.UtcNow)
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
        try
        {
            Transaction(command =>
            {
                foreach (Player player in players)
                {
                    CharacterSave(player, online, command);
                }
            });
        }
        catch(Exception e)
        {
            Debug.Log("SQL error :" + e );
        }
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
                                new MySqlParameter("@guild", guild.name),
                                new MySqlParameter("@notice", guild.notice));

            ExecuteNonQueryMySql(command, "DELETE FROM character_guild WHERE `guild` = @guild", new MySqlParameter("@guild", guild.name));

            var query2 = @"INSERT INTO character_guild SET `guild` = @guild, `rank`= @rank, `character`= @character
                            ON DUPLICATE KEY UPDATE `guild` = @guild, `rank`= @rank, `character`= @character";

            foreach (GuildMember member in guild.members)
            {
                ExecuteNonQueryMySql(command, query2,
                                new MySqlParameter("@guild", guild.name),
                                new MySqlParameter("@rank", member.rank),
                                new MySqlParameter("@character", member.name));
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
        return ((long)ExecuteScalarMySql("SELECT Count(*) FROM guild_info WHERE `name`=@name", new MySqlParameter("@name", guild))) == 1;
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
        ExecuteNonQueryMySql("DELETE FROM guild_info WHERE `name`=@name", new MySqlParameter("@name", guild));
        ExecuteNonQueryMySql("DELETE FROM character_guild WHERE guild=@guild", new MySqlParameter("@guild", guild));
#endif
    }

    // -----------------------------------------------------------------------------------
    // GrabCharacterOrders
    // -----------------------------------------------------------------------------------
    public List<long> GrabCharacterOrders(string characterName)
    {
        var result = new List<long>();
#if _SERVER
        var table = ExecuteReaderMySql("SELECT orderid, coins FROM character_orders WHERE `character`=@character AND processed=0", new MySqlParameter("@character", characterName));

        foreach (var row in table)
        {
            result.Add((long)row[1]);
            ExecuteNonQueryMySql("UPDATE character_orders SET processed=1 WHERE orderid=@orderid", new MySqlParameter("@orderid", (long)row[0]));
        }
#endif
        return result;
    }

    // -----------------------------------------------------------------------------------
}
#endif