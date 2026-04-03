using UnityEngine;
using Mirror;

// ACTIVATEABLE OBJECT
public partial class ActivateableObject : NetworkBehaviour
{
    [Tooltip("[Required] Link the activated game object here (its usually a child of this)")]
    public GameObject activateableObject;

    [Tooltip("[Required] Is the linked object visible by default or not?")]
    [SyncVar] public bool _visible = true;

    [Tooltip("[Optional] Automatic reset to default visibility after x seconds (0 to disable)")]
    [Min(0)] public float resetVisibility = 0;

#if _iMMOWORLDEVENTS
    [Header("[WORLD EVENT (object visibility is based on event status)]")]
    [Tooltip("[Optional] This world event will be checked")]
    public WorldEventTemplate worldEvent;

    [Tooltip("[Optional] Min count the world event has been completed (0 to disable)")]
    public int minEventCount;

    [Tooltip("[Optional] Max count the world event has been completed (0 to disable)")]
    public int maxEventCount;
#endif

    [Header("[UPDATE THROTTLE]")]
    [SerializeField] [Range(0.01f, 3f)] private float updateInterval = 0.25f;

    protected bool _defaultVisible = true;
    protected float fInterval;


    private void Start()
    {
        _defaultVisible = _visible;
    }


    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    private void Update()
    {
        if (Time.time > fInterval)
        {
            SlowUpdate();
            fInterval = Time.time + updateInterval;
        }
    }

    // -----------------------------------------------------------------------------------
    // SlowUpdate
    // -----------------------------------------------------------------------------------
    private void SlowUpdate()
    {
#if _SERVER
#if _iMMOWORLDEVENTS
        if (worldEvent != null)
        {
            _visible = NetworkManagerMMOWorldEvents.CheckWorldEvent(worldEvent, minEventCount, maxEventCount);
        }
#else
		_visible = _defaultVisible;
#endif
#endif
        activateableObject.SetActive(_visible);
    }
#if _SERVER
    // -----------------------------------------------------------------------------------
    // Toggle
    // -----------------------------------------------------------------------------------
    [ServerCallback]
    public void Toggle(bool visible)
    {
        if (visible != _visible)
        {
            _visible = visible;
            if (resetVisibility > 0)
            {
                Invoke(nameof(Reset), resetVisibility);
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // Reset
    // -----------------------------------------------------------------------------------
    public void Reset()
    {
        Toggle(_defaultVisible);
        CancelInvoke(nameof(Reset));
    }
#endif
    // -----------------------------------------------------------------------------------
}