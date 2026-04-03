#if _iMMOWORLDEVENTS
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// WORLD EVENT

[CreateAssetMenu(fileName = "New WorldEvent", menuName = "ADDON/Templates/New WorldEvent", order = 999)]
public partial class WorldEventTemplate : ScriptableObject
{
    [Header("[EVENT THRESHOLDS (checked top to bottom)]")]
    public WorldEventData[] thresholdData;

    // -----------------------------------------------------------------------------------
    // Caching
    // -----------------------------------------------------------------------------------
    static Dictionary<int, WorldEventTemplate> cache;
    public static Dictionary<int, WorldEventTemplate> All
    {
        get
        {
            // not loaded yet?
            /*if (cache == null)
            {
                // get all ScriptableItems in resources
                //ScripableObjectEntry entry = TemplateConfiguration.singleton.GetEntry(typeof(WorldEventTemplate));

                //ScripableObjectEntry[] entry = TemplateConfiguration.singleton.GetEntry(typeof(WorldEventTemplate));
                WorldEventTemplate[] entry = Resources.LoadAll<WorldEventTemplate>("");
                // check for duplicates, then add to cache
                List<string> duplicates = entry.ToList().FindDuplicates(item => item.name);
                if (duplicates.Count == 0)
                {
                    cache = entry.ToDictionary(item => item.name.GetStableHashCode(), item => item);
                }
                else
                {
                    foreach (string duplicate in duplicates)
                        Debug.LogError("Resources folder contains multiple WorldEventTemplate with the name " + duplicate + ". If you are using subfolders like 'WorldEvent/EventName' and 'Resources/EventName' and rename it.");
                }
            }*/
            if (cache == null)
            {
                ScripableObjectEntry entry = TemplateConfiguration.singleton.GetEntry(typeof(WorldEventTemplate));
                string folderName = entry != null ? entry.folderName : "";
#if _iMMOASSETBUNDLEMANAGER
                if (entry != null && entry.loadFromAssetBundle)
                    _cache = AssetBundleMagic.LoadBundle(entry.bundleName).LoadAllAssets<Tmpl_Faction>().ToDictionary(x => x.name.GetStableHashCode(), x => x);
                else
                    _cache = Resources.LoadAll<Tmpl_Faction>(folderName).ToDictionary(x => x.name.GetStableHashCode(), x => x);
#else
                cache = Resources.LoadAll<WorldEventTemplate>(TemplateConfiguration.singleton.GetTemplatePath(typeof(WorldEventTemplate))).ToDictionary(x => x.name.GetStableHashCode(), x => x);
#endif
            }
            return cache;
        }
    }
    // -----------------------------------------------------------------------------------
}
#endif