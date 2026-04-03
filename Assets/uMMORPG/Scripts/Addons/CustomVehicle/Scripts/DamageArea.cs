
//(c8

using Mirror;
using System.Collections.Generic;
using UnityEngine;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    public class DamageArea : MonoBehaviour
    {
        public Entity owner;
        public int damage = 10;
        public float damageDelay = 2f;
        public bool damageFalloff = true;
        public float fallOffRange = 100f;

        public bool dmgPlayer = true;
        public bool dmgMonster = false;

        private bool damageActive = false;
        private List<Entity> entityInside = new List<Entity>();
        private float lastDamageTime = 0f;
        private int finalDamage = 0;

        [ServerCallback]
        public void OnTriggerEnter(Collider other)
        {
            Entity entity = other.GetComponent<Entity>();
            if (entity == null)
                entity = other.GetComponentInParent<Entity>();

            EnterDamageArea(entity);
        }

        [ServerCallback]
        public void OnTriggerExit(Collider other)
        {
            Entity entity = other.GetComponent<Entity>();
            if (entity == null)
                entity = other.GetComponentInParent<Entity>();

            ExitDamageArea(entity);
        }

        [ServerCallback]
        public void EnterDamageArea(Entity entity)
        {
            if (entity != null && entity != owner && entity.health.current > 0 && !entityInside.Contains(entity))
            {
                if (entity is Player && !dmgPlayer)
                    return;
                if (entity is Monster && !dmgMonster)
                    return;

                entityInside.Add(entity);

                damageActive = true;
            }
        }

        [ServerCallback]
        public void ExitDamageArea(Entity entity, bool forceExit = false)
        {
            if (entity != null && entityInside.Contains(entity))
            {
                int index = entityInside.IndexOf(entity);
                entityInside.RemoveAt(index);
            }

            if (entityInside.Count == 0)
                damageActive = false;
        }

        [ServerCallback]
        private void Update()
        {
            if (damageActive)
            {
                if (Time.time - lastDamageTime >= damageDelay)
                {
                    for (int i = 0; i < entityInside.Count; i++)
                    {
                        if (entityInside[i] == null)
                            continue;

                        finalDamage = damage;

                        // If use the distance to diminish the damage done
                        if (damageFalloff)
                        {
                            // Check for distance, maybe we collide with a big collider and entity is far way
                            float distance = Vector3.Distance(owner.transform.position, entityInside[i].transform.root.position);

                            finalDamage = (int)(finalDamage * Mathf.Clamp01(1f - distance / fallOffRange));
                        }

                        owner.combat.DealDamageAt(entityInside[i], finalDamage);
                        //owner.combat.DealDamageAt(entityInside[i], finalDamage, entityInside[i].transform.position, Vector3.one, entityInside[i].collider);
                    }

                    lastDamageTime = Time.time;
                }
            }
        }
    }
}
