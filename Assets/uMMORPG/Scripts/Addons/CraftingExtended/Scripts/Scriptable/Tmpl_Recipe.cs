using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
#if _iMMOASSETBUNDLEMANAGER
using Jacovone.AssetBundleMagic;
#endif

#if _iMMOCRAFTING

// CRAFT RECIPE TEMPLATE

[CreateAssetMenu(fileName = "New Recipe", menuName = "ADDON/Crafting/New Recipe", order = 998)]
public class Tmpl_Recipe : ScriptableObject
{
    [Header("[-=-[ Skill & Requirements (See Tooltips) ]-=-]")]
    [Tooltip("[Required] The crafting category this recipe will show up")]
    public string category;

    [Tooltip("[Required] The icon to represent this recipe in the list")]
    public Sprite image;

    [Tooltip("[Required] The craft profession required to craft this recipe")]
    public CraftingProfessionTemplate requiredCraft;

    [Tooltip("[Optional] The minimum skill level of the craft profession required")]
    public int minSkillLevel;

    [Tooltip("[Optional] Profession experience per crafting attempt")]
    public int ProfessionExperienceRewardMin = 0;

    public int ProfessionExperienceRewardMax = 2;

    [Header("[-=-[ Ingredients (See Tooltips) ]-=-]")]
    [Tooltip("[Optional] A list of ingredients and their amounts required to craft this item")]
    public CraftingRecipeIngredient[] ingredients;

    [Tooltip("[Optional] The amount of mana required to craft this item")]
    public int manaCost;

#if _iMMOSTAMINA
    [Tooltip("[Optional] The amount of stamina required to craft this item")]
    public int staminaCost;
#endif
    [Header("[-=-[ Results (See Tooltips) ]-=-]")]
    [Tooltip("[Optional] Extra Experience gained on craft Success")]
    public int experience;

    [Tooltip("[Optional] Extra Skill Experience gained on craft Success")]
    public int skillExp;

    [Tooltip("[Required] Item and amount gained on normal Success")]
    public CraftingRecipeIngredient[] defaultResult;

    [Tooltip("[Optional] Item and amount gained on Failure (none if left empty)")]
    public CraftingRecipeIngredient[] failureResult;

    [Tooltip("[Optional] Item and amount gained on critical Success (defaultResult is left empty)")]
    public CraftingRecipeIngredient[] criticalResult;

    [Header("[-=-[ Requirements (See Tooltips) ]-=-]")]
    [Tooltip("[Required] Basic probability for a normal Success")]
    [Range(0, 1)] public float probability = 1;

    [Tooltip("[Optional] Probability for a critical Success (checked after Success)")]
    [Range(0, 1)] public float criticalProbability = 0.05f;

    [Tooltip("[Optional] Probability for generating a Failure Result (checked after Failure)")]
    [Range(0, 1)] public float failureProbability = 0.5f;

    [Tooltip("[Required] Basic duration of the craft process (in Seconds)")]
    public float duration;

    [Header("[-=-[ Required Tools (See Tooltips) ]-=-]")]
    [Tooltip("[Required] Are all Tools required or just any one of them?")]
    public bool requiresAllTools;

    [Tooltip("[Optional] Tool item required, must be equipped? Modifies Success Probability? Modifies Duration?")]
    public CraftingRecipeTool[] tools;

    [Header("[-=-[ Optional Tools (See Tooltips) ]-=-]")]
    [Tooltip("[Optional] Optional tool item, must be equipped? Modifies Success Probability? Modifies Duration?")]
    public CraftingRecipeTool[] optionalTools;

    [Header("[-=-[ Modifiers (See Tooltips) ]-=-]")]
    [Tooltip("[Optional] +/- to basic Success chance per craft profession level")]
    public float probabilityPerSkillLevel;

    [Tooltip("[Optional] +/- to craft duration per craft profession level")]
    public float durationPerSkillLevel;

    [Tooltip("[Optional] +/- to probability of generating a Critical Result per craft profession level")]
    public float criticalResultPerSkillLevel;

    [Tooltip("[Optional] +/- to probability of generating a Failure Result per craft profession level")]
    public float failureResultPerSkillLevel;

    [Header("[-=-[ Tooltip ]-=-]")]
    public string mustBeEquipped = " [Equipped]";

    public string notGuaranteed = " [No Guarantee]";

    [TextArea(1, 30)] public string toolTip;

    // -----------------------------------------------------------------------------------
    // ToolTip
    // -----------------------------------------------------------------------------------
    public string ToolTip()
    {
        string s = "";
        StringBuilder tip = new StringBuilder(toolTip);

        tip.Replace("{NAME}", name);
        tip.Replace("{SKILLNAME}", requiredCraft.name);
        tip.Replace("{MINSKILLLVL}", minSkillLevel.ToString());

        tip.Replace("{PROBABILITY}", Mathf.RoundToInt(probability * 100).ToString());
        tip.Replace("{PROBABILITYBONUS}", Mathf.RoundToInt(probabilityPerSkillLevel * 100).ToString());
        tip.Replace("{DURATION}", duration.ToString());
        tip.Replace("{DURATIONBONUS}", durationPerSkillLevel.ToString());

        // ----- ingredients
        s = "";
        foreach (CraftingRecipeIngredient ingredient in ingredients)
        {
            s += ingredient.amount + "x " + ingredient.item.name + "\n";
        }
        tip.Replace("{INGREDIENTS}", s);

        // ----- required tools
        s = "";
        foreach (CraftingRecipeTool tool in tools)
        {
            s += tool.requiredItem.name;
            if (tool.equippedItem) s += mustBeEquipped + "\n";
            else
                s += "\n";
        }
        tip.Replace("{TOOLS}", s);

        // ----- optional tools
        s = "";
        foreach (CraftingRecipeTool tool in optionalTools)
        {
            s += tool.requiredItem.name;
            if (tool.equippedItem) s += mustBeEquipped + "\n";
            else
                s += "\n";
        }
        tip.Replace("{BOOSTERS}", s);

        // ----- default result
        s = "";
        foreach (CraftingRecipeIngredient result in defaultResult)
            s += result.amount + "x " + result.item.name + "\n";
        tip.Replace("{DEFAULTRESULT}", s);

        // ----- critical result
        s = "";
        foreach (CraftingRecipeIngredient result in criticalResult)
            s += result.amount + "x " + result.item.name + notGuaranteed + "\n";

        tip.Replace("{CRITICALRESULT}", s);

        // ----- failure result
        s = "";
        foreach (CraftingRecipeIngredient result in failureResult)
            s += result.amount + "x " + result.item.name + notGuaranteed + "\n";

        tip.Replace("{FAILURERESULT}", s);

        return tip.ToString();
    }

    // -----------------------------------------------------------------------------------
    // Caching
    // -----------------------------------------------------------------------------------
    private static Dictionary<int, Tmpl_Recipe> cache;

    public static Dictionary<int, Tmpl_Recipe> All
    {
        get
        {
            if (cache == null)
            {
                ScripableObjectEntry entry = TemplateConfiguration.singleton.GetEntry(typeof(Tmpl_Recipe));
                string folderName = entry != null ? entry.folderName : "";
#if _iMMOASSETBUNDLEMANAGER
                if (entry != null && entry.loadFromAssetBundle)
                    _cache = AssetBundleMagic.LoadBundle(entry.bundleName).LoadAllAssets<Tmpl_Recipe>().ToDictionary(x => x.name.GetDeterministicHashCode(), x => x);
                else
                    _cache = Resources.LoadAll<Tmpl_Recipe>(folderName).ToDictionary(x => x.name.GetDeterministicHashCode(), x => x);
#else
                cache = Resources.LoadAll<Tmpl_Recipe>("CraftingExtended").ToDictionary(x => x.name.GetStableHashCode(), x => x);
#endif
            }

            return cache;

        }
    }
    // -----------------------------------------------------------------------------------
}
#endif