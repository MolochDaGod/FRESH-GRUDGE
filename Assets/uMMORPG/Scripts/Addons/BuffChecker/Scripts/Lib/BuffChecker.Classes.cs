using UnityEngine;

// BuffCheckerEntry
[System.Serializable]
public class BuffCheckerEntry
{
    [Tooltip("[Optional] The buff that must be active to toggle the game objects (set to null to use state)")]
    public BuffSkill buffSkill;

    [Tooltip("[Optional] The state the entity has to be in, in order to toggle the game objects (set to empty to use buff)")]
    public string state;
    [Tooltip("[Optional] Use skill name in order to toggle the game objects (set to empty to use buff)")]
    public string skillName;
    [Tooltip("[Optional] One or more game objects that are children of the entity, will be toggled active/inactive")]
    public GameObject[] gameObjects;

    [Tooltip("[Optional] Will set an animation parameter Integer to that value, can be used to additional animations (set -1 to disable)")]
    public string animationName = "";
    public bool revert;

    // -----------------------------------------------------------------------------------
    // isActive
    // -----------------------------------------------------------------------------------
    public bool isActive(Entity entity, string _skillname = "")
    {

        if (buffSkill)
        {
            int index = entity.skills.buffs.FindIndex(s => s.name == buffSkill.name);

            if (index != -1)
            {
                Buff buff = entity.skills.buffs[index];
                return buff.BuffTimeRemaining() > 0;
            }
        }
        else if (!string.IsNullOrWhiteSpace(skillName) &&  _skillname == skillName)
        {
            return true;
        }
        return !string.IsNullOrWhiteSpace(state) && entity.state == state;
    }

    // -----------------------------------------------------------------------------------
    // ToggleGameObject
    // -----------------------------------------------------------------------------------
    public void ToggleGameObject(bool bActive = false)
    {
        foreach (GameObject gameObject in gameObjects)
            gameObject.SetActive(bActive);
    }
    // -----------------------------------------------------------------------------------
}