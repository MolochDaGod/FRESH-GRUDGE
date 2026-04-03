#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_CharacterCreation()
    {
        AddOn addon = new AddOn()
        {
            name = "CharacterCreation",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOCHARACTERCREATION",
            author = "trugord",
            version = "",
            dependencies = "none",
            comments = "none",
            active = true
        };

        addons.Add(addon);
    }
}
#endif