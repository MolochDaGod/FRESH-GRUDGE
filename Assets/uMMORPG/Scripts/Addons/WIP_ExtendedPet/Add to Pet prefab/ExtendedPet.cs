using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtendedPet : MonoBehaviour
{
    public Pet pet;
    public Health health;
    public PetSkills petSkills;
    public ScriptableSkill[] skillTemplates;
    public int buffLevelMax = 1;


    // Update is called once per frame
    void Update()
    {
        if (pet.owner.health.current < (pet.owner.health.max / 2))
        {
            if (skillTemplates.Length > 0)
            {
                for (int i = 0; i < skillTemplates.Length; ++i)
                {
                    bool isIn = false;
                    foreach (Buff buff in pet.owner.skills.buffs)
                    {
                        if (buff.name == skillTemplates[i].name)
                        {
                            isIn = true;
                        }
                        Debug.Log(" buff name > " + buff.name+ " est dedans : "+ ((isIn) ? "oui" : "non "));
                    }

                    if (!isIn)
                    {
                        AddBuff();
                    }
                }
                 
            }
        }
    }

    private void AddBuff()
    {

          
      for (int i = 0; i < skillTemplates.Length; ++i)
      {
            ScriptableSkill skillData;
            if (ScriptableSkill.All.TryGetValue(skillTemplates[i].name.GetStableHashCode(), out skillData)){

                int level = Mathf.Clamp(buffLevelMax, 1, skillData.maxLevel);
                Buff buff = new Buff((BuffSkill)skillData, level);
                if (pet.owner.skills.buffs.Contains(buff))
                {
                    Debug.Log("contien : " +buff.name);
                }
                else
                {
                    Debug.Log("contien pas: " + buff.name);
                }

                if (!pet.owner.skills.buffs.Contains(buff))
                {
                    Debug.Log("add buff!");
                    buff.buffTimeEnd = buff.data.buffTime.Get(buffLevelMax) + Time.time;
                    pet.owner.skills.buffs.Add(buff);
                }
                else
                {
                    Debug.Log("déjà buff");
                }
            }
        }
    }
}
