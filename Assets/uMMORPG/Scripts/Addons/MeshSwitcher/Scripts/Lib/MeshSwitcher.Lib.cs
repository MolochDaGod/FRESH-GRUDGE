#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_MeshSwitcher()
    {
         AddOn addon = new AddOn();

        addon.name          = "MeshSwitcher";
        addon.basis         = "uMMORPG3d Remastered";
        addon.define        = "_iMMOMESHSWITCHER";
        addon.author        = "Trugord";
        addon.version       = "2020.3.37-37.1";
        addon.dependencies  = "_Tools";
        addon.comments      = "none";
        addon.active        = true;

        addons.Add(addon);
    }
}
#endif