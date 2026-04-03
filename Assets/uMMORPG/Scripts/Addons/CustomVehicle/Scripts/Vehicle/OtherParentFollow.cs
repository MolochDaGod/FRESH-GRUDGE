
//(c8

using UnityEngine;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    // Use to make wheels effects follow tire
    public class OtherParentFollow : MonoBehaviour
    {
        public Transform parentToFollow;

        private void Update()
        {
            transform.position = new Vector3(parentToFollow.position.x, parentToFollow.position.y - 0.4f, parentToFollow.position.z);
        }
    }
}
