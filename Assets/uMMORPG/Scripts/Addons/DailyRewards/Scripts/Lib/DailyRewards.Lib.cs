#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_DailyRewards()
    {
        AddOn addon = new AddOn();

        addon.name          = "DailyRewards";
        addon.basis         = "uMMORPG Remastered";
        addon.define        = "_iMMODAILYREWARDS";
        addon.author        = "Trugord";
        addon.version       = "2021.3.20-38.1";
        addon.dependencies  = "none";
        addon.comments      = "none";
        addon.active        = true;

        addons.Add(addon);
    }
}
#endif