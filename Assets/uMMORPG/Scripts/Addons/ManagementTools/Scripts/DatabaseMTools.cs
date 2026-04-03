// SQLite-only management database helpers.
using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if _SQLITE

using SQLite;
//using UnityEngine.Profiling;

public partial class Database
{
#if UMMORPG_3D_R || UMMORPG_2D_R
    void Start()
    {
#if ITEM_DROP_R
        StartItemDrop();
#endif
        StartMTools();
#if _SERVER
#if _SQLITE
        Utils.InvokeMany(typeof(Database), this, "Start_Tools_");
#endif
#endif
    }

    void StartMTools()
    {
        onConnected?.AddListener(Initialize_EffectsTable);
        onConnected?.AddListener(Initialize_SpawnersTable);
        onConnected?.AddListener(Initialize_SpawnersGroupsTable);
        onConnected?.AddListener(Initialize_NotesTable);
        onConnected?.AddListener(Initialize_CheckpointsTable);
        onConnected?.AddListener(Initialize_BugReportsTable);
        onConnected?.AddListener(Initialize_HelpRequestsTable);
        onConnected?.AddListener(Initialize_MonstersTable);
        onConnected?.AddListener(Initialize_NpcsTable);
        onConnected?.AddListener(Initialize_EnvironmentsTable);

        onCharacterLoad?.AddListener(CharacterLoad_Effects);
        onCharacterSave?.AddListener(CharacterSave_Effects);
    }
#endif

    #region Accounts Table
    // counting all accounts
    public string CountAccounts()
    {
        return connection.ExecuteScalar<string>("SELECT count(*) FROM accounts");
    }

    // counting staff accounts
    public string CountStaffAccounts()
    {
        return connection.ExecuteScalar<string>("SELECT count(*) FROM accounts WHERE AccessLevel>=2");
    }

    // counting accounts by type
    public string CountAccountsByType(int access)
    {
        return connection.ExecuteScalar<string>("SELECT count(*) FROM accounts WHERE AccessLevel=?", access);
    }

    // returns the list of account names except for access level 3
    public List<string> GetAccountsToDelete()
    {
        List<string> result = new List<string>();
        foreach (accounts account in connection.Query<accounts>("SELECT * FROM accounts WHERE AccessLevel=3"))
            result.Add(account.name);
        return result;
    }

    // returns the list of banned account names
    public List<string> GetBannedAccounts()
    {
        List<string> result = new List<string>();
        foreach (accounts account in connection.Query<accounts>("SELECT * FROM accounts WHERE AccessLevel=0"))
            result.Add(account.name);
        return result;
    }

    // returns the list of character names in the account
    public List<string> GetCharactersOnAccount(string account)
    {
        List<string> result = new List<string>();
        foreach (characters character in connection.Query<characters>("SELECT * FROM characters WHERE account=?", account))
            result.Add(character.name);
        return result;
    }

    // deleting all accounts with characters and other related tables from the database except for access level 3
    public void DeleteAccounts()
    {
        List<string> accountsToDelete = GetAccountsToDelete();
        //Debug.Log($"account: {string.Join(", ", accountsToDelete)}");

        foreach (string accountName in accountsToDelete)
        {
            DeleteCharacters(accountName);
        }

        connection.BeginTransaction();
        // delete all accounts from the list
        connection.Execute("DELETE FROM accounts WHERE NOT AccessLevel=3");
        connection.Commit();
    }

    public void DeleteCharacters(string accountName)
    {
        List<string> charactersToDelete = GetCharactersOnAccount(accountName);
        //Debug.Log($"characters: {string.Join(", ", charactersToDelete)}");

        foreach (string characterName in charactersToDelete)
        {
            DeleteCharacterTables(characterName);
        }
    }

    // deleting character and other related tables from the database
    public void DeleteCharacterTables(string characterName)
    {
        connection.BeginTransaction();
        // delete character
        connection.Execute("DELETE FROM characters WHERE name=?", characterName);

        // and other related tables
        connection.Execute("DELETE FROM character_inventory WHERE character=?", characterName);
        connection.Execute("DELETE FROM character_equipment WHERE character=?", characterName);
        connection.Execute("DELETE FROM character_skills WHERE character=?", characterName);
        connection.Execute("DELETE FROM character_buffs WHERE character=?", characterName);
        connection.Execute("DELETE FROM character_quests WHERE character=?", characterName);
        connection.Execute("DELETE FROM character_orders WHERE character=?", characterName);
        connection.Execute("DELETE FROM character_quests WHERE character=?", characterName);
        connection.Execute("DELETE FROM character_guild WHERE character=?", characterName);
        connection.Execute("DELETE FROM character_effects WHERE character=?", characterName);
        connection.Execute("DELETE FROM character_notes WHERE character=?", characterName);
        connection.Commit();
    }

    // returns the level of access assigned to the account
    public int AccessLevel(string account)
    {
        return connection.ExecuteScalar<int>("SELECT abs(AccessLevel) FROM accounts WHERE name=?", account);
    }

    // checks whether the account has special access or not
    public bool CommandAccess(string account)
    {
        return connection.ExecuteScalar<int>("SELECT Count(*) FROM accounts WHERE name=? AND AccessLevel>=2", account) == 1;
    }

    // changing the level of access
    public void ChangeAccess(string account, int access)
    {
        connection.BeginTransaction();
        connection.Execute("UPDATE accounts SET AccessLevel=? WHERE name=?", access, account);
        connection.Commit();
    }

    // changing the password
    public void ChangePassword(string account, string password)
    {
        connection.BeginTransaction();
        string hash = Utils.PBKDF2Hash(password, UIPasswordChange.singleton.authenticator.passwordSalt + account);
        connection.Execute("UPDATE accounts SET password='" + hash + "' WHERE name=?", account);
        connection.Commit();
    }
    #endregion

    #region Characters Table
    // counting all players saved in the database
    public string CountCharacters()
    {
        return connection.ExecuteScalar<string>("SELECT count(*) FROM characters");
    }

    // returns the name of the character with the largest amount of gold
    public string MaxGoldCharacter()
    {
        return connection.ExecuteScalar<string>("SELECT name, max(gold) FROM characters");
    }
    #endregion

    #region Effects Table 
    // character_effects table for simplifying the installation process, instead of changing the main table

    class Character_Effects
    {
        [PrimaryKey] public string Character { get; set; }
        public bool Immortal { get; set; }
        public bool Invisible { get; set; }
        public bool Frozen { get; set; }
    }

    void Initialize_EffectsTable()
    {
        connection.CreateTable<Character_Effects>();
    }

    void CharacterLoad_Effects(Player player)
    {
        Character_Effects row = connection.FindWithQuery<Character_Effects>("SELECT Immortal, Invisible, Frozen FROM Character_Effects WHERE character=?", player.name);
        if (row != null)
        {
#if UMMORPG_3D_C || UMMORPG_2D_C
            player.invincible = row.Immortal;
#endif
#if UMMORPG_3D_R || UMMORPG_2D_R
            player.combat.invincible = row.Immortal;
#endif
            player.invisible = row.Invisible;
            player.frozen = row.Frozen;

            if (player.invisible)
            {
                player.Hide();
            }

            // set the access level according to the data from the database
            int access = AccessLevel(player.account);
            player.Access = access;

            //Debug.Log($"Command access: {CommandAccess(player.account)}");
        }
    }

    void CharacterSave_Effects(Player player)
    {
        connection.InsertOrReplace(new Character_Effects
        {
            Character = player.name,
#if UMMORPG_3D_C || UMMORPG_2D_C
            Immortal = player.invincible,
#endif
#if UMMORPG_3D_R || UMMORPG_2D_R
            Immortal = player.combat.invincible,
#endif
            Invisible = player.invisible,
            Frozen = player.frozen
        });
    }
    #endregion

    #region Spawners Table
    class Spawners
    {
        public string Name { get; set; }
        [PrimaryKey] public string ID { get; set; }
        public int Area { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
#if UMMORPG_3D_C || UMMORPG_3D_R
        public float Z { get; set; }
#endif
    }

    void Initialize_SpawnersTable()
    {
        connection.CreateTable<Spawners>();
    }

    // counting all spawners saved in the database
    public string CountSpawners()
    {
        return connection.ExecuteScalar<string>("SELECT count(*) FROM Spawners");
    }

    // deleting a spawner with its entire content from the database by unique ID
    public void SpawnerDelete(string uniqueId)
    {
        connection.BeginTransaction();
        connection.Execute("DELETE FROM Spawners WHERE ID=?", uniqueId);
        connection.Execute("DELETE FROM Spawners_Groups WHERE ID=?", uniqueId);
        connection.Commit();
    }

    // deleting all spawners with its entire content from the database
    public void SpawnersDelete()
    {
        connection.BeginTransaction();
        connection.Execute("DELETE FROM Spawners");
        connection.Execute("DELETE FROM Spawners_Groups");
        connection.Commit();
    }

    public Spawner[] LoadSpawners()
    {
        //Profiler.BeginSample("Management LoadSpawners");

        // create a list of row-based structs in the table
        List<Spawner> spawners = new List<Spawner>();
        foreach (Spawners row in connection.Query<Spawners>("SELECT * FROM Spawners"))
        {
            Spawner go = new Spawner
            {
                spawnName = row.Name
            };

            string uniqueId = row.ID;
            go.uniqueId = uniqueId;
            go.area = row.Area;
#if UMMORPG_3D_C || UMMORPG_3D_R
            Vector3 position = new Vector3(row.X, row.Y, row.Z);
#else
            Vector2 position = new Vector2(row.X, row.Y);
#endif
            go.position = position;

            // assign groups
            SpawnGroup[] spawnGroups = LoadSpawnerGroups(uniqueId);
            go.groups = spawnGroups;

            spawners.Add(go);

            // check if the spawner has not been activated yet
            if (!AddonManagement.spawners.TryGetValue(uniqueId, out var activeSpawner))
            {
                // create spawner on the server and add it to the dictionary
                SpawnerActivate(go);
            }
        }
        //Profiler.EndSample();
        return spawners.ToArray();
    }

    public void SpawnerActivate(Spawner spawnerData)
    {
        //Profiler.BeginSample("Management SpawnerActivate");

        // create a list and assign all groups based on data loaded from the database
        List<M_Spawner.SpawnGroup> spawnGroups = new List<M_Spawner.SpawnGroup>();
        for (int i = 0; i < spawnerData.groups.Length; ++i)
        {
            SpawnGroup group = spawnerData.groups[i];
            string monsterName = group.monsterName;
            int monsterAmount = group.amount;

            Monster prefab = AddonManagement.Monsters.Find(p => p.name == monsterName);
            if (prefab != null)
            {
                M_Spawner.SpawnGroup go = new M_Spawner.SpawnGroup
                {
                    monster = prefab,
                    amount = monsterAmount
                };
                spawnGroups.Add(go);
            }
        }

        // instantiate a spawner
        M_Spawner spawner = Instantiate(AddonManagement.SpawnerPrefab, spawnerData.position, Quaternion.identity);
        spawner.uniqueId = spawnerData.uniqueId;
        spawner.range = spawnerData.area;
        spawner.spawnGroups = spawnGroups.ToArray();

        // create this spawner on the server
        NetworkServer.Spawn(spawner.gameObject);

        // add it to the dictionary
        AddonManagement.spawners.Add(spawnerData.uniqueId, spawner);

        //Profiler.EndSample();
    }

    public void SaveSpawner(Spawner spawnerData)
    {
        connection.BeginTransaction();
#if UMMORPG_3D_C || UMMORPG_3D_R
        connection.InsertOrReplace(new Spawners
        {
            Name = spawnerData.spawnName,
            ID = spawnerData.uniqueId,
            Area = spawnerData.area,
            X = spawnerData.position.x,
            Y = spawnerData.position.y,
            Z = spawnerData.position.z
        });
#else
        connection.InsertOrReplace(new Spawners
        {
            Name = spawnerData.spawnName,
            ID = spawnerData.uniqueId,
            Area = spawnerData.area,
            X = spawnerData.position.x,
            Y = spawnerData.position.y
        });
#endif
        SaveSpawnGroup(spawnerData.groups, spawnerData.uniqueId);
        connection.Commit();
    }

    class Spawners_Groups
    {
        public string ID { get; set; }
        public int Slot { get; set; }
        public string Monster { get; set; }
        public int Amount { get; set; }
    }

    void Initialize_SpawnersGroupsTable()
    {
        connection.CreateTable<Spawners_Groups>();
        connection.CreateIndex(nameof(Spawners_Groups), new[] { "ID", "Slot" });
    }

    public SpawnGroup[] LoadSpawnerGroups(string uniqueId)
    {
        List<SpawnGroup> spawnGroups = new List<SpawnGroup>();
        foreach (Spawners_Groups row in connection.Query<Spawners_Groups>("SELECT Monster, Amount FROM Spawners_Groups WHERE ID=?", uniqueId))
        {
            SpawnGroup group = new SpawnGroup
            {
                monsterName = row.Monster,
                amount = row.Amount
            };
            spawnGroups.Add(group);
        }
        return spawnGroups.ToArray();
    }

    void SaveSpawnGroup(SpawnGroup[] spawnerGroups, string uniqueId)
    {
        connection.Execute("DELETE FROM Spawners_Groups WHERE ID=?", uniqueId);
        foreach (SpawnGroup group in spawnerGroups)
        {
            connection.Insert(new Spawners_Groups
            {
                ID = uniqueId,
                Slot = group.slot,
                Monster = group.monsterName,
                Amount = group.amount
            });
        }
    }
    #endregion

    #region Notes Table
    class Character_Notes
    {
        [PrimaryKey] public string Character { get; set; }
        public string Note { get; set; }
    }

    void Initialize_NotesTable()
    {
        connection.CreateTable<Character_Notes>();
    }

    public string LoadNote(string characterName)
    {
        connection.Execute("INSERT OR IGNORE INTO Character_Notes VALUES (@Character, @Note)", characterName, "");

        return connection.ExecuteScalar<string>("SELECT Note FROM Character_Notes WHERE Character=?", characterName);
    }

    public void ChangeNote(string characterName, string note)
    {
        connection.BeginTransaction();
        connection.Execute("UPDATE Character_Notes SET Note=? WHERE Character=?", note, characterName);
        connection.Commit();
    }
    #endregion

    #region Checkpoints Table
    class Checkpoints
    {
        public string Name { get; set; }
        [PrimaryKey] public string ID { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
#if UMMORPG_3D_C || UMMORPG_3D_R
        public float Z { get; set; }
#endif
    }

    void Initialize_CheckpointsTable()
    {
        connection.CreateTable<Checkpoints>();
    }

    // counting all checkpoints saved in the database
    public string CountCheckpoints()
    {
        return connection.ExecuteScalar<string>("SELECT count(*) FROM Checkpoints");
    }

    // deleting a checkpoint from the database by unique ID
    public void CheckpointDelete(string uniqueId)
    {
        connection.BeginTransaction();
        connection.Execute("DELETE FROM checkpoints WHERE ID=?", uniqueId);
        connection.Commit();
    }

    // deleting all checkpoints from the database
    public void CheckpointsDelete()
    {
        connection.BeginTransaction();
        connection.Execute("DELETE FROM checkpoints");
        connection.Commit();
    }

    public Checkpoint[] LoadCheckpoints()
    {
        List<Checkpoint> checkpoints = new List<Checkpoint>();
        foreach (Checkpoints row in connection.Query<Checkpoints>("SELECT * FROM Checkpoints"))
        {
            Checkpoint go = new Checkpoint
            {
                checkpointName = row.Name,
                uniqueId = row.ID
            };
#if UMMORPG_3D_C || UMMORPG_3D_R
            Vector3 position = new Vector3(row.X, row.Y, row.Z);
#else
            Vector2 position = new Vector3(row.X, row.Y);
#endif
            go.position = position;

            checkpoints.Add(go);
        }
        return checkpoints.ToArray();
    }

    public void SaveCheckpoint(Checkpoint checkpoint)
    {
        connection.BeginTransaction();
#if UMMORPG_3D_C || UMMORPG_3D_R
        connection.InsertOrReplace(new Checkpoints
        {
            Name = checkpoint.checkpointName,
            ID = checkpoint.uniqueId,
            X = checkpoint.position.x,
            Y = checkpoint.position.y,
            Z = checkpoint.position.z
        });
#else
        connection.InsertOrReplace(new Checkpoints
        {
            Name = checkpoint.checkpointName,
            ID = checkpoint.uniqueId,
            X = checkpoint.position.x,
            Y = checkpoint.position.y
        });
#endif
        connection.Commit();
    }
    #endregion

    #region Bug Reports Table
    class Bug_Reports
    {
        public string Character { get; set; }
        [PrimaryKey] public string ID { get; set; }
        public int Status { get; set; }
        public string Date { get; set; }
        public string Description { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
#if UMMORPG_3D_C || UMMORPG_3D_R
        public float Z { get; set; }
#endif
    }

    void Initialize_BugReportsTable()
    {
        connection.CreateTable<Bug_Reports>();
    }

    // counting all bug reports saved in the database by status
    public string CountBugReportsByStatus(int status)
    {
        return connection.ExecuteScalar<string>("SELECT count(*) FROM Bug_Reports WHERE Status=?", status);
    }

    // deleting a bug report from the database by unique ID
    public void BugReportDelete(string uniqueId)
    {
        connection.BeginTransaction();
        connection.Execute("DELETE FROM Bug_Reports WHERE ID=?", uniqueId);
        connection.Commit();
    }

    // deleting all bug reports from the database by status
    public void BugReportsDelete(int status)
    {
        connection.BeginTransaction();
        connection.Execute("DELETE FROM Bug_Reports WHERE Status=?", status);
        connection.Commit();
    }

    public BugReport[] LoadBugReports(int status)
    {
        //Profiler.BeginSample("Management LoadBugReports");
        List<BugReport> bugReports = new List<BugReport>();
        foreach (Bug_Reports row in connection.Query<Bug_Reports>("SELECT * FROM Bug_Reports WHERE Status=?", status))
        {
            BugReport go = new BugReport
            {
                characterName = row.Character,
                uniqueId = row.ID,
                status = row.Status,
                date = row.Date,
                description = row.Description
            };
#if UMMORPG_3D_C || UMMORPG_3D_R
            Vector3 position = new Vector3(row.X, row.Y, row.Z);
#else
            Vector2 position = new Vector2(row.X, row.Y);
#endif
            go.position = position;

            bugReports.Add(go);
        }
        //Profiler.EndSample();
        return bugReports.ToArray();
    }

    public void SaveBugReport(BugReport bugReport)
    {
        connection.BeginTransaction();
#if UMMORPG_3D_C || UMMORPG_3D_R
        connection.InsertOrReplace(new Bug_Reports
        {
            Character = bugReport.characterName,
            ID = bugReport.uniqueId,
            Status = bugReport.status,
            Date = bugReport.date,
            Description = bugReport.description,
            X = bugReport.position.x,
            Y = bugReport.position.y,
            Z = bugReport.position.z
        });
#else
        connection.InsertOrReplace(new Bug_Reports
        {
            Character = bugReport.characterName,
            ID = bugReport.uniqueId,
            Status = bugReport.status,
            Date = bugReport.date,
            Description = bugReport.description,
            X = bugReport.position.x,
            Y = bugReport.position.y
        });
#endif
        connection.Commit();
    }

    public void EditBugReportStatus(string uniqueId, int status)
    {
        connection.BeginTransaction();
        connection.Execute("UPDATE Bug_Reports SET Status=? WHERE ID=?", status, uniqueId);
        connection.Commit();
    }

    public void EditBugReportDescription(string uniqueId, string description)
    {
        connection.BeginTransaction();
        connection.Execute("UPDATE Bug_Reports SET Description=? WHERE ID=?", description, uniqueId);
        connection.Commit();
    }
    #endregion

    #region Help Requests Table
    class Help_Requests
    {
        public string Character { get; set; }
        [PrimaryKey] public string ID { get; set; }
        public string Date { get; set; }
        public string Request { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
#if UMMORPG_3D_C || UMMORPG_3D_R
        public float Z { get; set; }
#endif
    }

    void Initialize_HelpRequestsTable()
    {
        connection.CreateTable<Help_Requests>();
    }

    // counting all help requests saved in the database
    public string CountHelpRequests()
    {
        return connection.ExecuteScalar<string>("SELECT count(*) FROM Help_Requests");
    }

    // deleting a help request from the database by unique ID
    public void HelpRequestDelete(string uniqueId)
    {
        connection.BeginTransaction();
        connection.Execute("DELETE FROM Help_Requests WHERE ID=?", uniqueId);
        connection.Commit();
    }

    // deleting all help requests from the database
    public void HelpRequestsDelete()
    {
        connection.BeginTransaction();
        connection.Execute("DELETE FROM Help_Requests");
        connection.Commit();
    }

    public HelpRequest[] LoadHelpRequests()
    {
        //Profiler.BeginSample("Management LoadHelpRequests");
        List<HelpRequest> requests = new List<HelpRequest>();
        foreach (Help_Requests row in connection.Query<Help_Requests>("SELECT * FROM Help_Requests"))
        {
            HelpRequest go = new HelpRequest
            {
                characterName = row.Character,
                uniqueId = row.ID,
                date = row.Date,
                description = row.Request
            };
#if UMMORPG_3D_C || UMMORPG_3D_R
            Vector3 position = new Vector3(row.X, row.Y, row.Z);
#else
            Vector2 position = new Vector2(row.X, row.Y);
#endif
            go.position = position;

            requests.Add(go);
        }
        //Profiler.EndSample();
        return requests.ToArray();
    }

    public void SaveHelpRequest(HelpRequest helpRequest)
    {
        connection.BeginTransaction();
#if UMMORPG_3D_C || UMMORPG_3D_R
        connection.InsertOrReplace(new Help_Requests
        {
            Character = helpRequest.characterName,
            ID = helpRequest.uniqueId,
            Date = helpRequest.date,
            Request = helpRequest.description,
            X = helpRequest.position.x,
            Y = helpRequest.position.y,
            Z = helpRequest.position.z
        });
#else
        connection.InsertOrReplace(new Help_Requests
        {
            Character = helpRequest.characterName,
            ID = helpRequest.uniqueId,
            Date = helpRequest.date,
            Request = helpRequest.description,
            X = helpRequest.position.x,
            Y = helpRequest.position.y
        });
#endif
        connection.Commit();
    }
    #endregion

    #region Monsters Table
    class Monsters
    {
        public string Name { get; set; }
        [PrimaryKey] public string ID { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
#if UMMORPG_3D_C || UMMORPG_3D_R
        public float Z { get; set; }
        public float YAngle { get; set; }
#endif
    }

    void Initialize_MonstersTable()
    {
        connection.CreateTable<Monsters>();
    }

    // counting all monsters saved in the database
    public string CountMonsters()
    {
        return connection.ExecuteScalar<string>("SELECT count(*) FROM Monsters");
    }

    // deleting a monster from the database by unique ID
    public void MonsterDelete(string uniqueId)
    {
        connection.BeginTransaction();
        connection.Execute("DELETE FROM Monsters WHERE ID=?", uniqueId);
        connection.Commit();
    }

    // deleting all monsters from the database
    public void MonstersDelete()
    {
        connection.BeginTransaction();
        connection.Execute("DELETE FROM Monsters");
        connection.Commit();
    }

    public GameObject MonsterLoad(string monsterName, List<Monster> prefabs)
    {
        foreach (Monsters row in connection.Query<Monsters>("SELECT * FROM Monsters WHERE Name=?", monsterName))
        {
            Monster prefab = prefabs.Find(p => p.name == monsterName);
            if (prefab != null)
            {
                GameObject go = Instantiate(prefab.gameObject);
                Monster monster = go.GetComponent<Monster>();

                monster.name = row.Name;
                monster.uniqueId = row.ID;
#if UMMORPG_3D_C
                Vector3 position = new Vector3(row.X, row.Y, row.Z);
                Vector3 rotation = new Vector3(0, row.YAngle, 0);

                monster.Warp(position);
                monster.transform.eulerAngles = rotation;
#endif
#if UMMORPG_2D_C
                Vector2 position = new Vector2(row.X, row.Y);
                monster.agent.Warp(position);
#endif
#if UMMORPG_3D_R
                Vector3 position = new Vector3(row.X, row.Y, row.Z);
                Vector3 rotation = new Vector3(0, row.YAngle, 0);

                monster.movement.Warp(position);
                monster.transform.eulerAngles = rotation;
#endif
#if UMMORPG_2D_R
                Vector2 position = new Vector2(row.X, row.Y);
                monster.movement.Warp(position);
#endif
                NetworkServer.Spawn(go);
            }
            else Debug.LogWarning("no prefab found for monster: " + monsterName);
        }
        return null;
    }

    public void MonsterSave(Monster monster, bool useTransaction = true)
    {
        // only use a transaction if not called within SaveMany transaction
        if (useTransaction) connection.BeginTransaction();
        if (monster.uniqueId != "")
        {
#if UMMORPG_3D_C || UMMORPG_3D_R
            connection.InsertOrReplace(new Monsters
            {
                Name = monster.name,
                ID = monster.uniqueId,
                X = monster.transform.position.x,
                Y = monster.transform.position.y,
                Z = monster.transform.position.z,
                YAngle = monster.transform.eulerAngles.y
            });
#else
            connection.InsertOrReplace(new Monsters
            {
                Name = monster.name,
                ID = monster.uniqueId,
                X = monster.transform.position.x,
                Y = monster.transform.position.y
            });
#endif
        }
        if (useTransaction) connection.Commit();
    }

    // save multiple monsters at once (useful for ultra fast transactions)
    public void MonsterSaveMany(List<Monster> monsters)
    {
        connection.BeginTransaction();
        foreach (Monster monster in monsters) MonsterSave(monster, false);
        connection.Commit();
    }
    #endregion

    #region Npcs Table
    class Npcs
    {
        public string Name { get; set; }
        [PrimaryKey] public string ID { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
#if UMMORPG_3D_C || UMMORPG_3D_R
        public float Z { get; set; }
        public float YAngle { get; set; }
#endif
    }

    void Initialize_NpcsTable()
    {
        connection.CreateTable<Npcs>();
    }

    // counting all npcs saved in the database
    public string CountNpcs()
    {
        return connection.ExecuteScalar<string>("SELECT count(*) FROM Npcs");
    }

    // deleting a npc from the database by unique ID
    public void NpcDelete(string uniqueId)
    {
        connection.BeginTransaction();
        connection.Execute("DELETE FROM Npcs WHERE ID=?", uniqueId);
        connection.Commit();
    }

    // deleting all npcs from the database
    public void NpcsDelete()
    {
        connection.BeginTransaction();
        connection.Execute("DELETE FROM Npcs");
        connection.Commit();
    }

    public GameObject NpcLoad(string npcName, List<Npc> prefabs)
    {
        foreach (Npcs row in connection.Query<Npcs>("SELECT * FROM Npcs WHERE Name=?", npcName))
        {
            Npc prefab = prefabs.Find(p => p.name == npcName);
            if (prefab != null)
            {
                GameObject go = Instantiate(prefab.gameObject);
                Npc npc = go.GetComponent<Npc>();

                npc.name = row.Name;
                npc.uniqueId = row.ID;
#if UMMORPG_3D_C
                Vector3 position = new Vector3(row.X, row.Y, row.Z);
                Vector3 rotation = new Vector3(0, row.YAngle, 0);

                npc.Warp(position);
                npc.transform.eulerAngles = rotation;
#endif
#if UMMORPG_2D_C
                Vector2 position = new Vector2(row.X, row.Y);
                npc.agent.Warp(position);
#endif
#if UMMORPG_3D_R
                Vector3 position = new Vector3(row.X, row.Y, row.Z);
                Vector3 rotation = new Vector3(0, row.YAngle, 0);

                npc.movement.Warp(position);
                npc.transform.eulerAngles = rotation;
#endif
#if UMMORPG_2D_R
                Vector2 position = new Vector2(row.X, row.Y);
                npc.movement.Warp(position);
#endif
                NetworkServer.Spawn(go);
            }
            else Debug.LogWarning("no prefab found for npc: " + npcName);
        }
        return null;
    }

    public void NpcSave(Npc npc, bool useTransaction = true)
    {
        // only use a transaction if not called within SaveMany transaction
        if (useTransaction) connection.BeginTransaction();
        if (npc.uniqueId != "")
        {
#if UMMORPG_3D_C || UMMORPG_3D_R
            connection.InsertOrReplace(new Npcs
            {
                Name = npc.name,
                ID = npc.uniqueId,
                X = npc.transform.position.x,
                Y = npc.transform.position.y,
                Z = npc.transform.position.z,
                YAngle = npc.transform.eulerAngles.y
            });
#else
            connection.InsertOrReplace(new Npcs
            {
                Name = npc.name,
                ID = npc.uniqueId,
                X = npc.transform.position.x,
                Y = npc.transform.position.y
            });
#endif
        }
        if (useTransaction) connection.Commit();
    }

    // save multiple npcs at once (useful for ultra fast transactions)
    public void NpcSaveMany(List<Npc> npcs)
    {
        connection.BeginTransaction();
        foreach (Npc npc in npcs) NpcSave(npc, false);
        connection.Commit();
    }
    #endregion

    #region Environments Table
    class Environments
    {
        public string Name { get; set; }
        [PrimaryKey] public string ID { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
#if UMMORPG_3D_C || UMMORPG_3D_R
        public float Z { get; set; }
        public float XAngle { get; set; }
        public float YAngle { get; set; }
        public float ZAngle { get; set; }
#endif
    }

    void Initialize_EnvironmentsTable()
    {
        connection.CreateTable<Environments>();
    }

    // counting all environments saved in the database
    public string CountEnvironments()
    {
        return connection.ExecuteScalar<string>("SELECT count(*) FROM Environments");
    }

    // deleting an environment from the database by unique ID
    public void EnvironmentDelete(string uniqueId)
    {
        connection.BeginTransaction();
        connection.Execute("DELETE FROM Environments WHERE ID=?", uniqueId);
        connection.Commit();
    }

    // deleting all environments from the database
    public void EnvironmentsDelete()
    {
        connection.BeginTransaction();
        connection.Execute("DELETE FROM Environments");
        connection.Commit();
    }

    public GameObject EnvironmentLoad(string environmentName, List<M_Environment> prefabs)
    {
        foreach (Environments row in connection.Query<Environments>("SELECT * FROM Environments WHERE Name=?", environmentName))
        {
            M_Environment prefab = prefabs.Find(p => p.name == environmentName);
            if (prefab != null)
            {
                GameObject go = Instantiate(prefab.gameObject);
                M_Environment environment = go.GetComponent<M_Environment>();

                environment.name = row.Name;
                environment.uniqueId = row.ID;
#if UMMORPG_3D_C || UMMORPG_3D_R
                Vector3 position = new Vector3(row.X, row.Y, row.Z);
                Vector3 rotation = new Vector3(row.XAngle, row.YAngle, row.ZAngle);

                environment.transform.position = position;
                environment.transform.eulerAngles = rotation;
#else
                Vector2 position = new Vector2(row.X, row.Y);
                environment.transform.position = position;
#endif
                NetworkServer.Spawn(go);
            }
            else Debug.LogWarning("no prefab found for environment: " + environmentName);
        }
        return null;
    }

    public void EnvironmentSave(M_Environment environment, bool useTransaction = true)
    {
        // only use a transaction if not called within SaveMany transaction
        if (useTransaction) connection.BeginTransaction();
        if (environment.uniqueId != "")
        {
#if UMMORPG_3D_C || UMMORPG_3D_R
            connection.InsertOrReplace(new Environments
            {
                Name = environment.name,
                ID = environment.uniqueId,
                X = environment.transform.position.x,
                Y = environment.transform.position.y,
                Z = environment.transform.position.z,
                XAngle = environment.transform.eulerAngles.x,
                YAngle = environment.transform.eulerAngles.y,
                ZAngle = environment.transform.eulerAngles.z
            });
#else
            connection.InsertOrReplace(new Environments
            {
                Name = environment.name,
                ID = environment.uniqueId,
                X = environment.transform.position.x,
                Y = environment.transform.position.y
            });
#endif
        }
        if (useTransaction) connection.Commit();
    }

    // save multiple environments at once (useful for ultra fast transactions)
    public void EnvironmentSaveMany(List<M_Environment> environments)
    {
        connection.BeginTransaction();
        foreach (M_Environment environment in environments) EnvironmentSave(environment, false);
        connection.Commit();
    }
    #endregion

    // Item Drop
    // counting all items saved in the database
    public string CountItems()
    {
        return connection.ExecuteScalar<string>("SELECT count(*) FROM Items");
    }
}

#endif

#if _SQLITE
public partial class NetworkManagerMMO
{
    // returns all prefabs by selected tags in registered spawnable prefabs
    public List<Transform> RegisteredPrefabs()
    {
        return (from go in spawnPrefabs
                where go.CompareTag("Monster") || go.CompareTag("Npc") || go.CompareTag("Environment")
                select go.GetComponent<Transform>()).ToList();
    }

    // returns all monsters in registered spawnable prefabs
    public List<Monster> RegisteredMonsters()
    {
        return (from go in spawnPrefabs
                where go.GetComponent<Monster>() != null
                select go.GetComponent<Monster>()).ToList();
    }

    // returns all npcs in registered spawnable prefabs
    public List<Npc> RegisteredNpcs()
    {
        return (from go in spawnPrefabs
                where go.GetComponent<Npc>() != null
                select go.GetComponent<Npc>()).ToList();
    }

    // returns all environments in registered spawnable prefabs
    public List<M_Environment> RegisteredEnvironments()
    {
        return (from go in spawnPrefabs
                where go.GetComponent<M_Environment>() != null
                select go.GetComponent<M_Environment>()).ToList();
    }

#if UMMORPG_3D_R || UMMORPG_2D_R
    public override void Start()
    {
        base.Start();
#if ITEM_DROP_R
        StartItemDrop();
#endif
        StartMTools();
    }

    void StartMTools()
    {
        onStartServer?.AddListener(OnStartServer_Management);
        onServerCharacterSelect?.AddListener(OnServerCharacterSelect_Management);
        onServerDisconnect?.AddListener(OnServerDisconnect_Management);
    }
#endif

    void OnServerCharacterSelect_Management(string account, GameObject player, Mirror.NetworkConnection conn, CharacterSelectMsg message)
    {
        OnlinePlayerCounter.singleton.numPlayers = numPlayers;
    }

    void OnServerDisconnect_Management(Mirror.NetworkConnection conn)
    {
        OnlinePlayerCounter.singleton.numPlayers = numPlayers;
    }

    void OnStartServer_Management()
    {
        Invoke(nameof(LoadEntities), 1.0f);
        Invoke(nameof(LoadSpawners), 1.0f);
    }

    void LoadSpawners()
    {
        Database.singleton.LoadSpawners();
    }

    public void LoadEntities()
    {
        // monsters
        List<string> monsters = AddonManagement.Monsters.Select(m => m.name).Distinct().ToList();
        for (int i = 0; i < monsters.Count; ++i)
        {
            string monsterName = monsters[i];
            Database.singleton.MonsterLoad(monsterName, AddonManagement.Monsters);
        }

        // npcs
        List<string> npcs = AddonManagement.Npcs.Select(n => n.name).Distinct().ToList();
        for (int i = 0; i < npcs.Count; ++i)
        {
            string npcName = npcs[i];
            Database.singleton.NpcLoad(npcName, AddonManagement.Npcs);
        }

        // environments
        List<string> environments = AddonManagement.Environments.Select(e => e.name).Distinct().ToList();
        for (int i = 0; i < environments.Count; ++i)
        {
            string environmentName = environments[i];
            Database.singleton.EnvironmentLoad(environmentName, AddonManagement.Environments);
        }
    }
}

#endif
