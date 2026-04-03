using Mirror;
using UnityEngine;

// BALLISTIC PROJECTILE

[RequireComponent(typeof(SphereCollider), typeof(Rigidbody), typeof(NetworkIdentity))]
public partial class BallisticProjectile : Projectile
{
    [Header("[-=-[ Visual Effects ]-=-]")]
    [Tooltip("[Required] Should be 15-45 for best results")]
    [Range(10f, 80f)] public float angle = 15f;

    // -----------------------------------------------------------------------------------
    // Init
    // -----------------------------------------------------------------------------------
    public override void Init(Entity entity)
    {
        GetComponent<Rigidbody>().velocity = CalcBallisticVelocityVector(caster.transform, target.collider.bounds.center, angle);
        base.Init(target);
    }

    // -----------------------------------------------------------------------------------
    // OnStartClient
    // -----------------------------------------------------------------------------------
    public override void OnStartClient()
    {
        GetComponent<Rigidbody>().velocity = CalcBallisticVelocityVector(caster.transform, target.collider.bounds.center, angle);
    }

    // -----------------------------------------------------------------------------------
    // OnTriggerEnter
    // -----------------------------------------------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer || bArrivedAtTarget) return;

        if (wallTag != "" && other.gameObject.tag == wallTag)
        {
            OnDestroyDelayed();
            return;
        }

        Entity candidate = other.gameObject.GetComponentInParent<Entity>();
        Debug.Log("Ballistic target : " + candidate);
        if (isServer && candidate != null && candidate != caster && caster.CanAttack(candidate) && candidate.isAlive)
        {
            Debug.Log("Ballistic target  if : " + candidate);

            OnProjectileImpact();

            if (destroyDelay != 0)
            {
                Invoke("OnDestroyDelayed", destroyDelay);
            }
            else
            {
                OnDestroyDelayed();
            }
        }
    }

    // -----------------------------------------------------------------------------------
}
