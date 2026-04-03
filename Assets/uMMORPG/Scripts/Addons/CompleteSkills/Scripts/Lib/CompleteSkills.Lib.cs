#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_CompleteSkills()
    {
        AddOn addon = new AddOn()
        {
            name = "Complete Skills",
            basis = "uMMORPG3d Remastered 3d and 2d",
            define = "_iMMOCOMPLETESKILLS",
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
