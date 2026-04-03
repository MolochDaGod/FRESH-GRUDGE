
using UnityEngine;

public static class RarityColor
{
    public static Color SetRarityColorResult(Item slot)
    {
        if (slot.data == null)
            return Color.clear;

        // Validate tier matches rarity - if not, log warning but continue
        EquipmentItem equip = slot.data as EquipmentItem;
        if (equip != null && !CraftingTier.ValidateTierForRarity(equip.rarity, slot.craftingTier))
        {
            Debug.LogWarning($"Item {slot.data.name} has tier {CraftingTier.TierToString(slot.craftingTier)} but rarity {equip.rarity} allows max {CraftingTier.TierToString(CraftingTier.GetMaxTier(equip.rarity))}");
        }

#if _iMMOITEMLEVELUP
        if (slot.equipmentLevel > 0 && ((EquipmentItem)slot.data).enableLevelUp)
            //((EquipmentItem)slot.data).LevelUpParameters[slot.equipmentLevel - 1].equipmentLevelUpModifier.rarity
            //return SetRarityColorResult();
            //
           
        switch (((EquipmentItem)slot.data).LevelUpParameters[slot.equipmentLevel - 1].equipmentLevelUpModifier.rarity)
         {
             case ScriptableItem.ItemRarity.Poor:
                 return SetratityColor(ScriptableItem.ItemRarity.Poor.ToString());

             case ScriptableItem.ItemRarity.Common:
                 return SetratityColor(ScriptableItem.ItemRarity.Common.ToString());

             case ScriptableItem.ItemRarity.Uncommon:
                 return SetratityColor(ScriptableItem.ItemRarity.Uncommon.ToString());

             case ScriptableItem.ItemRarity.Rare:
                 return SetratityColor(ScriptableItem.ItemRarity.Rare.ToString());

             case ScriptableItem.ItemRarity.Epic:
                 return SetratityColor(ScriptableItem.ItemRarity.Epic.ToString());

             case ScriptableItem.ItemRarity.Legendary:
                 return SetratityColor(ScriptableItem.ItemRarity.Legendary.ToString());

             default:
                 return Color.clear;
         }
        else
#endif
            return SetRarityColorResult(slot.data);
    }
    public static Color SetRarityColorResult(ScriptableItem slot)
    {
        if (slot == null)
            return Color.clear;

        switch (slot.rarity)
        {
            case ScriptableItem.ItemRarity.Poor:
                return SetratityColor(ScriptableItem.ItemRarity.Poor.ToString());

            case ScriptableItem.ItemRarity.Common:
                return SetratityColor(ScriptableItem.ItemRarity.Common.ToString());

            case ScriptableItem.ItemRarity.Uncommon:
                return SetratityColor(ScriptableItem.ItemRarity.Uncommon.ToString());

            case ScriptableItem.ItemRarity.Rare:
                return SetratityColor(ScriptableItem.ItemRarity.Rare.ToString());

            case ScriptableItem.ItemRarity.Epic:
                return SetratityColor(ScriptableItem.ItemRarity.Epic.ToString());

            case ScriptableItem.ItemRarity.Legendary:
                return SetratityColor(ScriptableItem.ItemRarity.Legendary.ToString());

            default:
                return Color.clear;
        }
    }

    public static string SetratityColorHtml(string rarity)
    {
        if (!TryGetRarityData(out ScriptableRarityColor data))
            return ColorUtility.ToHtmlStringRGB(Color.gray);

        Color returnColor = Color.gray;
        switch (rarity)
        {
            case "Poor": returnColor = data.poor; break;
            case "Common": returnColor = data.common; break;
            case "Uncommon": returnColor = data.unCommon; break;
            case "Rare": returnColor = data.rare; break;
            case "Epic": returnColor = data.Epic; break;
            case "Legendary": returnColor = data.Legendary; break;
        }
        return ColorUtility.ToHtmlStringRGB(returnColor);
    }
    public static Color SetratityColor(string rarity)
    {
        if (!TryGetRarityData(out ScriptableRarityColor data))
            return Color.gray;

        Color returnColor = Color.gray;
        switch (rarity)
        {
            case "Poor": returnColor = data.poor; break;
            case "Common": returnColor = data.common; break;
            case "Uncommon": returnColor = data.unCommon; break;
            case "Rare": returnColor = data.rare; break;
            case "Epic": returnColor = data.Epic; break;
            case "Legendary": returnColor = data.Legendary; break;
        }
        return returnColor;
    }

    static bool TryGetRarityData(out ScriptableRarityColor data)
    {
        data = null;

        var all = ScriptableRarityColor.All;
        if (all == null)
            return false;

        string output = "ScriptableRarityColor";
        int key = output.GetStableHashCode();
        return all.TryGetValue(key, out data) && data != null;
    }

    /// <summary>
    /// Check if item can be crafted up to next tier.
    /// Respects rarity limits (Uncommon can only reach 2.3, etc.)
    /// </summary>
    public static bool CanCraftUp(Item item)
    {
        if (item.data == null)
            return false;

        EquipmentItem equip = item.data as EquipmentItem;
        if (equip == null)
            return false; // Non-equipment items can't craft up

        return CraftingTier.CanCraftUp(equip.rarity, item.craftingTier);
    }

    /// <summary>
    /// Get the next tier this item can be crafted to.
    /// Returns -1f if at max tier or invalid progression.
    /// </summary>
    public static float GetNextTierForItem(Item item)
    {
        if (item.data == null)
            return -1f;

        EquipmentItem equip = item.data as EquipmentItem;
        if (equip == null)
            return -1f;

        float nextTier = CraftingTier.GetNextTier(item.craftingTier);
        if (nextTier < 0f)
            return -1f;

        // Ensure next tier doesn't exceed rarity max
        float maxTier = CraftingTier.GetMaxTier(equip.rarity);
        return nextTier <= maxTier ? nextTier : -1f;
    }

    /// <summary>
    /// Safely craft item up to next tier.
    /// Returns true if successful, false otherwise.
    /// </summary>
    public static bool TryCraftTierUp(ref Item item)
    {
        float nextTier = GetNextTierForItem(item);
        if (nextTier < 0f)
            return false;

        item.craftingTier = nextTier;
        return true;
    }
}