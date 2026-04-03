#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_Leaderboard()
    {
        AddOn addon = new AddOn()
        {
            name = "Leaderboard",
            basis = "uMMORPG3d Remastered 3d & 2d",
            define = "_iMMOLEADERBOARD",
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