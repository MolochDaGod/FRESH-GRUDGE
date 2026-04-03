
using UnityEngine;

// Word Filter

[CreateAssetMenu(fileName = "WordFilter", menuName = "ADDON/Templates/New WordFilter", order = 999)]
public class Tmpl_WordFilter : ScriptableObject
{
    [Tooltip("[Required] Enter all bad words here. If a chatext or player name contains one of them, it will be denied.")]
    public string[] badwords;
}
