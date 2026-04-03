using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// PLAYER
[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerIndicator))]
[DisallowMultipleComponent]
public partial class PlayerBetterTabTargeting : NetworkBehaviour
{

    [Header("Components")]
    public Player player;
    public PlayerIndicator indicator;
    public float tabTargetMultiplier = 6;

    [Header("Targeting")]
    public KeyCode key = KeyCode.Tab;

    protected int tabTargetIndex = 0;


    void Update()
    {
        // only for local player
        if (!isLocalPlayer) return;

        {
            // key pressed?
            if (Input.GetKeyDown(key)
#if _iMMOMOBILECONTROLS
        || player.targetButtonPressed)
            {
#else
        )
            {
#endif
                TargetNearest();
            }
        }

        player.clearindicator();
    }

    // -----------------------------------------------------------------------------------
    // TargetNearest
    // -----------------------------------------------------------------------------------
    [Client]
    public void TargetNearest()
    {
        if (Input.GetKeyDown(key)
#if _iMMOMOBILECONTROLS
        || player.targetButtonPressed)
        {
            player.targetButtonPressed = false;
#else
		)	{
#endif
            if (player.target == null || player.target.health.current == 0 || player.target == player) // on devrais rajouter un système de delai afin de repartir a zero au bout de 2 secs par exemple
            {
                player.target = null;
                tabTargetIndex = 0;
               // player.indicator.Clear();
            }
            List<Entity> correctedTargets = new List<Entity>();

            int layerMask = ~(1 << 2);

#if _iMMO2D
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, player.interactionRange * tabTargetMultiplier, layerMask);
            foreach (Collider2D hitCollider in hitColliders)
#else
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, player.interactionRange * tabTargetMultiplier, layerMask);
            foreach (Collider hitCollider in hitColliders)
#endif
            {

                    Entity target = hitCollider.GetComponentInParent<Entity>();
                if (target != null && target != this && player.CanAttack(target) && target.isAlive && !correctedTargets.Any(x => x == target))
                    correctedTargets.Add(target);
            }

#if _iMMO2D
            List<Entity> sortedTargets = correctedTargets.OrderBy(x => Vector2.Distance(transform.position, x.transform.position)).ToList();
#else
            List<Entity> sortedTargets = correctedTargets.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).ToList();
#endif
            if (sortedTargets.Count > 0)
            {
                if (tabTargetIndex >= sortedTargets.Count)  tabTargetIndex = 0;

                indicator.SetViaParent(sortedTargets[tabTargetIndex].transform);
                player.CmdSetTarget(sortedTargets[tabTargetIndex].netIdentity);
                sortedTargets.Clear();

                tabTargetIndex++;
            }
            else
            {
                tabTargetIndex = 0;
            }
            correctedTargets.Clear();
        }
    }
    // -----------------------------------------------------------------------------------
}