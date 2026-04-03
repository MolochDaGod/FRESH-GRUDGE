#if _SERVER

#if _MYSQL
using MySql.Data.MySqlClient;
using System;
#endif


public partial class Database
    {
#if _SQLITE
	// -----------------------------------------------------------------------------------
    // Character Currencies
    // -----------------------------------------------------------------------------------
    class character_titles
    {
        public string character { get; set; }
        public string title { get; set; }
        public bool earned { get; set; }
    }

    class character_titlechosen
    {
        public string character { get; set; }
        public string title { get; set; }
    }
#endif

    private void Start_Tools_PlayerTitles()
    {
        onConnected.AddListener(Connect_PlayerTitles);
        onCharacterLoad.AddListener(CharacterLoad_PlayerTitles);
        onCharacterSave.AddListener(CharacterSave_PlayerTitles);
    }
    // -----------------------------------------------------------------------------------
    // Connect_Sanctuary
    // -----------------------------------------------------------------------------------
    private void Connect_PlayerTitles()
    {
#if _MYSQL
        ExecuteNonQueryMySql(@"CREATE TABLE IF NOT EXISTS character_titles (
                            `character` VARCHAR(32) NOT NULL,
                           `title` VARCHAR(32) NOT NULL,
                            earned INTEGER NOT NULL,
                            PRIMARY KEY(`character`,title,earned)) CHARACTER SET=utf8mb4");
        ExecuteNonQueryMySql(@"CREATE TABLE IF NOT EXISTS character_titlechosen (
                            `character` VARCHAR(32) NOT NULL,
                            `title` VARCHAR(32) NOT NULL,
                            PRIMARY KEY(`character`,title)) CHARACTER SET=utf8mb4");
#elif _SQLITE
        connection.CreateTable<character_titles>();
        connection.CreateTable<character_titlechosen>();
#endif
    }


    private void CharacterLoad_PlayerTitles(Player player)
    {
        // get all earned titles from players database
#if _MYSQL
        var table = ExecuteReaderMySql("SELECT `character`, title, earned FROM character_titles WHERE `character`=@character", new MySqlParameter("@character", player.name));
        //var table = ExecuteReader("SELECT character, title, earned FROM character_titles WHERE character=@character", new SqliteParameter("@character", player.name));
        if (table.Count > 0)
        {
            foreach (var row in table)
            {
                // get the title name
                //string titleName = 
                // create title
                Titles newTitle = new Titles();
                 // set title info from database
                newTitle.title = (string)row[1];//titleName;
                                                         
                // we know the title is learned because its in our DB so we can just set to true
                newTitle.isLearned = true;
                // add to player title with relevent info to players titles dictionary
                player.playerTitles.EarnedTitles.Add(newTitle);
            }
        }
       // else Debug.Log("Aucun title");
#elif _SQLITE
        var table = connection.Query<character_titles>("SELECT character, title, earned FROM character_titles WHERE character=?", player.name);
        foreach (var row in table)
        {
            Titles newTitle = new Titles();
            newTitle.title = row.title;
            newTitle.isLearned = row.earned;

            player.playerTitles.EarnedTitles.Add(newTitle);
        }
#endif
        //LoadChosenTitle(player);
    }

    private void CharacterSave_PlayerTitles(Player player)
    {
        SaveAllTitles(player);
    }

    //=======Custom Functions=================================//
    public void SaveAllTitles(Player player)
    {
#if _MYSQL
        var table = ExecuteReaderMySql("DELETE FROM character_titles WHERE `character`=@character", new MySqlParameter("@character", player.name));
        foreach (Titles title in player.playerTitles.EarnedTitles)
        {
                //string key = keyValue.Key;
                // create new title and set info from dictionary key values
                //Titles newTitle = keyValue.Value;
                ExecuteNonQueryMySql("INSERT INTO character_titles VALUES (@character, @title, @earned)",
                     new MySqlParameter("@character", player.name),
                     new MySqlParameter("@title", title.title),
                     new MySqlParameter("@earned", Convert.ToInt32(title.isLearned))// sqlite has no bool
                );

        }
#elif _SQLITE
        connection.Execute("DELETE FROM character_titles WHERE character=?", player.name);
        foreach (Titles title in player.playerTitles.EarnedTitles)
        {
            connection.Insert(new character_titles
            {
                character = player.name,
                title = title.title,
                earned = title.isLearned
            });
        }
#endif
        SaveCurrentTitle(player);

    }

    public void SaveCurrentTitle(Player player)
    {
#if _MYSQL
        // titles: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        ExecuteNonQueryMySql("DELETE FROM character_titlechosen WHERE `character`=@character", new MySqlParameter("@character", player.name));
        // create new values
        if(!string.IsNullOrEmpty(player.playerTitles.currentTitle))
        //if (!player.playerTitles.noTitle)
        {
           // Debug.Log("no title sérieux ?");

            ExecuteNonQueryMySql("INSERT INTO character_titlechosen VALUES (@character, @title)",
                     new MySqlParameter("@character", player.name),
                     new MySqlParameter("@title", player.playerTitles.currentTitle)
                     );
        }
        //Debug.Log("player name =>" +player.name + ", Title choisi save " + player.playerTitles.currentTitle);
            /*ExecuteNonQuery("DELETE FROM character_titlechosen WHERE character=@character", new SqliteParameter("@character", player.name));
            // create new values
            if (!player.notitle)
            {
                ExecuteNonQuery("INSERT INTO character_titlechosen VALUES (@character, @title)",
                         new SqliteParameter("@character", player.name),
                         new SqliteParameter("@title", player.currentTitle)
                         );
            }
            else
            {
                ExecuteNonQuery("INSERT INTO character_titlechosen VALUES (@character, @title)",
                         new SqliteParameter("@character", player.name),
                         new SqliteParameter("@title", player.name)
                         );
            }*/
#elif _SQLITE
        connection.Execute("DELETE FROM character_titlechosen WHERE character=?", player.name);
        connection.Insert(new character_titlechosen
            {
                character = player.name,
                title = player.playerTitles.currentTitle
            });
#endif
    }

    public void LoadChosenTitle(Player player)
    {
#if _MYSQL
        // load the players chosen title from database
        var table = ExecuteReaderMySql("SELECT `character`, title FROM character_titlechosen WHERE `character`=@character", new MySqlParameter("@character", player.name));
        // entry exists and is valid
        if (table.Count == 1)
        {
            // set the player boolean for not using a title to false
            //player.playerTitles.noTitle = false;
            var row = table[0];
            // get the Title 
            string mytitle = ((string)row[1]);
            // set the players chosen title 
            //player.playerTitles.currentTitle = mytitle;
            player.playerTitles.ChangeTitle(mytitle);
        } 
#elif _SQLITE
        var table = connection.Query<character_titlechosen>("SELECT  `character`, title FROM character_titlechosen WHERE character=?", player.name);
        if (table.Count == 1)
        {
            // set the player boolean for not using a title to false
            //player.playerTitles.noTitle = false;
            var row = table[0];
            // get the Title 
            string mytitle = row.title;
            // set the players chosen title 
            //player.playerTitles.currentTitle = mytitle;
            player.playerTitles.currentTitle = mytitle;
        }
#endif
    }

    public bool TitleEarned(string stitle, Player player)
    {
#if _MYSQL
        // check if we have earned this achievements already
         var table = ExecuteReaderMySql("SELECT earned FROM character_titles WHERE  `character`=@character AND title=@title", new MySqlParameter("@character", player.name), new MySqlParameter("@title", stitle));
         // entry exists and is valid
         if (table.Count == 1)
         {
             var row = table[0];
             // check if this title has been earned
             if ((string)(row[0]) != "0")// sqlite has no bool
             {
                 // this title has been earned 
                 return true;
             }
         } 

#elif _SQLITE
        var table = connection.Query<character_titles>("SELECT earned FROM character_titles WHERE character=? AND title=?", player.name, stitle);
        if (table.Count == 1)
        {
            var row = table[0];
 
            // this title has been earned 
            return (row.earned) ? true : false;

        }
#endif
        return false;
    }

}
#endif