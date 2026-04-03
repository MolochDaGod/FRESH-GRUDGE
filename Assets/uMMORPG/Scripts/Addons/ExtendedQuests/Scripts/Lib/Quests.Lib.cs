#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_Quests()
    {
        AddOn addon = new AddOn()
        {
            name = "Extended Quests",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOQUESTS",
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