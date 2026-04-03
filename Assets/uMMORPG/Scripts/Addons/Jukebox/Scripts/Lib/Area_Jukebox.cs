using UnityEngine;
using Mirror;

// ===================================================================================
// JUKEBOX AREA
// ===================================================================================
#if _iMMO2D
[RequireComponent(typeof(CircleCollider2D))]
#else
[RequireComponent(typeof(SphereCollider))]
#endif
public class Area_Jukebox : MonoBehaviour
{
    [Header("[-=-[ JUKEBOX AREA ]-=-]")]
    [Tooltip("The audio clip that is played while the player is inside this area")]
    public AudioClip areaMusicClip;

    [Tooltip("The duration it takes to fade the music in/out when entering/leaving the area")]
    public float fadeInFadeOut = 2.0f;

    [Range(0, 1)] public float adjustedVolume;

    [Tooltip("Set to true if you want the music to loop while the player stays inside the area")]
    public bool loop = true;
#if _CLIENT
    // -------------------------------------------------------------------------------
    // OnTriggerEnter
    // -------------------------------------------------------------------------------
    [Client]
#if _iMMO2D
    private void OnTriggerEnter2D(Collider2D co)
#else
    private void OnTriggerEnter(Collider co)
#endif
    {
        if (!NetworkClient.active || areaMusicClip == null) return;

        Player player = Player.localPlayer;
        if (!player) return;

        Player entity = co.GetComponentInParent<Player>();

        if (entity != null && entity == player)
            Jukebox.singleton.PlayBGM(areaMusicClip, fadeInFadeOut, adjustedVolume, loop);
    }

    // -------------------------------------------------------------------------------
    // OnTriggerExit
    // -------------------------------------------------------------------------------
    [Client]
#if _iMMO2D
    private void OnTriggerExit2D(Collider2D co)
#else
    private void OnTriggerExit(Collider co)
#endif
    {
        if (!NetworkClient.active || areaMusicClip == null) return;

        Player player = Player.localPlayer;
        if (!player) return;

        Player entity = co.GetComponentInParent<Player>();

        if (entity != null && entity == player)
            Jukebox.singleton.revertBGM(areaMusicClip, fadeInFadeOut, adjustedVolume);
    }
#endif
    // -------------------------------------------------------------------------------
}