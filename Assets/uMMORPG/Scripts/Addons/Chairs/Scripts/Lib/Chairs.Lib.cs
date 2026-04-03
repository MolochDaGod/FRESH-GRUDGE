#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_Chairs()
    {
        AddOn addon = new AddOn()
        {
            name = "Chairs",
            basis = "uMMORPG3d Remastered 3d",
            define = "_iMMOCHAIRS",
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