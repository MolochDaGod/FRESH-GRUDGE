using UnityEngine;

// Level Up Template
[CreateAssetMenu(fileName = "Logout Timer", menuName = "ADDON/Templates/Logout Timer", order = 999)]
public partial class Tmpl_LogoutTimer : ScriptableObject
{
    [Header("[-=-[ Game Event ]-=-]")]
    public GameEvent onLogoutTimer;

    [Header("[-=-[ Logout Timer ]-=-]")]
    [Range(1, 3600)] public float logoutWarningTime = 30f;
    [Range(1, 3600)] public float logoutKickTime = 60f;
}
