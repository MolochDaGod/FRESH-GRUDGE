using UnityEngine;

public class ObjectGrowth : MonoBehaviour
{
	public float totalHeightIncrease = 5f; // Total height increase for the object
	public float stageDuration = 1f; // Duration of each growth stage
	private float targetHeight; // Target height for each stage
	private float initialHeight; // Initial height of the object
	private float startTime; // Start time of the current stage
	private float totalElapsedTime; // Total elapsed time

	private void Start()
	{
		initialHeight = transform.localScale.y;
		targetHeight = initialHeight + (totalHeightIncrease / 5f);
		startTime = Time.time;
	}

	private void Update()
	{
		float elapsedTime = Time.time - startTime;
		totalElapsedTime += Time.deltaTime;

		if (totalElapsedTime >= 5f)
		{
			// Stop the growth after 5 seconds
			//destroy the gameobject after it's done growing. Alternatively, replace gameObject with this
			Destroy(this);
			return;
		}

		float t = Mathf.Clamp01(elapsedTime / stageDuration); // Normalized time between 0 and 1

		// Interpolate the height value
		float newHeight = Mathf.Lerp(initialHeight, targetHeight, t);

		// Update the object's scale
		Vector3 newScale = transform.localScale;
		newScale.y = newHeight;
		transform.localScale = newScale;

		// Check if the current stage is complete
		if (elapsedTime >= stageDuration)
		{
			// Move to the next stage
			initialHeight = targetHeight;
			targetHeight += totalHeightIncrease / 5f;
			startTime = Time.time;
		}
	}
}