#if _SERVER
#if _MYSQL
using MySql.Data.MySqlClient;
#elif _SQLITE
using SQLite;
#endif
using System;
using UnityEngine;
// DATABASE (SQLite / mySQL Hybrid)

public partial class Database
{

#if _SQLITE
	// -----------------------------------------------------------------------------------
    // Character Friends
    // -----------------------------------------------------------------------------------
    class character_friends
    {
        [Indexed]
        public string playerRequester { get; set; }
        [Indexed]
        public string playerAccepted { get; set; }
        public bool coupled { get; set; }
        public int friendPoint { get; set; }
        public DateTime created { get; set; }
        public string lastGiftedRequester { get; set; }
        public string lastGiftedAccepted { get; set; }
    }
#endif
    private void Start_Tools_Friendlist()
    {
        onConnected.AddListener(Connect_Friendlist);
        onCharacterLoad.AddListener(CharacterLoad_Friendlist);
    }
    // -----------------------------------------------------------------------------------
    // Connect_Friendlist
    // -----------------------------------------------------------------------------------
    private void Connect_Friendlist()
    {
#if _MYSQL
		ExecuteNonQueryMySql(@"CREATE TABLE IF NOT EXISTS character_friends (
                        playerRequester VARCHAR(32) NOT NULL,
                        playerAccepted VARCHAR(32) NOT NULL,
                        coupled int(1),
                        friendPoint int(11) NOT NULL,
                        created DATETIME NOT NULL,
                        lastGiftedRequester VARCHAR(32) NOT NULL,
                        lastGiftedAccepted VARCHAR(32) NOT NULL,
                        PRIMARY KEY(playerRequester, playerAccepted)
                        ) CHARACTER SET=utf8mb4");
#elif _SQLITE
        connection.CreateTable<character_friends>();
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterLoad_Friendlist
    // -----------------------------------------------------------------------------------
    private void CharacterLoad_Friendlist(Player player)
    {
#if _MYSQL
		var table = ExecuteReaderMySql("SELECT playerRequester, playerAccepted, friendPoint, coupled, created, lastGiftedRequester, lastGiftedAccepted " +
            "FROM character_friends WHERE playerRequester=@playerRequester or playerAccepted=@playerAccepted", new MySqlParameter("playerRequester", player.name), new MySqlParameter("playerAccepted", player.name));
        player.playerAddonsConfigurator.Friends.Clear();
        player.playerAddonsConfigurator.coupled = false;
        if (table.Count > 0) {
            for (int i = 0; i < table.Count; i++) {
                var row = table[i];
                string friend = (((string)row[0] == player.name) ? (string)row[1] : (string)row[0]);
                DateTime dateTime = DateTime.Parse((string)row[4].ToString());
                Friend frnd = new Friend(
                    friend, 
                    (string)row[0], 
                    (string)row[1], 
                    (int)row[2],
                    dateTime,
                    (((int)row[3] == 1)), 
                    ((string)row[5] ?? ""),
                    ((string)row[6] ?? "")
                    );
                player.playerAddonsConfigurator.Friends.Add(frnd);
                if ((int)row[3] == 1)
                    player.playerAddonsConfigurator.coupled = true;
            }
        }
#elif _SQLITE
        var table = connection.Query<character_friends>("SELECT playerRequester, playerAccepted, coupled, friendPoint, created, lastGiftedRequester, lastGiftedAccepted " +
            "FROM character_friends WHERE playerRequester=? or playerAccepted=?", player.name, player.name);
        player.playerAddonsConfigurator.Friends.Clear();
        player.playerAddonsConfigurator.coupled = false;
        if (table.Count > 0)
        {
            for (int i = 0; i < table.Count; i++)
            {
                var row = table[i];
                string friend = ((row.playerRequester == player.name) ? row.playerAccepted : row.playerRequester);
                Friend frnd = new Friend(friend, row.playerRequester, row.playerAccepted, row.friendPoint, row.created, row.coupled, row.lastGiftedRequester, row.lastGiftedAccepted);
                player.playerAddonsConfigurator.Friends.Add(frnd);
                if (row.coupled == true)
                    player.playerAddonsConfigurator.coupled = true;
            }
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // DeleteFriend
    // -----------------------------------------------------------------------------------
    public void DeleteFriend(Friend friend)
    {

#if _MYSQL
        var query2 = @"DELETE FROM character_friends WHERE playerRequester=@playerRequester and playerAccepted=@playerAccepted";
        ExecuteNonQueryMySql(query2, new MySqlParameter("@playerRequester", friend.name_requester), new MySqlParameter("@playerAccepted", friend.name_accepted));
#elif _SQLITE
        var table = connection.Query<character_friends>("DELETE FROM character_friends WHERE playerRequester=? and playerAccepted=?", friend.name_requester, friend.name_accepted);
        if (table.Count > 0)
        {
            Debug.Log("Friend line deleted");
        }
        else
        {
            Debug.Log("Friend not deleted -> " + friend.name_requester + " / " + friend.name_accepted);
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // AddFriend
    // -----------------------------------------------------------------------------------
    public void AddFriend(Friend frnd)
    {
#if _MYSQL
		var query2 = @"
            INSERT INTO character_friends SET
                playerRequester=@playerRequester,
                playerAccepted=@playerAccepted,
                friendPoint=@friendPoint,
                created=@created,
                coupled=@coupled,
                lastGiftedRequester=@lastGiftedRequester,
                lastGiftedAccepted=@lastGiftedAccepted";

        ExecuteNonQueryMySql(query2,
            new MySqlParameter("@playerRequester", frnd.name_requester),
            new MySqlParameter("@playerAccepted", frnd.name_accepted),
            new MySqlParameter("@friendPoint", frnd.friendPoint),
            new MySqlParameter("@created", frnd.created),
            new MySqlParameter("@coupled", frnd.coupled),
            new MySqlParameter("@lastGiftedRequester", frnd.lastGiftedRequester),
            new MySqlParameter("@lastGiftedAccepted", frnd.lastGiftedAccepted)
            );
#elif _SQLITE
        
        connection.Insert(new character_friends
            {
                playerRequester = frnd.name_requester,
                playerAccepted = frnd.name_accepted,
                friendPoint = frnd.friendPoint,
                created = frnd.created,
                coupled = frnd.coupled,
                lastGiftedRequester = frnd.lastGiftedRequester,
                lastGiftedAccepted = frnd.lastGiftedAccepted
            });
#endif
}

    // -----------------------------------------------------------------------------------
    // UpdateFriend
    public void UpdateFriend(Friend friend)
    {
#if _MYSQL
		var query2 = @"
            UPDATE character_friends SET
                friendPoint=@friendPoint,
                created = @created,
                coupled = @coupled,
                lastGiftedRequester = @lastGiftedRequester,
                lastGiftedAccepted = @lastGiftedAccepted
                    WHERE playerRequester=@playerRequester and playerAccepted=@playerAccepted";
        
        ExecuteNonQueryMySql(query2,
            new MySqlParameter("@friendPoint", friend.friendPoint),
            new MySqlParameter("@created", friend.created),
            new MySqlParameter("@coupled", friend.coupled),
            new MySqlParameter("@lastGiftedRequester", friend.lastGiftedRequester),
            new MySqlParameter("@lastGiftedAccepted", friend.lastGiftedAccepted),
            new MySqlParameter("@playerRequester", friend.name_requester),
            new MySqlParameter("@playerAccepted", friend.name_accepted)
        );
#elif _SQLITE
        connection.Query<character_friends>(@"
            UPDATE character_friends SET 
                friendPoint = ?,
                created = ?, 
                coupled = ?, 
                lastGiftedRequester = ?, 
                lastGiftedAccepted = ? 
                    WHERE playerRequester= ? and playerAccepted= ?",
               
            friend.friendPoint,
            friend.created,
            friend.coupled,
            friend.lastGiftedRequester,
            friend.lastGiftedAccepted,
            friend.name_requester,
            friend.name_accepted
        );
#endif
    }
    // -----------------------------------------------------------------------------------
}
#endif