#if _iMMOMESHSWITCHER
using UnityEngine;

// EQUIPMENT ITEM
public partial class EquipmentItem
{
    [Header("[-=-[ Mesh Switcher ]-=-]")]
    public int[] meshIndex;

    public Material meshMaterial;
    public SwitchableColor[] switchableColors;
}
#endif