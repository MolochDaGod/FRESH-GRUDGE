using Mirror;
using UnityEngine;

// MONSTER
public partial class Monster
{
    public AggroOverlay aggroOverlay;
    private string _currentAggro;
    // -----------------------------------------------------------------------------------
    // OnClientAggro_AggroOverlay
    // -----------------------------------------------------------------------------------

    public override void OnStartServer()
    {
        onAggro.AddListener(EvAggro_Overlay);
    }


    [ClientCallback]
    public void OnClientAggro_AggroOverlay(Entity entity)
    {
        if (aggroOverlay == null) return;

        if (target != entity || !(entity is Player))
            aggroOverlay.Hide();
        else if (target == null || entity is Player)
            aggroOverlay.Show();
    }
    private void EvAggro_Overlay(Entity entity)
    {
        onAggro_overLay();
    }
    [ClientRpc]
    private void onAggro_overLay()
    {
        if (aggroOverlay)
        {
            if (state == "DEAD")
            {
                _currentAggro = null;
                aggroOverlay.Hide();
            }
            else if (target == null)
            {
                _currentAggro = null;
            }
            else
            {
                if (_currentAggro == null || _currentAggro != target.name)
                {
                    _currentAggro = (target != null) ? target.name : null;
                    aggroOverlay.Show();
                }
            }
        }
    }
    // -----------------------------------------------------------------------------------
}