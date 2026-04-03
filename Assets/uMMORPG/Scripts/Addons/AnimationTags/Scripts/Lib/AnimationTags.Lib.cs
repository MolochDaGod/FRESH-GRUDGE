#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_AnimationTags()
    {
        AddOn addon = new AddOn();

        addon.name          = "Animation Tags";
        addon.basis         = "uMMORPG3d Remastered";
        addon.define        = "_iMMOANIMATIONTAGS";
        addon.author        = "Trugord";
        addon.version       = "2020.3.37-37.1";
        addon.dependencies  = "none";
        addon.comments      = "none";
        addon.active        = true;

        addons.Add(addon);
        
    }
}
#endif