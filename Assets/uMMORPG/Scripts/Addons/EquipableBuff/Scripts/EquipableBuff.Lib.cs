#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_EquipableBuffs()
    {
        AddOn addon = new AddOn()
        {
            name = "Equipable Buff",
            basis = "uMMORPG3d Remastered 3d and 2d",
            define = "_iMMOEQUIPABLEBUFF",
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

