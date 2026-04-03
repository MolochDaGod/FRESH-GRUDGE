using UnityEngine;

[CreateAssetMenu(fileName = "Tombstone Configuration", menuName = "ADDON/Templates/Tombstone Configuration", order = 999)]
public class Tmpl_PlayerTombstone : ScriptableObject
{
    [Header("[-=-[ Displayed Model  ]-=-]")]
    public Tombstone tombstoneModel;

    [Header("[-=-[ Displayed Model  ]-=-]")]
    public float destroytombDelay;

    [Header("[-=-[ Skill Pray ]-=-]")]
    public TargetBuffSkill praySkill;

    [Header("[-=-[ Xp return of player ]-=-]")]
    [Range(0, 100)] public int xpPercentReturn = 80;

    [Header("[-=-[ Chance to Spawn Tombstone ]-=-]")]
    [Range(0, 1)] public float tombstoneChance = 1.0f;

    [Header("[-=-[ Position in the ground ]-=-]")]
    [Range(0, 1)] public float tombstoneFallHeight = 0.5f;
}
