#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_Settings()
    {
        AddOn addon = new AddOn()
        {
            name = "Settings",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOSETTINGS",
            author = "Trugord",
            version = "",
            dependencies = "none",
            comments = "none",
            active = true
        };

        addons.Add(addon);
    }
}
#endif