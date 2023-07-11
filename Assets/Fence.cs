using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StuPro
{
    public class Fence : MonoBehaviour
    {
        public RobotAI robot;

        void OnTriggerEnter(Collider collider)
        {
            Debug.Log("OnTriggerEnter");
            if (collider.name.Contains("AI")) robot.OnCollisionWithFence();
        }
    }
}