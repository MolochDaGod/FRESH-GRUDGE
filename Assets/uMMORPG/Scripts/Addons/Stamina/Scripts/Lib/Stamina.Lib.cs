#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_Stamina()
    {
        AddOn addon = new AddOn();
        addon.name          = "Stamina";
        addon.basis         = "uMMORPG3d Remastered";
        addon.define        = "_iMMOSTAMINA";
        addon.author        = "Trugord";
        addon.version       = "2021.3.16-38.1";
        addon.dependencies  = "none";
        addon.comments      = "none";
        addon.active        = true;
        addons.Add(addon);
    }
}
#endif