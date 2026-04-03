
using Mirror;
using UnityEngine;

// HAZARD FLOOR
#if _iMMO2D
[RequireComponent(typeof(CircleCollider2D))]
#else
[RequireComponent(typeof(Collider))]
#endif
public class AreaHazardFloor : NetworkBehaviour
{
#if _SERVER || UNITY_EDITOR
    [Header("[-=-[ Hazard Floor Settings ]-=-]")]
    public HazardBuff[] onEnterBuff;

    public TargetBuffSkill[] onExitBuff;

    public bool showMessage = false;
    [Header("[-=-[ Popups ]-=-]")]
    public Tools_PopupClass enterPopup;

    public Tools_PopupClass exitPopup;

    [Header("[-=-[ Editor ]-=-]")]
    public Color gizmoColor = new Color(0, 1, 1, 0.25f);

    public Color gizmoWireColor = new Color(1, 1, 1, 0.8f);

    // -----------------------------------------------------------------------------------
    // OnDrawGizmos
    // @Editor
    // -----------------------------------------------------------------------------------
    private void OnDrawGizmos()
    {
#if !_iMMO2D
        Collider collider = GetComponent<Collider>();

#endif
    }

    // -----------------------------------------------------------------------------------
    // Start
    // -----------------------------------------------------------------------------------
    public virtual void Start()
    {
        foreach (HazardBuff buff in onEnterBuff)
        {
            if (buff.buff != null)
            {
                buff.protectiveRequirements.setParent(this.gameObject);
            }
        }
    }
#endif
#if _SERVER

    // -------------------------------------------------------------------------------
    // OnTriggerEnter
    // -------------------------------------------------------------------------------
#if _iMMO2D
    private void OnTriggerEnter2D(Collider2D co)
#else
    private void OnTriggerStay(Collider co)
#endif
    {
        Entity entity = co.GetComponentInParent<Entity>();
        if (entity)
        {
            entity.HazardFloorEnter(onEnterBuff);

            if (entity is Player player && showMessage)
                player.Tools_ShowPopup(enterPopup.message);
        }
    }

    // -------------------------------------------------------------------------------
    // OnTriggerExit
    // -------------------------------------------------------------------------------
#if _iMMO2D
    private void OnTriggerExit2D(Collider2D co)
#else
    private void OnTriggerExit(Collider co)
#endif
    {
        Entity entity = co.GetComponentInParent<Entity>();
        if (entity)
        {
            entity.HazardFloorLeave(onExitBuff);

            if (entity is Player player && showMessage)
                player.Tools_ShowPopup(exitPopup.message);
        }
    }
#endif
    // -------------------------------------------------------------------------------
}
