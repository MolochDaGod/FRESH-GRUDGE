/// Database integration for ClassLevelingTree addon.
///
/// REQUIRES: _IMMOCLASSTREE and _SQLITE defines active.
///
/// Table: character_classtree
///   character  VARCHAR(32) — character name
///   nodehash   INTEGER     — name.GetStableHashCode() of chosen ClassTreeNodeTemplate
///
/// HOW TO WIRE UP (if not already using partial method pattern):
///   In your main Database.cs Connect() method, add:
///       Connect_ClassLevelingTree();
///   In CharacterLoad(), after skills are loaded, add:
///       CharacterLoad_ClassLevelingTree(player);
///   In CharacterSave(), add:
///       CharacterSave_ClassLevelingTree(player, online);

#if _IMMOCLASSTREE && _SQLITE
public partial class Database
{
    class character_classtree_nodehash
    {
        public int nodehash { get; set; }
    }

    // ── Schema ────────────────────────────────────────────────────────────
    void Connect_ClassLevelingTree()
    {
        connection.Execute(@"CREATE TABLE IF NOT EXISTS character_classtree (
            character VARCHAR(32) NOT NULL,
            nodehash  INTEGER     NOT NULL,
            PRIMARY KEY (character, nodehash)
        )");
    }

    // ── Load ──────────────────────────────────────────────────────────────
    void CharacterLoad_ClassLevelingTree(Player player)
    {
        if (player.classTree == null) return;

        player.classTree.chosenNodes.Clear();

        foreach (character_classtree_nodehash row in connection.Query<character_classtree_nodehash>(
                     "SELECT nodehash FROM character_classtree WHERE character=?", player.name))
            player.classTree.chosenNodes.Add(row.nodehash);
    }

    // ── Save ──────────────────────────────────────────────────────────────
    void CharacterSave_ClassLevelingTree(Player player, bool online)
    {
        if (player.classTree == null) return;

        connection.Execute("DELETE FROM character_classtree WHERE character=?", player.name);

        foreach (int hash in player.classTree.chosenNodes)
            connection.Execute(
                "INSERT OR IGNORE INTO character_classtree VALUES (?, ?)",
                player.name,
                hash);
    }
}
#endif
