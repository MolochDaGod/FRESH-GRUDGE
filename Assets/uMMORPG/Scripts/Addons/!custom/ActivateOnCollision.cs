using UnityEngine;

[RequireComponent(typeof(Collider))]

public class ActivateOnCollision : MonoBehaviour
{
    public GameObject objectToActivate;

	private void OnTriggerEnter(Collider co)
    {
		Entity entity = co.GetComponentInParent<Entity>();
		if (entity)
		{
			objectToActivate.SetActive(true);
			Invoke("SetFalse",7.0f);
		}
	}
	void SetFalse()
	{
		objectToActivate.SetActive(false);
    }
}