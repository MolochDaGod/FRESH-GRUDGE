using UnityEngine;

// FACTION
public struct Faction
{
    public string name;
    public int rating;

    public Tmpl_Faction data
    {
        get
        {
            Tmpl_Faction _data;
            Tmpl_Faction.dict.TryGetValue(name.GetStableHashCode(), out _data);
            return _data;
        }
    }
}

// FACTION RANK

[System.Serializable]
public class FactionRank
{
    public string name;
    [Range(-9999, 9999)] public int min;
    [Range(-9999, 9999)] public int max;
}

// FACTION RATING

[System.Serializable]
public class FactionRating
{
    public Tmpl_Faction faction;
    [Range(-9999, 9999)] public int startRating;
}

// FACTION REQUIREMENT

[System.Serializable]
public class FactionRequirement
{
    public Tmpl_Faction faction;
    [Range(-9999, 9999)] public int min;
    [Range(-9999, 9999)] public int max;
}

// FACTION QUEST

[System.Serializable]
public class Quest_Faction
{
    public Tmpl_Faction faction;
    [Range(-9999, 9999)] public int min;
}

// FACTION MODIFIER

[System.Serializable]
public class FactionModifier
{
    public Tmpl_Faction faction;
    [Range(-9999, 9999)] public int amount;
}