using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StuPro
{
    public class Target : MonoBehaviour
    {
        public RobotAI robot;
        [SerializeField] public ArticulationBody ab;
        // Calls robot if target was successfully pushed into drop zone
        void OnTriggerEnter(Collider collider)
        {
            if (collider.name == "Goal")
            {
                robot.OnTargetCollected();
            }
            else if (collider.name == "BoxCollider")
            {
                robot.OnBoxTouched();
            }
            

        }

        void ResetVelocity()
        {
            
            ab.velocity = Vector3.zero;
            ab.angularVelocity = Vector3.zero;
        }
    }
}