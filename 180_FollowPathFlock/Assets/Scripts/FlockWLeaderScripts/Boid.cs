using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Boid : MonoBehaviour
{
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _maxForce; 
    [SerializeField] private float _maxDetectionRange;

    private Vector3 _velocity;
    private Vector3 _accel; 

    private Bounds _bounds;
    public Bounds SetBounds { set { _bounds = value; } }

    //added
    [SerializeField] private float _followRange = 5.0f; // Range at which boids will follow the leader
    public float predictionTime = 1f; // How far ahead to predict

    private void Start()
    {
        _velocity = new Vector3(Random.Range(-5, 5), Random.Range(-5, 5), Random.Range(-5, 5));
    }

    public void Flock(Boid[] flockMembers, GameObject leader, bool allFollow, float followDist)
    {
        _accel = Vector3.zero; //reset

        //find neighboring boids, boidsInRange
        Boid[] neighbors = GetAllOtherBoidsInDetectionRadius(flockMembers);

        Vector3 separation = Separation(neighbors);
        Vector3 alignment = Align(neighbors);
        Vector3 cohesion = Cohesion(neighbors);
        Vector3 avoidance = AvoidObject(neighbors); // New avoidance behavior

        _accel += separation;
        _accel += alignment; 
        _accel += cohesion;
        _accel += avoidance; // Add avoidance force

        //added
        if (Vector3.Distance(transform.position, leader.transform.position) < _followRange
    || allFollow)
        {
            _accel += FollowLeader(leader, followDist); // Add follow leader force
        }
    }

    private Vector3 FollowLeader(GameObject leader, float followDistance)
    {
        CreatePath _createPath = leader.GetComponent<CreatePath>();

        if (_createPath == null || _createPath.waypoints.Count == 0)
            return Vector3.zero; // No path to follow

        List<Vector3> waypoints = _createPath.waypoints;
        int closestIndex = 0;
        float minDist = float.MaxValue;

        // Find the closest waypoint
        for (int i = 0; i < waypoints.Count; i++)
        {
            float dist = Vector3.Distance(transform.position, waypoints[i]);
            if (dist < minDist)
            {
                minDist = dist;
                closestIndex = i;
            }
        }

        // Predict a future waypoint
        int predictedIndex = Mathf.Min(closestIndex + Mathf.FloorToInt(predictionTime), waypoints.Count - 1);
        Vector3 futureTarget = waypoints[predictedIndex];

        //***find predicted future current, not current pos***
        if(Vector3.Distance(transform.position, waypoints[predictedIndex]) < followDistance)
        {
            // Use Seek behavior to move toward predicted target
            Vector3 force = futureTarget - transform.position;
            force.Normalize();
            force *= _maxSpeed;
            force -= _velocity;
            force = Vector3.ClampMagnitude(force, _maxForce);

            return force;
            //return Seek(futureTarget);
        }
        return Vector3.zero;
    }

    private Vector3 AvoidObject(Boid[] boidsInRadius)
    {
        Vector3 steering = Vector3.zero;
        int count = 0;

        foreach (Boid boid in boidsInRadius)
        {
            float distance = Vector3.Distance(transform.position, boid.transform.position);
            if (distance > 0 && distance < _maxDetectionRange * 0.5f) // Stronger avoidance at closer range
            {
                Vector3 difference = transform.position - boid.transform.position;
                difference /= distance; // Weight closer objects more
                steering += difference;
                count++;
            }
        }

        if (count > 0)
        {
            steering /= count;
            steering.Normalize();
            steering *= _maxSpeed;
            steering -= _velocity;
            steering = Vector3.ClampMagnitude(steering, _maxForce);
        }

        return steering;
    }

    private Vector3 Align(Boid[] neighbors)
    {
        Vector3 steering = Vector3.zero;

        if (neighbors.Length == 0)
        {
            return steering;
        }

        for (int i = 0; i < neighbors.Length; i++)
        {
            steering += neighbors[i]._velocity; 
        }

        steering /= neighbors.Length; //the if above protects from dividing by 0

        steering.Normalize();
        steering *= _maxSpeed;
        steering -= _velocity;
        steering = Vector3.ClampMagnitude(steering, _maxForce);

        return steering;
    }

    //bad method
    private Boid[] GetAllOtherBoidsInDetectionRadius(Boid[] flockMembers) 
    {
        List<Boid> foundBoids = new List<Boid>();

        for (int i = 0; i < flockMembers.Length; i++)
        {
            if (flockMembers[i] == this ||
                Vector3.Distance(flockMembers[i].transform.position, transform.position) > _maxDetectionRange)
            {
                continue;
            }

            foundBoids.Add(flockMembers[i]);
        }

        return foundBoids.ToArray();
    }

    private void LateUpdate()
    {
        transform.position += _velocity * Time.deltaTime;
        _velocity += _accel;

        if (_velocity.sqrMagnitude > 0.001f) // Prevent jitter when nearly stationary, look forward
        {
            transform.rotation = Quaternion.LookRotation(_velocity.normalized);
        }

        //The following code is dirty and bad, it is just to contains the flock
        float x = transform.position.x;
        float y = transform.position.y;
        float z = transform.position.z;

        if (x > _bounds.max.x)
        {
            x = _bounds.min.x;
        }
        else if (x < _bounds.min.x)
        {
            x = _bounds.max.x;
        }

        if (y > _bounds.max.y)
        {
            y = _bounds.min.y;
        }
        else if (y < _bounds.min.y)
        {
            y = _bounds.max.y;
        }

        if (z > _bounds.max.z)
        {
            z = _bounds.min.z;
        }
        else if (z < _bounds.min.z)
        {
            z = _bounds.max.z;
        }

        transform.position = new Vector3(x, y, z);
    }

    private Vector3 Cohesion(Boid[] boidsInRadius)
    {
        Vector3 steering = Vector3.zero;

        if (boidsInRadius.Length == 0)
        {
            return steering;
        }

        for (int i = 0; i < boidsInRadius.Length; i++)
        {
            steering += boidsInRadius[i].transform.position;
        }

        steering /= boidsInRadius.Length;

        steering -= transform.position;
        steering.Normalize();
        steering *= _maxSpeed;
        steering -= _velocity;
        steering = Vector3.ClampMagnitude(steering, _maxForce);

        return steering;
    }

    private Vector3 Separation(Boid[] boidsInRadius)
    {
        Vector3 steering = Vector3.zero;

        if (boidsInRadius.Length == 0)
        {
            return steering;
        }

        for (int i = 0; i < boidsInRadius.Length; i++)
        {
            Vector3 difference = transform.position - boidsInRadius[i].transform.position;
            difference /= Vector3.Distance(transform.position, boidsInRadius[i].transform.position);
            steering += difference;
        }

        steering /= boidsInRadius.Length;

        steering.Normalize();
        steering *= _maxSpeed;
        steering -= _velocity;
        steering = Vector3.ClampMagnitude(steering, _maxForce);

        return steering;
    }
}
