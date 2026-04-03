
//(c8

using Mirror;
using UnityEngine;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    public class EnterVehicleInteraction : MonoBehaviour//, IAdditionalInteractable
    {
        public EnterVehicle enterVehicle;
        public int key;
        public string nameOfkey = "";

        //public float HoldActionTimeForSecondaryInteraction => 0f;

        //public string GetAdditionalInfoText()
        //{
        //    return "";
        //}

        //public string GetAdditionalInteractionText()
        //{
        //    return nameOfkey;
        //}

        //public void OnAdditionalInteractClient(player player)
        //{
        //    //throw new System.NotImplementedException();
        //}

        //public void OnAdditionalInteractServer(player player)
        //{
        //    enterVehicle.EnterVehicleServer(player, key);
        //}

        //public void OnAdditionalSecondaryInteractClient(player player)
        //{
        //    //throw new System.NotImplementedException();
        //}

        //public void OnAdditionalSecondaryInteractServer(player player)
        //{
        //    //throw new System.NotImplementedException();
        //}

        // selection & interaction /////////////////////////////////////////////////
        // use Unity's OnMouseDown function. no need for raycasts.
        void OnMouseDown()
        {
            // joined world yet? (not character selection?)
            // not over UI? (avoid targeting through windows)
            // and in a state where local player can select things?
            if (Player.localPlayer != null &&
                !Utils.IsCursorOverUserInterface() &&
                (Player.localPlayer.state == "IDLE" ||
                 Player.localPlayer.state == "MOVING"))
            {
                // clear requested skill in any case because if we clicked
                // somewhere else then we don't care about it anymore
                //player.localPlayer.useSkillWhenCloser = -1;

                // alive, close enough? then talk
                // use collider point(s) to also work with big entities
                if (Player.localPlayer.health.current > 0 &&
                    ClosestDistance(Player.localPlayer.transform.position, Player.localPlayer.collider, transform.position, GetComponent<Collider>()) <= Player.localPlayer.interactionRange)
                {
                    Player.localPlayer.GetComponent<PlayerVehicle>().CmdTryEnterVehicle(GetComponentInParent<NetworkIdentity>(), key);
                }
                // otherwise walk there
                // use collider point(s) to also work with big entities
                //else
                //{
                //    Vector3 destination = ClosestPoint(transform.position, GetComponent<Collider>(), player.localPlayer.transform.position);
                //    player.localPlayer.movement.Navigate(destination, player.localPlayer.interactionRange);
                //}
            }
        }

        public float ClosestDistance(Vector3 a, Collider b, Vector3 c, Collider d)
        {
            // IMPORTANT: DO NOT use the collider itself. the position changes
            //            during animations, causing situations where attacks are
            //            interrupted because the target's hips moved a bit out of
            //            attack range, even though the target didn't actually move!
            //            => use transform.position and collider.radius instead!
            //
            //            this is probably faster than collider.ClosestPoints too

            // at first calculate the distance from A to B, subtract both radius
            // IMPORTANT: use entity.transform.position not
            //            collider.transform.position. that would still be the hip!
            float distance = Vector3.Distance(a, c);

            // calculate both collider radius
            float radiusA = Utils.BoundsRadius(b.bounds);
            float radiusB = Utils.BoundsRadius(d.bounds);

            // subtract both radius
            float distanceInside = distance - radiusA - radiusB;

            // return distance. if it's <0 because they are inside each other, then
            // return 0.
            return Mathf.Max(distanceInside, 0);
        }

        public Vector3 ClosestPoint(Vector3 a, Collider b, Vector3 point)
        {
            // IMPORTANT: DO NOT use the collider itself. the position changes
            //            during animations, causing situations where attacks are
            //            interrupted because the target's hips moved a bit out of
            //            attack range, even though the target didn't actually move!
            //            => use transform.position and collider.radius instead!
            //
            //            this is probably faster than collider.ClosestPoints too

            // first of all, get radius but in WORLD SPACE not in LOCAL SPACE.
            // otherwise parent scales are not applied.
            float radius = Utils.BoundsRadius(b.bounds);

            // now get the direction from point to entity
            // IMPORTANT: use entity.transform.position not
            //            collider.transform.position. that would still be the hip!
            Vector3 direction = a - point;
            //Debug.DrawLine(point, point + direction, Color.red, 1, false);

            // subtract radius from direction's length
            Vector3 directionSubtracted = Vector3.ClampMagnitude(direction, direction.magnitude - radius);

            // return the point
            //Debug.DrawLine(point, point + directionSubtracted, Color.green, 1, false);
            return point + directionSubtracted;
        }
    }
}
