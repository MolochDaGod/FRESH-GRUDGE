using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Area_Revive : MonoBehaviour
{
    // Update is called once per frame
    private void OnTriggerEnter(Collider co)
    {
        Player player = co.GetComponentInParent<Player>();
        if (player.health.current == 0)
        {
            player.Revive(0.5f);
        }
    }
}
