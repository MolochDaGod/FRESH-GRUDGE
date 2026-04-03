using System.Text;
using UnityEngine;
using UnityEngine.UI;
#if _iMMOCRAFTING
// ===================================================================================
// CRAFTING SLOT
// ===================================================================================
public class UI_RecipeSlot : MonoBehaviour
{
    public Text nameText;
    public Image image;
    public UIShowToolTip tooltip;

    // -----------------------------------------------------------------------------------
    // Show
    // -----------------------------------------------------------------------------------
    public void Show(string recipeName)
    {
        Tmpl_Recipe.All.TryGetValue(recipeName.GetStableHashCode(), out Tmpl_Recipe recipe);

        nameText.text = recipe.name;
        image.sprite = recipe.image;
        tooltip.enabled = true;
        tooltip.text = recipe.ToolTip();
    }

    // -----------------------------------------------------------------------------------
    // ToolTip
    // -----------------------------------------------------------------------------------
    public string ToolTip(Tmpl_Recipe tpl)
    {
        StringBuilder tip = new StringBuilder();

        tip.Append(tpl.name + "\n");
        tip.Append(tpl.toolTip + "\n");

        return tip.ToString();
    }
    // -----------------------------------------------------------------------------------
}
#endif