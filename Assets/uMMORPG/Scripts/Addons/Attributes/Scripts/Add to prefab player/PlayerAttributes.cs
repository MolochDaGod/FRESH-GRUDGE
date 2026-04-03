using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

// PLAYER
[RequireComponent(typeof(Equipment))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Mana))]
public class PlayerAttributes : NetworkBehaviour, IHealthBonus, IManaBonus
{
#if _iMMOATTRIBUTES

    public Player player;
    public Health health;
#if _iMMOSTAMINA
    public Stamina stamina;
#endif
    public Equipment equipment;

    [Header("[-=-[ ATTRIBUTES ]-=-]")]
    public _playerAttributes playerAttributes;

    public Dictionary<string, AttributeCache> _attributeCache = new Dictionary<string, AttributeCache>();

    public readonly SyncList<TrugordAttribute> Attributes = new SyncList<TrugordAttribute>();

    private AttributeCache GetCachedValue(string functionName)
    {
        AttributeCache cachedValue;
        _attributeCache.TryGetValue(functionName, out cachedValue);
        return cachedValue;
    }

    // Fonction pour mettre à jour le cache avec la nouvelle valeur pour une fonction donnée
    private void SetCachedValue(string functionName, int value)
    {
        _attributeCache[functionName] = new AttributeCache { timer = Time.time + player.cacheTimerInterval, value = value };
    }

    public int CalcHealthMax()
    {
        // Vérifier le cache pour la fonction CalcHealthMax
        AttributeCache cachedHealthMax = GetCachedValue("CalcHealthMax");
        if (cachedHealthMax != null && cachedHealthMax.timer >= Time.time)
        {
            //UnityEngine.Debug.Log("CalcHealthMax by cache :" + cachedHealthMax.value);
            return cachedHealthMax.value;
        }

        int total = 0;

        if (player.playerAttribute.Attributes.Count > 0)
        {
            int points = 0;
            int flatBonus = 0;
            float pctBonus = 0f;

            foreach (TrugordAttribute attrib in player.playerAttribute.Attributes)
            {
                points = player.playerAttribute.calculateBonusAttribute(attrib) + attrib.points;

                flatBonus += attrib.flatHealth * points;
                pctBonus += attrib.percentHealth * points;
            }

            total += flatBonus;
            total += (int)Mathf.Round(health.baseHealth.Get(player.level.current) * pctBonus);
        }


        // -- Bonus: Traits
        int iTraitBonus = 0;
#if _iMMOTRAITS
        //if (player.Traits.Count > 0) Debug.Log("il y à des trait !");
            iTraitBonus = player.playerTraits.Traits.Sum(trait => trait.healthBonus);
        //.Log(" Trait health = " + player.Traits.Sum(trait => trait.healthBonus));
#endif
        // -- Bonus: Equipment Sets
        int iSetBonus = 0;
#if _iMMOEQUIPMENTSETS
        foreach (var slot in equipment.slots)
        {
            if (slot.amount > 0)
            {
                if (slot.item.HasIndividualSetBonus())
                    iSetBonus += Convert.ToInt32(slot.item.setIndividualBonusHealth(equipment.slots));

                if (slot.item.HasPartialSetBonus())
                    iSetBonus += Convert.ToInt32(slot.item.setPartialBonusHealth(equipment.slots));

                if (slot.item.HasCompleteSetBonus())
                    iSetBonus += Convert.ToInt32(slot.item.setCompleteBonusHealth(equipment.slots));
            }
        }
#endif
         int returnValue = total + iTraitBonus + iSetBonus;

        // Mettre à jour le cache pour la fonction CalcHealthMax avec la nouvelle valeur
        SetCachedValue("CalcHealthMax", returnValue);

        return returnValue;

    }
    public int GetHealthBonus(int baseHealth)
    {
        // Vérifier le cache pour la fonction CalcHealthMax
        AttributeCache cachedHealthMax = GetCachedValue("GetHealthBonus");
        if (cachedHealthMax != null && cachedHealthMax.timer >= Time.time)
        {
            return cachedHealthMax.value;
        }

        int total = 0;

        if (player.playerAttribute.Attributes.Count > 0)
        {
            int points = 0;
            int flatBonus = 0;
            float pctBonus = 0f;

            foreach (TrugordAttribute attrib in player.playerAttribute.Attributes)
            {
                points = player.playerAttribute.calculateBonusAttribute(attrib) + attrib.points;

                flatBonus += attrib.flatHealth * points;
                pctBonus += attrib.percentHealth * points;
            }

            total += flatBonus;
            total += (int)Mathf.Round(health.baseHealth.Get(player.level.current) * pctBonus);
        }


        // -- Bonus: Traits
        int iTraitBonus = 0;
#if _iMMOTRAITS
        if (player.playerTraits && player.playerTraits.Traits.Count > 0)
            iTraitBonus = player.playerTraits.Traits.Sum(trait => trait.healthBonus);
        //.Log(" Trait health = " + player.Traits.Sum(trait => trait.healthBonus));
#endif
        // -- Bonus: Equipment Sets
        int iSetBonus = 0;
#if _iMMOEQUIPMENTSETS
        foreach (var slot in equipment.slots)
        {
            if (slot.amount > 0)
            {
                if (slot.item.HasIndividualSetBonus())
                    iSetBonus += Convert.ToInt32(slot.item.setIndividualBonusHealth(equipment.slots));

                if (slot.item.HasPartialSetBonus())
                    iSetBonus += Convert.ToInt32(slot.item.setPartialBonusHealth(equipment.slots));

                if (slot.item.HasCompleteSetBonus())
                    iSetBonus += Convert.ToInt32(slot.item.setCompleteBonusHealth(equipment.slots));
            }
        }
#endif
        int returnValue = total + iTraitBonus + iSetBonus;

        // Mettre à jour le cache pour la fonction CalcHealthMax avec la nouvelle valeur
        SetCachedValue("GetHealthBonus", returnValue);

        return returnValue;
    }
    //public int GetHealthBonus(int yt) =>
    //Convert.ToInt32(CalcHealthMax());

    public int GetHealthRecoveryBonus() => 0;

    /********************
     * Mana 
     *******************/
   
    public int GetManaBonus(int mana) {
        // Vérifier le cache pour la fonction CalcHealthMax
        AttributeCache cachedHealthMax = GetCachedValue("GetManaBonus");
        if (cachedHealthMax != null && cachedHealthMax.timer >= Time.time)
        {
            //UnityEngine.Debug.Log("GetManaBonus by cache :" + cachedHealthMax.value);
            return cachedHealthMax.value;
        }
        int total = 0;

        if (player.playerAttribute.Attributes.Count > 0)
        {
            int points = 0;
            int flatBonus = 0;
            float pctBonus = 0f;

            foreach (TrugordAttribute attrib in player.playerAttribute.Attributes)
            {
                points = player.playerAttribute.calculateBonusAttribute(attrib) + attrib.points;

                flatBonus += attrib.flatMana * points;
                pctBonus += attrib.percentMana * points;
            }

            total += flatBonus;
            total += (int)Mathf.Round(player.mana.baseMana.Get(player.level.current) * pctBonus);
        }


        // -- Bonus: Traits
        int iTraitBonus = 0;
#if _iMMOTRAITS

        if (player.playerTraits && player.playerTraits.Traits.Count > 0)
            iTraitBonus = player.playerTraits.Traits.Sum(trait => trait.manaBonus);
#endif

        // -- Bonus: Equipment Sets
        int iSetBonus = 0;
#if _iMMOEQUIPMENTSETS
        foreach (var slot in equipment.slots)
        {
            if (slot.amount > 0)
            {
                if (slot.item.HasIndividualSetBonus())
                    iSetBonus += Convert.ToInt32(slot.item.setIndividualBonusMana(equipment.slots));

                if (slot.item.HasPartialSetBonus())
                    iSetBonus += Convert.ToInt32(slot.item.setPartialBonusMana(equipment.slots));

                if (slot.item.HasCompleteSetBonus())
                    iSetBonus += Convert.ToInt32(slot.item.setCompleteBonusMana(equipment.slots));
            }
        }
#endif
        int returnValue = total + iTraitBonus + iSetBonus;

        // Mettre à jour le cache pour la fonction CalcHealthMax avec la nouvelle valeur
        SetCachedValue("GetManaBonus", returnValue);

        return returnValue;
    }

    public int GetManaRecoveryBonus() => 0;
    // -----------------------------------------------------------------------------------
    // calculateBonusAttribute
    // -----------------------------------------------------------------------------------
    public int calculateBonusAttribute(TrugordAttribute attrib)
    {
        AttributeCache cachedHealthMax = GetCachedValue("calculateBonusAttribute_"+ attrib.name);
        if (cachedHealthMax != null && cachedHealthMax.timer >= Time.time)
        {
            return cachedHealthMax.value;
        }
        int _points = attrib.points;
        int points = 0;


        // ------------------------------- Calculation -----------------------------------

        // -- Buff Bonus
        foreach (Buff buff in player.skills.buffs)
        {
            if (buff.data.AttributeModifiers.Length > 0)
            {
                var validItems = buff.data.AttributeModifiers.ToList().Where(attr => attr.template == attrib.template);
                validItems.ToList().ForEach(attr => { points += attr.flatBonus; points += (int)Math.Round(attr.percentBonus * _points); });
            }
        }

        // -- Skill Bonus (Passives)
        foreach (Skill skill in player.skills.skills)
        {
            if (skill.level > 0 && skill.data is PassiveSkill && ((PassiveSkill)skill.data).AttributeModifiers.Length > 0)
            {
                var validItems = ((PassiveSkill)skill.data).AttributeModifiers.ToList().Where(attr => attr.template == attrib.template);
                validItems.ToList().ForEach(attr => { points += attr.flatBonus; points += (int)Math.Round(attr.percentBonus * _points); });
            }
        }

        // -- Equipment Bonus
        foreach (ItemSlot slot in player.equipment.slots)
        {
            if (slot.amount > 0)
            {
                EquipmentItem item = (EquipmentItem)slot.item.data;
                if (item.AttributeModifiers.Length > 0)
                {
                    var validItems = ((EquipmentItem)slot.item.data).AttributeModifiers.ToList().Where(attr => attr.template == attrib.template);
                    validItems.ToList().ForEach(attr => { points += attr.flatBonus; points += (int)Math.Round(attr.percentBonus * _points); });
                }
#if _iMMOITEMLEVELUP
                if (slot.item.equipmentLevel > 0 && item.enableLevelUp && slot.item.equipmentLevel <= item.LevelUpParameters.Length)
                {
                    var validItems = item.LevelUpParameters[slot.item.equipmentLevel - 1].equipmentLevelUpModifier.attribute.ToList().Where(attr => attr.template == attrib.template);
                    validItems.ToList().ForEach(attr => { points += attr.flatBonus; points += (int)Math.Round(attr.percentBonus * _points); });
                }
#endif
#if _iMMOEQUIPMENTSETS
                // -- Equipment Bonus (Set Bonus)  AttributeIndividual
                points += slot.item.setBonusAttributeIndividual(slot, player.equipment.slots, attrib);

                // -- Equipment Bonus (Set Bonus) AttributePartial
                int tmpPointsP = slot.item.setBonusAttributePartial(slot, player.equipment.slots, attrib);
                points += tmpPointsP;

                // -- Equipment Bonus (Set Bonus) AttributeComplete
                int tmpPointsC = slot.item.setBonusAttributeComplete(slot, player.equipment.slots, attrib);
                points += tmpPointsC;
#endif

            }
        }


#if _iMMOTRAITS

        if (player.playerTraits && player.playerTraits.Traits.Count > 0)
            // -- Trait Bonus
            foreach (Trait trait in player.playerTraits.Traits)
            {
                foreach (Tools_AttributeModifier modifier in trait.data.statModifiers.AttributeModifiers)
                {
                    if (modifier.template == attrib.template)
                    {
                        points += modifier.flatBonus;
                        points += (int)Math.Round(modifier.percentBonus * _points);
                    }
                }
            }
#endif


#if _iMMOTITLES
        // -- Trait Bonus
        if (player.playerTitles.currentTitle != "")
        {
            int idx = player.playerTitles.EarnedTitles.FindIndex(x => x.title == player.playerTitles.currentTitle);
            //Debug.Log("attribute>>" + idx + ">> " + playerTitles.currentTitle);
            foreach (Tmpl_Titles title in player.playerTitles.titlesConfiguration.titles)
            {
                if (title.name == player.playerTitles.currentTitle)
                {
                    foreach (Tools_AttributeModifier modifier in title.AttributeModifiers)
                    {
                        if (modifier.template == attrib.template)
                        {
                            points += modifier.flatBonus;
                            points += (int)Math.Round(modifier.percentBonus * _points);
                        }
                    }

                }
                //Debug.Log(title.displayName);
            }
        }
#endif

        // ----------------------------- Calculation End ---------------------------------

        int returnValue = points;

        // Mettre à jour le cache pour la fonction CalcHealthMax avec la nouvelle valeur
        SetCachedValue("calculateBonusAttribute_" + attrib.name, returnValue);
        //UnityEngine.Debug.Log("calculateBonusAttribute_" + attrib.name + " no cache :" + returnValue);

        return returnValue;
    }

    // ============================= CALCULATION FUNCTIONS ===============================
    #region Calc Factor

#if _iMMOSTAMINA
     // -----------------------------------------------------------------------------------
    // CalcStaminaMax
    // -----------------------------------------------------------------------------------
    public int CalcStaminaMax()
    {
        /*AttributeCache cachedHealthMax = GetCachedValue("CalcStaminaMax");
        if (cachedHealthMax != null && cachedHealthMax.timer >= Time.time)
        {
            UnityEngine.Debug.Log("CalcStaminaMax by cache :" + cachedHealthMax.value);
            return cachedHealthMax.value;
        }
        UnityEngine.Debug.Log("<color:red>calllllc</<color>");
        */
        int returnValue = (int)Mathf.Round(CalculateFactor(calculateBonusAttribute, attrib => attrib.flatStamina, attrib => attrib.percentStamina, level => stamina.baseStamina.Get(level)));

        // Mettre à jour le cache pour la fonction CalcHealthMax avec la nouvelle valeur
        //SetCachedValue("CalcStaminaMax", returnValue);
        return returnValue;
    }

#endif

    // -----------------------------------------------------------------------------------
    // CalcDamage
    // -----------------------------------------------------------------------------------
    public int CalcDamage()
    {
        return (int)Mathf.Round(CalculateFactor(calculateBonusAttribute, attrib => attrib.flatDamage, attrib => attrib.percentDamage, level => player.combat.baseDamage.Get(level)));
    }

    // -----------------------------------------------------------------------------------
    // CalcDefense
    // -----------------------------------------------------------------------------------
    public int CalcDefense()
    {
        return (int)Mathf.Round(CalculateFactor(calculateBonusAttribute, attrib => attrib.flatDefense, attrib => attrib.percentDefense, level => player.combat.baseDefense.Get(level)));
    }

    // -----------------------------------------------------------------------------------
    // CalcBlock
    // -----------------------------------------------------------------------------------
    public float CalcBlock()
    {
        return CalculateFactor(calculateBonusAttribute, attrib => attrib.flatBlock, attrib => attrib.percentBlock, level => player.combat.baseBlockChance.Get(level));
    }

    // -----------------------------------------------------------------------------------
    // CalcCritical
    // -----------------------------------------------------------------------------------
    public float CalcCritical()
    {
        return CalculateFactor(calculateBonusAttribute, attrib => attrib.flatCritical, attrib => attrib.percentCritical, level => player.combat.baseCriticalChance.Get(level));
    }

    // -----------------------------------------------------------------------------------
    // CalcBlockFactor
    // -----------------------------------------------------------------------------------
    public float CalcBlockFactor()
    {
        return CalculateFactor(calculateBonusAttribute, attrib => attrib.flatBlockFactor, attrib => attrib.percentBlockFactor, level => player.combat.extraStats.blockFactor.Get(level));
    }

    // -----------------------------------------------------------------------------------
    // CalcCriticalFactor
    // -----------------------------------------------------------------------------------
    public float CalcCriticalFactor()
    {
        return CalculateFactor(calculateBonusAttribute, attrib => attrib.flatCriticalFactor, attrib => attrib.percentCriticalFactor, level => player.combat.extraStats.criticalFactor.Get(level));
    }

    // -----------------------------------------------------------------------------------
    // CalcAccuracy
    // -----------------------------------------------------------------------------------
    public float CalcAccuracy()
    {
        return CalculateFactor(calculateBonusAttribute, attrib => attrib.flatAccuracy, attrib => attrib.percentAccuracy, level => player.combat.extraStats.accuracy.Get(level));
    }

    // -----------------------------------------------------------------------------------
    // CalcResistance
    // -----------------------------------------------------------------------------------
    public float CalcResistance()
    {
        return CalculateFactor(calculateBonusAttribute, attrib => attrib.flatResistance, attrib => attrib.percentResistance, level => player.combat.extraStats.resistance.Get(level));
    }

    // -----------------------------------------------------------------------------------
    // CalcDrainHealthFactor
    // -----------------------------------------------------------------------------------
    public float CalcDrainHealthFactor()
    {
        return CalculateFactor(calculateBonusAttribute, attrib => attrib.flatDrainHealthFactor, attrib => attrib.percentDrainHealthFactor, level => player.combat.extraStats.drainHealthFactor.Get(level));
    }

    // -----------------------------------------------------------------------------------
    // CalcDrainManaFactor
    // -----------------------------------------------------------------------------------
    public float CalcDrainManaFactor()
    {
        return CalculateFactor(calculateBonusAttribute, attrib => attrib.flatDrainManaFactor, attrib => attrib.percentDrainManaFactor, level => player.combat.extraStats.drainManaFactor.Get(level));
    }

    // -----------------------------------------------------------------------------------
    // CalcReflectDamageFactor
    // -----------------------------------------------------------------------------------
    public float CalcReflectDamageFactor()
    {
        return CalculateFactor(calculateBonusAttribute, attrib => attrib.flatReflectDamageFactor, attrib => attrib.percentReflectDamageFactor, level => player.combat.extraStats.reflectDamageFactor.Get(level));
    }

    // -----------------------------------------------------------------------------------
    // CalcDefenseBreakFactor
    // -----------------------------------------------------------------------------------

    public float CalcDefenseBreakFactor()
    {
        return CalculateFactor(calculateBonusAttribute, attrib => attrib.flatDefenseBreakFactor, attrib => attrib.percentDefenseBreakFactor, level => player.combat.extraStats.defenseBreakFactor.Get(level));
    }

    // -----------------------------------------------------------------------------------
    // CalcBlockBreakFactor
    // -----------------------------------------------------------------------------------
    public float CalcBlockBreakFactor()
    {
        return CalculateFactor(calculateBonusAttribute, attrib => attrib.flatBlockBreakFactor, attrib => attrib.percentBlockBreakFactor, level => player.combat.extraStats.blockBreakFactor.Get(level));
    }

    // -----------------------------------------------------------------------------------
    // CalcCriticalEvasion
    // -----------------------------------------------------------------------------------
    public float CalcCriticalEvasion()
    {
        return CalculateFactor(calculateBonusAttribute, attrib => attrib.flatCriticalEvasion, attrib => attrib.percentCriticalEvasion, level => player.combat.extraStats.criticalEvasion.Get(level));
    }

    // -----------------------------------------------------------------------------------
    // CalcAbsorbHealthFactor
    // -----------------------------------------------------------------------------------
    public float CalcAbsorbHealthFactor()
    {
        return CalculateFactor(calculateBonusAttribute, attrib => attrib.flatAbsorbHealthFactor, attrib => attrib.percentAbsorbHealthFactor, level => player.combat.extraStats.absorbHealthFactor.Get(level));
    }

    // -----------------------------------------------------------------------------------
    // CalcAbsorbManaFactor
    // -----------------------------------------------------------------------------------
    public float CalcAbsorbManaFactor()
    {
        return CalculateFactor(calculateBonusAttribute, attrib => attrib.flatAbsorbManaFactor, attrib => attrib.percentAbsorbManaFactor, level => player.combat.extraStats.absorbManaFactor.Get(level));
    }


    /// <summary>
    /// Calculates the factor based on various attributes.
    /// </summary>
    /// <param name="calculateBonusAttribute">Delegate function to calculate bonus attribute points.</param>
    /// <param name="getFlatFactor">Delegate function to get the flat factor for an attribute.</param>
    /// <param name="getPercentFactor">Delegate function to get the percent factor for an attribute.</param>
    /// <param name="getBaseFactor">Delegate function to get the base factor based on player level.</param>
    /// <returns>The calculated factor.</returns>
    public float CalculateFactor(Func<TrugordAttribute, int> calculateBonusAttribute, Func<TrugordAttribute, float> getFlatFactor, Func<TrugordAttribute, float> getPercentFactor, Func<int, float> getBaseFactor)
    {
        float total = 0f;

        // Calculate bonuses only if there are attributes
        if (Attributes.Count > 0)
        {
            float flatBonus = 0f;
            float pctBonus = 0f;

            // Iterate through each attribute and calculate bonuses
            foreach (TrugordAttribute attrib in Attributes)
            {
                int points = calculateBonusAttribute(attrib) + attrib.points;

                // Calculate flat and percent bonuses for the attribute
                flatBonus += getFlatFactor(attrib) * points;
                pctBonus += getPercentFactor(attrib) * points;
            }

            // Add flat and percent bonuses to the total factor
            total += flatBonus;
            total += getBaseFactor(player.level.current) * pctBonus;
        }

        return total;
    }

    #endregion Calc Factor
    // =============================== OTHER FUNCTIONS ===================================

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public int AttributesSpendable()
    {
        int pointsSpent = (from _Attribute in Attributes select _Attribute.points).Sum();

        int totalPoints = 0;

        //prevent divide by zero error
        if (playerAttributes.everyXLevels > 0)
        {
            //adjust for starting reward level
            totalPoints = player.level.current - (playerAttributes.startingRewardLevel - 1);
            //divide so we get points only every x levels
            totalPoints = Mathf.CeilToInt((float)totalPoints / (float)playerAttributes.everyXLevels);
            //adjust if less than zero and multiply by the number of points per level
            totalPoints = Mathf.Max(totalPoints, 0) * playerAttributes.rewardPoints;
            //add starting points
            totalPoints += playerAttributes.startingAttributePoints;
        }

        //final available points is total the client should have so far minus the number they have spent
        return totalPoints - pointsSpent;
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_IncreaseAttribute(int index)
    {
        // validate.
        // If we have health and we have greater than zero spendable points and we can see the attribute passed over, increment it
        if (player.isAlive && AttributesSpendable() > 0 && 0 <= index && index < Attributes.Count())
        {
            TrugordAttribute attr = Attributes[index];
            attr.points += 1;
            Attributes[index] = attr;
        }
    }

#else
    public int GetHealthBonus(int baseHealth)
    {
        return 0;
    }
    public int GetHealthRecoveryBonus() => 0;
    public int CalcManaMax()
    {
        return 0;
    }
    public int GetManaBonus(int mana) =>
            Convert.ToInt32(CalcManaMax());

    public int GetManaRecoveryBonus() => 0;
#endif

    // -----------------------------------------------------------------------------------
}
