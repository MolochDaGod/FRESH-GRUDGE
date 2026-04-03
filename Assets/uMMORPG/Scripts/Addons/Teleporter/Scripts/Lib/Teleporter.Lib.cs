#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_Teleporter()
    {
        AddOn addon = new AddOn()
        {
            name = "Teleporter",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOTELEPORTER",
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