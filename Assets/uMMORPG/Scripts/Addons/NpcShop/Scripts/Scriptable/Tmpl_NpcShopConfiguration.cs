using UnityEngine;

[CreateAssetMenu(fileName ="Shop Configuration", menuName = "ADDON/Templates/Npc Shop Configuration", order = 999)]
public class Tmpl_NpcShopConfiguration : ScriptableObject
{
    [Header("[-=-[ Event UI called ]-=-]")]
    public GameEvent uiNpcShop;

    [Header("[-=-[ NPC Button Shop name ]-=-]")]
    public string buttonShopName = "Npc Shop";

    [Header("[-=-[ Categories ]-=-]")]
    [Tooltip("[Required] Item categories to show, add at least one category (can be empty)")]
    public string[] shopCategories;

    [Header("[-=-[ Items ]-=-]")]
    public ScriptableItem[] saleItems;

    [Header("[-=-[ Unique sell ]-=-]")]
    public SellUniqueItems[] saleUniqueItems;

    [Header("[-=-[ Allow player Sell items ]-=-]")]
    public bool offertSale = true;

#if !_iMMO2D
    [Header("[-=-[ Allow player repair items ]-=-]")]
    public bool offersRepair = true;
    public bool repairInventory = true;
    public bool repairEquipment = true;
    public int repairGoldCostPerDurabilityPoint = 1;
#endif

}