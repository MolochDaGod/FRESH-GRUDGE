using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
#if _iMMOASSETBUNDLEMANAGER
using Jacovone.AssetBundleMagic;
#endif

// TRAIT TEMPLATE

[CreateAssetMenu(fileName = "Trait", menuName = "ADDON/Templates/New Trait", order = 999)]
public class TraitTemplate : ScriptableObject
{
    [Header("[-=-[ TRAIT ]-=-]")]
    public Sprite image;

    [TextArea(1, 30)]
    public string toolTip;

    public int traitCost;

    [Tooltip("A player can feature only one trait of each unique group id (0 = none)")]
    public int uniqueGroup;

    public Player[] allowedClasses;

    [Header("[-=-[ SKILL MODIFIERS (must be in prefabs skill list!) ]-=-]")]
    public Tools_SkillRequirement[] startingSkills;

    [Header("[-=-[ STARTING ITEMS ]-=-]")]
    public Tools_ItemModifier[] startingItems;

#if _iMMOPRESTIGECLASSES
    [Header("[-=-[ STARTING PRESTIGE CLASS ]-=-]")]
    public PrestigeClassTemplate startingPrestigeClass;
#endif

#if _iMMOHONORSHOP
    [Header("[-=-[ STARING HONOR CURRENCY ]-=-]")]
    public HonorShopCurrencyCost[] startingHonorCurrency;
#endif

#if _iMMOFACTIONS
    [Header("[-=-[ STARTING FACTION MODIFIER ]-=-]")]
    public FactionRating[] startingFactions;
#endif

#if _iMMOCRAFTING
    [Header("[-=-[ CRAFT PROFESSION ]-=-]")]
    public Tools_CraftingProfessionRequirement[] startingCraftingProfession;
#endif

#if _iMMOHARVESTING
    [Header("[-=-[ HARVEST PROFESSION ]-=-]")]
    public HarvestingProfessionRequirement[] startingHarvestingProfession;
#endif

#if _iMMOPVP
    [Header("[-=-[ PVP REALM CHANGE ]-=-]")]
    public Tmpl_Realm changeRealm;
    public Tmpl_Realm changeAlliedRealm;
#endif

    public Tools_StatModifier statModifiers;

    // -----------------------------------------------------------------------------------
    // Caching
    // -----------------------------------------------------------------------------------
    private static Dictionary<int, TraitTemplate> _cache;

    public static Dictionary<int, TraitTemplate> dict
    {
        get
        {
            if (_cache == null)
            {
//                ScripableObjectEntry entry = TemplateConfiguration.singleton.GetEntry(typeof(TraitTemplate));
//                string folderName = entry != null ? entry.folderName : "";
//#if _iMMOASSETBUNDLEMANAGER
//                if (entry != null && entry.loadFromAssetBundle)
//                    _cache = AssetBundleMagic.LoadBundle(entry.bundleName).LoadAllAssets<TraitTemplate>().ToDictionary(x => x.name.GetStableHashCode(), x => x);
//                else
//                    _cache = Resources.LoadAll<TraitTemplate>(folderName).ToDictionary(x => x.name.GetStableHashCode(), x => x);
//#else
                _cache = Resources.LoadAll<TraitTemplate>("Traits").ToDictionary(x => x.name.GetStableHashCode(), x => x);
//#endif
            }

            return _cache;

        }
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public string ToolTip()
    {
        // we use a StringBuilder so that addons can modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        StringBuilder tip = new StringBuilder(toolTip);
        tip.Replace("{NAME}", name);

        /*tip.Replace("{PERCENTHEALTH}", percentHealth.ToString("0.0"));
        tip.Replace("{FLATHEALTH}", flatHealth.ToString());
        tip.Replace("{PERCENTMANA}", percentMana.ToString("0.0"));
        tip.Replace("{FLATMANA}", flatMana.ToString());
        tip.Replace("{PERCENTDAMAGE}", percentDamage.ToString("0.0"));
        tip.Replace("{FLATDAMAGE}", flatDamage.ToString());
        tip.Replace("{PERCENTDEFENSE}", percentDefense.ToString("0.0"));
        tip.Replace("{FLATDEFENSE}", flatDefense.ToString());
        tip.Replace("{PERCENTBLOCK}", percentBlock.ToString("0.0"));
        tip.Replace("{PERCENTCRITICAL}", percentCritical.ToString("0.0"));*/

        return tip.ToString();
    }

    // -----------------------------------------------------------------------------------
}
