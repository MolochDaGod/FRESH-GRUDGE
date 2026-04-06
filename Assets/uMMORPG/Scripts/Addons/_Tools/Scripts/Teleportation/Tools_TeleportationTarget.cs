#if _iMMOTOOLS
using UnityEngine;

// TELEPORTATION TARGET

[System.Serializable]
public partial class Tools_TeleportationTarget
{
    public enum TeleportationType { onScene, offScene }


    public Transform targetPosition;
    public string sceneTarget;
    //private Player playerTo;

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
   /* public void Update()
    {
        Debug.Log("rrr");
        if (onWaitingTeleporting)
        {
            Debug.Log("on attend la le téléchargement complet de la map avant la téléportation téléportation");
            if (SceneLoaderAsync.Instance.LoadingProgress == 100 && SceneLoaderAsync.Instance.LoadedScene)
            {
                Debug.Log("NetworManagerMMOForceDisconnect");
                playerTo.Tools_Warp(targetPosition.position);
            }
        }
    }*/
}
#endif