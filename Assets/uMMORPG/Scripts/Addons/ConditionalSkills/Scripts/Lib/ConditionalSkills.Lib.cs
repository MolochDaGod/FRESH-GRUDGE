#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_ConditionalSkills()
    {
        AddOn addon = new AddOn();
        addon.name          = "Conditional Skills";
        addon.basis         = "uMMORPG3d Remastered";
        addon.define        = "_iMMOCONDITIONALSKILLS";
        addon.author        = "Trugord";
        addon.version       = "2020.3.37-37.2";
        addon.dependencies  = "Tools, Attache";
        addon.comments      = "none";
        addon.active        = true;

        addons.Add(addon);
    }
}
#endif