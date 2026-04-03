#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_AccountUnlockables()
    {

        AddOn addon = new AddOn()
        {
            name = "Account Unlockables",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOACCOUNTUNLOCKABLES",
            author = "Trugord",
            version = "",
            dependencies = "_Tools, UsageRequirements",
            comments = "none",
            active = true
        };
        addons.Add(addon);   
    }
}
#endif