// Racalvin's Pirate King Boat — Timed World Event
// Place this component on an empty GameObject in the scene (server-side event manager).
// The boat prefab/object should be in the scene but DISABLED at start.
// Assign waypoints as empty GameObjects placed in the scene.

using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grudge
{
    public class RacalvinBoatEvent : NetworkBehaviour
    {
        // ─────────────────────────────────────────────────────────────────
        //  Boat Reference
        // ─────────────────────────────────────────────────────────────────
        [Header("Racalvin's Boat")]
        [Tooltip("The boat GameObject — must be in the scene, DISABLED at start.")]
        public GameObject boatObject;

        [Tooltip("Rigidbody on the boat root. Used for physics-based movement.")]
        public Rigidbody boatRigidbody;

        // ─────────────────────────────────────────────────────────────────
        //  Waypoints  (place empty GameObjects in the scene at these spots)
        // ─────────────────────────────────────────────────────────────────
        [Header("Waypoints  (Spawn → Approach → Dock → Depart → Despawn)")]
        [Tooltip("Off-map origin point — boat spawns here invisible to players.")]
        public Transform spawnPoint;

        [Tooltip("Mid-sea approach point — boat slows and aligns before docking.")]
        public Transform approachPoint;

        [Tooltip("Dock position — boat stops here, crates are dropped.")]
        public Transform dockPoint;

        [Tooltip("Depart point — boat sails here before disappearing.")]
        public Transform departPoint;

        [Tooltip("Despawn point — boat is disabled here (far from map).")]
        public Transform despawnPoint;

        // ─────────────────────────────────────────────────────────────────
        //  Movement
        // ─────────────────────────────────────────────────────────────────
        [Header("Movement")]
        [Tooltip("Sailing speed in units/second.")]
        public float sailSpeed = 9f;

        [Tooltip("How fast the boat turns to face its next waypoint.")]
        public float rotationSpeed = 1.5f;

        [Tooltip("Distance threshold to consider a waypoint 'reached'.")]
        public float arrivalThreshold = 4f;

        // ─────────────────────────────────────────────────────────────────
        //  Schedule
        // ─────────────────────────────────────────────────────────────────
        [Header("Event Schedule")]
        [Tooltip("Minutes between events. Default: every 60 min.")]
        public float eventIntervalMinutes = 60f;

        [Tooltip("Seconds the boat stays docked before departing.")]
        public float dockDuration = 180f;

        [Tooltip("Seconds after server start before the first visit.")]
        public float initialDelaySeconds = 120f;

        // ─────────────────────────────────────────────────────────────────
        //  Supply Drop
        // ─────────────────────────────────────────────────────────────────
        [Header("Supply Drop")]
        [Tooltip("Prefab for supply crate / chest. Must be registered as a Mirror networked prefab.")]
        public GameObject supplyPrefab;

        [Tooltip("How many crates to drop at the dock.")]
        public int supplyCount = 4;

        [Tooltip("Random spread radius around the dock point when placing crates.")]
        public float supplySpreadRadius = 5f;

        [Tooltip("If true, uncollected crates are removed when the boat departs.")]
        public bool despawnUncollectedSupplies = true;

        // ─────────────────────────────────────────────────────────────────
        //  Announcements
        // ─────────────────────────────────────────────────────────────────
        [Header("Announcements")]
        [TextArea(1, 3)]
        public string msgApproaching  = "⚓ A black sail has been spotted on the horizon — Pirate King Racalvin approaches!";

        [TextArea(1, 3)]
        public string msgDocked       = "💀 Racalvin's ship has docked! Raid the supplies before he leaves!";

        [TextArea(1, 3)]
        public string msgDeparting    = "⚓ Racalvin's ship is departing... he will return.";

        // ─────────────────────────────────────────────────────────────────
        //  Internal State (SyncVar so clients can query)
        // ─────────────────────────────────────────────────────────────────
        [SyncVar] public bool eventActive = false;
        [SyncVar] public string eventPhase = "idle"; // "approaching" | "docked" | "departing" | "idle"

        private readonly List<GameObject> spawnedSupplies = new List<GameObject>();

        // ─────────────────────────────────────────────────────────────────
        //  Server Entry
        // ─────────────────────────────────────────────────────────────────
        public override void OnStartServer()
        {
            StartCoroutine(ScheduleLoop());
        }

        // Repeating loop — fires the event then waits
        private IEnumerator ScheduleLoop()
        {
            yield return new WaitForSeconds(initialDelaySeconds);

            while (true)
            {
                yield return StartCoroutine(RunEvent());
                yield return new WaitForSeconds(eventIntervalMinutes * 60f);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        //  Public: GM / admin can force-trigger via inspector button or code
        // ─────────────────────────────────────────────────────────────────
        [Server]
        public void TriggerNow()
        {
            if (!eventActive)
                StartCoroutine(RunEvent());
        }

        // ─────────────────────────────────────────────────────────────────
        //  Main Event Coroutine
        // ─────────────────────────────────────────────────────────────────
        [Server]
        private IEnumerator RunEvent()
        {
            if (boatObject == null || dockPoint == null)
            {
                Debug.LogWarning("[RacalvinEvent] Boat or DockPoint not assigned — skipping event.");
                yield break;
            }

            eventActive = true;

            // ── 1. Spawn / reset boat ──────────────────────────────────
            Vector3 origin = spawnPoint != null ? spawnPoint.position : dockPoint.position + Vector3.back * 300f;
            Quaternion originRot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

            boatObject.transform.SetPositionAndRotation(origin, originRot);
            ResetRigidbody();
            boatObject.SetActive(true);

            // ── 2. Sail to approach point (if set) ─────────────────────
            eventPhase = "approaching";
            RpcAnnounce(msgApproaching);

            if (approachPoint != null)
                yield return StartCoroutine(SailTo(approachPoint));

            // ── 3. Sail to dock ────────────────────────────────────────
            yield return StartCoroutine(SailTo(dockPoint));

            RpcAnnounce(msgDocked);
            eventPhase = "docked";
            SpawnSupplies();

            // ── 4. Wait at dock ────────────────────────────────────────
            yield return new WaitForSeconds(dockDuration);

            // ── 5. Depart ──────────────────────────────────────────────
            eventPhase = "departing";
            RpcAnnounce(msgDeparting);

            if (despawnUncollectedSupplies)
                DespawnSupplies();

            if (departPoint != null)
                yield return StartCoroutine(SailTo(departPoint));

            if (despawnPoint != null)
                yield return StartCoroutine(SailTo(despawnPoint));

            // ── 6. Hide boat ───────────────────────────────────────────
            boatObject.SetActive(false);
            ResetRigidbody();

            eventPhase = "idle";
            eventActive = false;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Sailing (server-side Rigidbody movement, synced via Mirror)
        // ─────────────────────────────────────────────────────────────────
        private IEnumerator SailTo(Transform waypoint)
        {
            Vector3 target = waypoint.position;
            // Keep the same Y (water surface level) to avoid sinking
            target.y = boatObject.transform.position.y;

            while (Vector3.Distance(boatObject.transform.position, target) > arrivalThreshold)
            {
                Vector3 dir = (target - boatObject.transform.position);
                dir.y = 0f;
                dir.Normalize();

                // Rotate smoothly toward waypoint
                if (dir.sqrMagnitude > 0.01f)
                {
                    Quaternion desired = Quaternion.LookRotation(dir);
                    boatObject.transform.rotation = Quaternion.Slerp(
                        boatObject.transform.rotation, desired, rotationSpeed * Time.deltaTime);
                }

                // Move via Rigidbody if available, otherwise Transform
                if (boatRigidbody != null)
                    boatRigidbody.MovePosition(boatObject.transform.position + dir * sailSpeed * Time.fixedDeltaTime);
                else
                    boatObject.transform.position += dir * sailSpeed * Time.deltaTime;

                yield return new WaitForFixedUpdate();
            }

            // Snap to waypoint rotation on arrival
            boatObject.transform.rotation = waypoint.rotation;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Supply crates
        // ─────────────────────────────────────────────────────────────────
        [Server]
        private void SpawnSupplies()
        {
            if (supplyPrefab == null || dockPoint == null) return;

            for (int i = 0; i < supplyCount; i++)
            {
                Vector2 spread = Random.insideUnitCircle * supplySpreadRadius;
                Vector3 pos = dockPoint.position + new Vector3(spread.x, 0f, spread.y);
                Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                GameObject crate = Instantiate(supplyPrefab, pos, rot);
                NetworkServer.Spawn(crate);
                spawnedSupplies.Add(crate);
            }
        }

        [Server]
        private void DespawnSupplies()
        {
            foreach (GameObject crate in spawnedSupplies)
            {
                if (crate != null)
                    NetworkServer.Destroy(crate);
            }
            spawnedSupplies.Clear();
        }

        // ─────────────────────────────────────────────────────────────────
        //  Broadcast announcement to all connected clients
        // ─────────────────────────────────────────────────────────────────
        [ClientRpc]
        private void RpcAnnounce(string message)
        {
            // Uses the uMMORPG UIPopup singleton — works for any addon popup
            if (UIPopup.singleton != null)
                UIPopup.singleton.Show(message);

            Debug.Log("[RacalvinEvent] " + message);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Helper
        // ─────────────────────────────────────────────────────────────────
        private void ResetRigidbody()
        {
            if (boatRigidbody == null) return;
            boatRigidbody.velocity        = Vector3.zero;
            boatRigidbody.angularVelocity = Vector3.zero;
        }
    }
}
