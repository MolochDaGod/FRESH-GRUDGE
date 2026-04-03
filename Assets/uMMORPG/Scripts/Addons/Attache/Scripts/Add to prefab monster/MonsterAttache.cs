using UnityEngine;
using Mirror;

public class MonsterAttache : NetworkBehaviour
{
    public Monster monster;
    public Health health;

    [Header("[-=-[ Entity : Attache ]-=-]")]
    public AttachmentChild[] attachedChilds;

    #region SERVER ONLY
#if _SERVER
    public override void OnStartServer()
    {
        //health.onChangeEnergy.AddListener(UpdateEvent);
        //health.onChange.AddListener(UpdateEvent);
        health.onEmpty.AddListener(OnDeath);
    }
    // -----------------------------------------------------------------------------------
    // Update
    // @Server
    // -----------------------------------------------------------------------------------
    [Server]
    private void FixedUpdate() // Not need calculate every frame
    //private void UpdateEvent()
    {
        if (attachedChilds.Length > 0 && monster.isAlive)
        {
            AttacheSpawn();
        }
    }

    // -----------------------------------------------------------------------------------
    // OnDeath
    // @Server
    // -----------------------------------------------------------------------------------
    [Server]
    private void OnDeath()
    {
        AttacheUnspawn();
    }

    // -----------------------------------------------------------------------------------
    // AttacheReset
    // @Server
    // -----------------------------------------------------------------------------------
    public void AttacheReset()
    {
        AttacheUnspawn();
        AttacheSpawn();
    }
    // -----------------------------------------------------------------------------------
    // GetAttache
    // @Server
    // -----------------------------------------------------------------------------------
    protected GameObject GetAttache(int index)
    {
        if (attachedChilds.Length > 0 && attachedChilds.Length >= index && attachedChilds[index].childGameObjects.Length > 0 && attachedChilds[index].childGameObjects.Length >= monster.level.current)
            return attachedChilds[index].childGameObjects[monster.level.current - 1];

        if (attachedChilds.Length > 0 && attachedChilds.Length >= index)
            return attachedChilds[index].childGameObjects[0];

        return attachedChilds[0].childGameObjects[0];
    }

    // -----------------------------------------------------------------------------------
    // AttacheUnspawn
    // @Server
    // -----------------------------------------------------------------------------------
    protected void AttacheUnspawn()
    {
        if (attachedChilds.Length > 0)
        {
            for (int i = 0; i < attachedChilds.Length; ++i)
            {
                if (attachedChilds[i].cacheGameObject != null && attachedChilds[i].autoUnspawn)
                {
                    if (attachedChilds[i].cacheGameObject.GetComponent<Entity>() != null && attachedChilds[i].killOnUnspawn)
                        attachedChilds[i].cacheGameObject.GetComponent<Entity>().health.current = 0;
                    else
                        NetworkServer.Destroy(attachedChilds[i].cacheGameObject);
                    attachedChilds[i].cacheGameObject = null;
                }
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // AttacheSpawn
    // @Server
    // -----------------------------------------------------------------------------------
    protected void AttacheSpawn()
    {
        if (attachedChilds.Length > 0)
        { 
            for (int i = 0; i < attachedChilds.Length; ++i)
            {
                if (attachedChilds[i].autoSpawn && CheckAttachmentConditions(i, Entity.ChildAction.Spawn) && attachedChilds[i].cacheGameObject == null)
                {
                    var go = (GameObject)Entity.Instantiate(GetAttache(i), attachedChilds[i].mountPoint.position + (Random.insideUnitSphere * attachedChilds[i].aroundSpawnPoint), Quaternion.identity);
                  //  Debug.Log("instanciate :" + go.name);
                    if (go)
                    {
                        NetworkServer.Spawn(go);
#if _iMMOMINION
                        if (go.GetComponent<Monster>() != null)
                        {
                            go.GetComponent<Monster>().monsterMinion.myMaster = monster;
                            go.GetComponent<Monster>().monsterMinion.followMaster = attachedChilds[i].followMaster;
                        }
#endif
                        attachedChilds[i].cacheGameObject = go;
                    }
                }
                else if (CheckAttachmentConditions(i, Entity.ChildAction.Destroy) && attachedChilds[i].cacheGameObject != null)
                {
                    NetworkServer.Destroy(attachedChilds[i].cacheGameObject);
                    attachedChilds[i].cacheGameObject = null;
                }
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // CheckAttachmentConditions
    // @Server
    // -----------------------------------------------------------------------------------
    protected bool CheckAttachmentConditions(int index, Entity.ChildAction action)
    {
        foreach (AttachmentChildCondition condi in attachedChilds[index].childConditions)
        {
            if (condi.childAction == action)
            {
                if (
                    (condi.activeBuff == null || monster.Tools_checkHasBuff(condi.activeBuff)) &&
                    (condi.equippedItem == null || monster.Tools_checkHasEquipment(condi.equippedItem)) &&
                    //(condi.inventoryItem == null || Tools_checkHasItem(condi.inventoryItem)) && //TODO MonsterAttache CheckAttachmentConditions manque une vérifications
                    (
                    (condi.parentThreshold == Entity.ParentThreshold.Above && (health.current >= health.max * condi.parentHealth)) ||
                    (condi.parentThreshold == Entity.ParentThreshold.Below && (health.current < health.max * condi.parentHealth)))
                    )
                {
                    return true;
                }
            }
        }

        return false;
    }
#endif

    #endregion 
    // -----------------------------------------------------------------------------------
}