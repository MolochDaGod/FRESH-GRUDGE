using Mirror;
using UnityEngine;

// ARROW PROJECTILE

[RequireComponent(typeof(NetworkIdentity))]
public partial class ArrowProjectile : Projectile
{
    private Entity _myTarget;
    // -----------------------------------------------------------------------------------
    // Start
    // -----------------------------------------------------------------------------------
    public override void Init(Entity target)
    {
        base.Init(target);
        _myTarget = target;
    }

    // -----------------------------------------------------------------------------------
    // FixedUpdate
    // -----------------------------------------------------------------------------------
    public override void FixedUpdate()
    {
        if (target != null && caster != null)
        {
            checkWall();

            Vector3 goal = target.collider.bounds.center;
            transform.position = Vector3.MoveTowards(transform.position, goal, speed * Time.fixedDeltaTime);
            transform.LookAt(goal);

            if (isServer && transform.position == goal)
            {
                if (target.isAlive)
                    OnProjectileImpact(_myTarget);

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
        else if (isServer) NetworkServer.Destroy(gameObject);
    }

    // -----------------------------------------------------------------------------------
    // checkWall
    // -----------------------------------------------------------------------------------
    protected void checkWall()
    {
        if (!isServer || wallTag == "") return;

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 0.25f))
        {
            if (hit.collider.tag == wallTag)
            {
                if (data.impactEffect != null)
                {
                    GameObject go = Instantiate(data.impactEffect.gameObject, transform.position, Quaternion.identity);
                    go.GetComponent<OneTimeTargetSkillEffect>().caster = caster;
                    go.GetComponent<OneTimeTargetSkillEffect>().target = target;
                    NetworkServer.Spawn(go);
                    NetworkServer.Destroy(gameObject);
                }
            }
        }
    }

    // -----------------------------------------------------------------------------------
}
