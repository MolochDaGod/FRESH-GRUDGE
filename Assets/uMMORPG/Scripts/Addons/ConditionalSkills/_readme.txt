Install

NOTE: This AddOn also requires "Attache" it wont work without

1. CORE CHANGES
1.1 In "MonsterSkills.cs" and in PetSkills.cs Search 

    public int NextSkill()
    {
        // find the next ready skill, starting at 'lastSkill+1' (= next one)
        // and looping at max once through them all (up to skill.Count)
        //  note: no skills.count == 0 check needed, this works with empty lists
        //  note: also works if lastSkill is still -1 from initialization
        for (int i = 0; i < skills.Count; ++i)
        {
            int index = (lastSkill + 1 + i) % skills.Count;
            // could we cast this skill right now? (enough mana, skill ready, etc.)
            if (CastCheckSelf(skills[index]))
                return index;
        }
        return -1;
    }

Replace to

#if !_iMMOCONDITIONALSKILLS
    public int NextSkill()
    {
        // find the next ready skill, starting at 'lastSkill+1' (= next one)
        // and looping at max once through them all (up to skill.Count)
        //  note: no skills.count == 0 check needed, this works with empty lists
        //  note: also works if lastSkill is still -1 from initialization
        for (int i = 0; i < skills.Count; ++i)
        {
            int index = (lastSkill + 1 + i) % skills.Count;
            // could we cast this skill right now? (enough mana, skill ready, etc.)
            if (CastCheckSelf(skills[index]))
                return index;
        }
        return -1;
    }
#endif

------------------------------------------------------
1.2 In PetSkills 
Search :

public class PetSkills : Skills

replace to 
public partial class PetSkills : Skills

--------------------------------------------------------



2. INSTALL
2.1 Each skill now features a new "Conditional Skill" section that you can edit in the inspector.
2.2 all settings applied there only affect the skill activation of of Pets, Monsters, Npc and Mounts.


/!\ Mount require another addons "Not published" for use Skill, and Npc Reuire "NpcExtended" no need Edit file