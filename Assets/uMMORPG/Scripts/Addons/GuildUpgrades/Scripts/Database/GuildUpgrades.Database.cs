#if _SERVER && _iMMOGUILDUPGRADES
using UnityEngine;
#if _MYSQL
using MySql.Data.MySqlClient;
using System;
#elif _SQLITE
using SQLite;
#endif

// DATABASE (SQLite / mySQL Hybrid)

public partial class Database
{

#if _SQLITE
	// -----------------------------------------------------------------------------------
    // Guild Upgrades
    // -----------------------------------------------------------------------------------
    class guild_upgrades
    {
        [PrimaryKey] // important for performance: O(log n) instead of O(n)
        public string guild { get; set; }
        public int level { get; set; }
        public int busy { get; set; }
    }
#endif
    private void Start_Tools_GuildUpgrades()
    {
        onConnected.AddListener(Connect_GuildUpgrades);
    }
    // -----------------------------------------------------------------------------------
    // Connect_GuildUpgrades
    // -----------------------------------------------------------------------------------
    private void Connect_GuildUpgrades()
    {
#if _MYSQL
		ExecuteNonQueryMySql(@"
            CREATE TABLE IF NOT EXISTS guild_upgrades(
			        guild VARCHAR(32) NOT NULL,
					level INTEGER(16) NOT NULL DEFAULT 0,
                    PRIMARY KEY(guild)
                  ) CHARACTER SET=utf8mb4");
#elif _SQLITE
        connection.CreateTable<guild_upgrades>();
#endif
    }

    // -----------------------------------------------------------------------------------
    // CharacterSave_GuildUpgrades
    // -----------------------------------------------------------------------------------
    private void CharacterSave_GuildUpgrades(Player player)
    {
#if _SERVER
       SaveGuildUpgrades(player);
#endif
    }

    // -----------------------------------------------------------------------------------
    // LoadGuildUpgrades
    // -----------------------------------------------------------------------------------
    public void LoadGuildUpgrades(Player player)
    {
        if (!player.guild.InGuild()) return;

#if _MYSQL

		var guildLevel = ExecuteScalarMySql("SELECT level FROM guild_upgrades WHERE guild=@guild", new MySqlParameter("@guild", player.guild.guild.name));

        // -- exists already? load to player
        if (guildLevel != null)
        {
            player.playerGuildUpgrades.guildLevel = Convert.ToInt32((int)guildLevel);
        }
        else
        {
        	// -- does not exist? create new
            ExecuteNonQueryMySql("INSERT INTO guild_upgrades (guild, level) VALUES(@guild, 0)", new MySqlParameter("@guild", player.guild.guild.name));
            player.playerGuildUpgrades.guildLevel = 0;
        }

#elif _SQLITE

        var results = connection.FindWithQuery<guild_upgrades>("SELECT level FROM guild_upgrades WHERE guild=?", player.guild.guild.name);

        // -- exists already? load to player
        if (results != null)
        {
            int guildLevel = results.level;
            player.playerGuildUpgrades.guildLevel = guildLevel;
            //connection.Execute("UPDATE guild_upgrades WHERE guild=?", player.guild.guild.name);
        }
        else
        {
            // -- does not exist? create new
            connection.Insert(new guild_upgrades
            {
                guild = player.guild.guild.name,
                level = 0
            });
            player.playerGuildUpgrades.guildLevel = 0;
        }

#endif

    }

    // -----------------------------------------------------------------------------------
    // SaveGuildUpgrades
    // -----------------------------------------------------------------------------------
    public void SaveGuildUpgrades(Player player)
    {
        if (!player.guild.InGuild()) return;

#if _MYSQL
		ExecuteNonQueryMySql("DELETE FROM guild_upgrades WHERE guild=@guild", new MySqlParameter("@guild", player.guild.guild.name));
		ExecuteNonQueryMySql("INSERT INTO guild_upgrades (guild, level) VALUES(@guild, @level)",
			new MySqlParameter("@level", 	player.playerGuildUpgrades.guildLevel),
			new MySqlParameter("@guild", 	player.guild.guild.name));

#elif _SQLITE
            connection.Execute("DELETE FROM guild_upgrades WHERE guild=?", player.guild.guild.name);
            connection.Insert(new guild_upgrades
            {
                guild = player.guild.guild.name,
                level = player.playerGuildUpgrades.guildLevel
            });
#endif
    }

    // -----------------------------------------------------------------------------------
}
#endif