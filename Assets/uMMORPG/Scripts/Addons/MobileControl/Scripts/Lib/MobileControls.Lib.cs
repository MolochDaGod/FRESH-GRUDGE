#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_MobileControls()
    {
        AddOn addon = new AddOn()
        {
            name = "Mobile Control",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOMOBILECONTROLS",
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