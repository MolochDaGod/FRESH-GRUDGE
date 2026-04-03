using System;
using UnityEngine;

#if _iMMOCRAFTING
public enum CraftingResult { None, Failure, Success, CriticalSuccess }

// ---------------------------------------------------------------------------------------
// DefaultCraftingProfession
// ---------------------------------------------------------------------------------------
[Serializable]
public partial class DefaultCraftingProfession
{
    public CraftingProfessionTemplate craftProfession;
    [Min(1)] public int startingExp = 1;
}

// ---------------------------------------------------------------------------------------
// CraftingRecipeIngredient
// ---------------------------------------------------------------------------------------
[Serializable]
public partial class CraftingRecipeIngredient
{
    public ScriptableItem item;
    public int amount;
    public bool DontDestroyOnFailure;
    public bool DontDestroyOnCriticalSuccess;
}

// ---------------------------------------------------------------------------------------
// CraftingRecipeTool
// ---------------------------------------------------------------------------------------
[Serializable]
public partial class CraftingRecipeTool
{
    [Header("[-=-[ Crafting Tool ]-=-]")]
    [Tooltip("[Required] Required item for the crafting process and modifiers below")]
    public ScriptableItem requiredItem;

    [Tooltip("[Optional] Does this item have to be equipped to be effective (any slot)?")]
    public bool equippedItem;

    [Tooltip("[Optional] Is this item destroyed during the crafting process (0 disable, 0.5=50%, 1=100%)?")]
    [Range(0, 1)] public float toolDestroyChance;

    [Tooltip("[Optional] Modify basic success chance when this tool is present?")]
    [Range(0, 1)] public float modifyProbability;

    [Tooltip("[Optional] Modify critical success chance when this tool is present?")]
    [Range(0, 1)] public float modifyCriticalProbability;

    [Tooltip("[Optional] Modify profession experience gained when this tool is present?")]
    public int modifyExperienceMin;

    public int modifyExperienceMax;

    [Tooltip("[Optional] +/- Modify the craft process duration (in Seconds)")]
    public float modifyDuration;
}

// ---------------------------------------------------------------------------------------
// CraftingPopupMessages
// ---------------------------------------------------------------------------------------
[Serializable]
public partial class CraftingPopupMessages
{
    public string breakMessage = "Your tool broke!";
    public string boosterMessage = "You used a booster!";

    public string learnedMessage = "Learned: ";
    [Range(0, 255)] public byte learnedIconId;
    [Range(0, 255)] public byte learnedSoundId;

    public string successMessage = "Crafting successful!";
    [Range(0, 255)] public byte successIconId;
    [Range(0, 255)] public byte successSoundId;

    public string critMessage = "Critical Crafting success!";
    [Range(0, 255)] public byte critIconId;
    [Range(0, 255)] public byte critSoundId;

    public string failMessage = "Crafting failed!";
    [Range(0, 255)] public byte failIconId;
    [Range(0, 255)] public byte failSoundId;
}
#endif