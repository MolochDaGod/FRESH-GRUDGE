#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_NpcRestrictions()
    {
        AddOn addon = new AddOn();

        addon.name          = "NpcRestrictions";
        addon.basis         = "uMMORPG3d Remastered";
        addon.define        = "_iMMONPCRESTRICTIONS";
        addon.author        = "Trugord";
        addon.version       = "2021.3.13-38.1";
        addon.dependencies  = "none";
        addon.comments      = "none";
        addon.active        = true;

        addons.Add(addon);
    }
}
#endif