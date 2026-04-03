using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if _iMMOASSETBUNDLEMANAGER
using Jacovone.AssetBundleMagic;
#endif

// PRESTIGE CLASS - TEMPLATE

[CreateAssetMenu(fileName = "Tmpl_Faction", menuName = "ADDON/Templates/New Faction", order = 998)]
public class Tmpl_Faction : ScriptableObject
{
    [Header("[-=-[ FACTION ]-=-]")]
    public Sprite image;

    [TextArea(1, 10)] public string description;

    public FactionRank[] ranks;
#if _iMMOPVP

    [Tooltip("Monsters set to 'aggressive' will only attack a player when their faction ranking falls below this threshold.")]
    public float aggressiveThreshold;

#endif

    // -----------------------------------------------------------------------------------
    // getRank
    // -----------------------------------------------------------------------------------
    public string getRank(int rating)
    {
        foreach (FactionRank rank in ranks)
            if (rank.min <= rating && rank.max >= rating) return rank.name;

        return "???";
    }

    // -----------------------------------------------------------------------------------
    // getRank
    // -----------------------------------------------------------------------------------
    public string getRank(int min, int max)
    {
        foreach (FactionRank rank in ranks)
            if (min >= rank.min && max <= rank.max) return rank.name;

        return "???";
    }

    // -----------------------------------------------------------------------------------
    // checkAggressive
    // -----------------------------------------------------------------------------------
    public bool checkAggressive(int rating)
    {
#if _iMMOPVP
        return (rating <= aggressiveThreshold);
#else
		return false;
#endif
    }

    // -----------------------------------------------------------------------------------
    // Caching
    // -----------------------------------------------------------------------------------
    private static Dictionary<int, Tmpl_Faction> _cache;

    public static Dictionary<int, Tmpl_Faction> dict
    {
        get
        {
            if (_cache == null)
            {
//                ScripableObjectEntry entry = TemplateConfiguration.singleton.GetEntry(typeof(Tmpl_Faction));
//                string folderName = entry != null ? entry.folderName : "";
//#if _iMMOASSETBUNDLEMANAGER
//                if (entry != null && entry.loadFromAssetBundle)
//                    _cache = AssetBundleMagic.LoadBundle(entry.bundleName).LoadAllAssets<Tmpl_Faction>().ToDictionary(x => x.name.GetStableHashCode(), x => x);
//                else
//                    _cache = Resources.LoadAll<Tmpl_Faction>(folderName).ToDictionary(x => x.name.GetStableHashCode(), x => x);
//#else
                _cache = Resources.LoadAll<Tmpl_Faction>("Factions").ToDictionary(x => x.name.GetStableHashCode(), x => x);
//#endif
            }
            return _cache;
        }
    }
    // -----------------------------------------------------------------------------------
}