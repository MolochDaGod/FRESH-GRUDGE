#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_Help()
    {
        AddOn addon = new AddOn()
        {
            name = "Reports System",
            basis = "uMMORPG 2D&3D Remastered",
            define = "_iMMOREPORTS",
            author = "Trugord",
            version = "",
            dependencies = "Tools",
            comments = "none",
            active = true
        };
        addons.Add(addon);
    }
}
#endif