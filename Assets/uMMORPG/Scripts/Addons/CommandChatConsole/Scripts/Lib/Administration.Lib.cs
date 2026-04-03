#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_Administration()
    {
        AddOn addon = new AddOn()
        {
            name = "Command Chat Console",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOADMINCONSOLE",
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