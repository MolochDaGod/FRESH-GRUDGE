#if _iMMOTOOLS
using UnityEngine;

// TELEPORTATION TARGET

[System.Serializable]
public partial class Tools_TeleportationTarget
{

    [Header("[Warning this is Old]")]
    public Transform targetPosition;
    public string sceneTarget;
/*
    [Header("[-=-=-[ Target Teleportation Info ]-=-=-]")]
    public string nameTeleportTarget;
#if !_iMMO2D
    public Vector3 targetTeleportPosition;
#else
    public Vector2 vectorTargetPosition;
#endif


#if !_iMMO2D
    public float GetDistance(Vector3 currentPosition)
#else
    public float GetDistance(Vector2 position)
#endif
    {

#if !_iMMO2D
        return Vector3.Distance(targetTeleportPosition, currentPosition);
#else
        return Vector2.Distance(targetPosition, currentPosition);
#endif
    }
*/
    //private bool onWaitingTeleporting = false;
    // -----------------------------------------------------------------------------------
    // name
    // -----------------------------------------------------------------------------------
    public string name
    {
        get
        {
            if (targetPosition != null)
                return targetPosition.name;
            else
                return "";
        }
    }

    // -----------------------------------------------------------------------------------
    // getDistance
    // Returns the distance of the stated transform to the target
    // -----------------------------------------------------------------------------------
    public float getDistance(Transform transform)
    {
#if !_iMMO2D
        return Vector3.Distance(targetPosition.position, transform.position);
#else
        return Vector2.Distance(targetPosition.position, transform.position);
#endif
    }

    // -----------------------------------------------------------------------------------
    // Valid
    // -----------------------------------------------------------------------------------
    public bool Valid
    {
        get
        {
            return targetPosition != null;
        }
    }

    // -----------------------------------------------------------------------------------
    // OnTeleport
    // @Server
    // -----------------------------------------------------------------------------------
    public void OnTeleport(Player player)
    {
        if (!player || !Valid) return;
        if (sceneTarget != "")
        {
            //on passe a waiting Téléport car on et pas sur la même scene
            //onWaitingTeleporting = true;
           // playerTo = player;
            //SceneLoaderAsync.Instance.LoadScene(sceneTarget);      <----------------------------------------------------- à décomenté
            
            player.Tools_Warp(targetPosition.position);
            //if (onWaitingTeleporting)
            //{
                Debug.Log("on attend la le téléchargement complet de la map avant la téléportation téléportation");
                /*if (SceneLoaderAsync.Instance.LoadingProgress == 100 && SceneLoaderAsync.Instance.LoadedScene)   < -----------------------------------------------------à décomenté
                {
                    Debug.Log("NetworManagerMMOForceDisconnect");
                    
                }*/
            //}
        }
        else
        {
            player.Tools_Warp(targetPosition.position);
        }
    }
    // -----------------------------------------------------------------------------------

}
#endif