using UnityEngine;

[CreateAssetMenu(fileName = "Default Unlocked Classes", menuName = "ADDON/Templates/New Default Unlocked Classes", order = 999)]
public class Tmpl_UnlockableClasses : ScriptableObject
{
    [Header("[-=-[ Default Classe available ]-=-]")]
    [Tooltip("[Required] add all classe default authorized")]
    public Player[] defaultClasses;
}
