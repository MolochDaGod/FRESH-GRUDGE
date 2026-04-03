#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_PlayerWarehouse()
    {
        AddOn addon = new AddOn();

        addon.name          = "player Warehouse";
        addon.basis         = "uMMORPG3d Remastered";
        addon.define        = "_iMMOWAREHOUSE";
        addon.author        = "Trugord";
        addon.version       = "2020.3.30-37.1";
        addon.dependencies  = "none";
        addon.comments      = "none";
        addon.active        = true;

        addons.Add(addon);
    }
}
#endif