#if _SERVER
using System.Collections.Generic;
using System;

#if _MYSQL
using MySql.Data.MySqlClient;
#elif _SQLITE
using SQLite;
#endif

// DATABASE

public partial class Database
{

#if _SQLITE
 	// -----------------------------------------------------------------------------------
    // Mail
    // -----------------------------------------------------------------------------------
    public class mail
    {
        [PrimaryKey] // important for performance: O(log n) instead of O(n)
        public int id { get; set; }
        public string messageFrom { get; set; }
        public string messageTo { get; set; }
        public string subject { get; set; }
        public string body { get; set; }
        public long sent { get; set; }
        public long expires { get; set; }
        public int read { get; set; }
        public int deleted { get; set; }
        public string item { get; set; }
    }
#endif

    // -----------------------------------------------------------------------------------
    // MySql Server Add Events
    // -----------------------------------------------------------------------------------
    private void Start_Tools_Mail()
    {
        onConnected.AddListener(Connect_Mail);
        onCharacterLoad.AddListener(CharacterLoad_Mail);
    }
    // -----------------------------------------------------------------------------------
    // Connect_Mail
    // -----------------------------------------------------------------------------------
    public void Connect_Mail()
    {
#if _MYSQL
        //ExecuteNonQueryMySql(@"
		MySqlHelper.ExecuteNonQuery(connectionString, @"
                            CREATE TABLE IF NOT EXISTS mail(
							id INTEGER(16) NOT NULL AUTO_INCREMENT,
							messageFrom VARCHAR(32) NOT NULL,
							messageTo VARCHAR(32) NOT NULL,
							subject VARCHAR(255) NOT NULL,
							body VARCHAR(2048) NOT NULL,
							sent BIGINT(16) NOT NULL,
							expires BIGINT(16) NOT NULL,
							`read` BIGINT(16) NOT NULL,
							`deleted` BIGINT(16) NOT NULL,
							`item` VARCHAR(50) NOT NULL,
                            PRIMARY KEY(id)
                            ) CHARACTER SET=utf8mb4");
#elif _SQLITE
        connection.CreateTable<mail>();
#endif
    }

#if _MYSQL
    // -----------------------------------------------------------------------------------
    // MySql Mail_BuildMessageFromDBRow
    // -----------------------------------------------------------------------------------
    public MailMessage Mail_BuildMessageFromDBRow(List<object> row)
    {
        MailMessage message = new MailMessage();

        int colNum = 0;

		message.id		= (int)row[colNum++];
		message.from	= (string)row[colNum++];
		message.to		= (string)row[colNum++];
		message.subject = (string)row[colNum++];
		message.body	= (string)row[colNum++];
		message.sent	= (long)row[colNum++];
		message.expires = (long)row[colNum++];
		message.read	= (long)row[colNum++];
		message.deleted = (long)row[colNum++];

		string name = (string)row[colNum++];
		if (ScriptableItem.All.TryGetValue(name.GetStableHashCode(), out ScriptableItem itemData))
        	message.item = itemData;

        return message;
    }
#endif

#if _SQLITE
    // -----------------------------------------------------------------------------------
    // Sqlite Mail_BuildMessageFromDBRow
    // -----------------------------------------------------------------------------------
    public MailMessage Mail_BuildMessageFromDBRow(mail row)
    {
        MailMessage message = new MailMessage();

        message.id = row.id;
        message.from = row.messageFrom;
        message.to = row.messageTo;
        message.subject = row.subject;
        message.body = row.body;
        message.sent = row.sent;
        message.expires = row.expires;
        message.read = row.read;
        message.deleted = row.deleted;

        string item = row.item;
        if (ScriptableItem.All.TryGetValue(item.GetStableHashCode(), out ScriptableItem itemData))
            message.item = itemData;

        return message;
    }

#endif

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    private void CharacterLoad_Mail(Player player)
    {
#if _MYSQL
        //var table = ExecuteReaderMySql("SELECT * FROM mail WHERE messageTo=@character AND deleted=0 AND expires > @expires ORDER BY sent", new MySqlParameter("@character", player.name), new MySqlParameter("@expires", Epoch.Current())); 
        var table = ExecuteReaderMySql("SELECT * FROM mail WHERE messageTo=@character AND deleted=0 AND expires > @expires ORDER BY sent", new MySqlParameter("@character", player.name), new MySqlParameter("@expires", Epoch.Current()));
#elif _SQLITE
        var table = connection.Query<mail>("SELECT * FROM mail WHERE messageTo=? AND deleted=0 AND expires > " + Epoch.Current() + " ORDER BY sent", player.name);
#endif
        foreach (var row in table)
        {
            MailMessage message = Mail_BuildMessageFromDBRow(row);
            player.playerAddonsConfigurator.mailMessages.Add(message);
        }
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public List<MailSearch> Mail_SearchForCharacter(string name, string selfPlayer)
    {
        List<MailSearch> result = new List<MailSearch>();

#if _MYSQL
        //var table = ExecuteReaderMySql(@"SELECT `name`, level FROM characters WHERE `name` LIKE @search AND name<> @selfPlayer ORDER BY `name` LIMIT 30", new MySqlParameter("@search", "%" + name + "%"), new MySqlParameter("@selfPlayer", selfPlayer)); ;
        var table = ExecuteReaderMySql(@"SELECT `name`, level FROM characters WHERE `name` LIKE @search AND name<> @selfPlayer ORDER BY `name` LIMIT 30", new MySqlParameter("@search", "%" + name + "%"), new MySqlParameter("@selfPlayer", selfPlayer)); ;

        foreach (var row in table) {
			MailSearch res = new MailSearch();
			res.name = (string)row[0];
			res.level = Convert.ToInt32((int)row[1]);
			res.guild = "";

			result.Add(res);
		}

#elif _SQLITE
        /**
		 * Order by here is setup in such a way that:
		 *		exact matches appear first
		 *		followed by names where the search string is closer to the front of the name
		 */
        var table = connection.Query<characters>("SELECT `name`, level FROM characters WHERE name LIKE '%' || ? || '%' AND name<> ? ORDER BY CASE WHEN name =? THEN 0 ELSE INSTR(LOWER(name), LOWER(?)) END, name LIMIT 30", name, selfPlayer, name, name);


        foreach (var row in table)
        {
            MailSearch res = new MailSearch();
            res.name = row.name;
            res.level = row.level;
            res.guild = "";

            result.Add(res);
        }

#endif

        return result;
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public void Mail_CreateMessage(string from, string to, string subject, string body, string itemName, long expiration = 0)
    {
        long sent = Epoch.Current();
        long expires = 0;
        
        if (expiration > 0)
            expires = sent + expiration;

        if (itemName == null) itemName = "";

#if _MYSQL
        //ExecuteNonQueryMySql(@"INSERT INTO mail ( messageFrom, messageTo, subject, body, sent, `expires`, `read`, `deleted`, `item` ) VALUES ( @from, @to, @subject, @body, @sent, @expires, 0, 0, @item)",
        MySqlHelper.ExecuteNonQuery(connectionString, @"INSERT INTO mail ( messageFrom, messageTo, subject, body, sent, `expires`, `read`, `deleted`, `item` ) VALUES ( @from, @to, @subject, @body, @sent, @expires, 0, 0, @item)",
                        new MySqlParameter("@from", from),
						new MySqlParameter("@to", to),
						new MySqlParameter("@subject", subject),
						new MySqlParameter("@body", body),
						new MySqlParameter("@sent", sent),
						new MySqlParameter("@expires", expires),
						new MySqlParameter("@item", itemName )
						);
#elif _SQLITE
        connection.Insert(new mail
        {
            messageFrom = from,
            messageTo = to,
            subject = subject,
            body = body,
            sent = sent,
            expires = expires,
            item = itemName
        });
#endif
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public void Mail_UpdateMessage(MailMessage message)
    {
        string itemName = "";
        if (message.item != null)
            itemName = message.item.name;

#if _MYSQL
        //ExecuteNonQueryMySql(@"UPDATE mail SET `read`=@read, deleted=@deleted, item=@item WHERE id=@id", new MySqlParameter("@read", message.read), new MySqlParameter("@deleted", message.deleted), new MySqlParameter("@item", itemName), new MySqlParameter("@id", message.id));
        MySqlHelper.ExecuteNonQuery(connectionString, @"UPDATE mail SET `read`=@read, deleted=@deleted, item=@item WHERE id=@id", new MySqlParameter("@read", message.read), new MySqlParameter("@deleted", message.deleted), new MySqlParameter("@item", itemName), new MySqlParameter("@id", message.id));

#elif _SQLITE
        connection.Execute(@"UPDATE mail SET read=?, deleted=?, item=? WHERE id=?", message.read, message.deleted, itemName, message.id);
#endif
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public MailMessage Mail_MessageById(long id)
    {
    	MailMessage message = new MailMessage();
#if _MYSQL
        //var table = ExecuteReaderMySql("SELECT * FROM mail WHERE id=@id", new MySqlParameter("@id", id));
        var table = ExecuteReaderMySql("SELECT * FROM mail WHERE id=@id", new MySqlParameter("@id", id));
#elif _SQLITE
        var table = connection.Query<mail>("SELECT * FROM mail WHERE id=?", id);
#endif
        if (table.Count == 1)
        {
            message = Mail_BuildMessageFromDBRow(table[0]);
        }
		return message;
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public List<MailMessage> Mail_CheckForNewMessages(long maxID)
    {
        List<MailMessage> result = new List<MailMessage>();
#if _MYSQL
        //var table = ExecuteReaderMySql("SELECT * FROM mail WHERE id > @maxid AND deleted=0 AND expires > @expires ORDER BY sent", new MySqlParameter("@maxid", maxID), new MySqlParameter("@expires", Epoch.Current()));
        var table = ExecuteReaderMySql("SELECT * FROM mail WHERE id > @maxid AND deleted=0 AND expires > @expires ORDER BY sent", new MySqlParameter("@maxid", maxID), new MySqlParameter("@expires", Epoch.Current()));
#elif _SQLITE
        var table = connection.Query<mail>("SELECT * FROM mail WHERE id > " + maxID + " AND deleted=0 AND expires > " + Epoch.Current() + " ORDER BY sent");
#endif
        foreach (var row in table)
        {
            MailMessage message = Mail_BuildMessageFromDBRow(row);
            result.Add(message);
        }
        return result;
    }

    public long Mail_FindMaxID()
    {
        long maxID = 0;
#if _MYSQL
        string query = "SELECT IFNULL (id, 0) FROM (SELECT MAX(id)  AS id FROM mail) AS id";
        //long maxID = Convert.ToInt32(Database.singleton.ExecuteScalarMySql("SELECT IFNULL (id, 0) FROM (SELECT MAX(id)  AS id FROM mail) AS id"));
        maxID = Convert.ToInt32(MySqlHelper.ExecuteScalar(connectionString, query));
#elif _SQLITE
        var results = Database.singleton.connection.FindWithQuery<Database.mail>("SELECT IFNULL(id, 0) FROM (SELECT MAX(id) AS id FROM mail)");
        maxID = results.id;
#endif
        return maxID;
    }

    // -----------------------------------------------------------------------------------
}
#endif