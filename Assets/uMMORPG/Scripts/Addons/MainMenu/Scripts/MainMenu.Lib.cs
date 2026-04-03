#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_MainMenu()
    {
         AddOn addon = new AddOn();

        addon.name          = "MainMenu";
        addon.basis         = "uMMORPG3d Remastered";
        addon.define        = "_iMMOMAINMENU";
        addon.author        = "Trugord";
        addon.version       = "2020.3.30-37.3";
        addon.dependencies  = "none";
        addon.comments      = "none";
        addon.active        = true;

        addons.Add(addon);
    }
}
#endif