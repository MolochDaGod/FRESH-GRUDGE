
//(c8

using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class NoTargetProjectileSkillEffect : SkillEffect
{
    public float speed = 35f;

    [Header("If projectile touch nothing, it will be destroy after this range")]
    public float maxFlightDistance = 1000f;

    [Header("When use physic, must add NetworkTransform component on prefab root")]
    public bool usePhysic = false;
    public float bulletForce = 30f;

    [HideInInspector] public int damage = 1; // set by skill
    [HideInInspector] public float stunChance; // set by skill
    [HideInInspector] public float stunTime; // set by skill

    // effects like a trail or particles need to have their initial positions
    // corrected too. simply connect their .Clear() functions to the event.
    public UnityEvent onSetInitialPosition;

    public GameObject impactEffects;
    public GameObject impactWaterEffects;
    public GameObject impactUnderwaterEffects;

    public bool continueUnderwater = true;

    private Vector3 initialPosition;

    private bool destroyed = false;
    private bool nowUnderwater = false;

    public override void OnStartServer()
    {
        if (usePhysic)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * bulletForce, ForceMode.Impulse);
        }
    }

    public override void OnStartClient()
    {
        SetInitialPosition();
    }

    private void SetInitialPosition()
    {
        // the projectile should always start at the effectMount position.
        // -> server doesn't run animations, so it will never spawn it exactly
        //    where the effectMount is on the client by the time the packet
        //    reaches the client.
        // -> the best solution is to correct it here once
        if (caster != null)
        {
            transform.position = caster.skills.effectMount.position;
            transform.rotation = caster.skills.effectMount.rotation;
            onSetInitialPosition.Invoke();

            initialPosition = transform.position;
        }
    }

    // fixedupdate on client and server to simulate the same effect without
    // using a NetworkTransform
    private void FixedUpdate()
    {
        if (destroyed)
            return;

        // Check distance from initial position and destroy projectile if max flight distance reached
        if (Vector3.Distance(initialPosition, transform.position) < maxFlightDistance)
        {
            if (!usePhysic)
                transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward, speed * Time.fixedDeltaTime);
        }
        else
        {
            destroyed = true;
            gameObject.SetActive(false);

            if (isServer)
                Invoke(nameof(DestroyProjectile), 1f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (destroyed)
            return;

        if (!isServerOnly)
        {
            if (!other.tag.Contains("Water"))
            {
                if (!nowUnderwater)
                {
                    if (impactEffects != null)
                        Instantiate(impactEffects, transform.position, Quaternion.identity);
                }
                else
                {
                    if (impactUnderwaterEffects != null)
                        Instantiate(impactUnderwaterEffects, transform.position, Quaternion.identity);
                }
            }
            else
            {
                if (impactWaterEffects != null)
                    Instantiate(impactWaterEffects, transform.position, Quaternion.identity);

                if (continueUnderwater)
                {
                    nowUnderwater = true;
                    return;
                }
            }
        }
        else
        {
            if (other.tag.Contains("Water"))
            {
                if (continueUnderwater)
                {
                    nowUnderwater = true;
                    return;
                }
            }
        }

        destroyed = true;
        gameObject.SetActive(false);

        // server: reached it? apply skill and destroy self
        if (isServer)
        {
            Entity entity = other.GetComponent<Entity>();
            if (entity == null)
                entity = other.GetComponentInParent<Entity>();

            if (entity != null && caster != null)
            {
                // find the skill that we casted this effect with
                if (entity.health.current > 0)
                    caster.combat.DealDamageAt(entity, caster.combat.damage + damage, stunChance, stunTime);
            }

            Invoke(nameof(DestroyProjectile), 1f);
        }
    }

    private void DestroyProjectile()
    {
        NetworkServer.Destroy(gameObject);
    }
}
