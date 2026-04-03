using UnityEngine;


// NPC
public class NpcExtended  : MonoBehaviour
{
#if _iMMONPCEXTENDED
    public Npc npc;

    [Header("Movement")]
    [Range(0, 1)] public float moveProbability = 0.0f; // chance per second
    public float moveDistance = 10;
    public bool interactStopsMovement = true;
    public float playerRadiusDetect = 1.5f;
    [Tooltip("[Optional] true = keep patrolling between waypoints, false = walk to waypoints just once, then randomly")]


    protected int destPoint = 0;
    protected bool destReached = false;
    // save the start position for random movement distance and respawning
    [HideInInspector] public Vector3 startPosition;


    [Header("Patrol")]
    public bool patrol = true;
    
    [Tooltip("[Optional] Patrols between waypoints.")]
    public Transform[] waypoints;
    
    [Tooltip("[Optional] true = turns final waypoint into start position, false = returns to start position after final waypoint reached")]
    public bool stickToLastWaypoint = true;
    
    [Tooltip("[Optional] 0 = Waypoints only, 1 = Random only, or any in-between.")]
    [Range(0, 1)] public float WaypointToRandomRatio;


    // save the start position for random movement distance and respawning
    [Header("Respawn")]
    public float deathTime = 30f; // enough for animation & looting
    [HideInInspector] public double deathTimeEnd; // double for long term precision
    public bool respawn = true;
    public float respawnTime = 10f;
    [HideInInspector] public double respawnTimeEnd; // double for long term precision


    [Header("Combat")]
    public float followDistance = 20;
    [Range(0.1f, 1)] public float attackToMoveRangeRatio = 0.8f; // move as close as 0.8 * attackRange to a target


    // -----------------------------------------------------------------------------------
    // GetEntityNearby
    // -----------------------------------------------------------------------------------

    public bool GetEntityNearby(float radius)
    {
#if _iMMO2D
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D co in colliders)
#else
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider co in colliders)
#endif
        {
            Entity entity = co.GetComponentInParent<Entity>();
            if ( (entity is Player || entity is Monster || entity is Mount || entity is Pet) && entity != null && npc.Tools_SameRealm(entity))
            {
                return true;
            }
        }

        return false;
    }

    // remplacé dans la fonction appelante
    /**public void GotoRandomPoint()
    {
        Vector2 circle2D = Random.insideUnitCircle * npc.npcExtended.moveDistance;
        npc.movement.Navigate(npc.npcExtended.startPosition + new Vector3(circle2D.x, 0, circle2D.y), 0);
    }*/

    // -----------------------------------------------------------------------------------
    // DoPatrol
    // -----------------------------------------------------------------------------------
    public bool DoPatrol()
    {
        return (
                waypoints.Length > 0 &&
                ((!patrol && !destReached) || patrol)
                );
    }

    // -----------------------------------------------------------------------------------
    // GotoNextWayPoint
    // -----------------------------------------------------------------------------------
    public void GotoNextWayPoint()
    {
        if (waypoints.Length == 0) return;
        npc.movement.Navigate(waypoints[destPoint].position, 0);
        destPoint = (destPoint + 1) % waypoints.Length;
    }
    // -----------------------------------------------------------------------------------
#endif
    }