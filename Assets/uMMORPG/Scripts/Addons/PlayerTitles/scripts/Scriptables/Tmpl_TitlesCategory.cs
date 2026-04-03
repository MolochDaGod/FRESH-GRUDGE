using UnityEngine;

[CreateAssetMenu(fileName = "Title category", menuName = "ADDON/Templates/Title/Category", order = 999)]
public partial class Tmpl_TitlesCategory : ScriptableObject
{
    [Header("[-=-[ Title Category ]-=-]")]
    public string titleTypeName;
    public Sprite titleTypeImage;
    public bool displayed;
    public string description;
}
