#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_ItemLevelUp()
    {
        AddOn addon = new AddOn()
        {
            name = "Equipment Level Up",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOITEMLEVELUP",
            author = "Trugord",
            version = "none",
            dependencies = "none",
            comments = "none",
            active = true
        };

        addons.Add(addon);
    }
}
#endif