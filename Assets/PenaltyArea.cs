using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StuPro
{
    public class PenaltyArea : MonoBehaviour
    {
        public RobotAI robot;
        private bool triggered;
        private List<Collider> list = new List<Collider>();
        private bool triggerEntered = false;

        void FixedUpdate()
        {
            if (triggerEntered)
            {
                robot.OnCollisionWithPenaltyArea();
                triggerEntered = false;
            }
        }

        void OnTriggerStay(Collider collider)
        {
            if (collider.name.Contains("AI")) triggerEntered = true;
            Debug.Log("OnTriggerEnter");
        }
    }
}