using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

// EXPLORATION AREA
#if _iMMO2D
[RequireComponent(typeof(CircleCollider2D))]
#else
[RequireComponent(typeof(SphereCollider))]
#endif
public class Exploration_Area : NetworkBehaviour
{
    [Header("[-=-=-[ Exploration Area ]-=-=-]")]
    [Tooltip("[Optional] Always show the area name on enter (even if already explored)")]
    public bool noticeOnEnter = true;

    [Header("[Requirements]")]
    public Tools_Requirements explorationRequirements;

    [Header("[Rewards]")]
    public Tools_InteractionRewards explorationRewards;

    [Header("[Popups]")]
    [Tooltip("[Optional] When first explored - shown together with the area name")]
    public Tools_PopupClass explorePopup;

    [Tooltip("[Optional] Shown when entered - together with area name")]
    public Tools_PopupClass enterPopup;

    [Header("[Editor]")]
    public Color gizmoColor = new Color(0, 1, 1, 0.25f);

    public Color gizmoWireColor = new Color(1, 1, 1, 0.8f);

    // -----------------------------------------------------------------------------------
    // OnDrawGizmos
    // @Editor
    // -----------------------------------------------------------------------------------
    private void OnDrawGizmos()
    {
#if !_iMMO2D
        SphereCollider collider = GetComponent<SphereCollider>();

        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(collider.center, collider.radius);
        Gizmos.color = gizmoWireColor;
        Gizmos.DrawWireSphere(collider.center, collider.radius);
        Gizmos.matrix = Matrix4x4.identity;
#endif
    }

#if _SERVER
    // -----------------------------------------------------------------------------------
    // OnTriggerEnter
    // -----------------------------------------------------------------------------------
    [ServerCallback]
#if _iMMO2D
    private void OnTriggerEnter2D(Collider2D co)
#else
    private void OnTriggerEnter(Collider co)
#endif
    {
        Player player = co.GetComponentInParent<Player>();
        if (player && explorationRequirements.isActive && explorationRequirements.checkRequirements(player))
        {
            player.playerAddonsConfigurator.myExploration = this;
            player.playerAddonsConfigurator.ExploreArea();
        }
    }


    // -----------------------------------------------------------------------------------
    // OnTriggerExit
    // -----------------------------------------------------------------------------------
    [ServerCallback]
#if _iMMO2D
    private void OnTriggerExit2D(Collider2D co)
#else
    private void OnTriggerExit(Collider co)
#endif
    {
        Player player = co.GetComponentInParent<Player>();
        if (player && explorationRequirements.isActive && explorationRequirements.checkRequirements(player))
        {
            player.playerAddonsConfigurator.myExploration = null;
            player.Tools_MinimapSceneText(SceneManager.GetActiveScene().name);
        }
    }
#endif
    // -----------------------------------------------------------------------------------
}