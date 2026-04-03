#if  _SERVER && _iMMOATTRIBUTES

using UnityEngine;
#if _MYSQL
using MySql.Data.MySqlClient;
#endif

// DATABASE (SQLite / mySQL Hybrid)

public partial class Database
{

#if _SQLITE
    // -----------------------------------------------------------------------------------
    // character_attributes
    // -----------------------------------------------------------------------------------
    class character_attributes
    {
        public string character { get; set; }
        public int slot { get; set; }
        public string name { get; set; }
        public int points { get; set; }
    }
#endif


    private void Start_Tools_Attributes()
    {
        onConnected.AddListener(Connect_Attributes);
        onCharacterLoad.AddListener(CharacterLoad_Attributes);
        onCharacterSave.AddListener(CharacterSave_Attributes);
    }

    // -----------------------------------------------------------------------------------
    // Connec_Attributes
    // -----------------------------------------------------------------------------------
    private void Connect_Attributes()
    {
#if _MYSQL && _iMMOATTRIBUTES
		ExecuteNonQueryMySql(@"CREATE TABLE IF NOT EXISTS character_attributes (
                        `character` VARCHAR(32) NOT NULL,
                        slot INTEGER NOT NULL,
                        name TEXT NOT NULL,
                        points INTEGER NOT NULL
                        ) CHARACTER SET=utf8mb4");
#elif _SQLITE && _iMMOATTRIBUTES
        connection.CreateTable<character_attributes>();
#endif
    }


    public void LoadAttributes(Player player)
    {
#if _MYSQL && _iMMOATTRIBUTES
        foreach (AttributeTemplate template in player.playerAttribute.playerAttributes.AttributeTypes)
        {
            if (template == null) continue;
            TrugordAttribute attr = new TrugordAttribute(template);
            var table = ExecuteReaderMySql("SELECT points FROM character_attributes WHERE `character`=@character AND name=@name", new MySqlParameter("@character", player.name), new MySqlParameter("@name", attr.name));
            if (table.Count == 1)
            {
                var row = table[0];
                attr.points = (int)row[0];
            }
            //Debug.Log("<>" + template.name + " Point :" + attr.points);
            player.playerAttribute.Attributes.Add(attr);
            int total = 0;
            if (player.playerAttribute.Attributes.Count > 0)
            {
                int points = 0;
                int flatBonus = 0;
                float pctBonus = 0f;

                foreach (TrugordAttribute attrib in player.playerAttribute.Attributes)
                {
                    points = player.playerAttribute.calculateBonusAttribute(attrib) + attrib.points;

                    flatBonus += attrib.flatHealth * points;
                    pctBonus += attrib.percentHealth * points;
                }

                total += flatBonus;
                total += (int)Mathf.Round(player.health.baseHealth.Get(player.level.current) * pctBonus);
            }
            player.health.current = total;
        }
#elif _SQLITE && _iMMOATTRIBUTES
        foreach (AttributeTemplate template in player.playerAttribute.playerAttributes.AttributeTypes)
        {
            if (template == null) continue;
            TrugordAttribute attr = new TrugordAttribute(template);
            var table = connection.Query<character_attributes>("SELECT points FROM character_attributes WHERE character=? AND name=?", player.name, attr.name);
            if (table.Count == 1)
            {
                var row = table[0];
                attr.points = row.points;
            }
            player.playerAttribute.Attributes.Add(attr);
            int total = 0;
            if (player.playerAttribute.Attributes.Count > 0)
            {
                int points = 0;
                int flatBonus = 0;
                float pctBonus = 0f;

                foreach (TrugordAttribute attrib in player.playerAttribute.Attributes)
                {
                    points = player.playerAttribute.calculateBonusAttribute(attrib) + attrib.points;

                    flatBonus += attrib.flatHealth * points;
                    pctBonus += attrib.percentHealth * points;
                }

                total += flatBonus;
                total += (int)Mathf.Round(player.health.baseHealth.Get(player.level.current) * pctBonus);
            }
            player.health.current = total;
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterLoad_Attributes
    // -----------------------------------------------------------------------------------
    private void CharacterLoad_Attributes(Player player)
    {
#if _MYSQL && _iMMOATTRIBUTES
		foreach (AttributeTemplate template in player.playerAttribute.playerAttributes.AttributeTypes) {
            if (template == null) continue;
            TrugordAttribute attr = new TrugordAttribute(template);
            var table = ExecuteReaderMySql("SELECT points FROM character_attributes WHERE `character`=@character AND name=@name", new MySqlParameter("@character", player.name), new MySqlParameter("@name", attr.name));
            if (table.Count == 1) {
                var row = table[0];
                attr.points = (int)row[0];
            }
            player.playerAttribute.Attributes.Add(attr);
            int total = 0;
            if (player.playerAttribute.Attributes.Count > 0)
            {
                int points = 0;
                int flatBonus = 0;
                float pctBonus = 0f;

                foreach (TrugordAttribute attrib in player.playerAttribute.Attributes)
                {
                    points = player.playerAttribute.calculateBonusAttribute(attrib) + attrib.points;

                    flatBonus += attrib.flatHealth * points;
                    pctBonus += attrib.percentHealth * points;
                }

                total += flatBonus;
                total += (int)Mathf.Round(player.health.baseHealth.Get(player.level.current) * pctBonus);
            }
           
        }
#elif _SQLITE && _iMMOATTRIBUTES
        foreach (AttributeTemplate template in player.playerAttribute.playerAttributes.AttributeTypes)
        {
            if (template == null) continue;
            TrugordAttribute attr = new TrugordAttribute(template);
            var table = connection.Query<character_attributes>("SELECT points FROM character_attributes WHERE character=? AND name=?", player.name, attr.name);
            if (table.Count == 1)
            {
                var row = table[0];
                attr.points = row.points;
            }
            player.playerAttribute.Attributes.Add(attr);
            int total = 0;
            if (player.playerAttribute.Attributes.Count > 0)
            {
                int points = 0;
                int flatBonus = 0;
                float pctBonus = 0f;

                foreach (TrugordAttribute attrib in player.playerAttribute.Attributes)
                {
                    points = player.playerAttribute.calculateBonusAttribute(attrib) + attrib.points;

                    flatBonus += attrib.flatHealth * points;
                    pctBonus += attrib.percentHealth * points;
                }

                total += flatBonus;
                total += (int)Mathf.Round(player.health.baseHealth.Get(player.level.current) * pctBonus);
            }
           
        }

#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterSave_Attributes
    // -----------------------------------------------------------------------------------
    private void CharacterSave_Attributes(Player player)
    {
#if _MYSQL && _iMMOATTRIBUTES
		ExecuteNonQueryMySql("DELETE FROM character_attributes WHERE `character`=@character", new MySqlParameter("@character", player.name));
        for (int i = 0; i < player.playerAttribute.Attributes.Count; ++i) {
            var attr = player.playerAttribute.Attributes[i];
            ExecuteNonQueryMySql("INSERT INTO character_attributes VALUES (@character, @slot, @name, @points)",
                            new MySqlParameter("@character", player.name),
                            new MySqlParameter("@slot", i),
                            new MySqlParameter("@name", attr.name),
                            new MySqlParameter("@points", attr.points));
        }
#elif _SQLITE && _iMMOATTRIBUTES
        connection.Execute("DELETE FROM character_attributes WHERE character=?", player.name);
        for (int i = 0; i < player.playerAttribute.Attributes.Count; ++i)
        {
            var attr = player.playerAttribute.Attributes[i];
            connection.InsertOrReplace(new character_attributes
            {
                character = player.name,
                slot = i,
                name = attr.name,
                points = attr.points
            });
        }
#endif
    }

    // -----------------------------------------------------------------------------------
}
#endif