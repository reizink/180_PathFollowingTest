using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Ref: https://www.youtube.com/watch?v=rlZYT-uvmGQ&t=603s
public class PathFollowing : MonoBehaviour
{
    public Transform startPoint; // Start of path
    public Transform endPoint;   // End of path
    public float pathRadius = 2f; // How close we need to be before stopping correction
    public float maxSpeed = 6f;
    public float maxForce = 0.1f;
    public float slowRadius = 5f; // Slow down when approaching target

    private Vector3 velocity;
    private Vector3 acceleration;

    private void Start()
    {
        //get close to the start position first
        Vector3 move = startPoint.position - transform.position;
        move.Normalize();

        transform.position += move * maxSpeed * Time.deltaTime;
    }

    void Update()
    {
        FollowPath();
        ApplyMovement();
    }

    void FollowPath()
    {
        // 1. Predict future position
        Vector3 futurePos = transform.position + velocity.normalized * 2f;

        // 2. Get projection onto the path segment
        Vector3 target = GetProjectedPoint(startPoint.position, endPoint.position, futurePos);

        // 3. If near endpoint, swap start and end
        if (Vector3.Distance(transform.position, endPoint.position) < pathRadius)
        {
            SwapPathPoints();
            return;
        }

        // 4. Seek the projected point if off path
        if (Vector3.Distance(futurePos, target) > pathRadius)
        {
            Vector3 force = Seek(target); //delegate to Seek
            ApplyForce(force);
        }
    }

    // slightly different from Cameron's
    // same math, but added a slow down within a 'slowRadius' of our points
    Vector3 Seek(Vector3 target)
    {
        Vector3 desired = target - transform.position;
        float distance = desired.magnitude;

        // Slow down if within slowRadius
        float speed = maxSpeed;
        if (distance < slowRadius)
        {
            speed = Mathf.Lerp(0, maxSpeed, distance / slowRadius);
        }

        //normalize our force and * by speed
        desired = desired.normalized * speed;
        Vector3 steering = desired - velocity;
        return Vector3.ClampMagnitude(steering, maxForce);
    }

    void ApplyForce(Vector3 force)
    {
        acceleration += force;
    }

    void ApplyMovement()
    {
        velocity += acceleration;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        transform.position += velocity * Time.deltaTime; 
        acceleration = Vector3.zero;

        // Face the movement direction
        if (velocity.magnitude > 0.1f)
            transform.forward = velocity.normalized;
    }

    //Vector Projection
    Vector3 GetProjectedPoint(Vector3 start, Vector3 end, Vector3 future)
    {
        Vector3 AP = future - start;
        Vector3 AB = end - start;

        float sp = Vector3.Dot(AP, AB) / Vector3.Dot(AB, AB);
        sp = Mathf.Clamp01(sp); // Keep within the segment

        return start + sp * AB;
    }

    void SwapPathPoints()
    {
        Transform temp = startPoint;
        startPoint = endPoint;
        endPoint = temp;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(startPoint.position, endPoint.position);
    }
}
