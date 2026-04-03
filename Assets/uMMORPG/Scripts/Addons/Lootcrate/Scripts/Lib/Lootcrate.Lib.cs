#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_Lootcrate()
    {
        AddOn addon = new AddOn()
        {
            name = "Lootcrate",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOCHEST",
            author = "Trugord",
            version = "",
            dependencies = "none",
            comments = "none",
            active = true
        };

        addons.Add(addon);
    }
}
#endif