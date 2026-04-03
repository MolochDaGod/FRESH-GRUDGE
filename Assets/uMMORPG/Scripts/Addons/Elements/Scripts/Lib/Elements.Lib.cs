#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_Elements()
    {
        AddOn addon = new AddOn();

        addon.name          = "Elements";
        addon.basis         = "uMMORPG3d Remastered";
        addon.define        = "_iMMOELEMENTS";
        addon.author        = "Trugord";
        addon.version       = "2019.4.2-34.2";
        addon.dependencies  = "none";
        addon.comments      = "none";
        addon.active        = true;

        addons.Add(addon);
    }
}
#endif