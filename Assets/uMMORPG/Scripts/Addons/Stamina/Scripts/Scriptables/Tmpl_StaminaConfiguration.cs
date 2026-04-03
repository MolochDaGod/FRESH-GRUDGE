using UnityEngine;

[CreateAssetMenu(menuName = "ADDON/Templates/Stamina Template", order = 999)]
public class Tmpl_StaminaConfiguration : ScriptableObject
{
    [Header("[-=-=-[ Stamina ]-=-=-]")]
    public bool isActive = true;

    [Header("[-=-=-[ Stamina sprint Configuration ]-=-=-]")]
    [Tooltip("This is speed added for sprint player ")]
    [Min(0)] public float moveSprintSpeed = 0.2f;
    [Tooltip("This is cost stamina for sprint")]
    [Min(0)] public int costSprint = 5;
    [Tooltip("This define number of seconds between cost")]
    [Min(0)] public float costPerSeconds = 1;
    public KeyCode keyCode= KeyCode.LeftShift;

    ///[Header("[-=-=-[ Cost Mounted ]-=-=-]")]
    //[Min(0)] public int costMovingMounted = 1;

    //[Header("[-=-=-[ Cost Skill Default ]-=-=-]")]
    //[Min(0)] public int costSkillDefault = 0;
}
