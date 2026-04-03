#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_WordFilter()
    {
        AddOn addon = new AddOn()
        {
            name = "Word Filter",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOWORDFILTER",
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