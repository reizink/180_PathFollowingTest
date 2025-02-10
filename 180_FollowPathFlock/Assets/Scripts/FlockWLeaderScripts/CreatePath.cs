using System.Collections.Generic;
using UnityEngine;

//we need a path to follow
public class CreatePath : MonoBehaviour
{
    public GameObject waypointPrefab; // Prefab for waypoints
    public int maxPoints = 50;     // Max number of stored points
    public float pointSpacing = 2f; // Distance before placing next waypoint

    public List<Vector3> waypoints = new List<Vector3>();
    private Vector3 lastPoint;

    //added
    public float speed = 6f;
    private Vector3 goal;

    void Start()
    {
        lastPoint = transform.position;

        //added
        goal = new Vector3(Random.Range(-10,10), 1, Random.Range(-10,10));
    }

    private void Update() //added
    {
        if (Vector3.Distance(transform.position, goal) < 2f)
        {
            goal = new Vector3(Random.Range(-10, 10), 1, Random.Range(-10, 10));
        }

        Vector3 dir = goal - transform.position;
        dir.Normalize();
        //transform.position += speed * dir * Time.deltaTime;
    }

    void LateUpdate()
    {
        // Place new waypoints at intervals
        if (Vector3.Distance(transform.position, lastPoint) > pointSpacing)
        {
            waypoints.Add(transform.position);
            lastPoint = transform.position; 
            //Instantiate(waypointPrefab, transform.position, Quaternion.identity); //tmp

            // Keep list size under max limit
            if (waypoints.Count > maxPoints)
            {
                waypoints.RemoveAt(0);
            }
        }
    }
}

