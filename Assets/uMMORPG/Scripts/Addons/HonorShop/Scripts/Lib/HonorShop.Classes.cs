using System;

[Serializable]
public partial struct HonorShopCategory
{
    public string categoryName;
    public Tmpl_HonorCurrency honorCurrency;
    public ScriptableItem[] items;
}