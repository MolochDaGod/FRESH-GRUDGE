#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_GuildUpgrades()
    {
         AddOn addon = new AddOn();

        addon.name          = "GuildUpgrades";
        addon.basis         = "uMMORPG3d Remastered";
        addon.define        = "_iMMOGUILDUPGRADES";
        addon.author        = "Trugord";
        addon.version       = "2020.3.37-37.1";
        addon.dependencies  = "Guild Upgrade";
        addon.comments      = "none";
        addon.active        = true;

        addons.Add(addon);
    }
}
#endif