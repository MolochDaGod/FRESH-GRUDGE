using UnityEngine;
using System;
using Mirror;
using System.Linq;

// 
public partial class Combat
{
#if _iMMOATTRIBUTES

    public float randomDamageDeviation = 0.25f;
    public bool relationalDamage = true;

    [Header("[-=-[ Extra Stats ]-=-]")]
    public BaseExtraStats extraStats;

    [Server]
    public virtual void DealDamageAt(Entity victim, int amount, float stunChance = 0, float stunTime = 0)
    {

        Combat victimCombat = victim.combat;
        int damageDealt = 0;
        DamageType damageType = DamageType.Normal;

        //if (victim && !victim.invincible && victim.isAlive && (amount > 0 || stunChance > 0 || stunTime > 0))
        if (!victimCombat.invincible && victim.isAlive && (amount > 0 || stunChance > 0 || stunTime > 0))
        {
            victim.lastAggressor = entity;

            int target_Defense = victimCombat.defense - Convert.ToInt32(victimCombat.defense * entity.combat.defenseBreakFactor);
            float target_BlockChance = victimCombat.blockChance - Convert.ToInt32(victimCombat.blockChance * blockBreakFactor);
            //float self_critChance = victimCombat.criticalChance - Convert.ToInt32(victimCombat.criticalChance * victimCombat.criticalEvasion);
            float self_critChance = entity.combat.criticalChance - Convert.ToInt32(entity.combat.criticalChance * victimCombat.criticalEvasion);

            // -- Base Damage
            if (relationalDamage)
                damageDealt = Convert.ToInt32((amount * (100 - Mathf.Sqrt(target_Defense)) / 100));
            else
                damageDealt = Mathf.Max(amount - target_Defense, 1);

            // -- Elemental Modifiers
            ElementTemplate element = null;

#if _iMMOELEMENTS
            if (damageDealt > 0)
            {
                if (entity.skills.currentSkill != -1 && entity.skills.skills[entity.skills.currentSkill].data is DamageSkill)
                    element = ((DamageSkill)entity.skills.skills[entity.skills.currentSkill].data).element;

#if _iMMOCOMPLETESKILLS
                if (entity.skills.currentSkill != -1 && entity.skills.skills[entity.skills.currentSkill].data is DamageSkill)
                    element = ((DamageSkill)entity.skills.skills[entity.skills.currentSkill].data).element;
#endif

                if (element == null && entity is Player player)
                {
                    Player e = player;
                    element = e.GetAttackElement();
                }
            }
#endif

            // -- Custom Hook for Modifiers
            if (damageDealt > 0)
            {
                MutableWrapper<int> damageBonus = new MutableWrapper<int>(0);
                Utils.InvokeMany(typeof(Combat), this, "OnDealDamage_", victim, element, damageDealt, damageBonus);

                damageDealt += damageBonus.Value;
            }

            // -- Randomized Variance
            if (damageDealt > 0 && randomDamageDeviation != 0)
            {
                int minDamage = (int)UnityEngine.Random.Range((int)damageDealt * (1 - randomDamageDeviation), damageDealt);
                int maxDamage = (int)UnityEngine.Random.Range(damageDealt, (int)damageDealt * (1 + randomDamageDeviation));
                damageDealt = UnityEngine.Random.Range(minDamage, maxDamage);
            }

            // -- Block
            if (UnityEngine.Random.value < target_BlockChance)
            {
                damageDealt -= Convert.ToInt32(damageDealt * blockFactor);
                damageType = DamageType.Block;
            }
            else
            {
                // -- Crit
                if (UnityEngine.Random.value < self_critChance)
                {
                    damageDealt = Convert.ToInt32(damageDealt * criticalFactor);
                    damageType = DamageType.Crit;
                }

                // -- Deal Damage
                victim.health.current -= damageDealt;

                // call OnServerReceivedDamage event on the target
                // -> can be used for monsters to pull aggro
                // -> can be used by equipment to decrease durability etc.
                victimCombat.onServerReceivedDamage.Invoke(entity, damageDealt);

                // -- Check Stun
                if (UnityEngine.Random.value <= stunChance)
                {
                    double newStunEndTime = NetworkTime.time + stunTime;
                    victim.stunTimeEnd = Math.Max(newStunEndTime, entity.stunTimeEnd);
                }
            }

            //Utils.InvokeMany(typeof(Combat), this, "DealDamageAt_", victim, amount, damageDealt, damageType);
            //Utils.InvokeMany(typeof(Combat), this, "DealDamageAt_", victim, damageDealt, stunChance, damageType);
            DealDamageAt_Tools_Attributes(victim, damageDealt, stunChance, damageType);
            // call OnDamageDealtTo / OnKilledEnemy events
            onDamageDealtTo.Invoke(victim);

            if (victim.health.current == 0)
                onKilledEnemy.Invoke(victim);
        }
        victim.OnAggro(entity);

        // show effects on clients
        victimCombat.RpcOnReceivedDamaged(damageDealt, damageType);

        // reset last combat TimeLogout for both
        entity.lastCombatTime = NetworkTime.time;
        victim.lastCombatTime = NetworkTime.time;
        
        victim.lastAggressor = entity;   
    }

    // ============================== ATTRIBUTE GETTERS ==================================



    // -----------------------------------------------------------------------------------
    // damage
    // -----------------------------------------------------------------------------------
    //    public override int damage
    public int damage
    {
        get
        {
            // -- Bonus: Equipment
            int equipmentBonus = 0;
            foreach (ICombatBonus bonusComponent in bonusComponents)
                equipmentBonus += bonusComponent.GetDamageBonus();

            
           
            // -- Bonus: Traits
            int iTraitBonus = 0;
#if _iMMOTRAITS
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                iTraitBonus = Aplayer.playerTraits.Traits.Sum(trait => trait.damageBonus);
            }
#endif

            // -- Bonus: Equipment Sets
            int iSetBonus = 0;
#if _iMMOEQUIPMENTSETS
            if (entity is Player)
            {
                iSetBonus += Convert.ToInt32((from slot in entity.equipment.slots
                                              where slot.amount > 0 && slot.item.HasIndividualSetBonus()
                                              select (slot.item).setIndividualBonusDamage(entity.equipment.slots)).DefaultIfEmpty(0).Sum());
                iSetBonus += Convert.ToInt32((from slot in entity.equipment.slots
                                              where slot.amount > 0 && slot.item.HasPartialSetBonus()
                                              select (slot.item).setPartialBonusDamage(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault());
                iSetBonus += Convert.ToInt32((from slot in entity.equipment.slots
                                              where slot.amount > 0 && slot.item.HasCompleteSetBonus()
                                              select (slot.item).setCompleteBonusDamage(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault());
            }
#endif

            // -- Bonus: Attributes
            int attrBonus = 0;
            if (entity is Player)
            {
                Player Aplayer = (Player)entity; 
                attrBonus = Aplayer.playerAttribute.CalcDamage();
            }

            return Mathf.Max(0, baseDamage.Get(level.current) + equipmentBonus + iTraitBonus + iSetBonus + attrBonus);
        }
    }

    // -----------------------------------------------------------------------------------
    // defense
    // -----------------------------------------------------------------------------------
    //public override int defense
    public int defense
    {
        get
        {

            // -- Bonus: Equipment
            int equipmentBonus = 0;
            foreach (ICombatBonus bonusComponent in bonusComponents)
                equipmentBonus += bonusComponent.GetDefenseBonus();

            // -- Bonus: Traits
            int iTraitBonus = 0;
#if _iMMOTRAITS
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                iTraitBonus = Aplayer.playerTraits.Traits.Sum(trait => trait.defenseBonus);
            }
#endif

            // -- Bonus: Equipment Sets
            int iSetBonus = 0;
#if _iMMOEQUIPMENTSETS
            if (entity is Player)
            {
                iSetBonus += Convert.ToInt32((from slot in entity.equipment.slots
                                              where slot.amount > 0 && slot.item.HasIndividualSetBonus()
                                              select (slot.item).setIndividualBonusDefense(entity.equipment.slots)).DefaultIfEmpty(0).Sum());
                iSetBonus += Convert.ToInt32((from slot in entity.equipment.slots
                                              where slot.amount > 0 && slot.item.HasPartialSetBonus()
                                              select (slot.item).setPartialBonusDefense(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault());
                iSetBonus += Convert.ToInt32((from slot in entity.equipment.slots
                                              where slot.amount > 0 && slot.item.HasCompleteSetBonus()
                                              select (slot.item).setCompleteBonusDefense(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault());
            }
#endif

            int attrBonus = 0;
            // -- Bonus: Attributes
            if(entity is Player)
            {
                Player Aplayer = (Player)entity;
                attrBonus = Aplayer.playerAttribute.CalcDefense();
            }
            
            return Mathf.Max(0,  baseDefense.Get(level.current) + equipmentBonus + iTraitBonus + iSetBonus + attrBonus);
        }
    }

    // -----------------------------------------------------------------------------------
    // blockChance
    // -----------------------------------------------------------------------------------
    //   public override float blockChance
    public float blockChance
    {
        get
        {
            // -- Bonus: Equipment
            float equipmentBonus = 0;
            foreach (ICombatBonus bonusComponent in bonusComponents)
                equipmentBonus += bonusComponent.GetBlockChanceBonus();
            
            // -- Bonus: Traits
            float fTraitBonus = 0f;
#if _iMMOTRAITS
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                fTraitBonus = Aplayer.playerTraits.Traits.Sum(trait => trait.blockChanceBonus);
            }
#endif

            // -- Bonus: Equipment Sets
            float fSetBonus = 0;
#if _iMMOEQUIPMENTSETS
            if (entity is Player)
            {
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasIndividualSetBonus()
                              select (slot.item).setIndividualBonusBlockChance(entity.equipment.slots)).DefaultIfEmpty(0).Sum();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasPartialSetBonus()
                              select (slot.item).setPartialBonusBlockChance(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasCompleteSetBonus()
                              select (slot.item).setCompleteBonusBlockChance(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
            }
#endif

            // -- Bonus: Attributes
            float attrBonus = 0;
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                attrBonus = Aplayer.playerAttribute.CalcBlock();
            }
            return Mathf.Max(0, baseBlockChance.Get(level.current) + +equipmentBonus + fTraitBonus + fSetBonus + attrBonus);
        }
    }

    // -----------------------------------------------------------------------------------
    // criticalChance
    // -----------------------------------------------------------------------------------
    //   public override float criticalChance
    public float criticalChance
    {
        get
        {
            // -- Bonus: Equipment
            float equipmentBonus = 0;
            foreach (ICombatBonus bonusComponent in bonusComponents)
                equipmentBonus += bonusComponent.GetCriticalChanceBonus();
           
            // -- Bonus: Traits
            float fTraitBonus = 0f;
#if _iMMOTRAITS
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                fTraitBonus = Aplayer.playerTraits.Traits.Sum(trait => trait.criticalChanceBonus);
            }
#endif

            // -- Bonus: Equipment Sets
            float fSetBonus = 0;
#if _iMMOEQUIPMENTSETS
            if (entity is Player)
            {
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasIndividualSetBonus()
                              select (slot.item).setIndividualBonusCriticalChance(entity.equipment.slots)).DefaultIfEmpty(0).Sum();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasPartialSetBonus()
                              select (slot.item).setPartialBonusCriticalChance(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasCompleteSetBonus()
                              select (slot.item).setCompleteBonusCriticalChance(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
            }
#endif

            // -- Bonus: Attributes
            float attrBonus = 0;
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                attrBonus = Aplayer.playerAttribute.CalcCritical();
            }
            return Mathf.Max(0, baseCriticalChance.Get(level.current) + equipmentBonus + fTraitBonus + fSetBonus + attrBonus);
        }
    }

    // ================================== EXTRA STATS ====================================

    // -----------------------------------------------------------------------------------
    // blockFactor
    // -----------------------------------------------------------------------------------
    public float blockFactor
    {
        get
        {
          
            // -- Bonus: Equipment
            float equipmentBonus = 0;
            if (entity is Player)
            {
                foreach (ItemSlot slot in entity.equipment.slots)
                    if (slot.amount > 0)
                    {
                        equipmentBonus += ((EquipmentItem)slot.item.data).extraStats.blockFactor;
#if _iMMOITEMLEVELUP
                        if (slot.item.equipmentLevel > 0)
                            equipmentBonus += ((EquipmentItem)slot.item.data).LevelUpParameters[slot.item.equipmentLevel - 1].equipmentLevelUpModifier.extraModifier.blockFactor;
#endif
                    }
            }
            // -- Bonus: Traits
            float fTraitBonus = 0f;
#if _iMMOTRAITS
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                fTraitBonus = Aplayer.playerTraits.Traits.Sum(trait => trait.bonusBlockFactor);
            }
#endif
#if _iMMOTITLES
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                foreach (Tmpl_Titles title in Aplayer.playerTitles.titlesConfiguration.titles)
                {
                    if (title.name == Aplayer.playerTitles.currentTitle)
                        equipmentBonus += title.bonusBlockFactor;
                }
            }
#endif
            // -- Bonus: Equipment Sets
            float fSetBonus = 0;
#if _iMMOEQUIPMENTSETS
            if (entity is Player)
            {
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasIndividualSetBonus()
                              select (slot.item).setIndividualBonusBlockFactor(entity.equipment.slots)).DefaultIfEmpty(0).Sum();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasPartialSetBonus()
                              select (slot.item).setPartialBonusBlockFactor(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasCompleteSetBonus()
                              select (slot.item).setCompleteBonusBlockFactor(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
            }
#endif

            // -- Bonus: Attributes
            float attrBonus = 0;
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                attrBonus = Aplayer.playerAttribute.CalcBlockFactor();
            }
            return Mathf.Max(0, extraStats.blockBreakFactor.Get(level.current) + equipmentBonus + fTraitBonus + fSetBonus + attrBonus);
        }
    }

    // -----------------------------------------------------------------------------------
    // criticalFactor
    // -----------------------------------------------------------------------------------
    public float criticalFactor
    {
        get
        {

            // -- Bonus: Equipment
            float equipmentBonus = 0;
            if (entity is Player)
            {
                foreach (ItemSlot slot in entity.equipment.slots)
                    if (slot.amount > 0)
                    {
                        equipmentBonus += ((EquipmentItem)slot.item.data).extraStats.criticalFactor;
#if _iMMOITEMLEVELUP
                        if (slot.item.equipmentLevel > 0)
                            equipmentBonus += ((EquipmentItem)slot.item.data).LevelUpParameters[slot.item.equipmentLevel - 1].equipmentLevelUpModifier.extraModifier.criticalFactor;
#endif
                    }
            }
            // -- Bonus: Traits
            float fTraitBonus = 0f;
#if _iMMOTRAITS
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                fTraitBonus = Aplayer.playerTraits.Traits.Sum(trait => trait.bonusCriticalFactor);
            }
#endif
#if _iMMOTITLES
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                foreach (Tmpl_Titles title in Aplayer.playerTitles.titlesConfiguration.titles)
                {
                    if (title.name == Aplayer.playerTitles.currentTitle)
                        equipmentBonus += title.bonusCriticalFactor;
                }
            }
#endif
            // -- Bonus: Equipment Sets
            float fSetBonus = 0;
#if _iMMOEQUIPMENTSETS
            if (entity is Player)
            {
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasIndividualSetBonus()
                              select (slot.item).setIndividualBonusCriticalFactor(entity.equipment.slots)).DefaultIfEmpty(0).Sum();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasPartialSetBonus()
                              select (slot.item).setPartialBonusCriticalFactor(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasCompleteSetBonus()
                              select (slot.item).setCompleteBonusCriticalFactor(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
            }
#endif

            // -- Bonus: Attributes
            float attrBonus = 0;
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                attrBonus = Aplayer.playerAttribute.CalcCriticalFactor();
            }
            return Mathf.Max(0, extraStats.criticalFactor.Get(level.current) + equipmentBonus + fTraitBonus + fSetBonus + attrBonus);
        }
    }

    // -----------------------------------------------------------------------------------
    // accuracy
    // -----------------------------------------------------------------------------------
    public float accuracy
    {
        get
        {

            // -- Bonus: Equipment
            float equipmentBonus = 0;
            if (entity is Player)
            {
                foreach (ItemSlot slot in entity.equipment.slots)
                    if (slot.amount > 0)
                    {
                        equipmentBonus += ((EquipmentItem)slot.item.data).extraStats.accuracy;
#if _iMMOITEMLEVELUP
                        if (slot.item.equipmentLevel > 0)
                            equipmentBonus += ((EquipmentItem)slot.item.data).LevelUpParameters[slot.item.equipmentLevel - 1].equipmentLevelUpModifier.extraModifier.accuracy;
#endif
                    }
            }
            // -- Bonus: Traits
            float fTraitBonus = 0f;
#if _iMMOTRAITS
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                fTraitBonus = Aplayer.playerTraits.Traits.Sum(trait => trait.bonusAccuracy);
            }
#endif
#if _iMMOTITLES
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                foreach (Tmpl_Titles title in Aplayer.playerTitles.titlesConfiguration.titles)
                {
                    if (title.name == Aplayer.playerTitles.currentTitle)
                        equipmentBonus += title.bonusAccuracy;
                }
            }
#endif
            // -- Bonus: Equipment Sets
            float fSetBonus = 0;
#if _iMMOEQUIPMENTSETS
            if (entity is Player)
            {
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasIndividualSetBonus()
                              select (slot.item).setIndividualBonusAccuracy(entity.equipment.slots)).DefaultIfEmpty(0).Sum();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasPartialSetBonus()
                              select (slot.item).setPartialBonusAccuracy(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasCompleteSetBonus()
                              select (slot.item).setCompleteBonusAccuracy(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
            }
#endif

            // -- Bonus: Attributes
            float attrBonus = 0;
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                attrBonus = Aplayer.playerAttribute.CalcAccuracy();
            }
            return extraStats.accuracy.Get(level.current) + equipmentBonus + fTraitBonus + fSetBonus + attrBonus;
        }
    }

    // -----------------------------------------------------------------------------------
    // resistance
    // -----------------------------------------------------------------------------------
    public float resistance
    {
        get
        {
           
            // -- Bonus: Equipment
            float equipmentBonus = 0;
            if (entity is Player)
            {
                foreach (ItemSlot slot in entity.equipment.slots)
                    if (slot.amount > 0)
                    {
                        equipmentBonus += ((EquipmentItem)slot.item.data).extraStats.resistance;
#if _iMMOITEMLEVELUP
                        if (slot.item.equipmentLevel > 0)
                            equipmentBonus += ((EquipmentItem)slot.item.data).LevelUpParameters[slot.item.equipmentLevel - 1].equipmentLevelUpModifier.extraModifier.resistance;
#endif
                    }
            }
            // -- Bonus: Traits
            float fTraitBonus = 0f;
#if _iMMOTRAITS
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                fTraitBonus = Aplayer.playerTraits.Traits.Sum(trait => trait.bonusResistance);
            }
#endif
#if _iMMOTITLES
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                foreach (Tmpl_Titles title in Aplayer.playerTitles.titlesConfiguration.titles)
                {
                    if (title.name == Aplayer.playerTitles.currentTitle)
                        equipmentBonus += title.bonusResistance;
                }
            }
#endif
            // -- Bonus: Equipment Sets
            float fSetBonus = 0;
#if _iMMOEQUIPMENTSETS
            if (entity is Player)
            {
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasIndividualSetBonus()
                              select (slot.item).setIndividualBonusResistance(entity.equipment.slots)).DefaultIfEmpty(0).Sum();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasPartialSetBonus()
                              select (slot.item).setPartialBonusResistance(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasCompleteSetBonus()
                              select (slot.item).setCompleteBonusResistance(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
            }
#endif

            // -- Bonus: Attributes
            float attrBonus = 0;
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                attrBonus = Aplayer.playerAttribute.CalcResistance();
            }
            return extraStats.resistance.Get(level.current) + equipmentBonus + fTraitBonus + fSetBonus + attrBonus;
        }
    }

    // -----------------------------------------------------------------------------------
    // drainHealthFactor
    // -----------------------------------------------------------------------------------
    public float drainHealthFactor
    {
        get
        {

            // -- Bonus: Equipment
            float equipmentBonus = 0;
            if (entity is Player)
            {
                foreach (ItemSlot slot in entity.equipment.slots)
                    if (slot.amount > 0)
                    {
                        equipmentBonus += ((EquipmentItem)slot.item.data).extraStats.drainHealthFactor;
#if _iMMOITEMLEVELUP
                        if (slot.item.equipmentLevel > 0)
                            equipmentBonus += ((EquipmentItem)slot.item.data).LevelUpParameters[slot.item.equipmentLevel - 1].equipmentLevelUpModifier.extraModifier.drainHealthFactor;
#endif
                    }
            }
            // -- Bonus: Traits
            float fTraitBonus = 0f;
#if _iMMOTRAITS
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                fTraitBonus = Aplayer.playerTraits.Traits.Sum(trait => trait.bonusDrainHealthFactor);
            }
#endif
#if _iMMOTITLES
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                foreach (Tmpl_Titles title in Aplayer.playerTitles.titlesConfiguration.titles)
                {
                    if (title.name == Aplayer.playerTitles.currentTitle)
                        equipmentBonus += title.bonusDrainHealthFactor;
                }
            }
#endif
            // -- Bonus: Equipment Sets
            float fSetBonus = 0;
#if _iMMOEQUIPMENTSETS
            if (entity is Player)
            {
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasIndividualSetBonus()
                              select (slot.item).setIndividualBonusDrainHealthFactor(entity.equipment.slots)).DefaultIfEmpty(0).Sum();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasPartialSetBonus()
                              select (slot.item).setPartialBonusDrainHealthFactor(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasCompleteSetBonus()
                              select (slot.item).setCompleteBonusDrainHealthFactor(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
            }
#endif

            // -- Bonus: Attributes
            float attrBonus = 0;
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                attrBonus = Aplayer.playerAttribute.CalcDrainHealthFactor();
            }
            return extraStats.drainHealthFactor.Get(level.current) + equipmentBonus + fTraitBonus + fSetBonus + attrBonus;
        }
    }

    // -----------------------------------------------------------------------------------
    // drainManaFactor
    // -----------------------------------------------------------------------------------
    public float drainManaFactor
    {
        get
        {
            // -- Bonus: Equipment
            float equipmentBonus = 0;
            if (entity is Player)
            {
                foreach (ItemSlot slot in entity.equipment.slots)
                    if (slot.amount > 0)
                    {
                        equipmentBonus += ((EquipmentItem)slot.item.data).extraStats.drainManaFactor;
#if _iMMOITEMLEVELUP
                        if (slot.item.equipmentLevel > 0)
                            equipmentBonus += ((EquipmentItem)slot.item.data).LevelUpParameters[slot.item.equipmentLevel - 1].equipmentLevelUpModifier.extraModifier.drainManaFactor;
#endif
                    }
            }
            // -- Bonus: Traits
            float fTraitBonus = 0f;
#if _iMMOTRAITS
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                fTraitBonus = Aplayer.playerTraits.Traits.Sum(trait => trait.bonusDrainManaFactor);
            }
#endif
#if _iMMOTITLES
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                foreach (Tmpl_Titles title in Aplayer.playerTitles.titlesConfiguration.titles)
                {
                    if (title.name == Aplayer.playerTitles.currentTitle)
                        equipmentBonus += title.bonusDrainManaFactor;
                }
            }
#endif
            // -- Bonus: Equipment Sets
            float fSetBonus = 0;
#if _iMMOEQUIPMENTSETS
            if (entity is Player)
            {
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasIndividualSetBonus()
                              select (slot.item).setIndividualBonusDrainManaFactor(entity.equipment.slots)).DefaultIfEmpty(0).Sum();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasPartialSetBonus()
                              select (slot.item).setPartialBonusDrainManaFactor(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasCompleteSetBonus()
                              select (slot.item).setCompleteBonusDrainManaFactor(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
            }
#endif

            // -- Bonus: Attributes
            float attrBonus = 0;
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                attrBonus = Aplayer.playerAttribute.CalcDrainManaFactor();
            }
            return extraStats.drainManaFactor.Get(level.current) + equipmentBonus + fTraitBonus + fSetBonus + attrBonus;
        }
    }

    // -----------------------------------------------------------------------------------
    // reflectDamageFactor
    // -----------------------------------------------------------------------------------
    public float reflectDamageFactor
    {
        get
        {
            // -- Bonus: Equipment
            float equipmentBonus = 0;
            if (entity is Player)
            {
                foreach (ItemSlot slot in entity.equipment.slots)
                    if (slot.amount > 0)
                    {
                        equipmentBonus += ((EquipmentItem)slot.item.data).extraStats.reflectDamageFactor;
#if _iMMOITEMLEVELUP
                        if (slot.item.equipmentLevel > 0)
                            equipmentBonus += ((EquipmentItem)slot.item.data).LevelUpParameters[slot.item.equipmentLevel - 1].equipmentLevelUpModifier.extraModifier.reflectDamageFactor;
#endif
                    }
            }

            float buffBonus = 0;
            foreach (var item in entity.skills.buffs)
            {
                buffBonus += item.bonusReflectDamageFactor;
            }
            // -- Bonus: Traits
            float fTraitBonus = 0f;
#if _iMMOTRAITS
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                fTraitBonus = Aplayer.playerTraits.Traits.Sum(trait => trait.bonusReflectDamageFactor);
            }

#endif
#if _iMMOTITLES
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                foreach (Tmpl_Titles title in Aplayer.playerTitles.titlesConfiguration.titles)
                {
                    if (title.name == Aplayer.playerTitles.currentTitle)
                        equipmentBonus += title.bonusReflectDamageFactor;
                }
            }
#endif
            // -- Bonus: Equipment Sets
            float fSetBonus = 0;
#if _iMMOEQUIPMENTSETS
            if (entity is Player)
            {
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasIndividualSetBonus()
                              select (slot.item).setIndividualBonusReflectDamageFactor(entity.equipment.slots)).DefaultIfEmpty(0).Sum();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasPartialSetBonus()
                              select (slot.item).setPartialBonusReflectDamageFactor(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasCompleteSetBonus()
                              select (slot.item).setCompleteBonusReflectDamageFactor(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
            }
#endif

            // -- Bonus: Attributes
            float attrBonus = 0;
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                attrBonus = Aplayer.playerAttribute.CalcReflectDamageFactor();
            }
            return extraStats.reflectDamageFactor.Get(level.current) + equipmentBonus + buffBonus + fTraitBonus + fSetBonus + attrBonus;
        }
    }

    // -----------------------------------------------------------------------------------
    // defenseBreakFactor
    // -----------------------------------------------------------------------------------
    public float defenseBreakFactor
    {
        get
        {
            // -- Bonus: Equipment
            float equipmentBonus = 0;
            if (entity is Player)
            {
                foreach (ItemSlot slot in entity.equipment.slots)
                    if (slot.amount > 0)
                    {
                        equipmentBonus += ((EquipmentItem)slot.item.data).extraStats.defenseBreakFactor;
#if _iMMOITEMLEVELUP
                        if (slot.item.equipmentLevel > 0)
                            equipmentBonus += ((EquipmentItem)slot.item.data).LevelUpParameters[slot.item.equipmentLevel - 1].equipmentLevelUpModifier.extraModifier.defenseBreakFactor;
#endif
                    }
            }

            // -- Bonus: Traits
            float fTraitBonus = 0f;
#if _iMMOTRAITS
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                fTraitBonus = Aplayer.playerTraits.Traits.Sum(trait => trait.bonusDefenseBreakFactor);
            }
#endif
#if _iMMOTITLES
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                foreach (Tmpl_Titles title in Aplayer.playerTitles.titlesConfiguration.titles)
                {
                    if (title.name == Aplayer.playerTitles.currentTitle)
                        equipmentBonus += title.bonusDefenseBreakFactor;
                }
            }
#endif
            // -- Bonus: Equipment Sets
            float fSetBonus = 0;
#if _iMMOEQUIPMENTSETS
            if (entity is Player)
            {
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasIndividualSetBonus()
                              select (slot.item).setIndividualBonusDefenseBreakFactor(entity.equipment.slots)).DefaultIfEmpty(0).Sum();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasPartialSetBonus()
                              select (slot.item).setPartialBonusDefenseBreakFactor(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasCompleteSetBonus()
                              select (slot.item).setCompleteBonusDefenseBreakFactor(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
            }
#endif

            // -- Bonus: Attributes
            float attrBonus = 0;
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                attrBonus = Aplayer.playerAttribute.CalcDefenseBreakFactor();
            }
            return extraStats.defenseBreakFactor.Get(level.current) + equipmentBonus + fTraitBonus + fSetBonus + attrBonus;
        }
    }

    // -----------------------------------------------------------------------------------
    // blockBreakFactor
    // -----------------------------------------------------------------------------------
    public float blockBreakFactor
    {
        get
        {
           
            // -- Bonus: Equipment
            float equipmentBonus = 0;
            if (entity is Player)
            {
                foreach (ItemSlot slot in entity.equipment.slots)
                    if (slot.amount > 0)
                    {
                        equipmentBonus += ((EquipmentItem)slot.item.data).extraStats.blockBreakFactor;
#if _iMMOITEMLEVELUP
                        if (slot.item.equipmentLevel > 0)
                            equipmentBonus += ((EquipmentItem)slot.item.data).LevelUpParameters[slot.item.equipmentLevel - 1].equipmentLevelUpModifier.extraModifier.blockBreakFactor;
#endif
                    }
            }
            // -- Bonus: Traits
            float fTraitBonus = 0f;
#if _iMMOTRAITS
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                fTraitBonus = Aplayer.playerTraits.Traits.Sum(trait => trait.bonusBlockBreakFactor);
            }
#endif
#if _iMMOTITLES
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                foreach (Tmpl_Titles title in Aplayer.playerTitles.titlesConfiguration.titles)
                {
                    if (title.name == Aplayer.playerTitles.currentTitle)
                        equipmentBonus += title.bonusBlockBreakFactor;
                }
            }
#endif
            // -- Bonus: Equipment Sets
            float fSetBonus = 0;
#if _iMMOEQUIPMENTSETS
            if (entity is Player)
            {
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasIndividualSetBonus()
                              select (slot.item).setIndividualBonusBlockBreakFactor(entity.equipment.slots)).DefaultIfEmpty(0).Sum();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasPartialSetBonus()
                              select (slot.item).setPartialBonusBlockBreakFactor(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasCompleteSetBonus()
                              select (slot.item).setCompleteBonusBlockBreakFactor(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
            }
#endif

            // -- Bonus: Attributes
            float attrBonus = 0;
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                attrBonus = Aplayer.playerAttribute.CalcBlockBreakFactor();
            }
            return extraStats.blockBreakFactor.Get(level.current) + equipmentBonus + fTraitBonus + fSetBonus + attrBonus;
        }
    }

    // -----------------------------------------------------------------------------------
    // criticalEvasion
    // -----------------------------------------------------------------------------------
    public float criticalEvasion
    {
        get
        {
            
            // -- Bonus: Equipment
            float equipmentBonus = 0;
            if (entity is Player)
            {
                foreach (ItemSlot slot in entity.equipment.slots)
                    if (slot.amount > 0)
                    {
                        equipmentBonus += ((EquipmentItem)slot.item.data).extraStats.criticalEvasion;
#if _iMMOITEMLEVELUP
                        if (slot.item.equipmentLevel > 0)
                            equipmentBonus += ((EquipmentItem)slot.item.data).LevelUpParameters[slot.item.equipmentLevel - 1].equipmentLevelUpModifier.extraModifier.criticalEvasion;
#endif
                    }
            }
            // -- Bonus: Traits
            float fTraitBonus = 0f;
#if _iMMOTRAITS
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                fTraitBonus = Aplayer.playerTraits.Traits.Sum(trait => trait.bonusCriticalEvasion);
            }
#endif
#if _iMMOTITLES
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                foreach (Tmpl_Titles title in Aplayer.playerTitles.titlesConfiguration.titles)
                {
                    if (title.name == Aplayer.playerTitles.currentTitle)
                        equipmentBonus += title.bonusCriticalEvasion;
                }
            }
#endif
            // -- Bonus: Equipment Sets
            float fSetBonus = 0;
#if _iMMOEQUIPMENTSETS
            if (entity is Player)
            {
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasIndividualSetBonus()
                              select (slot.item).setIndividualBonusCriticalEvasion(entity.equipment.slots)).DefaultIfEmpty(0).Sum();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasPartialSetBonus()
                              select (slot.item).setPartialBonusCriticalEvasion(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasCompleteSetBonus()
                              select (slot.item).setCompleteBonusCriticalEvasion(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
            }
#endif

            // -- Bonus: Attributes
            float attrBonus = 0;
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                attrBonus = Aplayer.playerAttribute.CalcCriticalEvasion();
            }
            return extraStats.criticalEvasion.Get(level.current) + equipmentBonus + fTraitBonus + fSetBonus + attrBonus;
        }
    }

    // -----------------------------------------------------------------------------------
    // absorbHealthFactor
    // -----------------------------------------------------------------------------------
    public float absorbHealthFactor
    {
        get
        {

            // -- Bonus: Equipment
            float equipmentBonus = 0;
            if (entity is Player)
            {
                foreach (ItemSlot slot in entity.equipment.slots)
                    if (slot.amount > 0)
                    {
                        equipmentBonus += ((EquipmentItem)slot.item.data).extraStats.absorbHealthFactor;
#if _iMMOITEMLEVELUP
                        if (slot.item.equipmentLevel > 0)
                            equipmentBonus += ((EquipmentItem)slot.item.data).LevelUpParameters[slot.item.equipmentLevel - 1].equipmentLevelUpModifier.extraModifier.absorbHealthFactor;
#endif
                    }
            }

            // -- Bonus: Skills
            float buffBonus = 0;
            foreach (var item in entity.skills.buffs)
            {
                buffBonus += item.bonusAbsorbHealthFactor;
            }

            // -- Bonus: Traits
            float fTraitBonus = 0f;
#if _iMMOTRAITS
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                fTraitBonus = Aplayer.playerTraits.Traits.Sum(trait => trait.bonusAbsorbHealthFactor);
            }
#endif
            
#if _iMMOTITLES
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                foreach (Tmpl_Titles title in Aplayer.playerTitles.titlesConfiguration.titles)
                {
                    if (title.name == Aplayer.playerTitles.currentTitle)
                        buffBonus += title.bonusAbsorbHealthFactor;
                }
            }
#endif

            // -- Bonus: Equipment Sets
            float fSetBonus = 0;
#if _iMMOEQUIPMENTSETS
            if (entity is Player)
            {
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasIndividualSetBonus()
                              select (slot.item).setIndividualBonusAbsorbHealthFactor(entity.equipment.slots)).DefaultIfEmpty(0).Sum();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasPartialSetBonus()
                              select (slot.item).setPartialBonusAbsorbHealthFactor(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasCompleteSetBonus()
                              select (slot.item).setCompleteBonusAbsorbHealthFactor(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
            }
#endif

            // -- Bonus: Attributes
            float attrBonus = 0;
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                attrBonus = Aplayer.playerAttribute.CalcAbsorbHealthFactor();
            }
            return extraStats.absorbHealthFactor.Get(level.current) + equipmentBonus + buffBonus + fTraitBonus + fSetBonus + attrBonus;
        }
    }

    // -----------------------------------------------------------------------------------
    // absorbManaFactor
    // -----------------------------------------------------------------------------------
    public float absorbManaFactor
    {
        get
        {

            // -- Bonus: Equipment
            float equipmentBonus = 0;
            if (entity is Player)
            {
                foreach (ItemSlot slot in entity.equipment.slots)
                {
                    if (slot.amount > 0)
                    {
                        equipmentBonus += ((EquipmentItem)slot.item.data).extraStats.absorbManaFactor;
#if _iMMOITEMLEVELUP
                        if (slot.item.equipmentLevel > 0)
                            equipmentBonus += ((EquipmentItem)slot.item.data).LevelUpParameters[slot.item.equipmentLevel - 1].equipmentLevelUpModifier.extraModifier.absorbManaFactor;
#endif
                    }
                }
            }

            // -- Bonus: Skills
            float buffBonus = 0;
            foreach (var buff in entity.skills.buffs)
            {
                buffBonus += buff.bonusAbsorbManaFactor;
            }

            // -- Bonus: Traits
            float fTraitBonus = 0f;
#if _iMMOTRAITS
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                fTraitBonus = Aplayer.playerTraits.Traits.Sum(trait => trait.bonusAbsorbManaFactor);
            }
#endif
#if _iMMOTITLES
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                foreach (Tmpl_Titles title in Aplayer.playerTitles.titlesConfiguration.titles)
                {
                    if (title.name == Aplayer.playerTitles.currentTitle)
                        buffBonus += title.bonusAbsorbManaFactor;
                }
            }
#endif
            // -- Bonus: Equipment Sets
            float fSetBonus = 0;
#if _iMMOEQUIPMENTSETS
            if (entity is Player)
            {
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasIndividualSetBonus()
                              select (slot.item).setIndividualBonusAbsorbManaFactor(entity.equipment.slots)).DefaultIfEmpty(0).Sum();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasPartialSetBonus()
                              select (slot.item).setPartialBonusAbsorbManaFactor(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
                fSetBonus += (from slot in entity.equipment.slots
                              where slot.amount > 0 && slot.item.HasCompleteSetBonus()
                              select (slot.item).setCompleteBonusAbsorbManaFactor(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault();
            }
#endif

            // -- Bonus: Attributes
            float attrBonus = 0;
            if (entity is Player)
            {
                Player Aplayer = (Player)entity;
                attrBonus = Aplayer.playerAttribute.CalcAbsorbManaFactor();
            }
            return extraStats.absorbManaFactor.Get(level.current) + equipmentBonus + buffBonus + fTraitBonus + fSetBonus + attrBonus;
        }
    }

#if _iMMOSTAMINA
    // -----------------------------------------------------------------------------------
    // staminaMax
    // -----------------------------------------------------------------------------------
    //public override int staminaMax---------------------------------------------------
    public int staminaMax
    {
        get
        {
            
            //if (Time.TimeLogout < _cacheStatTimer[18])
            //    return _cacheStatInt[4];

            // -- Bonus: Equipment
            int equipmentBonus = 0; 
            int attrBonus = 0;
            foreach (ItemSlot slot in entity.equipment.slots)
                if (slot.amount > 0)
                {
                    equipmentBonus += ((EquipmentItem)slot.item.data).staminaBonus;
#if _iMMOITEMLEVELUP
                    /*if (slot.item.equipmentLevel > 0)
                        equipmentBonus += ((EquipmentItem)slot.item.data).LevelUpParameters[slot.item.equipmentLevel - 1].equipmentLevelUpModifier.extraModifier.staminaBonus;*/
#endif
                }

            // -- Bonus: Traits
            int iTraitBonus = 0;
#if _iMMOTRAITS
            if (entity is Player Aplayer)
            {
                iTraitBonus = Aplayer.playerTraits.Traits.Sum(trait => trait.staminaBonus);
            }
#endif
            // -- Bonus: Equipment Sets
            int iSetBonus = 0;
#if _iMMOEQUIPMENTSETS
            if (entity is Player)
            {
                iSetBonus += Convert.ToInt32((from slot in entity.equipment.slots
                                      where slot.amount > 0 && slot.item.HasIndividualSetBonus()
                                      select (slot.item).setIndividualBonusStamina(entity.equipment.slots)).DefaultIfEmpty(0).Sum());
                iSetBonus += Convert.ToInt32((from slot in entity.equipment.slots
                                      where slot.amount > 0 && slot.item.HasPartialSetBonus()
                                      select (slot.item).setPartialBonusStamina(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault());
                iSetBonus += Convert.ToInt32((from slot in entity.equipment.slots
                                      where slot.amount > 0 && slot.item.HasCompleteSetBonus()
                                      select (slot.item).setCompleteBonusStamina(entity.equipment.slots)).DefaultIfEmpty(0).FirstOrDefault());
            }
#endif
            // -- Bonus: Attributes
            if (entity is Player Bplayer)
            {
                attrBonus = Bplayer.playerAttribute.CalcStaminaMax();
            }
            //_cacheStatTimer[18] = Time.TimeLogout + cacheTimerInterval;
            
            //_cacheStatInt[4] = entity.stamina.max + equipmentBonus + iTraitBonus + iSetBonus + attrBonus;

            //return Mathf.Max(0, _cacheStatInt[4]);
            return Mathf.Max(0, entity.stamina.max + equipmentBonus + iTraitBonus + iSetBonus + attrBonus);

        }
    }
#endif

    // ============================= CALCULATION FUNCTIONS ===============================


#if _iMMOSTAMINA
     // -----------------------------------------------------------------------------------
    // CalcStaminaMax
    // -----------------------------------------------------------------------------------
    /*public int CalcStaminaMax()
    {
        int total = 0;

        if (Attributes.Count > 0)
        {
            int points = 0;
            int flatBonus = 0;
            float pctBonus = 0f;

            foreach (TrugordAttribute attrib in Attributes)
            {
                points = calculateBonusAttribute(attrib) + attrib.points;
                flatBonus += attrib.flatStamina * points;
                pctBonus += attrib.percentStamina * points;

            }

            total += flatBonus;
            total += (int)Mathf.Round(_staminaMax.Get(level) * pctBonus);
        }

        return total;
    }*/
#endif

    private void DealDamageAt_Tools_Attributes(Entity victim, int damageDealt, float stunChance, DamageType damageType)
    {
        //Debug.Log("DealDamageAt_Tools_Attributes call");
        if (victim == null || damageDealt <= 0 || !entity.isAlive || !victim.isAlive) return;

        // ---- Drain Health
        if (entity.combat.drainHealthFactor != 0)
        {
            int drainHealth = (int)(damageDealt * entity.combat.drainHealthFactor);
            entity.health.current += drainHealth;
            victim.combat.RpcOnReceivedHeal(drainHealth);
        }
        // ---- Drain Mana
        if (entity.combat.drainManaFactor != 0)
        {
            entity.mana.current += (int)(damageDealt * entity.combat.drainManaFactor);
        }
        // ---- Reflect Damage
        if (victim.combat.reflectDamageFactor != 0 && damageType != DamageType.Block)
        {
            int relfectDegat = (int)(damageDealt * victim.combat.reflectDamageFactor);
            entity.health.current -= relfectDegat;
            entity.combat.RpcOnReceivedDamaged(relfectDegat, damageType);
        }

        // ---- Absorb Health
        if (victim.combat.absorbHealthFactor != 0)
        {
            //Debug.Log("Absorb Health de :" + (damageDealt * victim.combat.absorbHealthFactor));
            int absorbHealth = (int)(damageDealt * victim.combat.absorbHealthFactor);
            victim.health.current += absorbHealth;
            victim.combat.RpcOnReceivedHeal(absorbHealth);
        }
        // ---- Absorb Mana
        if (victim.combat.absorbManaFactor != 0)
        {
            victim.mana.current += (int)(damageDealt * victim.combat.absorbManaFactor);
            //Debug.Log("DealDamageAt_Tools_Attributes called");
        }
    }

#else
    [Header("GO TO Tools/Congiguration/Define\r\nand click \"Edit addons list\"")]
    private int installAttribute;
#endif


}
