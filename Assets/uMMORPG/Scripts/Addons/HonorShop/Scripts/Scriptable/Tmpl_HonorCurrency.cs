using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if _iMMOASSETBUNDLEMANAGER
using Jacovone.AssetBundleMagic;
#endif

// HONOR CURRENCY - TEMPLATE

[CreateAssetMenu(fileName = "HonorCurrency", menuName = "ADDON/Templates/New HonorCurrency", order = 999)]
public class Tmpl_HonorCurrency : ScriptableObject
{
    public Sprite image;
    
    [Tooltip("[Optional] This currency is only dropped if all criteria are met")]
    public Tools_Requirements dropRequirements;

    [Tooltip("[Optional] Currency amount is awarded per level of the target")]
    public bool perLevel;

    [Tooltip("[Optional] Will share a fraction of this currency with online party members")]
    public bool shareWithParty;

    [Tooltip("[Optional] Will share a fraction of this currency with online guild members")]
    public bool shareWithGuild;

#if _iMMOPVP

    [Tooltip("[Optional] Will share a fraction of this currency with online realm members")]
    public bool FromHostileRealmsOnly;

    public bool shareWithRealm;
#endif

    // -----------------------------------------------------------------------------------
    // Caching
    // -----------------------------------------------------------------------------------
    private static Dictionary<int, Tmpl_HonorCurrency> _cache;

    public static Dictionary<int, Tmpl_HonorCurrency> All
    {
        get
        {
            if (_cache == null)
            {
//                ScripableObjectEntry entry = TemplateConfiguration.singleton.GetEntry(typeof(Tmpl_HonorCurrency));
//                string folderName = entry != null ? entry.folderName : "";
//#if _iMMOASSETBUNDLEMANAGER
//                if (entry != null && entry.loadFromAssetBundle)
//                    _cache = AssetBundleMagic.LoadBundle(entry.bundleName).LoadAllAssets<Tmpl_HonorCurrency>().ToDictionary(x => x.name.GetDeterministicHashCode(), x => x);
//                else
//                    _cache = Resources.LoadAll<Tmpl_HonorCurrency>(folderName).ToDictionary(x => x.name.GetDeterministicHashCode(), x => x);
//#else
                _cache = Resources.LoadAll<Tmpl_HonorCurrency>("HonorShop").ToDictionary(x => x.name.GetStableHashCode(), x => x);
//#endif
            }
            
            return _cache;

        }
    }
    // -----------------------------------------------------------------------------------
}