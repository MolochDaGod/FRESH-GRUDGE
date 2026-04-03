using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Title", menuName = "ADDON/Templates/Title/title", order = 999)]
public partial class Tmpl_Titles : ScriptableObject
{
    [Header("[-=-[ Title ]-=-]")]
    public string displayName;
    public Sprite image;
    public bool displayed;
    public string description;
    public Tmpl_TitlesCategory category;

#if _iMMOATTRIBUTES
    [Header("[-=-[ TrugordAttribute Modifiers ]-=-]")]
    public Tools_AttributeModifier[] AttributeModifiers = { };

    [Header("[-=-[ Extra Stats ]-=-]")]
    [Tooltip("[Optional] Accuracy - increases chance to inflict a (negative) buff on a target (0.01=1%, 0.5=50%, 1=100%)")]
    [Range(-1, 1)] public float bonusAccuracy;

    [Tooltip("[Optional] Resistances - increases chance to resist a (negative) buff being inflicted on self (0.01=1%, 0.5=50%, 1=100%)")]
    [Range(-1, 1)] public float bonusResistance;

    [Tooltip("[Optional] Block Factor - increases the power of blocking an attack (damage reduction) (0.01=1%, 0.5=50%, 1=100%)")]
    [Range(-1, 1)] public float bonusBlockFactor;

    [Tooltip("[Optional] Critical Factor - increases the damage of a critical hit (0.01=1%, 0.5=50%, 1=100%)")]
    [Range(-1, 1)] public float bonusCriticalFactor;

    [Tooltip("[Optional] Drain Health Factor - drains a percentage of health from the target with every hit (0.01=1%, 0.5=50%, 1=100%)")]
    [Range(-1, 1)] public float bonusDrainHealthFactor;

    [Tooltip("[Optional] Drain Mana Factor - drains a percentage of mana from the target with every hit (0.01=1%, 0.5=50%, 1=100%)")]
    [Range(-1, 1)] public float bonusDrainManaFactor;

    [Tooltip("[Optional] Reflect Damage Factor - reflects a percentage of damage dealt to self back onto the attacker (0.01=1%, 0.5=50%, 1=100%)")]
    [Range(-1, 1)] public float bonusReflectDamageFactor;

    [Tooltip("[Optional] Defense Break Factor - reduces the targets defense (0.01=1%, 0.5=50%, 1=100%)")]
    [Range(-1, 1)] public float bonusDefenseBreakFactor;

    [Tooltip("[Optional] Block Break Factor - reduces the targets block chance (0.01=1%, 0.5=50%, 1=100%)")]
    [Range(-1, 1)] public float bonusBlockBreakFactor;

    [Tooltip("[Optional] Critical Evasion - reduces the chance of receiving a critical hit (0.01=1%, 0.5=50%, 1=100%)")]
    [Range(-1, 1)] public float bonusCriticalEvasion;

    [Tooltip("[Optional] Absorb Health Factor - regenerates a percentage of health on every received hit (0.01=1%, 0.5=50%, 1=100%)")]
    [Range(-1, 1)] public float bonusAbsorbHealthFactor;

    [Tooltip("[Optional] Absorb Mana Factor - regenerates a percentage of health on every received hit (0.01=1%, 0.5=50%, 1=100%)")]
    [Range(-1, 1)] public float bonusAbsorbManaFactor;
#endif
    private static Dictionary<int, Tmpl_Titles> _cache;

    public static Dictionary<int, Tmpl_Titles> dict
    {
        get
        {
            if (_cache == null)
            {
//                ScripableObjectEntry entry = TemplateConfiguration.singleton.GetEntry(typeof(Tmpl_Titles));
//                string folderName = entry != null ? entry.folderName : "";
//#if _iMMOASSETBUNDLEMANAGER
//                if (entry != null && entry.loadFromAssetBundle)
//                    _cache = AssetBundleMagic.LoadBundle(entry.bundleName).LoadAllAssets<Tmpl_Titles>().ToDictionary(x => x.name.GetStableHashCode(), x => x);
//                else
//                    _cache = Resources.LoadAll<Tmpl_Faction>(folderName).ToDictionary(x => x.name.GetStableHashCode(), x => x);
//#else
                _cache = Resources.LoadAll<Tmpl_Titles>("PlayerTitles").ToDictionary(x => x.name.GetStableHashCode(), x => x);
//#endif
            }

            return _cache;

        }
    }
}