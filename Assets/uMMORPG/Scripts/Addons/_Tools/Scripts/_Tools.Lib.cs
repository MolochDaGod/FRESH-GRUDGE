#if _iMMOTOOLS
#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_Tools()
    {
        AddOn addon = new AddOn()
        {
            name = "Tools",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOTOOLS",
            author = "Trugord",
            version = "2021.2.8-37.28",
            dependencies = "none",
            comments = "none",
            active = true
        };

        addons.Add(addon);
        DefineSymbols_EditorTools.AddScriptingDefine(addon.define); // mandatory
#if !_MYSQL && !_SQLITE
        DefineSymbols_EditorTools.AddScriptingDefine("_SQLITE"); // mandatory
#endif
    }
}
#endif
#endif