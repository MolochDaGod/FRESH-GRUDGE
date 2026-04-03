#if _SERVER && _iMMOUNLOCKABLECLASSES
using System.Collections.Generic;

#if _MYSQL
using MySql.Data.MySqlClient;
#endif

// DATABASE (SQLite / mySQL Hybrid)
public partial class Database
{

#if _SQLITE
	// -----------------------------------------------------------------------------------
    // Account Unlocked Classes
    // -----------------------------------------------------------------------------------
    class account_unlockedclasses
    {
        public string account { get; set; }
        public string classname { get; set; }
    }
#endif
    private void Start_Tools_UnlockableClasses()
    {
        onConnected.AddListener(Connect_UnlockableClasses);
        onCharacterLoad.AddListener(CharacterLoad_UnlockableClasses);
        onCharacterSave.AddListener(CharacterSave_UnlockableClasses);
    }
    // -----------------------------------------------------------------------------------
    // Connect_UnlockableClasses
    // -----------------------------------------------------------------------------------
    private void Connect_UnlockableClasses()
    {
#if _MYSQL
        // TODO : Next require account_id for optimise query MySql/MariaDB
		ExecuteNonQueryMySql(@"CREATE TABLE IF NOT EXISTS account_unlockedclasses (
 			account VARCHAR(32) NOT NULL,
 			classname VARCHAR(32) NOT NULL
 		)");
#elif _SQLITE
        connection.CreateTable<account_unlockedclasses>();
#endif
    }

    // -----------------------------------------------------------------------------------
    // GetUnlockedClasses
    // -----------------------------------------------------------------------------------
    public List<string> GetUnlockedClasses(string accountName)
    {
        List<string> unlockedClasses = new List<string>();
#if _MYSQL
        MySqlParameter accountParameter = new("@account", MySqlDbType.VarChar) { Value = accountName };

        var table = ExecuteReaderMySql("SELECT classname FROM account_unlockedclasses WHERE `account`=@account", accountParameter);
        foreach (var row in table)
            unlockedClasses.Add((string)row[0]);
#elif _SQLITE
        var table = connection.Query<account_unlockedclasses>("SELECT classname FROM account_unlockedclasses WHERE account=?", accountName);
        foreach (var row in table)
            unlockedClasses.Add(row.classname);
#endif
        return unlockedClasses;
    }

    // -----------------------------------------------------------------------------------
    // CharacterLoad_UnlockableClasses
    // -----------------------------------------------------------------------------------
    private void CharacterLoad_UnlockableClasses(Player player)
    {
#if _MYSQL
        MySqlParameter accountParameter = new("@account", MySqlDbType.VarChar) { Value = player.account };

        var table = ExecuteReaderMySql("SELECT classname FROM account_unlockedclasses WHERE `account`=@account", accountParameter);
        foreach (var row in table)
            player.playerAddonsConfigurator.UnlockedClasses.Add((string)row[0]);
#elif _SQLITE
        var table = connection.Query<account_unlockedclasses>("SELECT classname FROM account_unlockedclasses WHERE account=?", player.account);
        foreach (var row in table)
            player.playerAddonsConfigurator.UnlockedClasses.Add(row.classname);
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterSave_UnlockableClasses
    // -----------------------------------------------------------------------------------
    private void CharacterSave_UnlockableClasses(Player player)
    {
#if _MYSQL
        MySqlParameter accountParameter = new("@account", MySqlDbType.VarChar) { Value = player.account };
        ExecuteNonQueryMySql("DELETE FROM account_unlockedclasses WHERE `account`=@account", accountParameter);

        for (int i = 0; i < player.playerAddonsConfigurator.UnlockedClasses.Count; ++i)
        {
            MySqlParameter classNameParameter = new("@classname", MySqlDbType.VarChar) { Value = player.playerAddonsConfigurator.UnlockedClasses[i] };
            ExecuteNonQueryMySql("INSERT INTO account_unlockedclasses VALUES (@account, @classname)", accountParameter, classNameParameter);
        }

#elif _SQLITE
        connection.Execute("DELETE FROM account_unlockedclasses WHERE account=?", player.account);
        for (int i = 0; i < player.playerAddonsConfigurator.UnlockedClasses.Count; ++i)
            connection.Insert(new account_unlockedclasses
            {
                account = player.account,
                classname = player.playerAddonsConfigurator.UnlockedClasses[i]
            });
#endif
    }

    // -----------------------------------------------------------------------------------
}
#endif