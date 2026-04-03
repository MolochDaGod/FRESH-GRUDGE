#if _SERVER
using System;

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
    // Character Daily Rewards
    // -----------------------------------------------------------------------------------
    class character_dailyrewards
    {
        [PrimaryKey] // important for performance: O(log n) instead of O(n)
        public string name { get; set; }
        public int counter { get; set; }
        public string lastClaim { get; set; }
    }

#endif
    private void Start_Tools_DailyRewards()
    {
        onConnected.AddListener(Connect_DailyRewards);
        onCharacterLoad.AddListener(CharacterLoad_DailyRewards);
        onCharacterSave.AddListener(CharacterSave_DailyRewards);
    }
    // -----------------------------------------------------------------------------------
    // Connec_DailyRewards
    // -----------------------------------------------------------------------------------
    private void Connect_DailyRewards()
    {
#if _MYSQL
		ExecuteNonQueryMySql(@"CREATE TABLE IF NOT EXISTS character_dailyrewards (
					`name` VARCHAR(32) NOT NULL,
					`counter` int(2) NOT NULL DEFAULT 0,
					`lastClaim` varchar(11) NOT NULL,
                     UNIQUE KEY `name` (`name`)
			) CHARACTER SET=utf8mb4");
#elif _SQLITE
        connection.CreateTable<character_dailyrewards>();
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterLoad_DailyRewards
    // -----------------------------------------------------------------------------------
    private void CharacterLoad_DailyRewards(Player player)
    {
#if _MYSQL
        var table = ExecuteReaderMySql("SELECT counter, lastClaim FROM character_dailyrewards WHERE `name`=@name", new MySqlParameter("@name", player.name));
        if (table.Count == 1)
        {
            var row = table[0];
            player.playerDailyRewards.dailyRewardCounter = (int)row[0];
            player.playerDailyRewards.lastClaimReward = (string)row[1];
        }
#elif _SQLITE
        var table = connection.Query<character_dailyrewards>("SELECT counter, lastClaim FROM character_dailyrewards WHERE name=?", player.name);
        if (table.Count == 1)
        {
            var row = table[0];
            player.playerDailyRewards.dailyRewardCounter = row.counter;
            player.playerDailyRewards.lastClaimReward = row.lastClaim;
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterSave_DailyRewards
    // -----------------------------------------------------------------------------------
    private void CharacterSave_DailyRewards(Player player)
    {
#if _MYSQL
        var query2 = @"INSERT INTO character_dailyrewards SET `name`=@name, counter = @counter, lastClaim = @lastClaim ON DUPLICATE KEY UPDATE counter = @counter, lastClaim = @lastClaim";
        ExecuteNonQueryMySql(query2,
                    new MySqlParameter("@name", player.name),
                    new MySqlParameter("@counter", player.playerDailyRewards.dailyRewardCounter),
                    new MySqlParameter("@lastClaim", player.playerDailyRewards.lastClaimReward));

#elif _SQLITE
        connection.Execute("DELETE FROM character_dailyrewards WHERE name=?", player.name);
        connection.Insert(new character_dailyrewards
        {
            name = player.name,
            counter = player.playerDailyRewards.dailyRewardCounter,
            lastClaim = player.playerDailyRewards.lastClaimReward
        });
#endif
    }
}
#endif