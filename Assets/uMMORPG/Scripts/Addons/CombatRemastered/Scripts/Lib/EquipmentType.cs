
public enum WeaponType
{
    None,
    // weapon
    Unarmed,
    Slash1Hand, // Dague, épée
    Pierce1Hand, // Dague
    Bludgeon1Hand, // ??
    Slash2Hand, // cimetère et autre grande épée
    Pierce2Hand, // 
    Bludgeon2Hand,
    RangedThrown, // baton pour les sorts par exemple
    RangedBow,
    RangedGun,
    Shield,
    Dual
}

public enum DefaultSkillType
{
    MainHand,
    OffHand,
    Duo,
    TwoHand,
    Ranged
}

public enum WeaponSkill
{
    // Melee weapons
    Slash,
    Thrust,

    // Ranged weapons
    Bow,
    CompositeBow,
    Longbow,
    Crossbow,

    // Other weapons
    Sword,
    Hammer,
    Haxe,
    Spear,
    Crush,
    Thrown,
    Flexible,
    Staff,
    Polearm,
    TwoHanded,
    CelticSpear,
    RecurveBow,
    PaintWorking,
    HandToHand,
    Scythe,
    FistWraps,
    MaulerStaff
}


public enum WeaponPosition
{
    None,
    Unarmed,
    // weapon
    OffHand,
    MainHand,
    TwoHand,
    Ranged
}

public enum ArmorPart
{
    none,

    // Armor slot
    Torso,
    Helm, // Helm
    Arms, // Arms
    Hand, // Gloves
    Necklace,
    Mythique,
    Cloak,
    Jewelry,
    Left_Ring,
    Right_ring,
    Left_Wrist,
    Right_Wrist,
    Legs, // Legs
    Feet, // Boots

    // Weapon slot
    MainHand,
    OffHand,
    TwoHand,
    Ranged
}

public enum ArmorType
{
    none,

    // Armor type
    Cloth,
    Leather,
    Studded, 
    chain,
    Plate
}

#pragma warning disable
public partial struct EquipmentInfo
{
    public ArmorPart EquipmentPart;
}
#pragma

