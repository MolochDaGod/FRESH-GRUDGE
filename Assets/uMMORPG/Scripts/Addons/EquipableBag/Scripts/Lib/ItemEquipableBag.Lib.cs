#if UNITY_EDITOR
public partial class DefinesManager
{
    public static void Constructor_ItemEquipableBag()
    {
        AddOn addon = new AddOn()
        {
            name = "Equipable Bag",
            basis = "uMMORPG3d Remastered",
            define = "_iMMOEQUIPABLEBAG",
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