#if UNITY_EDITOR
public partial class DefinesManager
{

    public static void Constructor_LootRules()
    {
        AddOn addon = new AddOn()
        {
            name = "LootRules",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOLOOTRULES",
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