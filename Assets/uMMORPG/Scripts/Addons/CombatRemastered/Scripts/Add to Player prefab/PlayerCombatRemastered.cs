using UnityEngine;
using Mirror;

public class PlayerCombatRemastered : NetworkBehaviour
{
    public Player player;
    public PlayerEquipment playerEquipment;

    public Tmpl_CombatRemasteredConfiguration classCombatConfiguration;
    // Our combat types that we have avaliable to use.
    [Header("Combat Remastered")]
    //public int weaponMainSlot = 0;
    //public int weaponOffSlot = 4;

    [HideInInspector] public int mainHandSlot = 0;
    [HideInInspector] public int offHandSlot = 0;
    [HideInInspector] public int twohHandSlot =0;
    [HideInInspector] public int rangedSlot = 0;
    //public ScriptableSkill[] skillDefaults;

    [HideInInspector] public bool changingEquipment = false;

    private WeaponType currentCombatType = WeaponType.Unarmed;
    //private Skill[] usableSkills;
    //private Animator animator;
    //private ItemSlot mainSlot, offSlot;
    private ItemSlot _mainHandSlot, _offHandSlot, _twoHandSlot, _rangedSlot;
    //private ItemSlot mainHandItem, offHandItem, twoHandItem, rangedItem;
    //private Item mainItem, offItem;

    private ScriptableItem autoAttack;

    private int skillIndex;
    
    private bool weapon = false;
    //private float waitTimer = 0;
    //private float resetTimer = 0.05f;

    // Grab our animator and player then populate our scriptable skills into actual skills for later use.

    public override void OnStartLocalPlayer()
    {

        if (player.combatRemastered)
        {
            int islot = 0;
            foreach (EquipmentInfo tslot in ((PlayerEquipment)player.equipment).slotInfo)
            {
                // Main Hand
                if (tslot.EquipmentPart == classCombatConfiguration.mainHand)
                    mainHandSlot = islot;
                // OffHand
                if (tslot.EquipmentPart == classCombatConfiguration.OffHand)
                    offHandSlot = islot;
                // TwoHand
                if (tslot.EquipmentPart == classCombatConfiguration.twoHand)
                    twohHandSlot = islot;
                // Ranged
                if (tslot.EquipmentPart == classCombatConfiguration.ranged)
                    rangedSlot = islot;

                ++islot;
            }

            playerEquipment.onEquipmentChanged.SetListener(onEquipmentChanged_CombatRemastered);
        }
    }

    // Update our auto attack so we actually perform the animation.
   /* private void Update()
    {
        if (player.combatRemastered && player != null)
        {
            if (player.isClient && player.state == "CASTING")
                if (player.skills.skills[0].IsCasting())
                {
                    //player.animator.SetBool(player.skills.skills[player.autoAttack].name, player.skills.skills[0].IsCasting());


                    Debug.Log(" >>> >>> >>> " + player.skills.skills[player.autoAttack].name);
                    //Debug.Log(" >>> >>> >>> " + player.autoAttack);
                }
        }
    }*/

    public int SmetSkillIndex()
    {
        int index = 0;
        /*if(weapon && autoAttack != null)
        {
            Debug.Log("skillIndex Checking : " + autoAttack.name);
            WeaponItem item = (WeaponItem)autoAttack;
            if (item.autoAttackSkill != null)
            {
                Debug.Log("Auto attack name : " + item.autoAttackSkill);
            }
        }*/
        return index;
    }

    public float CastRangeWeaponSkill()
    {
        float ret = 0f;
        if (weapon && autoAttack != null)
        {
            WeaponItem item = (WeaponItem)autoAttack;
            //ret = item.autoAttackSkill.castRange.baseValue;
            Debug.Log("item :" + item.name);
        }
        return ret;
    }

    public void TryUseWeaponSkill()
    {
        Debug.Log("<color=red>TryUseWeaponSkill</color>");
        if (weapon)
        {
            Debug.Log("item :" + autoAttack.name);
        }
        //player.equipment
    }

    public void onEquipmentChanged_CombatRemastered()
    {

    }

    // Checks our equipment for unwanted equipped items, sets our combat type, then sends command to remove items if need.
    //[Client]
    public void AutoRemoveEquipment(int slot)
    {
        _mainHandSlot = player.equipment.slots[mainHandSlot];
        _offHandSlot = player.equipment.slots[offHandSlot];
        _twoHandSlot = player.equipment.slots[twohHandSlot];
        _rangedSlot = player.equipment.slots[rangedSlot];

        // si on a une arme en main gauche et/ou droite il faut obligatoirement déséquipé les élément dans 2h et ranged
        if ((slot == mainHandSlot && _mainHandSlot.amount > 0) || (slot == offHandSlot && _offHandSlot.amount > 0))
        {
            //
            weapon = true;
            autoAttack = (_mainHandSlot.amount > 0) ? _mainHandSlot.item.data : _offHandSlot.item.data;
            //Debug.Log("lol...");
            WeaponItem t = (WeaponItem)autoAttack;

            for (int i = 0; i < player.skills.skills.Count; i++)
            {
                //Debug.Log(" " + player.skills.skills[i].data.name + " < vs > " + t.autoAttackSkill.name);
                if (t.autoAttackSkill != null && player.skills.skills[i].data.name == t.autoAttackSkill.name)
                    skillIndex = i;
            }
            //Debug.Log("skill index :" + skillIndex);
            player.autoAttack = skillIndex;
            player.useSkillWhenCloser = skillIndex;

            player.CmdAutoRemoveEquipment(twohHandSlot);
            player.CmdAutoRemoveEquipment(rangedSlot);
        }
        // sinon si on à une arme 2h on déséquipe ranged et main gauche/droite
        else if (slot == twohHandSlot && _twoHandSlot.amount > 0)
        {
            //
            weapon = true;
            autoAttack = _twoHandSlot.item.data;

            player.CmdAutoRemoveEquipment(mainHandSlot);
            player.CmdAutoRemoveEquipment(offHandSlot);
            player.CmdAutoRemoveEquipment(rangedSlot);
        }
        // sinon si on à une arme à distance on déséquipe les 2h et main gauche/droite
        else if (slot == rangedSlot && _rangedSlot.amount > 0)
        {
            //
            weapon = true;
            autoAttack = _rangedSlot.item.data;

            player.CmdAutoRemoveEquipment(mainHandSlot);
            player.CmdAutoRemoveEquipment(offHandSlot);
            player.CmdAutoRemoveEquipment(twohHandSlot);
        }

        // sinon rien
        else if (_mainHandSlot.amount == 0 && _offHandSlot.amount == 0 && _twoHandSlot.amount == 0 && _rangedSlot.amount == 0)
        {
            weapon = false;
            autoAttack = null;
            skillIndex = 0;
            //currentCombatType = WeaponType.Unarmed; SetupCombatType(); return;
        }

    }
    public void AutoRemoveEquipment(ItemSlot slot)
    {
        _mainHandSlot = player.equipment.slots[mainHandSlot];
        _offHandSlot = player.equipment.slots[offHandSlot];
        _twoHandSlot = player.equipment.slots[twohHandSlot];
        _rangedSlot = player.equipment.slots[rangedSlot];


        // si on a une arme en main gauche et/ou droite il faut obligatoirement déséquipé les élément dans 2h et ranged
        if (_mainHandSlot.amount > 0 || _offHandSlot.amount > 0)
        {
            //
            player.CmdAutoRemoveEquipment(player.equipment.slots.IndexOf(_twoHandSlot));
            player.CmdAutoRemoveEquipment(player.equipment.slots.IndexOf(_rangedSlot));
        }
        // sinon si on à une arme 2h on déséquipe ranged et main gauche/droite
        else if (_twoHandSlot.amount > 0)
        {
            //
            player.CmdAutoRemoveEquipment(player.equipment.slots.IndexOf(_mainHandSlot));
            player.CmdAutoRemoveEquipment(player.equipment.slots.IndexOf(_offHandSlot));
            player.CmdAutoRemoveEquipment(player.equipment.slots.IndexOf(_rangedSlot));
        }
        // sinon si on à une arme à distance on déséquipe les 2h et main gauche/droite
        else if (_rangedSlot.amount > 0)
        {
            //
            player.CmdAutoRemoveEquipment(player.equipment.slots.IndexOf(_mainHandSlot));
            player.CmdAutoRemoveEquipment(player.equipment.slots.IndexOf(_offHandSlot));
            player.CmdAutoRemoveEquipment(player.equipment.slots.IndexOf(_twoHandSlot));

            currentCombatType = WeaponType.RangedThrown; SetupCombatType(); return;
        }
        // sinon rien
        else
        {
            //
            player.CmdAutoRemoveEquipment(player.equipment.slots.IndexOf(_mainHandSlot));
            player.CmdAutoRemoveEquipment(player.equipment.slots.IndexOf(_offHandSlot));
            //
            player.CmdAutoRemoveEquipment(player.equipment.slots.IndexOf(_twoHandSlot));
            player.CmdAutoRemoveEquipment(player.equipment.slots.IndexOf(_rangedSlot));
            currentCombatType = WeaponType.Unarmed; SetupCombatType(); return;
        }

        /*if (waitTimer <= Time.time)
        {
            waitTimer = Time.time + resetTimer;
            changingEquipment = true;

            // Set our item slot, item, and scriptable item early to not use un-needed resources.
            mainSlot = player.equipment.slots[mainHandSlot];
            offSlot = player.equipment.slots[weaponOffSlot];
            mainItem = mainSlot.item;
            offItem = offSlot.item;

            if (mainSlot.amount == 0 && offSlot.amount == 0) { currentCombatType = WeaponType.Unarmed; SetupCombatType(); return; }

            if (slot.amount == 0) return;

            // Simplify our equipped item slots to scriptable items and our current slot changed.
            EquipmentItem changedScriptItem = (EquipmentItem)slot.item.data;
            print(changedScriptItem.equipType);
            switch (changedScriptItem.equipType)
            {
                case WeaponType.Slash1Hand:
                    if (offSlot.amount > 0 && player.inventory.SlotsFree() >= offSlot.amount)
                    {
                        EquipmentItem offScriptItem = (EquipmentItem)offSlot.item.data;
                        if ((offScriptItem.equipType == WeaponType.RangedBow || offScriptItem.equipType == WeaponType.RangedGun) && mainSlot.amount > 0)
                        {
                            player.CmdAutoRemoveEquipment(player.equipment.slots.IndexOf(offSlot));
                            player.inventory.Remove(mainItem, 1);
                        }
                    }
                    currentCombatType = WeaponType.Slash1Hand;
                    break;

                case WeaponType.Pierce1Hand:
                    if (offSlot.amount > 0 && player.inventory.SlotsFree() >= offSlot.amount)
                    {
                        EquipmentItem offScriptItem = (EquipmentItem)offSlot.item.data;
                        if (offScriptItem.equipType != WeaponType.Shield || offScriptItem.equipType != WeaponType.Dual)
                            player.CmdAutoRemoveEquipment(player.equipment.slots.IndexOf(offSlot));
                    }
                    currentCombatType = WeaponType.Pierce1Hand;
                    break;

                case WeaponType.Bludgeon1Hand:
                    if (offSlot.amount > 0 && player.inventory.SlotsFree() >= offSlot.amount)
                    {
                        EquipmentItem offScriptItem = (EquipmentItem)offSlot.item.data;
                        if (offScriptItem.equipType != WeaponType.Shield || offScriptItem.equipType != WeaponType.Dual)
                            player.CmdAutoRemoveEquipment(player.equipment.slots.IndexOf(offSlot));
                    }
                    currentCombatType = WeaponType.Bludgeon1Hand;
                    break;

                case WeaponType.Slash2Hand:
                    if (offSlot.amount > 0 && player.inventory.SlotsFree() >= offSlot.amount)
                        player.CmdAutoRemoveEquipment(player.equipment.slots.IndexOf(offSlot));
                    currentCombatType = WeaponType.Slash2Hand;
                    break;

                case WeaponType.Pierce2Hand:
                    if (offSlot.amount > 0 && player.inventory.SlotsFree() >= offSlot.amount)
                        player.CmdAutoRemoveEquipment(player.equipment.slots.IndexOf(offSlot));
                    currentCombatType = WeaponType.Pierce2Hand;
                    break;

                case WeaponType.Bludgeon2Hand:
                    if (offSlot.amount > 0 && player.inventory.SlotsFree() >= offSlot.amount)
                        player.CmdAutoRemoveEquipment(player.equipment.slots.IndexOf(offSlot));
                    currentCombatType = WeaponType.Bludgeon2Hand;
                    break;

                case WeaponType.RangedThrown:
                    if (offSlot.amount > 0 && player.inventory.SlotsFree() >= offSlot.amount)
                    {
                        EquipmentItem offScriptItem = (EquipmentItem)offSlot.item.data;
                        if (offScriptItem.equipType != WeaponType.Shield)
                            player.CmdAutoRemoveEquipment(player.equipment.slots.IndexOf(offSlot));
                    }
                    currentCombatType = WeaponType.RangedThrown;
                    break;

                case WeaponType.RangedBow:
                    if ((mainSlot.amount > 0 && player.inventory.SlotsFree() >= mainSlot.amount) && offSlot.amount > 0)
                    {
                        player.CmdAutoRemoveEquipment(player.equipment.slots.IndexOf(mainSlot));
                        player.inventory.Remove(offItem, 1);
                    }
                    currentCombatType = WeaponType.RangedBow;
                    break;

                case WeaponType.RangedGun:
                    if ((mainSlot.amount > 0 && player.inventory.SlotsFree() >= mainSlot.amount) && offSlot.amount > 0)
                    {
                        player.CmdAutoRemoveEquipment(player.equipment.slots.IndexOf(mainSlot));
                        player.inventory.Remove(offItem, 1);
                    }
                    currentCombatType = WeaponType.RangedGun;
                    break;

                case WeaponType.Shield:
                    if (mainSlot.amount > 0 && player.inventory.SlotsFree() >= mainSlot.amount)
                    {
                        EquipmentItem mainScriptItem = (EquipmentItem)mainSlot.item.data;
                        if (mainScriptItem.equipType == WeaponType.Slash2Hand || mainScriptItem.equipType == WeaponType.Pierce2Hand || mainScriptItem.equipType == WeaponType.Bludgeon2Hand)
                        {
                            player.CmdAutoRemoveEquipment(player.equipment.slots.IndexOf(mainSlot));
                            player.inventory.Remove(offItem, 1);
                        }

                        if (offSlot.amount > 0)
                            player.inventory.Remove(offItem, 1);
                    }
                    currentCombatType = WeaponType.Shield;
                    break;

                case WeaponType.Dual:
                    if (mainSlot.amount > 0 && player.inventory.SlotsFree() >= mainSlot.amount)
                    {
                        EquipmentItem mainScriptItem = (EquipmentItem)mainSlot.item.data;
                        if (mainScriptItem.equipType == WeaponType.Slash2Hand || mainScriptItem.equipType == WeaponType.Pierce2Hand || mainScriptItem.equipType == WeaponType.Bludgeon2Hand || mainScriptItem.equipType == WeaponType.RangedThrown)
                        {
                            player.CmdAutoRemoveEquipment(player.equipment.slots.IndexOf(mainSlot));
                            player.inventory.Remove(offItem, 1);
                        }

                        if (offSlot.amount > 0)
                            player.inventory.Remove(offItem, 1);
                    }
                    currentCombatType = WeaponType.Dual;
                    break;
            }

            if (mainSlot.amount > 0 && offSlot.amount > 0)
                if (((EquipmentItem)player.equipment.slots[weaponOffSlot].item.data).equipType == WeaponType.Shield && ((EquipmentItem)player.equipment.slots[mainHandSlot].item.data).equipType != WeaponType.RangedThrown)
                    currentCombatType = WeaponType.Shield;

            if (mainSlot.amount > 0 && offSlot.amount > 0)
                if (((EquipmentItem)player.equipment.slots[weaponOffSlot].item.data).equipType == WeaponType.Dual)
                    currentCombatType = WeaponType.Dual;

            SetupCombatType();
        }*/
    }

    // Setup our animators so we use the correct animations and our default auto attack skill.
    private void SetupCombatType()
    {
       Debug.Log("SetupCombatType() apellé");
        // Set all of our animator bools to false so we don't have overlaping issues.
        player.animator.SetBool("UNARMED", false); player.animator.SetBool("SLASH1H", false); player.animator.SetBool("PIERCE1H", false); player.animator.SetBool("BLUDGEON1H", false);
        player.animator.SetBool("SLASH2H", false); player.animator.SetBool("PIERCE2H", false); player.animator.SetBool("BLUDGEON2H", false); player.animator.SetBool("RANGEDTHROWN", false);
        player.animator.SetBool("RANGEDBOW", false); player.animator.SetBool("RANGEDGUN", false); player.animator.SetBool("SHIELD", false); player.animator.SetBool("DUAL", false);
        
      /*  //Set our current animator bool based on weapon type.
        switch (currentCombatType)
        {
            case WeaponType.Unarmed:
                player.animator.SetBool("UNARMED", true);
                player.skills.skills[0] = usableSkills[0];
                Debug.Log("on réattribue le skills[0] à celui du UNARMED");
                player.autoAttack = 0;
                break;

            case WeaponType.Slash1Hand:
                player.animator.SetBool("SLASH1H", true);
                player.skills.skills[0] = usableSkills[1];
                Debug.Log("on réattribue le skills[0] à celui du SLASH1H");
                player.autoAttack = 1;
                break;

            case WeaponType.Pierce1Hand:
                player.animator.SetBool("PIERCE1H", true);
                player.skills.skills[0] = usableSkills[2];
                Debug.Log("on réattribue le skills[0] à celui du PIERCE1H");
                player.autoAttack = 2;
                break;

            case WeaponType.Bludgeon1Hand:
                player.animator.SetBool("BLUDGEON1H", true);
                player.skills.skills[0] = usableSkills[3];
                Debug.Log("on réattribue le skills[0] à celui du BLUDGEON1H" +
                    "");
                player.autoAttack = 3;
                break;

            case WeaponType.Slash2Hand:
                player.animator.SetBool("SLASH2H", true);
                player.skills.skills[0] = usableSkills[4];
                Debug.Log("on réattribue le skills[0] à celui du SLASH2H");
                player.autoAttack = 4;
                break;

            case WeaponType.Pierce2Hand:
                player.animator.SetBool("PIERCE2H", true);
                player.skills.skills[0] = usableSkills[5];
                Debug.Log("on réattribue le skills[0] à celui du PIERCE2H");
                player.autoAttack = 5;
                break;

            case WeaponType.Bludgeon2Hand:
                player.animator.SetBool("BLUDGEON2H", true);
                player.skills.skills[0] = usableSkills[6];
                Debug.Log("on réattribue le skills[0] à celui du BLUDGEON2H");
                player.autoAttack = 6;
                break;

            case WeaponType.RangedThrown:
                player.animator.SetBool("RANGEDTHROWN", true);
                player.skills.skills[0] = usableSkills[7];
                Debug.Log("on réattribue le skills[0] à celui du RANGEDTHROWN");
                player.autoAttack = 7;
                break;

            case WeaponType.RangedBow:
                player.animator.SetBool("RANGEDBOW", true);
                player.skills.skills[0] = usableSkills[8];
                Debug.Log("on réattribue le skills[0] à celui du RANGEDBOW");
                player.autoAttack = 8;
                break;

            case WeaponType.RangedGun:
                player.animator.SetBool("RANGEDGUN", true);
                player.skills.skills[0] = usableSkills[9];
                Debug.Log("on réattribue le skills[0] à celui du RANGEDGUN");
                player.autoAttack = 9;
                break;

            case WeaponType.Shield:
                player.animator.SetBool("SHIELD", true);
                player.skills.skills[0] = usableSkills[10];
                Debug.Log("on réattribue le skills[0] à celui du SHIELD");
                player.autoAttack = 10;
                break;

            case WeaponType.Dual:
                player.animator.SetBool("DUAL", true);
                player.skills.skills[0] = usableSkills[11];
                Debug.Log("on réattribue le skills[0] à celui du DUAL");
                player.autoAttack = 11;
                break;
        }

        changingEquipment = false;*/
    }
}
