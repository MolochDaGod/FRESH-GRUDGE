#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_CombatRemastered()
    {
        AddOn addon = new AddOn()
        {
            name = "Combat Remastered",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOCOMBATREMASTERED",
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