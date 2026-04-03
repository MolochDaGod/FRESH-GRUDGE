using Mirror;
using System.Linq;
using UnityEngine;

// PLAYER

public partial class Player
{
    /*[Header("[-=-=-[ STAMINA ]-=-=-]")]
    public TargetBuffSkill exhaustedBuff;
    public int maxExhaustedBuffLevel = 1;

    protected float _updateTimerStamina;
    */
	// -----------------------------------------------------------------------------------
    // Stamina
    // -----------------------------------------------------------------------------------
    /*public override int Stamina
    {
        get { return Mathf.Min(_stamina, StaminaMax); } // min in case hp>hpmax after buff ends etc.
        set { _stamina = Mathf.Clamp(value, 0, StaminaMax); }
    }
    
    // -----------------------------------------------------------------------------------
    // Update_Stamina
    // @Server
    // -----------------------------------------------------------------------------------
    [ServerCallback]
    private void Update_Stamina()
    {
        if (exhaustedBuff == null || !isServer) return;

        // -- Delayed Update (once per second instead of once per frame)

        if (Time.TimeLogout > _updateTimerStamina)
        {

            // -- apply or remove burdened
            if (Stamina <= 0)
            {
                skills.AddOrRefreshBuff(new Buff(exhaustedBuff, maxExhaustedBuffLevel));
            }
            else
            {
                Tools_RemoveBuff(exhaustedBuff);
            }

            _updateTimerStamina = Time.TimeLogout + cacheTimerInterval;
        }
    }*/

    // -----------------------------------------------------------------------------------
}
