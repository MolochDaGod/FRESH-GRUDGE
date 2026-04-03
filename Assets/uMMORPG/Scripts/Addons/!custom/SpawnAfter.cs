using Mirror;
using UnityEngine;

public class SpawnAfter : NetworkBehaviour
{
	public GameObject objectPrefab;
	public float spawnDelay = 1800f; // 30 minutes in seconds

	private float timer;

	private void Start()
	{
		timer = spawnDelay;
	}

	private void Update()
	{
		if (!isServer)
			return;

		timer -= Time.deltaTime;

		if (timer <= 0f)
		{
			SpawnObject();
			timer = spawnDelay;
		}
	}

	private void SpawnObject()
	{
		GameObject newObject = Instantiate(objectPrefab, transform.position, Quaternion.identity);
		NetworkServer.Spawn(newObject);
	}
}