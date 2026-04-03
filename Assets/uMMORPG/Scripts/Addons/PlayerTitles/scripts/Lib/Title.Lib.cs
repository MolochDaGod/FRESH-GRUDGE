#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_Titles()
    {
        AddOn addon = new AddOn()
        {
            name = "player Titles",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOTITLES",
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