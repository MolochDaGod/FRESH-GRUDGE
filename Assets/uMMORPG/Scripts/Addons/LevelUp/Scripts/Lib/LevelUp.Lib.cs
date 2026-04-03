#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_LevelUp()
    {
        AddOn addon = new AddOn();

        addon.name = "Level Up";
        addon.basis = "uMMORPG 2D and 3D Remastered";
        addon.define = "_iMMOLEVELUP";
        addon.author = "Trugord";
        addon.version = "";
        addon.dependencies = "none";
        addon.comments = "none";
        addon.active = true;

        addons.Add(addon);
    }
}
#endif