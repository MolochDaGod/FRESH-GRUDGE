using System.Text;

public partial struct Item
{
    public string itemCategory
    {
        get { return data.itemCategory; }
    }

    private void ToolTip_NpcShop(StringBuilder tip)
    {
        tip.Replace("{ITEMCATEGORY}", itemCategory);
    }
}