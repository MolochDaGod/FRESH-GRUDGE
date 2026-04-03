#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_Faction()
    {
        AddOn addon = new AddOn()
        {
            name = "Factions",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOFACTIONS",
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