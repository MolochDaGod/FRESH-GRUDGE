#if _iMMOTOOLS
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if _iMMOASSETBUNDLEMANAGER
using Jacovone.AssetBundleMagic;
#endif

// ELEMENT TEMPLATE

[CreateAssetMenu(fileName = "New Element", menuName = "ADDON/Templates/New Element", order = 999)]
public partial class ElementTemplate : ScriptableObject
{
    [Header("[-=-=-[ Element ]-=-=-]")]
    [TextArea(1, 30)] public string toolTip;

    public Sprite image;

    // -----------------------------------------------------------------------------------
    // Caching
    // -----------------------------------------------------------------------------------
    private static Dictionary<int, ElementTemplate> _cache;

    public static Dictionary<int, ElementTemplate> dict
    {
        get
        {
            if (_cache == null)
            {
                ScripableObjectEntry entry = TemplateConfiguration.singleton.GetEntry(typeof(ElementTemplate));
                string folderName = entry != null ? entry.folderName : "";
#if _iMMOASSETBUNDLEMANAGER
                if (entry != null && entry.loadFromAssetBundle)
                    _cache = AssetBundleMagic.LoadBundle(entry.bundleName).LoadAllAssets<ElementTemplate>().ToDictionary(x => x.name.GetStableHashCode(), x => x);
                else
                    _cache = Resources.LoadAll<ElementTemplate>(folderName).ToDictionary(x => x.name.GetStableHashCode(), x => x);
#else
                //_cache = SGResources.LoadAll<ElementTemplate>(TemplateConfiguration.singleton.GetTemplatePath(typeof(ElementTemplate))).ToDictionary(x => x.name.GetStableHashCode(), x => x);
                _cache = Resources.LoadAll<ElementTemplate>(TemplateConfiguration.singleton.GetTemplatePath(typeof(ElementTemplate))).ToDictionary(x => x.name.GetStableHashCode(), x => x);
#endif
            }

            return _cache;

        }
    }

    // -----------------------------------------------------------------------------------

}
#endif