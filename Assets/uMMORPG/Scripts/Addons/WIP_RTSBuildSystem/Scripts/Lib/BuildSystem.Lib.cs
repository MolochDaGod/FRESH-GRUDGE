#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_BuildSystem()
    {
        AddOn addon = new AddOn();

        addon.name = "RTS BuildSystem";
        addon.basis = "uMMORPG3d Remastered";
        addon.define = "_iMMOBUILDSYSTEM";
        addon.author = "Trugord";
        addon.version = "2020.3.24-36.1";
        addon.dependencies = "none";
        addon.comments = "none";
        addon.active = true;

        addons.Add(addon);
    }
}
#endif