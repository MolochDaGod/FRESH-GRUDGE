#if UNITY_EDITOR && _iMMOTOOLS
public partial class DefinesManager
{
    public static void Constructor_WorldEvents()
    {
        AddOn addon = new AddOn()
        {
            name = "World Events",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOWORLDEVENTS",
            author = "Trugord",
            version = "2020.3.37-37.1",
            dependencies = "_Tools, Extended Quests",
            comments = "none",
            active = true
        };

        addons.Add(addon);
    }
}
#endif