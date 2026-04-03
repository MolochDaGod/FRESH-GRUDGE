// Setup our item rarity enum so they can be set from the scriptable item.
public partial class ScriptableItem
{
    public enum ItemRarity { Poor, Common, Uncommon, Rare, Epic, Legendary }

    public ItemRarity rarity = ItemRarity.Poor;
}