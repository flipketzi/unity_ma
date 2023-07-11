using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StuPro
{
    public class Boundary : MonoBehaviour
    {
        public RobotAI robot;

        // Calls robot if target was successfully pushed into drop zone
        void OnTriggerEnter(Collider collider)
        {
            robot.OnCollisionWithBoundary();
        }
    }
}