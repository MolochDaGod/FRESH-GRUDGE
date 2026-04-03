
//(c8

using Mirror;
using UnityEngine;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    public class PassengerFollowVehicle : NetworkBehaviour
    {
        public EnterVehicle enterVehicle;

        // This string is the target to follow data (netId, seatId)
        [SyncVar(hook = nameof(OnSetFollowChange))] public string niToFollow;

        public int seatId = -1;

        private Transform toFollow;

        private void Update()
        {
            if (toFollow != null)
            {
                transform.position = toFollow.position;
                transform.rotation = toFollow.rotation;
            }
        }

        private void Retrying()
        {
            OnSetFollowChange(niToFollow, niToFollow);
        }

        public void OnSetFollowChange(string oldNiToFollow, string newNiToFollow)
        {
            string[] toSplit = niToFollow.Split(',');

            seatId = int.Parse(toSplit[1]);

            if (isServer)
            {
                if (NetworkServer.spawned.ContainsKey(uint.Parse(toSplit[0])))
                {
                    enterVehicle = NetworkServer.spawned[uint.Parse(toSplit[0])].GetComponentInParent<EnterVehicle>();
                    enterVehicle.vehicleData[seatId].objectToTakeControl = transform;
                    toFollow = enterVehicle.vehicleData[seatId].seatPosition;
                }
                else
                {
                    // When you enter the visibility range of other remote player
                    // If player driving the boat appear before the boat
                    // It will retry each sec until boat is networkedspawned
                    Invoke(nameof(Retrying), 1f);
                }
            }
            else
            {
                if (NetworkClient.spawned.ContainsKey(uint.Parse(toSplit[0])))
                {
                    enterVehicle = NetworkClient.spawned[uint.Parse(toSplit[0])].GetComponentInParent<EnterVehicle>();
                    enterVehicle.vehicleData[seatId].objectToTakeControl = transform;
                    toFollow = enterVehicle.vehicleData[seatId].seatPosition;
                }
                else
                {
                    // When you enter the visibility range of other remote player
                    // If player driving the boat appear before the boat
                    // It will retry each sec until boat is networkedspawned
                    Invoke(nameof(Retrying), 1f);
                }
            }
        }
    }
}
