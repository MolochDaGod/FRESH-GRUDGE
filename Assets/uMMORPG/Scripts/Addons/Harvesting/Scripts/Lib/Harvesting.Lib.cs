#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_Harvesting()
    {
        AddOn addon = new AddOn()
        {
            name = "Harvesting",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOHARVESTING",
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