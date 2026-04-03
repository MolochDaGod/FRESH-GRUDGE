using UnityEngine;

/// <summary>
/// Defines gear tier progression for crafting and item upgrades.
/// Maps item rarity (drop) to craftable tiers (1.2, 1.3, 2.2, 2.3, 3.2-3.5).
/// Tier notation: X.Y where X=major tier (1-3), Y=subversion (2-5).
/// Example: 1.2 -> 1.3 -> 2.0 -> 2.2 -> 2.3 -> 3.0 -> 3.2 -> 3.3 -> 3.4 -> 3.5
/// </summary>
public static class CraftingTier
{
    /// <summary>Tier progression chain: what tier can be crafted from what previous tier</summary>
    private static readonly (float from, float to)[] tierProgression = new[]
    {
        (1.1f, 1.2f),  // Common → Common+
        (1.2f, 1.3f),  // Common+ → Common Premium
        (1.3f, 2.0f),  // Common → Rare (tier jump)
        (2.0f, 2.2f),  // Rare → Rare+
        (2.2f, 2.3f),  // Rare+ → Rare Premium
        (2.3f, 3.0f),  // Rare → Epic (tier jump)
        (3.0f, 3.2f),  // Epic → Epic+
        (3.2f, 3.3f),  // Epic+ → Epic Premium
        (3.3f, 3.4f),  // Epic Premium → Epic Legendary
        (3.4f, 3.5f),  // Epic Legendary → Max Legendary
    };

    /// <summary>Starting tier for each rarity when item is first dropped/created</summary>
    private static readonly System.Collections.Generic.Dictionary<ScriptableItem.ItemRarity, float> rarityToStartingTier =
        new System.Collections.Generic.Dictionary<ScriptableItem.ItemRarity, float>
    {
        { ScriptableItem.ItemRarity.Poor, 1.0f },
        { ScriptableItem.ItemRarity.Common, 1.1f },
        { ScriptableItem.ItemRarity.Uncommon, 1.2f },
        { ScriptableItem.ItemRarity.Rare, 2.0f },
        { ScriptableItem.ItemRarity.Epic, 3.0f },
        { ScriptableItem.ItemRarity.Legendary, 3.4f },
    };

    /// <summary>Maximum tier reachable for each rarity</summary>
    private static readonly System.Collections.Generic.Dictionary<ScriptableItem.ItemRarity, float> rarityToMaxTier =
        new System.Collections.Generic.Dictionary<ScriptableItem.ItemRarity, float>
    {
        { ScriptableItem.ItemRarity.Poor, 1.2f },
        { ScriptableItem.ItemRarity.Common, 2.0f },
        { ScriptableItem.ItemRarity.Uncommon, 2.3f },
        { ScriptableItem.ItemRarity.Rare, 3.3f },
        { ScriptableItem.ItemRarity.Epic, 3.4f },
        { ScriptableItem.ItemRarity.Legendary, 3.5f },
    };

    /// <summary>
    /// Get the starting tier for an item based on its rarity.
    /// Common items start at 1.1, Rare at 2.0, etc.
    /// </summary>
    public static float GetStartingTier(ScriptableItem.ItemRarity rarity)
    {
        if (rarityToStartingTier.TryGetValue(rarity, out float tier))
            return tier;
        return 1.1f; // Default: Common
    }

    /// <summary>
    /// Get the maximum tier an item can reach based on rarity.
    /// Legendary items can reach 3.5, Uncommon only reach 2.3, etc.
    /// </summary>
    public static float GetMaxTier(ScriptableItem.ItemRarity rarity)
    {
        if (rarityToMaxTier.TryGetValue(rarity, out float tier))
            return tier;
        return 1.2f; // Default: Poor max is 1.2
    }

    /// <summary>
    /// Validate if a tier-up craft is valid.
    /// Returns true if 'fromTier' can be crafted up to 'toTier'.
    /// </summary>
    public static bool IsValidTierProgression(float fromTier, float toTier)
    {
        foreach (var (from, to) in tierProgression)
        {
            if (Mathf.Approximately(from, fromTier) && Mathf.Approximately(to, toTier))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Get the next tier that can be crafted from the current tier.
    /// Returns -1f if at max tier or invalid tier.
    /// </summary>
    public static float GetNextTier(float currentTier)
    {
        foreach (var (from, to) in tierProgression)
        {
            if (Mathf.Approximately(from, currentTier))
                return to;
        }
        return -1f; // No next tier available
    }

    /// <summary>
    /// Check if item can be crafted up to a higher tier while respecting rarity limits.
    /// </summary>
    public static bool CanCraftUp(ScriptableItem.ItemRarity rarity, float currentTier)
    {
        float maxTier = GetMaxTier(rarity);
        float nextTier = GetNextTier(currentTier);

        if (nextTier < 0f)
            return false; // At final tier

        return nextTier <= maxTier; // Can craft up if next tier doesn't exceed rarity max
    }

    /// <summary>
    /// Get tier string representation for UI/debug (e.g. "1.2", "2.3", "3.5").
    /// Handles floating point representation safely.
    /// </summary>
    public static string TierToString(float tier)
    {
        // Round to 1 decimal place to handle float precision
        tier = Mathf.Round(tier * 10) / 10f;

        int major = (int)tier;
        int minor = (int)Mathf.Round((tier - major) * 10);

        return $"{major}.{minor}";
    }

    /// <summary>
    /// Parse tier string back to float (e.g. "1.2" -> 1.2f).
    /// Returns -1f if invalid format.
    /// </summary>
    public static float StringToTier(string tierString)
    {
        if (string.IsNullOrEmpty(tierString))
            return -1f;

        if (float.TryParse(tierString, out float tier))
            return Mathf.Round(tier * 10) / 10f;

        return -1f;
    }

    /// <summary>
    /// Validate that item tier matches its rarity constraints.
    /// Common items should be between starting and max tiers for their rarity.
    /// </summary>
    public static bool ValidateTierForRarity(ScriptableItem.ItemRarity rarity, float itemTier)
    {
        float startTier = GetStartingTier(rarity);
        float maxTier = GetMaxTier(rarity);

        return itemTier >= startTier && itemTier <= maxTier;
    }

    /// <summary>
    /// Get all possible tiers in the full progression chain.
    /// Useful for UI dropdowns or validation.
    /// </summary>
    public static float[] GetAllTiers()
    {
        // Collect all unique tiers from progression
        var tiers = new System.Collections.Generic.HashSet<float>();
        foreach (var (from, to) in tierProgression)
        {
            tiers.Add(from);
            tiers.Add(to);
        }

        // Convert to sorted array
        var tierList = new System.Collections.Generic.List<float>(tiers);
        tierList.Sort();
        return tierList.ToArray();
    }
}
