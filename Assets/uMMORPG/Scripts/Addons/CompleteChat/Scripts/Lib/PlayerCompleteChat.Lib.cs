#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_CompleteChat()
    {
        AddOn addon = new AddOn()
        {
            name = "Complete Chat",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOCOMPLETECHAT",
            author = "Trugord",
            version = "",
            dependencies = "_Tools",
            comments = "none",
            active = true
        };
        addons.Add(addon);
    }
}
#endif