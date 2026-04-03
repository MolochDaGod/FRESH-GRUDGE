using UnityEngine;
// MONSTER
public class MonsterMinion : MonoBehaviour
{
    public Monster monster;
    [Header("[-=-[ MINION ]-=-]")]
    public Entity myMaster;

    public bool followMaster;
    public bool defendMaster;
    public bool boundToMaster;
    public float stopDistance = 1f;


#if _SERVER && _iMMOMINION
    // -----------------------------------------------------------------------------------
    // LateUpdate
    // -----------------------------------------------------------------------------------
    public void LateUpdate()
    {
        if (myMaster != null && myMaster is Entity master && monster.isAlive)
        {

            if (defendMaster == true && master.target != null && monster.target != master.target)
            {
                monster.target = master.target;
                master.OnAggro(master.target);
                monster.movement.Navigate(master.target.transform.position, stopDistance);

            }

            if (followMaster && monster.target == null && master.target == null && Tools_ClosestDistance.ClosestDistance( monster.collider, master.collider ) > stopDistance  )
            {
                monster.animator.SetBool("MOVING", true);
                monster.movement.Navigate(myMaster.transform.position, stopDistance);
            }

            if (boundToMaster && myMaster != null && !myMaster.isAlive)
                monster.health.current = 0;
        }
    }
#endif
    // -----------------------------------------------------------------------------------
}