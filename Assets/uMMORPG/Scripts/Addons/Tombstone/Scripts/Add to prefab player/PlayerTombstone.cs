using Mirror;
using System;
using UnityEngine;


// ENTITY
public class PlayerTombstone : NetworkBehaviour
{
    public Player player;
    public Health health;

    [Header("[-=-[TOMBSTONE]-=-]")]
    public Tmpl_PlayerTombstone playerTombstone;

    // ----------------------------------------------------------------------------------
    // Auto Add in Health event
    // ----------------------------------------------------------------------------------
    public override void OnStartServer()
    {
        health.onEmpty.AddListener(OnDeath);
    }
    // ----------------------------------------------------------------------------------
    // OnDeath
    // ----------------------------------------------------------------------------------
    public void OnDeath()
    {

        if(playerTombstone!= null)
        {
            if(playerTombstone.tombstoneChance > 0 && playerTombstone.tombstoneModel != null)
            {
                long loosedExprerience = Convert.ToInt64(player.experience.max * player.experience.deathLossPercent);
                long xpToTombstone = Convert.ToInt64(loosedExprerience * 0.8);

                //Tombstone  tomb.GetComponent
                Vector3 postition = new Vector3(transform.position.x, transform.position.y + playerTombstone.tombstoneFallHeight, transform.position.z);

                Tombstone prefab = playerTombstone.tombstoneModel;
                GameObject go = Instantiate(prefab.gameObject, postition, transform.rotation);
                Tombstone tomb = go.GetComponent<Tombstone>();
                tomb.owner = player;
                tomb.lostExperience = loosedExprerience;
                tomb.restoreExperience = xpToTombstone;
                tomb.praySkill = playerTombstone.praySkill;
                tomb.destroyAfter = playerTombstone.destroytombDelay;
                NetworkServer.Spawn(go);
            }
        }
    }

    // -----------------------------------------------------------------------------------
}
