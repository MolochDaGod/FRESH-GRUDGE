#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_PVPZone()
    {
        AddOn addon = new AddOn()
        {
            name = "PVP Zone And Realms",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOPVP",
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