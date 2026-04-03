#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_Travelroutes()
    {
        AddOn addon = new AddOn()
        {
            name = "Travel Routes",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOTRAVEL",
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