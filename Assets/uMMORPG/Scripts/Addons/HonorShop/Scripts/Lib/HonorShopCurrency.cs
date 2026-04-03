using Mirror;
using System;
using UnityEngine;

// HonorShopCurrency
[Serializable]
public partial struct HonorShopCurrency
{
    public int hash;
    public long amount;
    [HideInInspector] public long total;

    public Tmpl_HonorCurrency honorCurrency
    {
        get
        {
            if (hash == 0)
            {
                return null;
            }
            else
            {
                Tmpl_HonorCurrency currency;

                return ((Tmpl_HonorCurrency.All.TryGetValue(hash, out currency)) ? currency : null);
            }
        }

        set
        {

            hash = ((value != null) ? value.name.GetStableHashCode() : 0);
        }
    }
}

public class SyncList_HonorShopCurrency : SyncList<HonorShopCurrency> { }