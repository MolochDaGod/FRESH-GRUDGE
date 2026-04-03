#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_Crafting()
    {
        AddOn addon = new AddOn()
        {
            name = "Crafting Extended",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOCRAFTING",
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