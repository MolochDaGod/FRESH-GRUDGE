using UnityEngine;
using Mirror;

// UI CHARACTER SELECTION
public partial class UI_Jukebox: MonoBehaviour
{
    [Tooltip("[Required] Assign UI Character Selection panel")]
    public GameObject panel;

    private bool startMusic;

    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    private void Update()
    {
        if (!panel.activeSelf || !NetworkClient.active || Player.localPlayer != null) return;

        if (!startMusic)
        {
            if (Jukebox.singleton.jukeboxTemplate != null &&
                Jukebox.singleton.jukeboxTemplate.isActive)
            {
                if (Jukebox.singleton.jukeboxTemplate.menuMusicClip != null)
                    Jukebox.singleton.PlayBGM(Jukebox.singleton.jukeboxTemplate.menuMusicClip, Jukebox.singleton.jukeboxTemplate.menuFadeInFadeOut, Jukebox.singleton.jukeboxTemplate.menuAdjustedVol, true);
            }
            else
            {
                Debug.LogWarning("Jukebox: Either inactive or ScriptableObject not found!");
            }

            startMusic = true;
        }
    }

    // -----------------------------------------------------------------------------------
}
