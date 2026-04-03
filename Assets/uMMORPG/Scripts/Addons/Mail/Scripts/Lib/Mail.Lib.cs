#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_Mail()
    {
        AddOn addon = new AddOn()
        {
            name = "Mail",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOMAIL",
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