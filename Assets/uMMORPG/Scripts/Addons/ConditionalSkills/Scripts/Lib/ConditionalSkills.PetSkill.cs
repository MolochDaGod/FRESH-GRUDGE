#if _iMMOCONDITIONALSKILLS
using UnityEngine;
public partial class PetSkills
{
    // -------------------------------------------------------------------------
    // NextSkill
    // -------------------------------------------------------------------------

    public int NextSkill()
    {
        // find the next ready skill, starting at 'lastSkill+1' (= next one)
        // and looping at max once through them all (up to skill.Count)
        //  note: no skills.count == 0 check needed, this works with empty lists
        //  note: also works if lastSkill is still -1 from initialization
        Debug.Log("countSkill = " + skills.Count + " countBuffs = " + buffs.Count);
        for (int i = 0; i < skills.Count; ++i)
        {
            int index = (lastSkill + 1 + i) % skills.Count;

            if (CastCheckSelf(skills[index]) && skills[index].CheckSkillConditions(entity))
            {
                return index;
            }
        }
        return -1;
    }
}
#endif
