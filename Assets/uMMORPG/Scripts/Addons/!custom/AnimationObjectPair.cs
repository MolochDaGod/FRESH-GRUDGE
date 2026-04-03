using System.Collections.Generic;
using UnityEngine;
using Mirror;

[System.Serializable]
public class AnimationObjectPair : NetworkBehaviour
{
	public string animationName;
	public GameObject childObject;
	public bool isVisible;
}

public class EnableChildOnAnimation : NetworkBehaviour
{
	public Animator animator;
	public List<AnimationObjectPair> animationObjectPairs;

	private void Start()
	{
		if (!isLocalPlayer)
		{
			enabled = false;
			return;
		}

		if (animator == null)
		{
			animator = GetComponent<Animator>();
		}
	}

	private void Update()
	{
		if (!isLocalPlayer)
		{
			return;
		}

		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

		foreach (AnimationObjectPair pair in animationObjectPairs)
		{
			// Check if the specified animation is playing
			if (stateInfo.IsName(pair.animationName))
			{
				if (!pair.isVisible)
				{
					pair.isVisible = true;
					CmdEnableChildObject(pair.childObject, true);
				}
			}
			else
			{
				if (pair.isVisible)
				{
					pair.isVisible = false;
					CmdEnableChildObject(pair.childObject, false);
				}
			}
		}
	}

	[Command]
	void CmdEnableChildObject(GameObject childObject, bool enable)
	{
		RpcEnableChildObject(childObject, enable);
	}

	[ClientRpc]
	void RpcEnableChildObject(GameObject childObject, bool enable)
	{
		childObject.SetActive(enable);
	}
}