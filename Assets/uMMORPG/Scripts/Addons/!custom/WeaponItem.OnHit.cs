using UnityEngine;

// Partial extension: adds on-hit proc effects to every WeaponItem asset.
// Configure these in the Inspector under "[-=-[ On-Hit Effects ]-=-]".
public partial class WeaponItem
{
    [Header("[-=-[ On-Hit Effects ]-=-]")]
    [Tooltip("List of BuffSkill procs applied to the target when this weapon lands a hit. Supports poison (negative HP regen), slow (negative speed), damage reduction (negative damage bonus), etc.")]
    public WeaponOnHitEffect[] onHitEffects;
}
