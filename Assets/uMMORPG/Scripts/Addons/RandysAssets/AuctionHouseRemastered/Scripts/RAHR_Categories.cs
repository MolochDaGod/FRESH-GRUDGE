using System;
using System.ComponentModel;

//Changeing order of enums after setting up your items categories can result in incorrect category on items. You will then need to resetup your items categories.

public enum RAHR_Category {
    [Description("Armor")] Armor,
	[Description("Trinket")] Trinket,
    [Description("Weapons")] Weapons,
	[Description("Offhand")] Offhand,
    [Description("Pets")] Pets,
	[Description("Mounts")] Mounts,
    [Description("Consumables")] Consumables,
    [Description("Misc")] Misc,
}

public enum RAHR_SubCategory {
    [RAHR_SubCategoryOf(RAHR_Category.Misc)] [Description("Undefined")] Undefined,
    [RAHR_SubCategoryOf(RAHR_Category.Armor)] [Description("Head")] Head,
    [RAHR_SubCategoryOf(RAHR_Category.Armor)] [Description("Shoulders")] Shoulders,
    [RAHR_SubCategoryOf(RAHR_Category.Armor)] [Description("Chest")] Chest,
    [RAHR_SubCategoryOf(RAHR_Category.Armor)] [Description("Hands")] Hands,
    [RAHR_SubCategoryOf(RAHR_Category.Armor)] [Description("Feet")] Feet,
	[RAHR_SubCategoryOf(RAHR_Category.Armor)] [Description("Cape")] Cape,
	[RAHR_SubCategoryOf(RAHR_Category.Trinket)] [Description("Ring")] Ring,
	[RAHR_SubCategoryOf(RAHR_Category.Trinket)] [Description("Relic")] Relic,
    [RAHR_SubCategoryOf(RAHR_Category.Offhand)] [Description("Shield")] Shield,
	[RAHR_SubCategoryOf(RAHR_Category.Offhand)] [Description("Staff")] Staff,
    [RAHR_SubCategoryOf(RAHR_Category.Weapons)] [Description("One Handed Sword")] OneHandedSword,
	[RAHR_SubCategoryOf(RAHR_Category.Weapons)] [Description("Two Handed Weapon")] TwoHandedWeapon,
    [RAHR_SubCategoryOf(RAHR_Category.Weapons)] [Description("Bow")] Bow,
	[RAHR_SubCategoryOf(RAHR_Category.Weapons)] [Description("Spear")] Spear,
	[RAHR_SubCategoryOf(RAHR_Category.Weapons)] [Description("Book")] Book,
    [RAHR_SubCategoryOf(RAHR_Category.Pets)] [Description("Pet")] Pet,
	[RAHR_SubCategoryOf(RAHR_Category.Mounts)] [Description("Mount")] Mount,
    [RAHR_SubCategoryOf(RAHR_Category.Consumables)] [Description("Potions")] Potions,
    [RAHR_SubCategoryOf(RAHR_Category.Consumables)] [Description("Scrolls")] Scrolls,
	[RAHR_SubCategoryOf(RAHR_Category.Misc)] [Description("Structure")] Structure,
}

[AttributeUsage(AttributeTargets.Field)]
public class RAHR_SubCategoryOf : Attribute {
    public RAHR_SubCategoryOf(RAHR_Category cat) {
        Category = cat;
    }

    public RAHR_Category Category { get; private set; }
}