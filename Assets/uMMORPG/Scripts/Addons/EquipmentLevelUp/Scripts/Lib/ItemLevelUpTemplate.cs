using System;
using UnityEngine;

[Serializable]
public partial struct ItemLeveUp
{
    [Header("[-=-[ Level Up Success rate ]-=-]")]
    [Range(0, 100)] public int successRate;

    public bool enableDestroyChance;
    [BoolShowConditional("enableDestroyChance", true)]
    [Range(0,100)] public int destroyRate;

    [Header("[-=-[ Requierement ]-=-]")]
    public Tools_Requirements usageRequirements;

    [Header("[-=-[ Level Up Cost ]-=-]")]
    public Tools_Cost levelUpCost;

    [Header("[-=-[ Equipment Level Up modifier ]-=-]")]
    public EquipmentLevelUpModifier equipmentLevelUpModifier;

}

[Serializable]
public partial class EquipmentLevelUpModifier
{

    [Header("[-=-[ Rarity modifier]-=-]")]
    public EquipmentItem.ItemRarity rarity;
#if _iMMOATTRIBUTES
    [Header("[-=-[ Extra Stat Modifier ]-=-]")]
    public ExtraStats extraModifier;
#endif
    [Header("[-=-[ Attribute modifier ]-=-]")]
    public Tools_AttributeModifier[] attribute;

    [Header("[-=-[ Elements Resistence ]-=-]")]
    public ElementModifier[] elementsResistenseBonus;

    [Header("[-=-[ Equipment Stat modifier ]-=-]")]
    public EquipmentStatModifier equipmentStatModifier;

    [Header("[-=-[ Prefab Modifier ]-=-]")]
    public GameObject ModelPrefab;

    [Header("[-=-[ Particle effect ]-=-]")]
    public GameObject particleEffectModifier;

    /**
    [Header("[-=-[ Material Modifier]-=-]")]
    public Material levelMaterialModifier;

    [Header("[-=-[ Mesh Modifier (in the prefab !) ]-=-]")]
    public string meshCategoryName;
    public int meshPositionModifier;*/
}

[Serializable]
public partial struct EquipmentStatModifier
{
    public int healthBonus;
    public int manaBonus;
    public int staminaBonus;
    public int damageBonus;
    public int defenseBonus;
    public int blockBonus;
    public int criticalBonus;
    [Range(0, 1)] public float blockChanceBonus;
    [Range(0, 1)] public float criticalChanceBonus;
}