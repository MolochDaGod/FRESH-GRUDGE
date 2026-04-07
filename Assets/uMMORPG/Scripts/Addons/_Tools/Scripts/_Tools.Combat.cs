#if _iMMOTOOLS_LEGACY_CORE
using System;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.Events;

[Serializable] public class UnityEventIntHeal : UnityEvent<int> { }
public partial class Combat
{

    [Header("Heal Popup")]
    public GameObject healPopupPrefab;

    public UnityEventIntHeal onClientReceivedHeal;
    public void Start()
    {
        onDamageDealtTo.AddListener(DealDamageAt_Tools);
    }

    private void DealDamageAt_Tools(Entity target)
    {
        if (entity == null || !target.isAlive) return;

        target.lastAggressor = entity;
    }

    [Client]
    public void ShowPopupHeal(int amount)
    {
        // spawn the damage popup (if any) and set the text
        if (healPopupPrefab != null)
        {
            // showing it above their head looks best, and we don't have to use
            // a custom shader to draw world space UI in front of the entity
            Bounds bounds = collider.bounds;
#if _iMMO2D
            Vector2 position = new Vector2(bounds.center.x, bounds.max.y);
#else
            Vector3 position = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
#endif

            GameObject popup = Instantiate(damagePopupPrefab, position, Quaternion.identity);
#if _iMMO2D
            popup.GetComponentInChildren<TextMesh>().color = Color.green;
            popup.GetComponentInChildren<TextMesh>().text = "+" + amount.ToString();
#else
            popup.GetComponentInChildren<TextMeshPro>().color = Color.green;
            popup.GetComponentInChildren<TextMeshPro>().text = "+" + amount.ToString();
#endif
            // NetworkServer.Spawn(popup);
        }
    }

    [ClientRpc]
    public void RpcOnReceivedHeal(int amount)
    {
        // show popup above receiver's head in all observers via ClientRpc
        ShowPopupHeal(amount);

        // call OnClientReceivedDamage event
        onClientReceivedHeal.Invoke(amount);
    }
}
#endif