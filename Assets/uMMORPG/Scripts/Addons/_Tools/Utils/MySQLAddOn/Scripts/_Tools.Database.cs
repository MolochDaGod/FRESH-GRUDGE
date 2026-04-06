#if _iMMOTOOLS
#if _SQLITE
using SQLite;
#endif
using UnityEngine;

// DATABASE CLASSES
public partial class Database
{
    public enum DatabaseType { SQLite, mySQL }
    // -----------------------------------------------------------------------------------
    // Tools_connection
    // @ workaround because uMMORPGs default database connection is private.
    // -----------------------------------------------------------------------------------
#if _SQLITE && _SERVER
    public SQLiteConnection Tools_connection {
        get {

            return connection;
        }
    }
#endif
    [Header("Database Type")]
    public DatabaseType databaseType = DatabaseType.SQLite;
#if _SERVER || UNITY_EDITOR
    [StringShowConditional(conditionFieldName: "databaseType", conditionValue: "mySQL")]
    public string dbHost = "localhost";
    [StringShowConditional(conditionFieldName: "databaseType", conditionValue: "mySQL")]
    public string dbName = "dbName";
    [StringShowConditional(conditionFieldName: "databaseType", conditionValue: "mySQL")]
    public string dbUser = "dbUser";
    [StringShowConditional(conditionFieldName: "databaseType", conditionValue: "mySQL")]
    public string dbPassword = "dbPassword";
    [StringShowConditional(conditionFieldName: "databaseType", conditionValue: "mySQL")]
    public uint dbPort = 3306;
    [StringShowConditional(conditionFieldName: "databaseType", conditionValue: "mySQL")]
    public string dbCharacterSet = "utf8mb4";
#endif
    protected const string DB_SQLITE = "_SQLITE";
    protected const string DB_MYSQL = "_MYSQL";

    // -----------------------------------------------------------------------------------
    // OnValidate
    // -----------------------------------------------------------------------------------
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (databaseType == Database.DatabaseType.SQLite)
        {
            DefineSymbols_EditorTools.RemoveScriptingDefine(DB_MYSQL);
            DefineSymbols_EditorTools.AddScriptingDefine(DB_SQLITE);
        }
        else if (databaseType == Database.DatabaseType.mySQL)
        {
            DefineSymbols_EditorTools.RemoveScriptingDefine(DB_SQLITE);
            DefineSymbols_EditorTools.AddScriptingDefine(DB_MYSQL);
        }
    }
#endif

#if _SERVER && _MYSQL
    // Called during Database.Start() in DatabaseMTools — do not declare Start() here to avoid
    // a duplicate method conflict with DatabaseMTools.cs on the same partial class.
    public void Start_Tools_MySQLAddOn()
    {
#if _DEBUGMODE
        Debug.Log("Search All addon require database and add in Events");
#endif
    }
#endif
    }
#endif