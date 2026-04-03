#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_Timegate()
    {
        AddOn addon = new AddOn()
        {
            name = "Timegate",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOTIMEGATE",
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