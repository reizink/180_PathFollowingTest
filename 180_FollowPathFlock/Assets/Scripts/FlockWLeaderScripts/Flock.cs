using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    [SerializeField] private GameObject _boidPrefab;
    [SerializeField, Min(0)] private int _flockSize;

    private Boid[] _flockMembers; 

    public Vector3 spawnLimits = new Vector3(10, 10, 10);

    public GameObject leader; // Reference to leader, y
    public float followDistance = 7f;
    public bool allFollow; //all follow or by distance?

    void Start()
    {
        _flockMembers = new Boid[_flockSize]; 

        for (int i = 0; i < _flockSize; i++)
        {
            Vector3 pos = transform.position + new Vector3(Random.Range(-spawnLimits.x, spawnLimits.x),
                Random.Range(-spawnLimits.y, spawnLimits.y),
                Random.Range(-spawnLimits.z, spawnLimits.z));

            Boid newBoid = Instantiate(_boidPrefab,
                pos, Quaternion.identity).GetComponent<Boid>();
            _flockMembers[i] = newBoid;
            newBoid.SetBounds = GetComponent<BoxCollider>().bounds;
        }
    }

    void Update()
    {
        for (int i = 0; i < _flockMembers.Length; i++) 
        {
            _flockMembers[i].Flock(_flockMembers, leader, allFollow, followDistance); //y, add leader and to parameters
        }
    }
}

