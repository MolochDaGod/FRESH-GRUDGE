
#region SERVER ONLY
#if _SERVER

using System.Collections.Generic;
#if _MYSQL
using MySql.Data.MySqlClient;
#endif

// DATABASE (SQLite / mySQL Hybrid)
public partial class Database
{

#if _SQLITE

	// -----------------------------------------------------------------------------------
    // Account Unlockables
    // -----------------------------------------------------------------------------------
    class account_unlockables
    {
        public string account { get; set; }
        public string unlockable { get; set; }
    }
#elif _MYSQL
    private void Account_unlockables()
    {
        ExecuteNonQueryMySql(@"CREATE TABLE IF NOT EXISTS account_unlockables ( account VARCHAR(255) NOT NULL, unlockable VARCHAR(255) NOT NULL )");
        /**
         * for future
         */ 
         /*ExecuteNonQueryMySql(@"DROP TABLE IF EXISTS `account_unlockables`;
            CREATE TABLE IF NOT EXISTS `account_unlockablesv2` (
              `account_id` int(11) NOT NULL,
              `unlocked` varchar(255) NOT NULL,
              UNIQUE KEY `account_id` (`account_id`,`unlocked`)
            ) ENGINE=MyISAM DEFAULT CHARSET=latin1; ");
         */
    }
#endif

    private void Start_Tools_AccoundUnlocables()
    {
        onConnected.AddListener(Connect_AccountUnlockables);
        onCharacterLoad.AddListener(CharacterLoad_AccountUnlockables);
        onCharacterSave.AddListener(CharacterSave_AccountUnlockables);
    }

    // -----------------------------------------------------------------------------------
    // Connect_AccountUnlockables
    // -----------------------------------------------------------------------------------
    private void Connect_AccountUnlockables()
    {
#if _MYSQL
        Account_unlockables();
#elif _SQLITE
        connection.CreateTable<account_unlockables>();
#endif
    }
    


    // -----------------------------------------------------------------------------------
    // CharacterLoad_AccountUnlockables
    // -----------------------------------------------------------------------------------
    private void CharacterLoad_AccountUnlockables(Player player)
    {
#if _MYSQL
        var table = ExecuteReaderMySql("SELECT unlockable FROM account_unlockables WHERE `account`=@account", new MySqlParameter("@account", player.account));
        foreach (var row in table)
            player.playerAddonsConfigurator.AccountUnlockables.Add((string)row[0]);
#elif _SQLITE
        var table = connection.Query<account_unlockables>("SELECT unlockable FROM account_unlockables WHERE account=?", player.account);
        foreach (var row in table)
            player.playerAddonsConfigurator.AccountUnlockables.Add(row.unlockable);
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterSave_AccountUnlockables
    // -----------------------------------------------------------------------------------
    private void CharacterSave_AccountUnlockables(Player player)
    {
#if _MYSQL
        ExecuteNonQueryMySql("DELETE FROM account_unlockables WHERE `account`=@account", new MySqlParameter("@account", player.account));
		for (int i = 0; i < player.playerAddonsConfigurator.AccountUnlockables.Count; ++i) {
			ExecuteNonQueryMySql("INSERT INTO account_unlockables VALUES (@account, @unlockable)",
 				new MySqlParameter("@account", player.account),
 				new MySqlParameter("@unlockable", player.playerAddonsConfigurator.AccountUnlockables[i]));
 		}
#elif _SQLITE
        connection.Execute("DELETE FROM account_unlockables WHERE account=?", player.account);
        for (int i = 0; i < player.playerAddonsConfigurator.AccountUnlockables.Count; ++i)
            connection.Insert(new account_unlockables
            {
                account = player.account,
                unlockable = player.playerAddonsConfigurator.AccountUnlockables[i]
            });
#endif
    }

    // -----------------------------------------------------------------------------------
    // GetAccountUnlockables
    // -----------------------------------------------------------------------------------
    public List<string> GetAccountUnlockables(string accountName)
    {
        List<string> unlockables = new List<string>();

#if _MYSQL
		var table = ExecuteReaderMySql("SELECT unlockable FROM account_unlockables WHERE `account`=@account", new MySqlParameter("@account", accountName));
        foreach (var row in table)
            unlockables.Add((string)row[0]);
#elif _SQLITE
        var table = connection.Query<account_unlockables>("SELECT unlockable FROM account_unlockables WHERE account=?", accountName);
        foreach (var row in table)
            unlockables.Add(row.unlockable);
#endif
        return unlockables;
    }
    // -----------------------------------------------------------------------------------

}
#endif
#endregion SERVER ONLY