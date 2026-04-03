using Mirror;

// PLAYER
public partial class Player
{
    // -----------------------------------------------------------------------------------
    // Cmd_Healer
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_Healer()
    {
        if (state == "IDLE" &&
            target != null &&
            target.isAlive &&
            isAlive &&
            target is Npc &&
            Utils.ClosestDistance(this, target) <= interactionRange &&
            ((Npc)target).npcHealer.healingServices.Valid(this))
        {
            Npc npc = (Npc)target;

            gold -= npc.npcHealer.healingServices.getCost(this);

            if (npc.npcHealer.healingServices.healHealth)
            {
                int diff = health.max - health.current;
                health.current = health.max;
                combat.RpcOnReceivedHeal(diff);
            }

            if (npc.npcHealer.healingServices.healMana)
                mana.current = mana.max;

            if (npc.npcHealer.healingServices.removeBuffs)
                Tools_CleanupStatusBuffs();

            if (npc.npcHealer.healingServices.removeNerfs)
                Tools_CleanupStatusNerfs();

#if _iMMOCURSEDEQUIPMENT && _iMMOTOOLS
            if (npc.npcHealer.healingServices.unequipMaxCursedLevel > 0)
                UnequipCursedEquipment(npc.npcHealer.healingServices.unequipMaxCursedLevel); // in Tools
#endif
        }
    }

    // -----------------------------------------------------------------------------------
}