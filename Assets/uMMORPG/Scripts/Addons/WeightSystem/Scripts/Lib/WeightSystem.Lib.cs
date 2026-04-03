#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_WeightSystem()
    {
        AddOn addon = new AddOn();

        addon.name          = "Weight System";
        addon.basis         = "uMMORPG3d Remastered";
        addon.define        = "_iMMOWEIGHTSYSTEM";
        addon.author        = "Trugord";
        addon.version       = "2020.3.37-37.2";
        addon.dependencies  = "none";
        addon.comments      = "none";
        addon.active        = true;

        addons.Add(addon);
    }
}
#endif