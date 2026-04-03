#if _SERVER && _iMMOQUESTS

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
    // Character extended Quests
    // -----------------------------------------------------------------------------------
    class character_extended_quests
    {
        public string character { get; set; }
        [Indexed]
        public string name { get; set; }
        //public string pvped { get; set; }
        public string killed { get; set; }
        public string gathered { get; set; }
        public string harvested { get; set; }
        public string visited { get; set; }
        public string crafted { get; set; }
        public string looted { get; set; }
        public bool completed { get; set; }
        public bool completedAgain { get; set; }
        public string lastCompleted { get; set; }
        public int counter { get; set; }
    }
#endif
    private void Start_Tools_Quest()
    {
        onConnected.AddListener(Connect_Quest);
        onCharacterLoad.AddListener(CharacterLoad_Quest);
        onCharacterSave.AddListener(CharacterSave_Quest);
    }
    // -----------------------------------------------------------------------------------
    // Connect_Quest
    // -----------------------------------------------------------------------------------
    private void Connect_Quest()
    {
#if _MYSQL
        MySqlHelper.ExecuteNonQuery(connectionString, @"CREATE TABLE IF NOT EXISTS character_extended_quests (
                            `character` VARCHAR(32) NOT NULL,
                            name VARCHAR(111) NOT NULL,
                            killed VARCHAR(111) NOT NULL,
                            gathered VARCHAR(111) NOT NULL,
                            harvested VARCHAR(111) NOT NULL,
                            visited VARCHAR(111) NOT NULL,
                            crafted VARCHAR(111) NOT NULL,
                            looted VARCHAR(111) NOT NULL,
                            completed INTEGER(16) NOT NULL,
                            completedAgain INTEGER(16) NOT NULL,
                            lastCompleted VARCHAR(111) NOT NULL,
                            counter INTEGER(16) NOT NULL,
                                PRIMARY KEY(`character`, name)
                            ) CHARACTER SET=utf8mb4");
#elif _SQLITE
        connection.CreateTable<character_extended_quests>();
        connection.CreateIndex(nameof(character_extended_quests), new[] { "character", "name" });
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterLoad_Quest
    // -----------------------------------------------------------------------------------
    private void CharacterLoad_Quest(Player player)
    {
        if (player.playerExtendedQuest)
        {
#if _MYSQL
            List<List<object>> table = ExecuteReaderMySql("SELECT name, killed, gathered, harvested, visited, crafted, looted, completed, completedAgain, lastCompleted, counter FROM character_extended_quests WHERE `character`=@character",
                            new MySqlParameter("@character", player.name));
            foreach (List<object> row in table)
            {
                string questName = (string)row[0];
                Scriptable_Quest questData;
                if (Scriptable_Quest.All.TryGetValue(questName.GetStableHashCode(), out questData))
                {
                    ExtendedQuest quest = new ExtendedQuest(questData)
                    {
                        killedTarget = UMMO_Tools.IntStringToArray((string)row[1]),
                        gatheredTarget = UMMO_Tools.IntStringToArray((string)row[2]),
                        harvestedTarget = UMMO_Tools.IntStringToArray((string)row[3]),
                        visitedTarget = UMMO_Tools.IntStringToArray((string)row[4]),
                        craftedTarget = UMMO_Tools.IntStringToArray((string)row[5]),
                        lootedTarget = UMMO_Tools.IntStringToArray((string)row[6])
                    };

                    foreach (int i in quest.visitedTarget)
                        if (i != 0) quest.visitedCount++;

                    quest.completed = ((int)row[7]) != 0; // sqlite has no bool
                    quest.completedAgain = ((int)row[8]) != 0; // sqlite has no bool
                    quest.lastCompleted = (string)row[9];
                    quest.counter = (int)row[10];
                    player.playerExtendedQuest.extendedQuests.Add(quest);
                }
            }
#elif _SQLITE
            var table = connection.Query<character_extended_quests>("SELECT name, killed, gathered, harvested, visited, crafted, looted, completed, completedAgain, lastCompleted, counter FROM character_extended_quests WHERE character=?", player.name);
            foreach (var row in table)
            {
                string questName = row.name;
                Scriptable_Quest questData;
                if (Scriptable_Quest.All.TryGetValue(questName.GetStableHashCode(), out questData))
                {
                    ExtendedQuest quest = new ExtendedQuest(questData)
                    {
                        //quest.pvpedTarget = Tools.IntStringToArray(row.pvped);
                        killedTarget = UMMO_Tools.IntStringToArray(row.killed),
                        gatheredTarget = UMMO_Tools.IntStringToArray(row.gathered),
                        harvestedTarget = UMMO_Tools.IntStringToArray(row.harvested),
                        visitedTarget = UMMO_Tools.IntStringToArray(row.visited),
                        craftedTarget = UMMO_Tools.IntStringToArray(row.crafted),
                        lootedTarget = UMMO_Tools.IntStringToArray(row.looted)
                    };

                    foreach (int i in quest.visitedTarget)
                        if (i != 0) quest.visitedCount++;

                    quest.completed = row.completed;
                    quest.completedAgain = row.completedAgain;
                    quest.lastCompleted = row.lastCompleted;
                    quest.counter = row.counter;
                     player.playerExtendedQuest.extendedQuests.Add(quest);
                }
            }
#endif
        }
    }

    // -----------------------------------------------------------------------------------
    // CharacterSave_Quest
    // -----------------------------------------------------------------------------------
    private void CharacterSave_Quest(Player player)
    {
        if (player.playerExtendedQuest)
        {
#if _MYSQL
           /* MySqlHelper.ExecuteNonQuery(connectionString, "DELETE FROM character_extended_quests WHERE `character`=@character", new MySqlParameter("@character", player.name));
            var query2 = @"
            INSERT INTO character_extended_quests
            SET
            `character`=@character,
            name=@name,
            killed=@killed,
            gathered=@gathered,
            harvested=@harvested,
            visited=@visited,
            crafted=@crafted,
            looted=@looted,
            completed=@completed,
            completedAgain=@completedAgain,
            lastCompleted=@lastCompleted,
            counter=@counter";

            //foreach (ExtendedQuest quest in player.extendedQuests)
            foreach (ExtendedQuest quest in player.playerExtendedQuest.extendedQuests)
                MySqlHelper.ExecuteNonQuery(connectionString, query2,
                            new MySqlParameter("@character", player.name),
                            new MySqlParameter("@name", quest.name),
                            new MySqlParameter("@killed", UMMO_Tools.IntArrayToString(quest.killedTarget)),
                            new MySqlParameter("@gathered", UMMO_Tools.IntArrayToString(quest.gatheredTarget)),
                            new MySqlParameter("@harvested", UMMO_Tools.IntArrayToString(quest.harvestedTarget)),
                            new MySqlParameter("@visited", UMMO_Tools.IntArrayToString(quest.visitedTarget)),
                            new MySqlParameter("@crafted", UMMO_Tools.IntArrayToString(quest.craftedTarget)),
                            new MySqlParameter("@looted", UMMO_Tools.IntArrayToString(quest.lootedTarget)),
                            new MySqlParameter("@completed", Convert.ToInt32(quest.completed)),
                            new MySqlParameter("@completedAgain", Convert.ToInt16(quest.completedAgain)),
                            new MySqlParameter("@lastCompleted", quest.lastCompleted),
                            new MySqlParameter("@counter", quest.counter)
                            );*/

            // Supprimer les entr�es existantes pour ce personnage
            MySqlHelper.ExecuteNonQuery(connectionString, "DELETE FROM character_extended_quests WHERE `character`=@character", new MySqlParameter("@character", player.name));

            // Construction de la requ�te INSERT group�e
            string insertQuery = @"
                INSERT INTO character_extended_quests ( `character`, name, killed, gathered, harvested, visited, crafted, looted, completed, completedAgain, lastCompleted, counter ) VALUES ";

            List<MySqlParameter> parameters = new List<MySqlParameter>();
            int index = 0;

            foreach (ExtendedQuest quest in player.playerExtendedQuest.extendedQuests)
            {
                if (index > 0)
                    insertQuery += ",";

                insertQuery += $"(@character_{index}, @name_{index}, @killed_{index}, @gathered_{index}, @harvested_{index}, @visited_{index}, @crafted_{index}, @looted_{index}, @completed_{index}, @completedAgain_{index}, @lastCompleted_{index}, @counter_{index})";

                parameters.Add(new MySqlParameter($"@character_{index}", player.name));
                parameters.Add(new MySqlParameter($"@name_{index}", quest.name));
                parameters.Add(new MySqlParameter($"@killed_{index}", UMMO_Tools.IntArrayToString(quest.killedTarget)));
                parameters.Add(new MySqlParameter($"@gathered_{index}", UMMO_Tools.IntArrayToString(quest.gatheredTarget)));
                parameters.Add(new MySqlParameter($"@harvested_{index}", UMMO_Tools.IntArrayToString(quest.harvestedTarget)));
                parameters.Add(new MySqlParameter($"@visited_{index}", UMMO_Tools.IntArrayToString(quest.visitedTarget)));
                parameters.Add(new MySqlParameter($"@crafted_{index}", UMMO_Tools.IntArrayToString(quest.craftedTarget)));
                parameters.Add(new MySqlParameter($"@looted_{index}", UMMO_Tools.IntArrayToString(quest.lootedTarget)));
                parameters.Add(new MySqlParameter($"@completed_{index}", Convert.ToInt32(quest.completed)));
                parameters.Add(new MySqlParameter($"@completedAgain_{index}", Convert.ToInt16(quest.completedAgain)));
                parameters.Add(new MySqlParameter($"@lastCompleted_{index}", quest.lastCompleted));
                parameters.Add(new MySqlParameter($"@counter_{index}", quest.counter));

                index++;
            }

            // Ex�cution de la requ�te si des param�tres ont �t� ajout�s
            if (parameters.Count > 0)
            {
                MySqlHelper.ExecuteNonQuery(connectionString, insertQuery, parameters.ToArray());
            }


#elif _SQLITE
            connection.Execute("DELETE FROM character_extended_quests WHERE character=?", player.name);
            foreach (ExtendedQuest quest in  player.playerExtendedQuest.extendedQuests)
                connection.Insert(new character_extended_quests
                {
                    character = player.name,
                    name = quest.name,
                    //pvped = Tools.IntArrayToString(quest.killedTarget),
                    killed = UMMO_Tools.IntArrayToString(quest.killedTarget),
                    gathered = UMMO_Tools.IntArrayToString(quest.gatheredTarget),
                    harvested = UMMO_Tools.IntArrayToString(quest.harvestedTarget),
                    visited = UMMO_Tools.IntArrayToString(quest.visitedTarget),
                    crafted = UMMO_Tools.IntArrayToString(quest.craftedTarget),
                    looted = UMMO_Tools.IntArrayToString(quest.lootedTarget),
                    completed = quest.completed,
                    completedAgain = quest.completedAgain,
                    lastCompleted = quest.lastCompleted,
                    counter = quest.counter
                });
#endif
        }
    }

    // -----------------------------------------------------------------------------------
}
#endif