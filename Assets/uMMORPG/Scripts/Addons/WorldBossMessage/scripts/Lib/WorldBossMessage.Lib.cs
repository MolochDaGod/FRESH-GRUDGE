#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_WorldBossMessage()
    {
        AddOn addon = new AddOn()
        {
            name = "WorldBossMessage",
            basis = "uMMORPG 3D & 2D Remastered",
            define = "_iMMOWORLDBOSSMESSAGE",
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