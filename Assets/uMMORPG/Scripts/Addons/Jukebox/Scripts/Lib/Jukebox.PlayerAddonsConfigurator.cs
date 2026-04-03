using UnityEngine;
using Mirror;

// PLAYER
public partial class PlayerAddonsConfigurator
{

    public void OnStartLocalPlayer_Jukebox()
    {
        health.onEmpty.AddListener(OnDeath_Jukkebox);
    }

    // -----------------------------------------------------------------------------------
    // Start
    // -----------------------------------------------------------------------------------
    [ClientCallback]
    private void Start_JukeBox()
    {
    	if (Jukebox.singleton == null)
    	{
    		return;
    	}
    	
        if (Jukebox.singleton.jukeboxTemplate != null &&
            Jukebox.singleton.jukeboxTemplate.isActive &&
            Jukebox.singleton.jukeboxTemplate.defaultMusicClip != null)
            Jukebox.singleton.PlayBGM(Jukebox.singleton.jukeboxTemplate.defaultMusicClip, Jukebox.singleton.jukeboxTemplate.defaultFadeInFadeOut, Jukebox.singleton.jukeboxTemplate.defaultAdjustedVol, true);
        else
            Debug.LogWarning("No Jukebox assigned or missing audio clip!");
    }

    // -----------------------------------------------------------------------------------
    // OnDeath_Jukebox
    // @Server
    // -----------------------------------------------------------------------------------
    private void OnDeath_Jukkebox()
    {
#if _SERVER
        Target_JukeboxRevert(player.connectionToClient);
#endif
    }

#if _SERVER
    // -----------------------------------------------------------------------------------
    // Target_JukeboxRevert
    // @Server -> @Client
    // -----------------------------------------------------------------------------------
    [TargetRpc]
    public void Target_JukeboxRevert(NetworkConnection target)
    {
        if (Jukebox.singleton != null && Jukebox.singleton.jukeboxTemplate != null)
            Jukebox.singleton.revertBGM(Jukebox.singleton.jukeboxTemplate.defaultMusicClip, Jukebox.singleton.jukeboxTemplate.defaultFadeInFadeOut, Jukebox.singleton.jukeboxTemplate.defaultAdjustedVol);
        else Debug.Log("Jukebox is missing!");
    }
#endif
    // -----------------------------------------------------------------------------------

    /**
     * Ajout de musique si on et en combat
     * 
     * Quand somme nous en combat ?
     *   - si nous utilisont des skills d'attaque (avec ou sans cible)
     *   - quand on reçois/fait des dégat ou qu'une personne de notre groupe et que l'on et à proximité reçois/fait des dégats
     *   - ??
     */
}
