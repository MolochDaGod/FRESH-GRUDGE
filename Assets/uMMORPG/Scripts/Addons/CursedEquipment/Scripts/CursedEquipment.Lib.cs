#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_CursedEquipment()
    {
        AddOn addon = new AddOn();

        addon.name          = "CursedEquipment";
        addon.basis         = "uMMORPG3d Remastered";
        addon.define        = "_iMMOCURSEDEQUIPMENT";
        addon.author        = "Trugord";
        addon.version       = "2020.3.37-37.1";
        addon.dependencies  = "none";
        addon.comments      = "none";
        addon.active        = true;

        addons.Add(addon);
    }
}
#endif