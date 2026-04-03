#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_SpeechBubbles()
    {
        AddOn addon = new AddOn();

        addon.name          = "Speech Bubbles";
        addon.basis         = "uMMORPG3d Remastered";
        addon.define        = "_iMMOSPEECHBUBBLES";
        addon.author        = "Trugord";
        addon.version       = "2020.3.30-37.2";
        addon.dependencies  = "none";
        addon.comments      = "none";
        addon.active        = true;

        addons.Add(addon);
    }
}
#endif