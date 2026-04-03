using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
#if _iMMOASSETBUNDLEMANAGER
using Jacovone.AssetBundleMagic;
#endif

// REALM - TEMPLATE

[CreateAssetMenu(fileName = "Realm", menuName = "ADDON/Templates/Pvp Zone and Realm/New Realm", order = 998)]
public class Tmpl_Realm : ScriptableObject
{
    [Header("[-=-[ REALM ]-=-]")]
    public Sprite image;

    [TextArea(1, 10)] public string description;

    // -----------------------------------------------------------------------------------
    // Caching
    // -----------------------------------------------------------------------------------
    private static Dictionary<int, Tmpl_Realm> _cache;

    public static Dictionary<int, Tmpl_Realm> All
    {
        get
        {
            if (_cache == null)
            {
//#if UNITY_EDITOR
//                string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, @"\uMMORPG\Scripts\Addons\PvPZoneAndRealms");
//#elif UNITY_ANDROID
//        string path = Path.Combine(Application.persistentDataPath, @"\uMMORPG\Scripts\Addons\PvPZoneAndRealms");
//#elif UNITY_IOS
//        string path = Path.Combine(Application.persistentDataPath, @"\uMMORPG\Scripts\Addons\PvPZoneAndRealms");
//#else
//        string path = Path.Combine(Application.dataPath, @"\uMMORPG\Scripts\Addons\PvPZoneAndRealms");
//#endif
                _cache = Resources.LoadAll<Tmpl_Realm>("PvPZoneAndRealms").ToDictionary(x => x.name.GetStableHashCode(), x => x);
            }

            return _cache;

        }
    }
    // -----------------------------------------------------------------------------------
}