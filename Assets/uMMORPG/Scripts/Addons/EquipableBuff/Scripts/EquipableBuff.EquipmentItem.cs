using UnityEngine;
using UnityEditor;
using Mirror;
// EQUIPMENT ITEM

public partial class EquipmentItem
{

    [Header("[-=-[ EQUIPMENT BUFFS ]-=-]")]
    public TargetBuffSkill onEquipBuffSelf;

    public int onEquipBuffSelfLevel;

    [Header("[-=-[ On getting Hit Buff: Self ]-=-]")]
    public TargetBuffSkill onHitBuffSelf;

    public int onHitBuffSelfLevel;
    [Range(0, 1)] public float onHitBuffSelfApplyChance;

    [Header("[-=-[ On Attack Buff: Target ]-=-]")]
    public TargetBuffSkill onAttackBuff;

    public int onAttackBuffLevel;
    [Range(0, 1)] public float onAttackBuffApplyChance;
    public float maxApplyRange;
 

    // -----------------------------------------------------------------------------------
    // ApplyOnAttackBuffTarget
    // -----------------------------------------------------------------------------------
    [Command]
    public void ApplyOnAttackBuffTarget(Player player, Entity entity)
    {
#if _SERVER
        //Debug.Log("Command apply to target");
        _ApplyOnAttackBuffTarget(player, entity);
#endif
    }

#if _SERVER
    [Server]
    public void _ApplyOnAttackBuffTarget(Player player, Entity entity)
    {
        //Debug.Log("Server apply to target ?");
        if (onAttackBuff == null || onAttackBuffApplyChance <= 0) return;

        if (UnityEngine.Random.value <= onAttackBuffApplyChance)
        {
            if (Utils.ClosestDistance(player, entity) <= maxApplyRange)
            {
                //Debug.Log("OK?");
                entity.skills.AddOrRefreshBuff(new Buff(onAttackBuff, onAttackBuffLevel));
                entity.Tools_SpawnEffect(player, onAttackBuff);
            }
        }
    }
    // -----------------------------------------------------------------------------------
    // ApplyOnHitBuffSelf
    // -----------------------------------------------------------------------------------
    // [Command]
    public void ApplyOnHitBuffSelf(Player player)
    {

        //Debug.Log("[Server] _ApplyOnHitBuffSelf server exec");
        if (onHitBuffSelf == null || onHitBuffSelfApplyChance <= 0) return;

        Entity oldTarget = null;

        // set target to player before applying buff
        if (player.target != null)
            oldTarget = player.target;

        player.target = player;

        if (UnityEngine.Random.value <= onHitBuffSelfApplyChance)
#if _iMMO2D
            onHitBuffSelf.Apply(player, onHitBuffSelfLevel, player.transform.position);
#else
            onHitBuffSelf.Apply(player, onHitBuffSelfLevel);
#endif
        // restore target
        if (oldTarget != null)
            player.target = oldTarget;
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    // [Command]
    public void ApplyEquipableBuff(Player player)
    {
        //Debug.Log("[ClientRpc] _ApplyEquipableBuff server exec");
        if (onEquipBuffSelf == null) return;

        Entity oldTarget = null;

        // set target to player before applying buff
        if (player.target != null)
            oldTarget = player.target;

        player.target = player;

#if _iMMO2D
        onEquipBuffSelf.Apply(player, onEquipBuffSelfLevel, player.transform.position);
#else
        onEquipBuffSelf.Apply(player, onEquipBuffSelfLevel);
#endif

        // restore target
        if (oldTarget != null)
            player.target = oldTarget;
    }
#endif
    // -----------------------------------------------------------------------------------
}
