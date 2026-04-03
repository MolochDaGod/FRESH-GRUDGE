using UnityEngine;

public partial class UI_SkillCategoryButton : MonoBehaviour
{
    public GameObject panel;
    public string category;

    // -----------------------------------------------------------------------------------
    // OnClick
    // -----------------------------------------------------------------------------------
    public void OnClick()
    {
        UI_Skills co = panel.GetComponent<UI_Skills>();
        if (co)
            co.changeCategory(category);
    }

    // -----------------------------------------------------------------------------------
}