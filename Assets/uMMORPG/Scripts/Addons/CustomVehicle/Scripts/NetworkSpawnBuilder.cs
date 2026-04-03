
//(c8

using UnityEngine;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    public class NetworkSpawnBuilder : MonoBehaviour
    {
        public GameObject[] allGameObjectForNetwork;

        private void Awake()
        {
            for (int i = 0; i < allGameObjectForNetwork.Length; i++)
            {
                var go = allGameObjectForNetwork[i];
                if (go == null) continue;

                // Skip if already registered by NetworkManager.spawnPrefabs to avoid
                // the "Replacing existing prefab" warnings every time StartClient is called.
                if (!Mirror.NetworkClient.prefabs.ContainsValue(go))
                    Mirror.NetworkClient.RegisterPrefab(go);
            }
        }
    }
}
