using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
 
/**
Instructions:
1. Get the car (model), child it to a new empty game object. Reset (zero-out) the car model childed to the new empty game object transform, but you can copy the transform of the car model before and paste it to the new empty game object, so the actual 
    position of the car where it was before doesn't get lost.
    Important is, that the model itself has a zero-position but it also has the needed rotation (so any rotation needed for the car to drive in the right direction should be applied to the model, not the parent gameobject).
2. Click on the new game object you created and deactivate Static (right corner of the inspector) -> apply to all child objects
3. Add this class as a component to the new game object
5. The placed nodes in the world are just an example, you can easily add more via selecting one and just go CTRL-D and pulling it on the X+Z axis rect to the next street corner/curve.
6. Assuming there's all the nodes existing you want/need for this particular car: Choose the node you want your car the spawn with (you have to use the nodes!)
   Meaning: Select the starting node in the scene-view, then it will highlight the position node (game object) in the hierarchy. Click it on the hierarchy and then copy its transform and paste it OVER your parent car game object. 
   The car should now be at the exact same spot as the position node game object itself.
7. Now click the small lock symbol on the inspector after selecting your car parent object (new game object (which will be the new car object) - This will make the route-drawing easier.
8. There is an array in the inspector called PathPositions. Go ahead and just click in the scene view on the first node (which HAS TO BE the node your car is spawned on) and then select in the hierarchy. 
   Now just DRAG AND DROP the position node (game object) from the hierarchy on your (still empty up to this point) PathPositions array. This way it will be added as the first index.
9. Continue by just clicking the first drive-to-position node in the scene-view and do like it in step 8. Just drag and drop over the newly selected position-node gameobject to the array in the inspector, where it will automatically add it as the next index.
10. The LAST position node should be in a straight line with the FIRST one (meaning the spawn), because when the car realizes there's no more positions left in the array it will "restart" and move to the first index (0 = spawn position node)
    from its current position. That's why the last position node should not be around a turn/corner relative to the start position node.

IMPORTANT STUFF: WHEN SPAWNING THE POSITIONS: 
 - USE THE SAME Y-COORDINATES FOR ALL CARS AND PUT THE SCRIPT ALONE ON THE PARENT OBJECT FOR THE OBJECTS MODELS ETC. (MOVE OR ROTATE THE MODEL IN THE CHILD OBJECT IF NECCESSARY)
 - THE INDEX 0 OF THE COODINATES SHOULD BE THE SPAWN POSITION OF THE OBJECT AT THE SAME TIME
 - IF YOU NEED TO RESET THE DRIVE-TO-POSITIONS: JUST SET THE ARRAY SIZE TO ZERO AND START AGAIN

 **/

public class PresetMovementBehaviour : MonoBehaviour
{
    [Header("Positions in order (Include a spawn position at index 0!)")]
    [Tooltip("The spawn position has to be a transform separate from the object to move, otherwise it will obviously change during runtime." + 
             "Only change the spawn position at runtime if you don't want the movement-behaviour to be in a real 'loop' with the same spots all the time!")]
    [SerializeField] private Transform[] _PathPositions;

    [Header("Tolerances")]
    [Tooltip("This is the tolerance at which the position snaps exactly to the destination. Adjust this to be as smooth as possible." + 
             "Also very important: You may have to adjust this to a higher value if you increase the speed! Especially with a high speed and no braking before the target position. " + 
             "Otherwise the object can snap multiple times and maybe snap to the target position itself into eternity, because it's too fast to register the tolerance.\n" + 
             "A value of 1 is usually enough even for high speeds. For more smoothing you could turn it down then.")]
    [SerializeField, Range(0f, 3f)] private float _RemainingDistanceTolerance = 0.1f;

    [Tooltip("Possibly very important: If you increase the turn speed, then you MAY have to increase this value as well, you have to work out a smooth as possible solution. " +
             "This is the tolerance which is used to compare the wanted rotation to the current rotation!" + 
             "Usually with a value of 5 this can be ignored and left as it is.")]
    [SerializeField, Range(0.1f, 10f)] private float _AngleToCompareWith = 5f;

    [Header("Locomotion")]
    [SerializeField, Range(1f, 300f)] private float _MaxVelocity = 10;
    [SerializeField] bool _UseAcceleration = true;
    [SerializeField, Range(0.001f, 5f)] private float _Acceleration = 0.08f;
    [SerializeField, Range (1f, 500f)] private float _TurnSpeed = 175;

    [Tooltip("In case the object should slow down shortly before the target")]
    [SerializeField] private bool _BrakeBeforeNextPosition = true;
    [SerializeField] private float _BrakeStartingDistance = 3.5f;
    [SerializeField, Range(1f, 25f)] private float _MinVelocityWhenBraking = 5.5f;

    private Vector3 _CurrentDestination = Vector3.zero;
    private float _CurrentVelocity = 0f;
    private int _CurrentPositionIndex = 0;
    private float _TotalDistanceAtBrakeStart = 0f;
    private bool _HasBrakingStarted = false;
    private bool _IsRotationDone = false; // Set the starting rotation to NOT done, so let him evaluate the needed rotation before moving


    private void Start()
    {
        if (_PathPositions.Length == 0) { Debug.LogError("Movement: No path positions found."); return; }

        //_NavAgent.destination = this.transform.position;

        // Set the first position (which HAS to be the spawn position (!))
        this.transform.position = _PathPositions[0].position;
    }


    // Update the object (nav mesh) and move it via the nav mesh
    // Rotate it separately from the nav mesh (!)
    // TODO No animation or steering at all yet - Just simple, plain movement
    // TODO Also: This only works on the Y-Axis for now
    private void Update()
    {
        // Get the forward vector of the next target
        Vector3 forwardDirectionOfNextPosition = (_PathPositions[_CurrentPositionIndex].position - this.transform.position).normalized;
        float remainingDistanceToTarget = Vector3.Distance(this.transform.position, _PathPositions[_CurrentPositionIndex].position);

        // Rotation is done AND the distance between the nav mesh and the target is smaller than the tolerance value? Also: Has the position already been snapped and the index been incremented?
        if (_IsRotationDone &&
            remainingDistanceToTarget < _RemainingDistanceTolerance)
        {
            // Snap-set the position after arriving, but only once for this movement-position
            this.transform.position = _PathPositions[_CurrentPositionIndex].position; // Snap-set the position exactly to the wanted one!

            _CurrentDestination = this.transform.position; // No new destination for now (!)...
            _IsRotationDone = false; // ...because the rotation needs to be done again first, so the rotation to not done here
            _CurrentVelocity = 0f; // Reset the velocity for the next acceleration

            _CurrentPositionIndex++; // Increment up front, for the next destination position to set later

            // If next position doesn't exist, use the first position again (reset), but start from the last position obviously
            if (_CurrentPositionIndex > _PathPositions.Length - 1)
            {
                _CurrentPositionIndex = 0;
            }
        }
        // Rotation not done? Rotate before setting the movement destination!
        else if (!_IsRotationDone)
        {
            // Calculate the desired rotation
            Quaternion desiredRotation = Quaternion.LookRotation(forwardDirectionOfNextPosition);

            // Evaluate the rotation to compare with the current rotation (this transforms rotation except for the Y-Axis (!))
            Quaternion rotationToCompare = Quaternion.Euler(new Vector3(this.transform.rotation.eulerAngles.x, desiredRotation.eulerAngles.y, this.transform.rotation.eulerAngles.z));

            // If the two positions (desired and current) have only the difference of _AngleToCompareWith-tolerance value..
            // Check BEFORE adding rotation
            if (Quaternion.Angle(this.transform.rotation, rotationToCompare) <= _AngleToCompareWith)
            {
                // Snap-set the rotation exactly to the wanted one - you may have to adjust the tolerance values if you want this to look smooth
                // Too much speed probably looks less smooth
                this.transform.rotation = Quaternion.Euler(new Vector3(this.transform.rotation.eulerAngles.x,
                                                                       desiredRotation.eulerAngles.y,
                                                                       this.transform.rotation.eulerAngles.z));

                // Rotation is done for this destination...
                _IsRotationDone = true;

                // ...meaning one can set the new destination now, so the nav mesh starts to move accordingly
                _CurrentDestination = _PathPositions[_CurrentPositionIndex].position;
            }

            // Determine wether to turn left or right (depending on whats the smaller angle)
            float turnDirectionModifier = CalculateDirectionModifier(this.transform.forward, forwardDirectionOfNextPosition);
            if (turnDirectionModifier == 0f && _CurrentPositionIndex == 0)
            {
                // If the rotation on the y-axis is wrong in the beginning, the object may not start to move, so do a work around here
                // Add some rotation for the next frame then...
                this.transform.rotation = Quaternion.Euler(new Vector3(this.transform.rotation.eulerAngles.x,
                                           this.transform.rotation.eulerAngles.y + 5f,
                                           this.transform.rotation.eulerAngles.z));
            }

            // Add some rotation here
            this.transform.rotation = Quaternion.Euler(new Vector3(this.transform.rotation.eulerAngles.x,
                                                        this.transform.rotation.eulerAngles.y + _TurnSpeed * turnDirectionModifier * Time.deltaTime,
                                                        this.transform.rotation.eulerAngles.z));

            // Draw desired rotation direction when rotating
            // Debug.DrawRay(this.transform.position, forwardDirectionOfNextPosition * 500f, Color.red);
        }
        // Is currently moving in this case
        else
        {
            // Debug.DrawLine(this.transform.position, _PathPositions[_CurrentPositionIndex].position, Color.cyan);

            // If the distance to the target position is accordingly AND braking is wanted: React and brake
            if (_BrakeBeforeNextPosition)
            {
                if (_BrakeStartingDistance <= _RemainingDistanceTolerance)
                {
                    Debug.LogWarning("The brake starting distance is smaller or equal than the snapping-tolerance (Remaining Distance Tolerance), so it was set a little higher, otherwise it would never begin to break and snap instantly.");
                    _BrakeStartingDistance = _RemainingDistanceTolerance + 1.5f;
                }

                if (remainingDistanceToTarget <= _BrakeStartingDistance &&
                    remainingDistanceToTarget > _RemainingDistanceTolerance)
                {
                    if (!_HasBrakingStarted)
                    {
                        _TotalDistanceAtBrakeStart = remainingDistanceToTarget; // Only get this distance once ofc!
                        _HasBrakingStarted = true;
                    }

                    // Calculate the deceleration value needed so the object is standing almost still when reaching this transforms position (not the snap distance, should work too though)
                    _CurrentVelocity *= (remainingDistanceToTarget / _TotalDistanceAtBrakeStart);

                    // Very important: Clamp the velocity to a min value, otherwise at some point the velocity will be zero and the object will never reach its destination
                    _CurrentVelocity = Mathf.Clamp(_CurrentVelocity, _MinVelocityWhenBraking, _MaxVelocity);
                }
                else if (remainingDistanceToTarget < _RemainingDistanceTolerance)
                {
                    _HasBrakingStarted = false;
                }
            }

            // No braking YET or wanted AT ALL?
            // Just use normal acceleration OR not, depending on the state of _UseAcceleration
            if ((_BrakeBeforeNextPosition && remainingDistanceToTarget > _BrakeStartingDistance) ||
                !_BrakeBeforeNextPosition)
            { 
                // Accelerate or the max velocity directly, depending on a booleon
                if (_UseAcceleration)
                {
                    _CurrentVelocity += _Acceleration;
                    _CurrentVelocity = Mathf.Clamp(_CurrentVelocity, 0f, _MaxVelocity);
                }
                else
                {
                    _CurrentVelocity = _MaxVelocity;
                }
            }

            // Actually move the object in the targets direction with the current, newly caluclated velocity
            if (this.transform.position != _CurrentDestination)
                this.transform.position += forwardDirectionOfNextPosition * _CurrentVelocity * Time.deltaTime;
        }
    }


    // Returns the direction modifier for the rotation so it knows which way to rotate is the shorter one
    private float CalculateDirectionModifier(Vector3 thisForward, Vector3 targetForward)
    {
        Vector3 crossProduct = Vector3.Cross(thisForward, targetForward);

        if (crossProduct.y > 0f)
            return 1f;
        else if (crossProduct.y < 0f)
            return -1f;

        Debug.LogWarning("Movement: CalculateDirectionModifier calculated 0f on " + this.gameObject.name);
        return 0f;
    }
}