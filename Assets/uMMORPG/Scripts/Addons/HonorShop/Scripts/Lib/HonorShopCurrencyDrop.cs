using System;

[Serializable]
public partial struct HonorShopCurrencyDrop
{
    public Tmpl_HonorCurrency honorCurrency;
    public long amountMin;
    public long amountMax;

    public long amount
    {
        get
        {
            return (long)UnityEngine.Random.Range(amountMin, amountMax);
        }
    }
}