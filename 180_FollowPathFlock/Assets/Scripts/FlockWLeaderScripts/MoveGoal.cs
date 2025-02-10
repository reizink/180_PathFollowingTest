using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveGoal : MonoBehaviour
{
    [SerializeField] private Vector3 MoveLimits = new Vector3(10, 10, 10);

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Leader"))
        {
            Vector3 pos = new Vector3(Random.Range(-MoveLimits.x, MoveLimits.x),
                1,
                Random.Range(-MoveLimits.y, MoveLimits.y));

            transform.position = pos;
        }
    }
}
