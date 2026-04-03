#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_Attributes()
    {
        AddOn addon = new AddOn()
        {
            name = "Attributes",
            basis = "uMMORPG3d 2D and 3D Remastered",
            define = "_iMMOATTRIBUTES",
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