using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if _iMMOASSETBUNDLEMANAGER
using Jacovone.AssetBundleMagic;
#endif

// PRESTIGE CLASS TEMPLATE
[CreateAssetMenu(fileName = "New Prestige Class", menuName = "ADDON/Templates/New Prestige Class", order = 999)]
public class PrestigeClassTemplate : ScriptableObject
{

    // -----------------------------------------------------------------------------------
    // Caching
    // -----------------------------------------------------------------------------------
    private static Dictionary<int, PrestigeClassTemplate> _cache;

    public static Dictionary<int, PrestigeClassTemplate> All
    {
        get
        {
            if (_cache == null)
            {
//                ScripableObjectEntry entry = TemplateConfiguration.singleton.GetEntry(typeof(PrestigeClassTemplate));
//                string folderName = entry != null ? entry.folderName : "";
//#if _iMMOASSETBUNDLEMANAGER
//                if (entry != null && entry.loadFromAssetBundle)
//                    _cache = AssetBundleMagic.LoadBundle(entry.bundleName).LoadAllAssets<PrestigeClassTemplate>().ToDictionary(x => x.name.GetStableHashCode(), x => x);
//                else
//                    _cache = Resources.LoadAll<PrestigeClassTemplate>(folderName).ToDictionary(x => x.name.GetStableHashCode(), x => x);
//#else
                _cache = Resources.LoadAll<PrestigeClassTemplate>("PrestigeClasses").ToDictionary(x => x.name.GetStableHashCode(), x => x);
//#endif
            }

            return _cache;
        }
    }

    // -----------------------------------------------------------------------------------
}