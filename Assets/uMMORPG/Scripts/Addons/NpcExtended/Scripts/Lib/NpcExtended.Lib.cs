#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_NpcExtended()
    {
        AddOn addon = new AddOn()
        {
            name = "Npc Extended",
            basis = "uMMORPG3d Remastered 2D and 3D",
            define = "_iMMONPCEXTENDED",
            author = "Trugord",
            version = "",
            dependencies = "",
            comments = "",
            active = true
        };
        addons.Add(addon);
    }
}
#endif