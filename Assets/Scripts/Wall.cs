using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StuPro
{
    public class Wall : MonoBehaviour
    {
        public RobotAI robot;

        // Calls robot if target was successfully pushed into drop zone
        void OnTriggerEnter(Collider collider)
        {
            Debug.Log("OnTriggerEnter");
            if (collider.name.Contains("AI")) robot.OnCollisionWithWall();
        }
    }
}