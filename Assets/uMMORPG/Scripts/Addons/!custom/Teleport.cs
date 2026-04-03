using System.Text;
using UnityEngine;
using Mirror;

[CreateAssetMenu(menuName="ADDON/Skills/Skill Teleport", order=998)]
public class Teleport : ScriptableSkill
{

	[Header("TeleportMouse")]
	[Tooltip("Teleport when you use the skill. Yes/No")]
	public bool teleportMouse = false;
	[Tooltip("The distance between you and the location when you teleport. Default is 1.")]
	public float offDistance = 1f;
    // Ignores targets
    public override bool CheckTarget(Entity caster) {
		caster.target = caster;
		return true;  
    }

    // Always able to swing
    public override bool CheckDistance(Entity caster, int skillLevel, out Vector3 destination) {
		destination = caster.transform.position;
		return true;
    }

    public override void Apply(Entity caster, int skillLevel) {
        Debug.Log("isServer: " + caster.isServer);
		

		var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast (ray, out hit))
		{
			var targetpos = hit.point;
			//caster.LookAtY(targetpos);
			var direction = (targetpos);
			var distance = Vector3.Distance(targetpos, caster.transform.position);
			var position = Vector3.MoveTowards (targetpos, caster.transform.position += Vector3.up *1, distance - offDistance);
			caster.movement.Warp(position);
		}
    }

    // tooltip /////////////////////////////////////////////////////////////////
    public override string ToolTip(int skillLevel, bool showRequirements = false) {
        StringBuilder tip = new StringBuilder(base.ToolTip(skillLevel, showRequirements));
        return tip.ToString();
    }
}
