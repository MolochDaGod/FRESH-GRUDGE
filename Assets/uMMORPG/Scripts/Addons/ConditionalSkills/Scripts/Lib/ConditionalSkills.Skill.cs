public partial struct Skill
{
//#if _SERVER //TODO require modication for adapt all other file for SERVER only
    // -------------------------------------------------------------------------
    // CheckSkillConditions
    // -------------------------------------------------------------------------
    public bool CheckSkillConditions(Entity caster)
    {
       	bool bValid = CheckSelf(caster);
       
        float chance = UnityEngine.Random.Range(1, 100);
        float refChance = (data.skillConditions.activationChance * 100);
        if (chance < refChance)
        {
            if (data.skillConditions.healthThreshold == Entity.ParentThreshold.Above && (caster.health.current < caster.health.max * data.skillConditions.casterHealth))
                bValid = false;

            else if (data.skillConditions.healthThreshold == Entity.ParentThreshold.Below && (caster.health.current > caster.health.max * data.skillConditions.casterHealth))
                bValid = false;

#if _iMMOMORALE
            if (data.skillConditions.moraleThreshold == Entity.ParentThreshold.Above && (caster.morale < caster.moraleMax * data.skillConditions.casterMorale))
                bValid = false;

            else if (data.skillConditions.moraleThreshold == Entity.ParentThreshold.Below && (caster.morale > caster.moraleMax * data.skillConditions.casterMorale))
                bValid = false;
#endif

            if (data.skillConditions.activeBuff != null)
            {
                if (!caster.Tools_checkHasBuff(data.skillConditions.activeBuff))
                {
                    bValid = false;
                }
            }

        }
        else
			bValid = false;
        return bValid;
    }
//#endif
}