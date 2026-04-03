#if _SERVER
#if _MYSQL
using MySql.Data.MySqlClient;
#endif

using UnityEngine.SceneManagement;
// DATABASE (SQLite / mySQL Hybrid)
public partial class Database
{

#if _SQLITE
    // -----------------------------------------------------------------------------------
    // Reports
    // -----------------------------------------------------------------------------------
    class reports
    {
        public string senderAcc { get; set; }
        public string senderCharacter { get; set; }
        public bool readBefore { get; set; }
        public string title { get; set; }
        public string message { get; set; }
        public bool solved { get; set; }
        public string time { get; set; }
        public string position { get; set; }
        public string scene { get; set; }
    }
#endif
    private void Start_Tools_Reports()
    {
        onConnected.AddListener(Connect_Reports);
        onCharacterLoad.AddListener(CharacterLoad_Reports);
    }
    #region Functions

    // -----------------------------------------------------------------------------------
    // Connect_Reports
    // Sets up the database to accept reports.
    // Create the reports table if it wasn't there for any reason.
    // -----------------------------------------------------------------------------------
    private void Connect_Reports()
    {
#if _MYSQL
		ExecuteNonQueryMySql(@"
							CREATE TABLE IF NOT EXISTS reports(
                           	senderAcc VARCHAR(32) NOT NULL,
                            senderCharacter VARCHAR(32) NOT NULL,
                           	readBefore INTEGER(16) NOT NULL,
                           	title VARCHAR(32) NOT NULL,
                           	message VARCHAR(512) NOT NULL,
                           	solved INTEGER(16) NOT NULL,
                           	time VARCHAR(128) NOT NULL,
                           	position VARCHAR(256) NOT NULL,
                           	scene VARCHAR(256) NOT NULL
                  			) CHARACTER SET=utf8mb4");
#elif _SQLITE
        connection.CreateTable<reports>();
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterLoad_Reports
    // Loads the reports when they're called. Currently not used by anything, here for future addon.
    // -----------------------------------------------------------------------------------
    private void CharacterLoad_Reports(Player player)
    {
#if _MYSQL
        var table = ExecuteReaderMySql("SELECT senderCharacter, readBefore, title, message, solved, time, position FROM reports WHERE senderAcc=@senderAcc ORDER by time DESC LIMIT 1", new MySqlParameter("@senderAcc", player.account));
        if (table.Count > 0)                                //If the table has anything then continue.
        {
            for (int i = 0; i < table.Count; i++)           //Loop through the table to gather the information.
            {
                var row = table[i];                         //Grab each row from the table.
                var report = new ReportsMember
                {
                    senderAcc = player.account,          //Set the account information.
                    senderCharacter = player.name,       //Set the character name of sender.
                    readBefore = ((int)row[1]) != 0,     //Set the read option.
                    title = (string)row[2],              //Set the title.
                    message = (string)row[3],            //Set the details message.
                    solved = ((int)row[4]) != 0,         //Set the solved or not option.
                    time = (string)row[5],               //Set the time and date.
                    position = (string)row[6]            //Set the position the player was on the map.
                };          //Make the report.
                player.playerAddonsConfigurator.reports.Add(report);                 //Add the report to a list to pull with other addon.
            }
        }
#elif _SQLITE
        var table = connection.Query<reports>("SELECT senderCharacter, readBefore, title, message, solved, time, position FROM 'reports' WHERE senderAcc=? ORDER by time DESC LIMIT 1", player.account);
        if (table.Count > 0)                                //If the table has anything then continue.
        {
            for (int i = 0; i < table.Count; i++)           //Loop through the table to gather the information.
            {
                var row = table[i];                         //Grab each row from the table.
                var report = new ReportsMember();          //Make the report.
                report.senderAcc = player.account;          //Set the account information.
                report.senderCharacter = player.name;       //Set the character name of sender.
                report.readBefore = row.readBefore;         //Set the read option.
                report.title = row.time;                    //Set the title.
                report.message = row.message;               //Set the details message.
                report.solved = row.solved;                 //Set the solved or not option.
                report.time = row.time;                     //Set the TimeLogout and date.
                report.position = row.position;             //Set the position the player was on the map.
                player.playerAddonsConfigurator.reports.Add(report);                 //Add the report to a list to pull with other addon.
            }
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // SaveReports
    //Saves the information provided to the database.
    // -----------------------------------------------------------------------------------
    public void SaveReports(ReportsMember report)
    {
#if _MYSQL
		ExecuteNonQueryMySql("INSERT INTO reports VALUES (@senderAcc, @senderCharacter, @readBefore, @title, @message, @solved, @time, @position, @scene)",
                        new MySqlParameter("@senderAcc", report.senderAcc),
                        new MySqlParameter("@senderCharacter", report.senderCharacter),
                        new MySqlParameter("@readBefore", report.readBefore ? 1 : 0),
                        new MySqlParameter("@title", report.title),
                        new MySqlParameter("@message", report.message),
                        new MySqlParameter("@solved", report.solved ? 1 : 0),
                        new MySqlParameter("@time", report.time),
                        new MySqlParameter("@position", report.position),
                        new MySqlParameter("@scene", SceneManager.GetActiveScene().name));
#elif _SQLITE
        connection.Insert(new reports
        {
            senderAcc = report.senderAcc,
            senderCharacter = report.senderCharacter,
            readBefore = report.readBefore,
            title = report.title,
            message = report.message,
            solved = report.solved,
            time = report.time,
            position = report.position,
            scene = SceneManager.GetActiveScene().name
        });
#endif
    }

    // -----------------------------------------------------------------------------------

    #endregion Functions
}
#endif