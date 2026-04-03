#if _iMMOPRESTIGECLASSES
using Mirror;

public partial class PlayerAddonsConfigurator
{

    protected PrestigeClassTemplate _prestigeClass;
    [SyncVar] protected int _hashPrestigeClass;


    // -----------------------------------------------------------------------------------
    // OnStartClient_Tools_PrestigeClasses
    // -----------------------------------------------------------------------------------
    public void OnStartClient_PrestigeClasses()
    {
        bool bValid = false;
        int totalEquipment = ((PlayerEquipment)player.equipment).slotInfo.Length;
        for (int index = 0; index < totalEquipment; ++index)
        {
            bValid = RefreshPrestigeClass(index);
        }

        //if (!bValid) prestigeClass = null;
        //Add Events in PlayerEquipment 
        playerEquipment.onRefreshLocation.AddListener(OnRefreshLocation_PrestigeClasses);
    }


    // -----------------------------------------------------------------------------------
    // PrestigeClassTemplate
    // -----------------------------------------------------------------------------------
    public PrestigeClassTemplate prestigeClass
    {
        set { SetPrestigeClass(value); }
        get { return GetPrestigeClass(); }
    }

    // -----------------------------------------------------------------------------------
    // SetPrestigeClass
    // -----------------------------------------------------------------------------------
    private void SetPrestigeClass(PrestigeClassTemplate prestigeClass)
    {
        _hashPrestigeClass = prestigeClass.name.GetStableHashCode();
        _prestigeClass = prestigeClass;
    }

    // -----------------------------------------------------------------------------------
    // GetPrestigeClass
    // -----------------------------------------------------------------------------------
    private PrestigeClassTemplate GetPrestigeClass()
    {
        if (_prestigeClass != null)
            return _prestigeClass;

        PrestigeClassTemplate prestigeClassData;

        if (_hashPrestigeClass != 0 && PrestigeClassTemplate.All.TryGetValue(_hashPrestigeClass, out prestigeClassData))
            _prestigeClass = prestigeClassData;

        return _prestigeClass;
    }


    

    public void OnRefreshLocation_PrestigeClasses(int index)
    {
        bool bValid = player.playerAddonsConfigurator.RefreshPrestigeClass(index);
    }
    // -----------------------------------------------------------------------------------
    // RefreshPrestigeClass
    // -----------------------------------------------------------------------------------
    public bool RefreshPrestigeClass(int index)
    {
        ItemSlot slot = player.equipment.slots[index];
        EquipmentInfo info = ((PlayerEquipment)player.equipment).slotInfo[index];

        if (slot.amount > 0 && ((EquipmentItem)slot.item.data).prestigeClass != null)
        {
            prestigeClass = ((EquipmentItem)slot.item.data).prestigeClass;
            return true;
        }

        return false;
    }

    // -----------------------------------------------------------------------------------
    // CheckPrestigeClass
    // -----------------------------------------------------------------------------------
    public bool CheckPrestigeClass(PrestigeClassTemplate[] requiredPrestigeClasses)
    {
        return player.Tools_checkHasPrestigeClass(requiredPrestigeClasses);
    }
    
    // -----------------------------------------------------------------------------------
    // CanUpgradeSkill
    // -----------------------------------------------------------------------------------
    public bool PrestigeClasses_CanUpgradeSkill(Skill skill)
    {
        return CheckPrestigeClass(skill.data.learnablePrestigeClasses);
    }
    // -----------------------------------------------------------------------------------
}
#endif