#if _SERVER
using System.Collections.Generic;

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
    // Character Scene
    // -----------------------------------------------------------------------------------
    class npc_shop
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        public string shopname { get; set; }
        public string character { get; set; }
        public string item { get; set; }
    }

#endif
    private void Start_Tools_NpcShop()
    {
        onConnected.AddListener(Connect_Tools_NpcShop);
    }
    // -----------------------------------------------------------------------------------
    // Connect_Tools_NpcShop
    // -----------------------------------------------------------------------------------
    private void Connect_Tools_NpcShop()
    {
#if _MYSQL
        MySqlHelper.ExecuteNonQuery(connectionString, @"
        CREATE TABLE IF NOT EXISTS npc_shop (
            `shopname` VARCHAR(32) NOT NULL,
            `character` VARCHAR(32) NOT NULL,
            `item` VARCHAR(32) NOT NULL,
            KEY `shopname` (`shopname`)
            )");


#elif _SQLITE
        connection.CreateTable<npc_shop>();
#endif
    }

    // -----------------------------------------------------------------------------------
    // GetCharacterScene
    // -----------------------------------------------------------------------------------
    public List<SoldUniqueItems> GetShopItemsSale2(string npcShop)
    {
        List<SoldUniqueItems> items = new List<SoldUniqueItems>();
#if _MYSQL
        var itemSales = ExecuteReaderMySql("SELECT item, `character` FROM npc_shop WHERE shopname=@shopname", new MySqlParameter("@shopname", npcShop));
        foreach (var item in itemSales)
        {
            //items.ad
            items.Add(new SoldUniqueItems { itemName = (string)item[0], character = (string)item[1] });
        }
#elif _SQLITE
        var itemSales = connection.Query<npc_shop>("SELECT item, character FROM npc_shop WHERE shopname=?", npcShop);
        foreach (var item in itemSales)
        {
            //items.ad
            items.Add(new SoldUniqueItems { itemName = item.item, character = item.character});
        }
 
#endif
        return items;
    }
    // -----------------------------------------------------------------------------------
    // SaveCharacterScene
    // -----------------------------------------------------------------------------------
    public void AddSelledItem(string shopName, string characterName, string itemName)
    {
#if _MYSQL
		var query = @"INSERT INTO npc_shop SET `shopname`=@shopName, `character`=@character , item=@itemName";

        MySqlHelper.ExecuteNonQuery(connectionString, query,
                             new MySqlParameter("@shopName", shopName),
                             new MySqlParameter("@character", characterName),
                             new MySqlParameter("@itemName", itemName));
#elif _SQLITE
        connection.Insert(new npc_shop
        {
            shopname = shopName,
            character = characterName,
            item = itemName
        });
#endif
    }
}
#endif