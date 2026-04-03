#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_UsageRequirements()
    {
        AddOn addon = new AddOn()
        {
            name = "Usage Requirements",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOUSAGEREQUIREMENTS",
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
