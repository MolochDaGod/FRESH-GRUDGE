#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_ItemRarity()
    {
         AddOn addon = new AddOn();

        addon.name          = "ItemRarity";
        addon.basis         = "uMMORPG3d Remastered";
        addon.define        = "_iMMOITEMRARITY";
        addon.author        = "Trugord";
        addon.version       = "2020.3.30-37.4";
        addon.dependencies  = "none";
        addon.comments      = "none";
        addon.active        = true;

        addons.Add(addon);
    }
}
#endif