using Mirror;
using System.Linq;

#if _SERVER
// ENTITY
public partial class Entity
{
    // -------------------------------------------------------------------------------
    // HazardFloorEnter
    // -------------------------------------------------------------------------------
    [ServerCallback]
    public void HazardFloorEnter(HazardBuff[] onEnterBuff)
    {
        foreach (HazardBuff buff in onEnterBuff)
        {
            if (this is Player && buff.protectiveRequirements.hasRequirements() && buff.protectiveRequirements.checkRequirements((Player)this))
            {
                ((Player)this).Tools_TargetAddMessage(buff.protectiveMessage);
                break;
            }

            int level = UnityEngine.Random.Range(buff.minBuffLevel, buff.maxBuffLevel);

            Tools_ApplyBuff(buff.buff, level, buff.chance);
        }
    }

    // -------------------------------------------------------------------------------
    // HazardFloorLeave
    // -------------------------------------------------------------------------------
    [ServerCallback]
    public void HazardFloorLeave(TargetBuffSkill[] onExitBuff)
    {
        if (onExitBuff.Length > 0)
        {
            /*for (int j = 0; j < onExitBuff.Length; ++j)
            {
                if (skills.buffs.Any(x => x.data == onExitBuff[j]))
                {
                    skills.buffs.RemoveAt(j);
                }
            }*/
            // Test revert for potential skill index change
            for (int j = onExitBuff.Length - 1; j >= 0; j--)
            {
                if (skills.buffs.Any(x => x.data == onExitBuff[j]))
                {
                    skills.buffs.RemoveAt(j);
                }
            }
        }


    }

    // -----------------------------------------------------------------------------------
}
#endif