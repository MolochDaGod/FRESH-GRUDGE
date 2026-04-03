#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_LogoutTimer()
    {
        AddOn addon = new AddOn();

        addon.name = "Logout Timer";
        addon.basis = "uMMORPG3d Remastered 3d & 2d";
        addon.define = "_iMMOLOGOUTTIMER";
        addon.author = "Trugord";
        addon.version = "";
        addon.dependencies = "none";
        addon.comments = "none";
        addon.active = true;

        addons.Add(addon);
    }
}
#endif