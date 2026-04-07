#if _iMMOTOOLS
#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_Tools()
    {
        AddOn addon = new AddOn();

        addon.name          = "Tools";
        addon.basis         = "uMMORPG3d Remastered";
        addon.define        = "_iMMOTOOLS";
        addon.author        = "Trugord";
        addon.version       = "2021.2.8-37.28";
        addon.dependencies  = "none";
        addon.comments      = "none";
        addon.active        = true;

        addons.Add(addon);
        DefineSymbols_EditorTools.AddScriptingDefine(addon.define); // mandatory
#if !_MYSQL && !_SQLITE
        DefineSymbols_EditorTools.AddScriptingDefine("_SQLITE"); // mandatory
#endif
    }
}
#endif
#endif