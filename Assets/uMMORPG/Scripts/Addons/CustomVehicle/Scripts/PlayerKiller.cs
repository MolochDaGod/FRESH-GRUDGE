
//(c8

using Mirror;
using UnityEngine;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    public class PlayerKiller : MonoBehaviour
    {
        public Entity entityKiller;

        [ServerCallback]
        private void OnTriggerEnter(Collider other)
        {
            Player player = other.GetComponent<Player>();
            if (player == null)
                player = other.GetComponentInParent<Player>();

            if (player != null && entityKiller != null)
            {
                // find the skill that we casted this effect with
                if (player.health.current > 0)
                {
                    //entityKiller.combat.DealDamageAt(player, player.health.current, Vector3.zero, Vector3.one, player.collider);
                    entityKiller.combat.DealDamageAt(player, 999999);
                }
            }
        }
    }
}
