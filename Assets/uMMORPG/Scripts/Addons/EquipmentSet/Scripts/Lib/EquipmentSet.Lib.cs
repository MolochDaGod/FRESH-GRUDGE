#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_EquipmentSet()
    {
        AddOn addon = new AddOn()
        {
            name = "Equipment Set",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOEQUIPMENTSETS",
            author = "Trugord",
            version = "",
            dependencies = "Tools",
            comments = "none",
            active = true
        };

        addons.Add(addon);
    }
}
#endif