#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_HonorShop()
    {
        AddOn addon = new AddOn()
        {
            name = "Honor Shop",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOHONORSHOP",
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