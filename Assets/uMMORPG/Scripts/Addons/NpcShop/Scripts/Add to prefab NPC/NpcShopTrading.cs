using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class NpcShopTrading : NpcOffer
{
    [Header("[-=-=-[ NPC SHOP ]-=-=-]")]
    public Tmpl_NpcShopConfiguration shopTrading;

    public readonly SyncList<SoldUniqueItems> shopItemsSold = new SyncList<SoldUniqueItems>();
    public override bool HasOffer(Player player) => (shopTrading != null && shopTrading.saleItems.Length > 0);

    public override string GetOfferName() => (shopTrading != null) ? shopTrading.buttonShopName : "Npc Shop";

    public override void OnSelect(Player player)
    {
        shopTrading.uiNpcShop.TriggerEvent();
        UIInventory.singleton.panel.SetActive(true); // better feedback
        UINpcDialogue.singleton.panel.SetActive(false);
    }
#if _SERVER
    public override void OnStartServer()
    {
        if (shopTrading.saleUniqueItems.Length > 0)
        {
            List<SoldUniqueItems> _shopItemsSold = Database.singleton.GetShopItemsSale2(shopTrading.name);
            foreach (var item in _shopItemsSold)
            {
                shopItemsSold.Add(item);
            }
#if MIRROR_90_OR_NEWER
            shopItemsSold.OnChange += OnShopItemsSoldyUpdated;
#else
#pragma warning disable CS0618
            shopItemsSold.Callback += OnShopItemsSoldyUpdated;
#pragma warning restore
#endif
        }
    }

    [Server]
    public void AddShopItemSold(SoldUniqueItems add)
    {
        shopItemsSold.Add(add);
    }

#if MIRROR_90_OR_NEWER
    void OnShopItemsSoldyUpdated(SyncList<SoldUniqueItems>.Operation op, int index, SoldUniqueItems oldItemSold)
#else
    void OnShopItemsSoldyUpdated(SyncList<SoldUniqueItems>.Operation op, int index, SoldUniqueItems oldItemSold, SoldUniqueItems newItemSold)
#endif
    {
        shopTrading.uiNpcShop.TriggerEvent();
    }
#endif
}
