#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_GuildWarehouse()
    {
        AddOn addon = new AddOn();

        addon.name          = "Guild Warehouse";
        addon.basis         = "uMMORPG3d Remastered";
        addon.define        = "_iMMOGUILDWAREHOUSE";
        addon.author        = "Trugord";
        addon.version       = "2020.3.30-37.1";
        addon.dependencies  = "none";
        addon.comments      = "none";
        addon.active        = true;

        addons.Add(addon);
    }
}
#endif