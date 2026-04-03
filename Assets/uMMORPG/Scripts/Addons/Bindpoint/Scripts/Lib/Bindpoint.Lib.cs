#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_Bindpoint()
    {
        AddOn addon = new AddOn()
        {
            name = "Bindpoint",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOBINDPOINT",
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