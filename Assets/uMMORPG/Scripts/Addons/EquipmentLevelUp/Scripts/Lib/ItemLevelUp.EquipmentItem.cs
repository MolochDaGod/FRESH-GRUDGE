using System.Collections.Generic;
using System.Text;

public partial class EquipmentItem
{
    private string prefix = " (+";
    private string suffix = ")";

    public string Tooltip_EquipmentLevel(int equipmentLevel)
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{CATEGORY}", category);
        tip.Replace("{HEALTHBONUS}", GetHealthBonus(equipmentLevel));
        tip.Replace("{MANABONUS}", GetManaBonus(equipmentLevel));
#if _iMMOSTAMINA
        tip.Replace("{STAMINABONUS}", GetStaminaBonus(equipmentLevel));
#endif
        tip.Replace("{DAMAGEBONUS}", GetDamageBonus(equipmentLevel));
        tip.Replace("{DEFENSEBONUS}", GetDefenseBonus(equipmentLevel));
        tip.Replace("{BLOCKCHANCEBONUS}", GetBlockChanceBonus(equipmentLevel));
        tip.Replace("{CRITICALCHANCEBONUS}", GetCriticalChanceBonus(equipmentLevel));


#if _iMMOATTRIBUTES
        tip.Replace("{ATTRIBUTESBONUS}", GetAttributesBonus(equipmentLevel));
#endif
#if _iMMOELEMENTS
        tip.Replace("{ELEMENTSBONUS}", GetElementsReistanceBonus(equipmentLevel));
#endif
#if _iMMOATTRIBUTES
        tip.Replace("{EXTRASTATSMODIFIERBONUS}", GetExtraStatsModifier(equipmentLevel));
#endif
        return tip.ToString();
    }
    //HealBonus
    private string GetHealthBonus(int equipmentLevel)
    {
        if (LevelUpParameters.Length > 0 && equipmentLevel > 0)
        {
            int levelUpBonus = LevelUpParameters[equipmentLevel - 1].equipmentLevelUpModifier.equipmentStatModifier.healthBonus;
            if (levelUpBonus > 0)
            {
                string finalResult = healthBonus + prefix + levelUpBonus.ToString() + suffix;
                return finalResult;
            }
        }
        return healthBonus.ToString();
    }

    //ManaBonus
    private string GetManaBonus(int equipmentLevel)
    {
        if (LevelUpParameters.Length > 0 && equipmentLevel > 0)
        {
            int levelUpBonus = LevelUpParameters[equipmentLevel - 1].equipmentLevelUpModifier.equipmentStatModifier.manaBonus;
            if (levelUpBonus > 0)
            {
                string finalResult = manaBonus + prefix + levelUpBonus.ToString() + suffix;
                return finalResult;
            }
        }
        return manaBonus.ToString();
    }

#if _iMMOSTAMINA
    //StaminaBonus
    private string GetStaminaBonus(int equipmentLevel)
    {
        if (LevelUpParameters.Length > 0 && equipmentLevel > 0)
        {
            int levelUpBonus = LevelUpParameters[equipmentLevel - 1].equipmentLevelUpModifier.equipmentStatModifier.staminaBonus;
            if (levelUpBonus > 0)
            {
                string finalResult = staminaBonus + prefix + levelUpBonus.ToString() + suffix;
                return finalResult;
            }
        }
        return staminaBonus.ToString();
    }
#endif

    //DamageBonus
    private string GetDamageBonus(int equipmentLevel)
    {
        if (LevelUpParameters.Length > 0 && equipmentLevel > 0)
        {
            int levelUpBonus = LevelUpParameters[equipmentLevel - 1].equipmentLevelUpModifier.equipmentStatModifier.damageBonus;
            if (levelUpBonus > 0)
            {
                string finalResult = damageBonus + prefix + levelUpBonus.ToString() + suffix;
                return finalResult;
            }
        }
        return damageBonus.ToString();
    }

    //DefenseBonus
    private string GetDefenseBonus(int equipmentLevel)
    {
        if(LevelUpParameters.Length > 0 && equipmentLevel > 0)
        {
           int levelUpBonus = LevelUpParameters[equipmentLevel-1].equipmentLevelUpModifier.equipmentStatModifier.defenseBonus;
            if(levelUpBonus > 0)
            {
                string finalResult = defenseBonus + prefix + levelUpBonus.ToString() + suffix;
                return  finalResult;
            }
        }
        return defenseBonus.ToString();
    }

    //BlockBonus
    // <- this is not needed (forgot delete)
    //CriticalBonus
    // <- this is not needed (forgot delete)

    //BlockChanceBonus
    private string GetBlockChanceBonus(int equipmentLevel)
    {
        if (LevelUpParameters.Length > 0 && equipmentLevel > 0)
        {
            float levelUpBonus = LevelUpParameters[equipmentLevel - 1].equipmentLevelUpModifier.equipmentStatModifier.blockChanceBonus;
            if (levelUpBonus > 0)
            {
                string finalResult = (blockChanceBonus*100)+"%" + prefix + (levelUpBonus*100).ToString()+"%" + suffix;
                return finalResult;
            }
        }
        return blockChanceBonus.ToString();
    }

    //CriticalChanceBonus
    private string GetCriticalChanceBonus(int equipmentLevel)
    {
        if (LevelUpParameters.Length > 0 && equipmentLevel > 0)
        {
            float levelUpBonus = LevelUpParameters[equipmentLevel - 1].equipmentLevelUpModifier.equipmentStatModifier.criticalChanceBonus;
            if (levelUpBonus > 0)
            {
                string finalResult = (criticalChanceBonus*100) + "%" + prefix + (levelUpBonus*100).ToString() + "%" + suffix;
                return finalResult;
            }
        }
        return criticalChanceBonus.ToString();
    }


#if _iMMOELEMENTS
    //Foreach ElementResistance
    private string GetElementsReistanceBonus(int equipmentLevel)
    {
        StringBuilder elementsBonusText = new StringBuilder();

        // Créer un dictionnaire pour stocker les attributs de base
        Dictionary<string, float> baseElementsResistance = new Dictionary<string, float>();

        // Parcourir les attributs de base
        foreach (var element in elementalResistances)
        {
            string elementName = element.template.name;
            float basePercentValue = element.value * 100; // Convertir en pourcentage

            // Ajouter l'attribut de base au dictionnaire
            baseElementsResistance.Add(elementName, (basePercentValue));

            // Construction du texte pour cet attribut
            string elementText = $"  - {elementName} : percent {basePercentValue}%";

            // Ajouter le texte de l'attribut à la sortie
            elementsBonusText.AppendLine(elementText);
        }

        if (equipmentLevel > 0)
        {
            // Parcourir les attributs débloqués lors de l'augmentation de l'arme
            var levelUpElementResistance = LevelUpParameters[equipmentLevel - 1].equipmentLevelUpModifier.elementsResistenseBonus;
            foreach (var modifier in levelUpElementResistance)
            {
                string elementName = modifier.template.name;
                float basePercentValue = 0; // Par défaut, l'attribut n'existe pas dans les attributs de base

                // Vérifier si l'attribut existe dans les attributs de base
                if (baseElementsResistance.ContainsKey(elementName))
                {
                    basePercentValue = baseElementsResistance[elementName];
                }

                float levelUpPercentValue = modifier.value * 100; // Convertir en pourcentage

                // Calculer les bonus
                float percentBonus = levelUpPercentValue - basePercentValue;

                // Construction du texte pour cet attribut
                string elementText = $"  - {elementName} : percent {basePercentValue}% ({(percentBonus >= 0 ? "+" : "")}{percentBonus}%)";

                // Ajouter le texte de l'attribut à la sortie
                elementsBonusText.AppendLine(elementText);
            }
        }
        if (elementsBonusText.Length == 0)
            elementsBonusText.AppendLine("None");
        // Retourner le texte des bonus d'attributs
        return elementsBonusText.ToString();
    }
#endif

#if _iMMOATTRIBUTES
    //Foreach Attributebonus
    private string GetAttributesBonus(int equipmentLevel)
    {
        StringBuilder attributeBonusText = new StringBuilder();

        // Créer un dictionnaire pour stocker les attributs de base
        Dictionary<string, (int flat, float percent)> baseAttributes = new Dictionary<string, (int, float)>();

        // Parcourir les attributs de base
        foreach (var modifier in AttributeModifiers)
        {
            string attributeName = modifier.template.name;
            int baseFlatValue = modifier.flatBonus;
            float basePercentValue = modifier.percentBonus * 100; // Convertir en pourcentage

            // Ajouter l'attribut de base au dictionnaire
            baseAttributes.Add(attributeName, (baseFlatValue, basePercentValue));

            // Construction du texte pour cet attribut
            string attributeText = $"  - {attributeName} : flat {baseFlatValue}, percent {basePercentValue}%";

            // Ajouter le texte de l'attribut à la sortie
            attributeBonusText.AppendLine(attributeText);
        }

        if (equipmentLevel > 0)
        {
            // Parcourir les attributs débloqués lors de l'augmentation de l'arme
            var levelUpAttributes = LevelUpParameters[equipmentLevel - 1].equipmentLevelUpModifier.attribute;
            foreach (var modifier in levelUpAttributes)
            {
                string attributeName = modifier.template.name;
                int baseFlatValue = 0; // Par défaut, l'attribut n'existe pas dans les attributs de base
                float basePercentValue = 0; // Par défaut, l'attribut n'existe pas dans les attributs de base

                // Vérifier si l'attribut existe dans les attributs de base
                if (baseAttributes.ContainsKey(attributeName))
                {
                    baseFlatValue = baseAttributes[attributeName].flat;
                    basePercentValue = baseAttributes[attributeName].percent;
                }

                int levelUpFlatValue = modifier.flatBonus;
                float levelUpPercentValue = modifier.percentBonus * 100; // Convertir en pourcentage

                // Calculer les bonus
                int flatBonus = levelUpFlatValue - baseFlatValue;
                float percentBonus = levelUpPercentValue - basePercentValue;

                // Construction du texte pour cet attribut
                string attributeText = $"  - {attributeName} : flat {baseFlatValue} ({(flatBonus >= 0 ? "+" : "")}{flatBonus}), percent {basePercentValue}% ({(percentBonus >= 0 ? "+" : "")}{percentBonus}%)";

                // Ajouter le texte de l'attribut à la sortie
                attributeBonusText.AppendLine(attributeText);
            }
        }
        if (attributeBonusText.Length == 0)
            attributeBonusText.AppendLine("None");
        // Retourner le texte des bonus d'attributs
        return attributeBonusText.ToString();
    }
#endif

#if _iMMOATTRIBUTES
    //ExtraStatsModifier
    private string GetExtraStatsModifier(int equipmentLevel)
    {
        StringBuilder extraStatsModifierBonusText = new StringBuilder();
        ExtraStats extraStatsLevel = equipmentLevel > 0 ? LevelUpParameters[equipmentLevel - 1].equipmentLevelUpModifier.extraModifier : default;

        // - Accuracy
        if (extraStats.accuracy > 0 || extraStatsLevel.accuracy > 0)
            extraStatsModifierBonusText.AppendLine($"  - Accuracy : {extraStats.accuracy*100}%{(extraStatsLevel.accuracy > 0 ? " (+" : "")}{(extraStatsLevel.accuracy*100) +"%)"}");
        // - Résistance
        if (extraStats.resistance > 0 || extraStatsLevel.resistance > 0)
            extraStatsModifierBonusText.AppendLine($"  - Resistance : {extraStats.resistance * 100}%{(extraStatsLevel.resistance > 0 ? " (+" : "")}{(extraStatsLevel.resistance * 100) + "%)"}");
        // - Block Fator
        if (extraStats.blockFactor > 0 || extraStatsLevel.blockFactor > 0)
            extraStatsModifierBonusText.AppendLine($"  - Block Fator : {extraStats.blockFactor * 100}%{(extraStatsLevel.blockFactor > 0 ? " (+" : "")}{(extraStatsLevel.blockFactor * 100) + "%)"}");
        // - Critical Factor
        if (extraStats.criticalFactor > 0 || extraStatsLevel.criticalFactor > 0)
            extraStatsModifierBonusText.AppendLine($"  - Critical Factor : {extraStats.criticalFactor * 100}%{(extraStatsLevel.criticalFactor > 0 ? " (+" : "")}{(extraStatsLevel.criticalFactor * 100) + "%)"}");
        // - Drain Heal Factor
        if (extraStats.drainHealthFactor > 0 || extraStatsLevel.drainHealthFactor > 0)
            extraStatsModifierBonusText.AppendLine($"  - Drain Heal Factor : {extraStats.drainHealthFactor * 100}%{(extraStatsLevel.drainHealthFactor > 0 ? " (+" : "")}{(extraStatsLevel.drainHealthFactor * 100) + "%)"}");
        // - Dran Mana Factor
        if (extraStats.drainManaFactor > 0 || extraStatsLevel.drainManaFactor > 0)
            extraStatsModifierBonusText.AppendLine($"  - Dran Mana Factor : {extraStats.drainManaFactor * 100}%{(extraStatsLevel.drainManaFactor > 0 ? " (+" : "")}{(extraStatsLevel.drainManaFactor * 100) + "%)"}");
        // - Reflect Damage Factor
        if (extraStats.reflectDamageFactor > 0 || extraStatsLevel.reflectDamageFactor > 0)
            extraStatsModifierBonusText.AppendLine($"  - Reflect Damage Factor : {extraStats.reflectDamageFactor * 100}%{(extraStatsLevel.reflectDamageFactor > 0 ? " (+" : "")}{(extraStatsLevel.reflectDamageFactor * 100) + "%)"}");
        // - Defense Break Factor
        if (extraStats.defenseBreakFactor > 0 || extraStatsLevel.defenseBreakFactor > 0)
            extraStatsModifierBonusText.AppendLine($"  - Defense Break Factor : {extraStats.defenseBreakFactor * 100}%{(extraStatsLevel.defenseBreakFactor > 0 ? " (+" : "")}{(extraStatsLevel.defenseBreakFactor * 100) + "%)"}");
        // - Block Break Factor
        if (extraStats.blockBreakFactor > 0 || extraStatsLevel.blockBreakFactor > 0)
            extraStatsModifierBonusText.AppendLine($"  - Block Break Factor : {extraStats.blockBreakFactor * 100}%{(extraStatsLevel.blockBreakFactor > 0 ? " (+" : "")}{(extraStatsLevel.blockBreakFactor * 100) + "%)"}");
        // - Critical Evasion
        if (extraStats.criticalEvasion > 0 || extraStatsLevel.criticalEvasion > 0)
            extraStatsModifierBonusText.AppendLine($"  - Critical Evasion : {extraStats.criticalEvasion * 100}%{(extraStatsLevel.criticalEvasion > 0 ? " (+" : "")}{(extraStatsLevel.criticalEvasion * 100) + "%)"}");
        // - Absorb Heal Factor
        if (extraStats.absorbHealthFactor > 0 || extraStatsLevel.absorbHealthFactor > 0)
            extraStatsModifierBonusText.AppendLine($"  - Absorb Heal Factor : {extraStats.absorbHealthFactor * 100}%{(extraStatsLevel.absorbHealthFactor > 0 ? " (+" : "")}{(extraStatsLevel.absorbHealthFactor * 100) + "%)"}");
        // - Absorb Mana Factor
        if (extraStats.absorbManaFactor > 0 || extraStatsLevel.absorbManaFactor > 0)
            extraStatsModifierBonusText.AppendLine($"  - Absorb Mana Factor : {extraStats.absorbManaFactor * 100}%{(extraStatsLevel.absorbManaFactor > 0 ? " (+" : "")}{(extraStatsLevel.absorbManaFactor * 100) + "%)"}");

        return extraStatsModifierBonusText.ToString();
    }
#endif
}
