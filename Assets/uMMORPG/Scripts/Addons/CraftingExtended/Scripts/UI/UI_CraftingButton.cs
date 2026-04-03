using UnityEngine;
using UnityEngine.UI;
#if _iMMOCRAFTING
// CRAFTING BUTTON
public partial class UI_CraftingButton : MonoBehaviour
{
    public GameObject panel;
    public Text text;

    protected string category;

    // -----------------------------------------------------------------------------------
    // SetCategory
    // -----------------------------------------------------------------------------------
    public void SetCategory(string _category)
    {
        category = _category;
        text.text = _category;
    }

    // -----------------------------------------------------------------------------------
    // OnClick
    // -----------------------------------------------------------------------------------
    public void OnClick()
    {
        UI_Crafting co = panel.GetComponent<UI_Crafting>();

        if (co)
        {
            co.ChangeCategory(category);
        }
    }
    // -----------------------------------------------------------------------------------
}
#endif