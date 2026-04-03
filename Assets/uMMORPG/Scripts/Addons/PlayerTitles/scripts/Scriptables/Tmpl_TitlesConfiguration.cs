using UnityEngine;

[CreateAssetMenu(fileName = "Title category", menuName = "ADDON/Templates/Title/Configuration", order = 999)]
public partial class Tmpl_TitlesConfiguration : ScriptableObject
{
    [Header("[-=-[ Title Configuration ]-=-]")]
    public string overlayPrefix = "{";
    public string overlaySuffix = "}";
    public Color color;
    public Tmpl_TitlesCategory[] category;
    public Tmpl_Titles[] titles;

    [Header("[-=-[ UI Update Event ]-=-]")]
    public GameEvent ui_titleUpdate;

    [Header("[-=-[ POP-UP : Earn Title  ]-=-]")]
    public Tools_PopupClass earnTitleUpNotice;
}
