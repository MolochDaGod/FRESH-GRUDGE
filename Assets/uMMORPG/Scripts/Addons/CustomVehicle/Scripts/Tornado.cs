using UnityEngine;
using Mirror;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NetworkNavMeshAgent))]
public partial class Tornado : Entity
{
    [Header("Movement")]
    [Range(0, 1)] public float moveProbability = 0.1f; // Chance per second to move
    public float moveDistance = 10f; // Max distance for random movement

    [Header("Respawn")]
    public float deathTime = 30f; // Duration of death state (animation + looting)
    [HideInInspector] public double deathTimeEnd; // Precise end time for death
    public bool respawn = true; // Whether the tornado respawns
    public float respawnTime = 10f; // Time before respawn
    [HideInInspector] public double respawnTimeEnd; // Precise end time for respawn

    [HideInInspector] public Vector3 startPosition; // Initial spawn position

    protected override void Start()
    {
        base.Start();
        InitializeTornado();
    }

    // Initialization for tornado-specific setup
    public void InitializeTornado()
    {
        startPosition = transform.position;
    }

    // Client-side animation updates
    private void LateUpdate()
    {
        if (!isClient || animator == null) return; // Animations only on client

        animator.SetBool("MOVING", state == "MOVING" && movement.GetVelocity() != Vector3.zero);
        animator.SetBool("DEAD", state == "DEAD");
    }

    // FSM Events
    private bool EventDied() => health.current == 0;
    private bool EventDeathTimeElapsed() => state == "DEAD" && NetworkTime.time >= deathTimeEnd;
    private bool EventRespawnTimeElapsed() => state == "DEAD" && respawn && NetworkTime.time >= respawnTimeEnd;
    private bool EventMoveEnd() => state == "MOVING" && !movement.IsMoving();
    private bool EventMoveRandomly() => Random.value <= moveProbability * Time.deltaTime;

    // FSM Server States
    [Server]
    private string UpdateTornadoServer_IDLE()
    {
        if (EventDied()) return "DEAD";
        if (EventMoveRandomly())
        {
            Vector3 randomOffset = Random.insideUnitCircle * moveDistance;
            movement.Navigate(startPosition + new Vector3(randomOffset.x, 0, randomOffset.y), 0);
            return "MOVING";
        }
        return "IDLE";
    }

    [Server]
    private string UpdateTornadoServer_MOVING()
    {
        if (EventDied())
        {
            movement.Reset();
            return "DEAD";
        }
        if (EventMoveEnd()) return "IDLE";
        return "MOVING";
    }

    [Server]
    private string UpdateTornadoServer_DEAD()
    {
        if (EventRespawnTimeElapsed())
        {
            Show();
            movement.Warp(startPosition);
            Revive();
            return "IDLE";
        }
        if (EventDeathTimeElapsed())
        {
            if (respawn) Hide();
            else NetworkServer.Destroy(gameObject);
            return "DEAD";
        }
        return "DEAD";
    }

    [Server]
    protected override string UpdateServer() => UpdateTornadoServer();

    [Server]
    public string UpdateTornadoServer()
    {
        switch (state)
        {
            case "IDLE": return UpdateTornadoServer_IDLE();
            case "MOVING": return UpdateTornadoServer_MOVING();
            case "DEAD": return UpdateTornadoServer_DEAD();
            default:
                Debug.LogError($"Invalid state: {state}");
                return "IDLE";
        }
    }

    [Client]
    protected override void UpdateClient() { }

    // Gizmos for visualization in editor
    private void OnDrawGizmos()
    {
        Vector3 gizmoPosition = Application.isPlaying ? startPosition : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(gizmoPosition, moveDistance);
    }

    // Handle death on server
    [Server]
    public override void OnDeath()
    {
        base.OnDeath();
        deathTimeEnd = NetworkTime.time + deathTime;
        respawnTimeEnd = deathTimeEnd + respawnTime;
    }

    public override void OnInteract()
    {
        Player player = Player.localPlayer;
        if (player == null) return;

        if (Utils.ClosestDistance(player, this) <= player.interactionRange)
        {
            if (isServer)
                OnPlayerInteract();
        }
        else
        {
            Vector3 destination = Utils.ClosestPoint(this, player.transform.position);
            player.movement.Navigate(destination, player.interactionRange);
        }
    }

    // Player interaction logic
    [Server]
    public void OnPlayerInteract()
    {
        if (state == "IDLE")
        {
            Vector3 randomOffset = Random.insideUnitCircle * moveDistance;
            movement.Navigate(startPosition + new Vector3(randomOffset.x, 0, randomOffset.y), 0);
            SetState("MOVING");
        }
        else if (state == "MOVING")
        {
            movement.Reset();
            SetState("IDLE");
        }
    }

    // Method to set the state
    private void SetState(string newState)
    {
        var stateField = typeof(Entity).GetField("_state", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        stateField?.SetValue(this, newState);
    }
}