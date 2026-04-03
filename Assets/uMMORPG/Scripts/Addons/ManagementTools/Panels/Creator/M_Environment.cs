#if _SQLITE
using UnityEngine;
using Mirror;

public partial class M_Environment : NetworkBehaviour
{
    [HideInInspector, SyncVar] public string uniqueId;

    void Awake()
    {
        transform.name = transform.name.Replace("(Clone)", "").Trim();
    }
}

#endif
