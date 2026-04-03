#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_Minion()
    {
         AddOn addon = new AddOn();

        addon.name          = "Minion";
        addon.basis         = "uMMORPG3d Remastered";
        addon.define        = "_iMMOMINION";
        addon.author        = "Trugord";
        addon.version       = "2019.4.28-34.2";
        addon.dependencies  = "Tools;";
        addon.comments      = "none";
        addon.active        = true;

        addons.Add(addon);
    }
}
#endif