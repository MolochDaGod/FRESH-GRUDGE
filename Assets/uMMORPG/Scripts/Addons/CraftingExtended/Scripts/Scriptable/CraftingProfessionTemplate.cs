using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if _iMMOASSETBUNDLEMANAGER
using Jacovone.AssetBundleMagic;
#endif
#if _iMMOCRAFTING
// CRAFTING PROFESSION TEMPLATE
[CreateAssetMenu(fileName = "New Crafting Profession", menuName = "ADDON/Crafting/New Crafting Profession", order = 999)]
public class CraftingProfessionTemplate : ScriptableObject
{
    [Header("Crafting Profession")]
    public int[] levels;

#if _iMMOTITLES
    [Header("[-=-[ Earn Title ]-=-]")]
    public Tmpl_Titles[] eanTitles;
#endif

    public string[] categories;
    public Sprite image;
    public string playerAnimation;

    [Tooltip("[Optional] Sound effect that is played, when the player starts crafting.")]
    public AudioClip startPlayerSound;

    [Tooltip("[Optional] Sound effect that is played, when the player finishes crafting.")]
    public AudioClip stopPlayerSound;

    [TextArea(1, 30)] public string toolTip;

    // -----------------------------------------------------------------------------------
    // Caching
    // -----------------------------------------------------------------------------------
    private static Dictionary<int, CraftingProfessionTemplate> _cache;

    public static Dictionary<int, CraftingProfessionTemplate> dict
    {
        get
        {
            if (_cache == null)
            {
                ScripableObjectEntry entry = TemplateConfiguration.singleton.GetEntry(typeof(CraftingProfessionTemplate));
                string folderName = entry != null ? entry.folderName : "";
#if _iMMOASSETBUNDLEMANAGER
                if (entry != null && entry.loadFromAssetBundle)
                    _cache = AssetBundleMagic.LoadBundle(entry.bundleName).LoadAllAssets<CraftingProfessionTemplate>().ToDictionary(x => x.name.GetDeterministicHashCode(), x => x);
                else
                    _cache = Resources.LoadAll<CraftingProfessionTemplate>(folderName).ToDictionary(x => x.name.GetDeterministicHashCode(), x => x);
#else
                _cache = Resources.LoadAll<CraftingProfessionTemplate>("CraftingExtended").ToDictionary(x => x.name.GetStableHashCode(), x => x);
#endif
            }

            return _cache;

        }
    }
    // -----------------------------------------------------------------------------------
}
#endif