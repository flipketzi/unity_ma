using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class Target_v2 : MonoBehaviour
    {
        public RobotAI_v2 robot;

        // Calls robot if target was successfully pushed into drop zone
        void OnTriggerEnter(Collider collider)
        {
            if (collider.tag == "Target")
            {
                Debug.Log("OnTriggerEnter");
                robot.OnTargetDelivered();
            }
        }
    }
