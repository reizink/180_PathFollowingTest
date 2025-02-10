using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

//Referense: https://www.red3d.com/cwr/steer/

[System.Serializable]
public class Steering : MonoBehaviour
{
    [SerializeField] private Transform _target; //What is this agent seeking/avoiding

    [SerializeField] private float _maxSpeed;   //What is the max speed this agent can move
    [SerializeField] private float _maxForce;   //What is the max force that can be applied, useful to limit things like turning

    [SerializeField] private State _currentState; //What state is this agent currently in //last var

    private Vector3 _velocity;      //What is the current veclocity
    private Vector3 _accel;  //The calculated acceleration, is returned to 0 every frame

    private CreatePath _createPath; //attach leader's Createpath script component
    public float predictionTime = 1f; // How far ahead to predict

    [SerializeField] private float avoidDist = 2f; // Minimum distance to avoid obstacles
    [SerializeField] private float avoidForce = 5f; // Strength of avoidance force
    [SerializeField] private float FollowDist = 1f;

    private void Start()
    {
        _createPath = _target.GetComponent<CreatePath>();
    }

    private void Update()
    {
        switch (_currentState)
        {
            case State.Seek:
                _accel += Seek(_target.position);
                break;
            case State.Flee:
                _accel += Flee();
                break;
            case State.Follow:
                _accel += FollowLeader(); //calls seek
                break;
        }

        // Apply avoidance before moving
        _accel += AvoidObjects();

        if (_velocity.sqrMagnitude > 0.001f) // Prevent jitter when nearly stationary, **x, look forward
        {
            transform.rotation = Quaternion.LookRotation(_velocity.normalized);
        }

        ApplyForce();
    }

    /// Creates a vector from an agents position to a target position
    /// This vector is limited by how fast the agent can move And how quickly it is allowed to turn
    private Vector3 Seek(Vector3 tar)
    {
        Vector3 force = tar - transform.position; //direction vector
        force.Normalize();

        force *= _maxSpeed;
        //velocity subtraction
        force -= _velocity;
        //truncate or clamp
        force = Vector3.ClampMagnitude(force, _maxForce);

        return force;
    }

    /// Flee is the opposite of seek, therefore just do seek but invert the result
    public Vector3 Flee()
    {
        return -Seek(_target.position);
    }

    /// Moves the agent in the calculated dircetion by its acceleration
    private void ApplyForce()
    {
        _velocity += _accel;
        transform.position += _velocity * Time.deltaTime; //familiar movement

        //constant adding will cause problems to accel, so let's reset
        _accel = Vector3.zero;

        transform.position = new Vector3(transform.position.x, 1, transform.position.z); //fixed pos?
    }

    private Vector3 FollowLeader()
    {
        List<Vector3> path = _createPath.waypoints;

        if (path.Count == 0) return Vector3.zero; // No path to follow

        // Step 1: Predict the follower's future position
        Vector3 futurePosition = transform.position + _velocity.normalized * predictionTime;

        // Step 2: Find the closest waypoint in the path
        Vector3? closestPoint = null;
        float closestDistance = Mathf.Infinity;
        int closestIndex = -1;

        for (int i = 0; i < path.Count; i++)
        {
            float distance = Vector3.Distance(futurePosition, path[i]);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = path[i];
                closestIndex = i;
            }
        }

        if (closestPoint == null) return Vector3.zero; // No valid point found

        // Step 3: Predict a future point on the path
        int predictedIndex = Mathf.Min(closestIndex + Mathf.FloorToInt(predictionTime), path.Count - 1);
        Vector3 futurePathPoint = path[predictedIndex];
        

        // Step 4: Check if the future position is within FollowDist of the future path point
        if (Vector3.Distance(futurePosition, futurePathPoint) > FollowDist)
        {
            return Seek(futurePathPoint); // Seek towards the future path point
        }
        else if (closestIndex < path.Count - 1)
        {
            // If we are too close, move to the next point, safe guard
            return Seek(path[closestIndex + 1]);
        }

        return Vector3.zero; // No need to adjust course
    }

    // Uses a sphere cast to detect nearby obstacles and generates a force to steer away.
    // basic implementation, not really tested
    private Vector3 AvoidObjects()
    {
        RaycastHit hit;
        Vector3 avoidanceForce = Vector3.zero;

        // SphereCast forward in the direction of movement, add layer if wanted
        if (Physics.SphereCast(transform.position, avoidDist, _velocity.normalized, out hit, avoidDist))
        {
            Vector3 awayFromObstacle = transform.position - hit.point; // Direction away from the obstacle
            awayFromObstacle.y = 0; // Ignore Y-axis movement for ground-level avoidance

            avoidanceForce = awayFromObstacle.normalized * avoidForce;
        }

        return avoidanceForce;
    }

    private enum State
    {
        Seek,
        Flee,
        Follow
    }
}
