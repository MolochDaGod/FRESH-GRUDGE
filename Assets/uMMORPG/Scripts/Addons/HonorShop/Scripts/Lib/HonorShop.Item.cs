using System.Linq;

// ITEM
public partial struct Item
{
    // -----------------------------------------------------------------------------------
    // GetHonorCurrency
    // -----------------------------------------------------------------------------------
    public long GetHonorCurrency(Tmpl_HonorCurrency honorCurrency)
    {
        return data.currencyCosts.FirstOrDefault(x => x.honorCurrency.name == honorCurrency.name).amount;
    }
}