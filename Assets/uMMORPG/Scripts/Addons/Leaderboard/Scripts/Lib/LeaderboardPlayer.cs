using System;

// ===================================================================================
// Leaderboard player
// ===================================================================================
[Serializable]
public partial struct LeaderboardPlayer
{
    public string name;
    public int rank;
    public int level;
    public long gold;
    public int death;
    public int playerkill;
    public int monsterkill;

    // -------------------------------------------------------------------------------
    // Extended_Quest (Constructor)
    // -------------------------------------------------------------------------------
    public LeaderboardPlayer(string _name, int _level, long _gold, int _death, int _playerkill, int _monsterkill)
    {
        name = _name;
        level = _level;
        gold = _gold;
        rank = 0;
        death = _death;
        playerkill = _playerkill;
        monsterkill = _monsterkill;

        rank = CalculateRank();
    }

    // -------------------------------------------------------------------------------
    //
    // -------------------------------------------------------------------------------
    public int CalculateRank()
    {
        return 0;
    }

    // -------------------------------------------------------------------------------
}