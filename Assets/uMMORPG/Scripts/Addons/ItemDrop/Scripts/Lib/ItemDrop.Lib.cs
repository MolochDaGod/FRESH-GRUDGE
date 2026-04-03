#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_ItemDrop()
    {
        AddOn addon = new AddOn()
        {
            name = "ItemDrop",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOITEMDROP",
            author = "Trugord",
            version = "",
            dependencies = "Tools, Loot Rules",
            comments = "none",
            active = true
        };
        addons.Add(addon);
    }
}
#endif