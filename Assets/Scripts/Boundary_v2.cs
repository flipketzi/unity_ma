using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundary_v2 : MonoBehaviour
{
    public RobotAI_v2 robot;

    void OnTriggerEnter(Collider collider)
    {
        Debug.Log("OnTriggerEnter");
        // Calls robot if target was successfully pushed into drop zone
        robot.OnCollisionWithBoundary();
    }
}