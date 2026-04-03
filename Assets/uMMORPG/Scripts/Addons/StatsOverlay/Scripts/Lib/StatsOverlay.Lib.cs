#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_StatsOverlay()
    {
        AddOn addon = new AddOn()
        {
            name = "Stats Overlay",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOSTATSOVERLAY",
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