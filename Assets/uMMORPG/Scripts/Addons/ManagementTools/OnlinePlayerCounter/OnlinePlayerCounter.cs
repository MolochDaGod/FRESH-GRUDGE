#if _SQLITE
using Mirror;
using UnityEngine;

public class OnlinePlayerCounter : NetworkBehaviour
{
    public static OnlinePlayerCounter singleton;

    [HideInInspector, SyncVar(hook = nameof(OnPropertyChanged))] public int numPlayers;

    void Awake()
    {
        // initialize singleton
        if (singleton == null) singleton = this;
    }

    void OnPropertyChanged(int oldValue, int newValue)
    {
        if (isClient)
        {
            // update is called once after online value changes,
            // to avoid updating players buttons once per frame
            UIManagement.singleton.OnlineValue = newValue;
            Invoke(nameof(PlayerListUpdate), 0.25f);
        }
    }

    void PlayerListUpdate()
    {
        if (UIManagement.singleton.canvas.enabled)
        {
            if (string.IsNullOrWhiteSpace(UIManagement.singleton.nameInput.text))
            {
                UIManagement.singleton.ShowOnline();
            }
            else
            {
                UIManagement.singleton.SearchOnline();
            }
        }
    }
}

#endif
