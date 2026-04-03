#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_PrestigeClasses()
    {
        AddOn addon = new AddOn();

        addon.name          = "PrestigeClasses";
        addon.basis         = "uMMORPG3d Remastered";
        addon.define        = "_iMMOPRESTIGECLASSES";
        addon.author        = "Trugord";
        addon.version       = "2019.4.28-34.2";
        addon.dependencies  = "_Tools,UsageRequirements,Traits";
        addon.comments      = "Install All Other Addon before this";
        addon.active        = true;

        addons.Add(addon);
    }
}
#endif