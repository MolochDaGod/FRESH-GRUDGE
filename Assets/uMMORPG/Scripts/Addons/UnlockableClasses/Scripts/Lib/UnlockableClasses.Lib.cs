#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_UnlockableClasses()
    {
        AddOn addon = new AddOn()
        {
            name = "Unlockable Classes",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOUNLOCKABLECLASSES",
            author = "Trugord",
            version = "",
            dependencies = "_Tools,CharacterCreation",
            comments = "none",
            active = true
        };
        addons.Add(addon);
    }
}
#endif