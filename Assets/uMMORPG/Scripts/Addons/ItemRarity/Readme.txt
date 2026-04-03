1) In scene search Inventory open and select InventoryPanel
    1.1) in inspector add new component ItemRarity and select Slot Type Inventory
    1.2) change item slot prefab to the one of the same name contained in the ItemRatity/Prefab addon folder

2) In scene search Equipment open and select EquipmentPanel
    2.1) in inspector add new component ItemRarity and select Slot Type Equipment
    2.2) change item slot prefab to the one of the same name contained in the ItemRatity/Prefab addon folder

3) In scene search Crafting open and select CraftingPanel
    3.1) in inspector add new component ItemRarity and select Slot Type Crafting
    3.2) change item slot prefab to the one of the same name contained in the ItemRatity/Prefab addon folder

4) In scene search ItemMall open and select ItemMallPanel
    4.1) in inspector add new component ItemRarity and select Slot Type Item Mall
    4.2) change item slot prefab to the one of the same name contained in the ItemRatity/Prefab addon folder


5) In scene search Loot open and select LootPanel
    5.1) in inspector add new component ItemRarity and select Slot Type Loot
    5.2) change item slot prefab to the one of the same name contained in the ItemRatity/Prefab addon folder

6) In scene search PlayerTrading open and select PlayerTradingPanel
    6.1) in inspector add new component ItemRarity and select Slot Type Player Trade
    6.2) change item slot prefab to the one of the same name contained in the ItemRatity/Prefab addon folder


7) In scene search NpcTrading add new component ItemRarity and select Slot Type Npc Trading
    7.1) change item slot prefab to the one of the same name contained in the ItemRatity/Prefab addon folder


8) Open UiItemMall.cs

replace 
    int currentCategory = 0;

to
    public int currentCategory = 0;