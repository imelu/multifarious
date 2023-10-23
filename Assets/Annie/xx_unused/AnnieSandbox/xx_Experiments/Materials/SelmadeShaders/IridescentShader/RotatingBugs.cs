using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingBugs : MonoBehaviour
{
    public float speed = 2f;
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.right * Time.deltaTime * speed);
    }
}
