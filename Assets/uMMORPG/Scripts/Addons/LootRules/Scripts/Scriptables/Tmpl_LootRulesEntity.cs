using UnityEngine;

[CreateAssetMenu(menuName = "ADDON/Templates/Loot Rules Configurration", order = 998)]
public class Tmpl_LootRulesEntity : ScriptableObject
{
    [Header("[-=-[ Loot Rules (Victor can always loot) ]-=-]")]
    [Tooltip("After x seconds loot rules are set to 'LootEverybody'. Set to 0 to disable")]
    [Range(0, 999)] public float LiftRulesAfter = 60;

    public bool LootVictorParty;
    public bool LootVictorGuild;
#if _iMMOPVP
    public bool LootVictorRealm;
#endif
    public bool LootEverybody;

    [Header("[Drop Only Entity]")]
    public EntityType entityType;
}
