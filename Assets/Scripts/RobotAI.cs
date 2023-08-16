using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

namespace StuPro
{
    public class RobotAI : Agent
    {
        // Debugging
        [SerializeField] bool showDebugMessages = false;

        // Articulation bodies
        [SerializeField] public ArticulationBody articulationBody;
        [SerializeField] ArticulationBody leftWheel, rightWheel, fork;
        [SerializeField] public ArticulationBody targetArticulationBody;
        [SerializeField] ArticulationDrive wheelDrive;
        [SerializeField] public GameObject dropZone;
        [SerializeField] Randomizer randomizer;
        [SerializeField] Recorder recorder;

        // Properties for Training
        public int actionM1 = 0;
        public int actionM2 = 0;
        [SerializeField] float batteryCapacity = 0;
        float BatteryCapacity
        {
            get { return batteryCapacity; }
            set { batteryCapacity = Mathf.Clamp(value, 0, 100); }
        }

        // Settings
        [SerializeField] bool batteryDeathIrrelevant = false;
        [SerializeField] float topSpeed = 1;
        [SerializeField] int maxEnergyCollectedTillEpisodeEnd = 0;
        [SerializeField] int energyCollectedCurrentRun = 0;

        // Randomization
        Vector3 randomTarget;
        int batteryFailure = 0;
        int boundaryFailure = 0;
        int targetFailure = 0;
        int wallFailure = 0;
        int fenceFailure = 0;
        float penaltyArea = 0;
        int targetCollected = 0;
        int imageCount = 0;
        bool boxTouched = false;
        [SerializeField] int recorderInterval = 50;
        float initDistance;
        string folderName = System.DateTime.Now.ToString("yyyyMMdd_hhmmss");

        private float newAction1 = 0f;
        private float newAction2 = 0f;
        public bool heuristic = false;

        void Start()
        {
            if (heuristic) Time.timeScale = 3.0f;
        }

        // Defines what happens at the beginning of a new episode (e.g. reset of robot and obstacles)
        public override void OnEpisodeBegin()
        {
            // Reset robot position and rotation
            //Vector3 position = transform.parent.transform.position + new Vector3(0, 0.5f, 0);
            //this.transform.position = position;
            //articulationBody.TeleportRoot(position, Quaternion.identity);

            // Reset battery capacity
            BatteryCapacity = 100;
            boxTouched = false;
            ResetWheels(leftWheel);
            ResetWheels(rightWheel);

            // Reset energyCollectedCurrentRun
            energyCollectedCurrentRun = 0;

            // Set position of target randomly
            randomizer.RandomizeObstacles(heuristic ? 11 : Academy.Instance.EnvironmentParameters.GetWithDefault("levelDifficulty", 0));
            randomizer.RandomizeAppearances();
            initDistance = Mathf.Sqrt(
                Mathf.Pow(targetArticulationBody.transform.position.x - dropZone.transform.position.x, 2) +
                Mathf.Pow(targetArticulationBody.transform.position.z - dropZone.transform.position.z, 2)
            );
            SetMaxEnergy(heuristic ? 11 : Academy.Instance.EnvironmentParameters.GetWithDefault("levelDifficulty", 0));
        }

    void ResetWheels(ArticulationBody articulationBody)
    {
        var leftWheelDrive = 0;
        var rightWheelDrive = 0;

        // Add velocity to wheels
        wheelDrive = leftWheel.xDrive;
        wheelDrive.targetVelocity = topSpeed * leftWheelDrive;
        leftWheel.xDrive = wheelDrive;

        wheelDrive = rightWheel.xDrive;
        wheelDrive.targetVelocity = topSpeed * rightWheelDrive;

        articulationBody.jointPosition = new ArticulationReducedSpace(0f, 0f, 0f);
        articulationBody.jointAcceleration = new ArticulationReducedSpace(0f, 0f, 0f);
        articulationBody.jointForce = new ArticulationReducedSpace(0f, 0f, 0f);
        articulationBody.jointVelocity = new ArticulationReducedSpace(0f, 0f, 0f);
        articulationBody.velocity = Vector3.zero;
        articulationBody.angularVelocity = Vector3.zero;
    }

    void updateBattery()
    {
        
        // Reduce battery capacity depending on the current difficulty
        var level = Academy.Instance.EnvironmentParameters.GetWithDefault("levelDifficulty", 0); 
        if(heuristic)
        {
            BatteryCapacity -= .05f;
            return;
        }

        
        if(level < 6)
            BatteryCapacity -= .2f;
        else if (level < 8)
            BatteryCapacity -= .01f;
        else
            BatteryCapacity -= .05f;

        
    }


        // Defines the action that the robot performs each step
        public override void OnActionReceived(ActionBuffers actions)
        {
            //Penalize for each step
            AddReward(-0.0001f);
                
            // AddReward((-1f) * ((1f / (MaxStep / decisionPeriod)) * 1.0f));
            // Read next action from action buffer
            // actionM1 = actions.ContinuousActions[1];
            // actionM2 = actions.ContinuousActions[0];

            actionM1 = actions.DiscreteActions[0] - 1;
            actionM2 = actions.DiscreteActions[1] - 1;

            Debug.Log(actionM1 + " " + actionM2);

            // Clamp actions
            // var leftWheelDrive = Mathf.Clamp(actionM1, -1f, 1f);
            // var rightWheelDrive = Mathf.Clamp(actionM2, -1f, 1f);
            var leftWheelDrive = actionM1;
            var rightWheelDrive = actionM2;

            updateBattery();

            // Add velocity to wheels
            wheelDrive = leftWheel.xDrive;
            wheelDrive.targetVelocity = topSpeed * leftWheelDrive;
            leftWheel.xDrive = wheelDrive;

            wheelDrive = rightWheel.xDrive;
            wheelDrive.targetVelocity = topSpeed * rightWheelDrive;
            rightWheel.xDrive = wheelDrive;

            // Record information
            if (imageCount % 50 == 0)
            {
               // recorder.Write(folderName, imageCount.ToString());
            }
            imageCount++;

            // If battery capacity is zero, the robot could not finish the episode successfully
            if (!batteryDeathIrrelevant && BatteryCapacity <= 0)
            {
                batteryFailure++;

                // Tensorflow Stats
                Academy.Instance.StatsRecorder.Add("Custom/Failure/Battery", batteryFailure);
                Academy.Instance.StatsRecorder.Add("Custom/Maximal energy collected", energyCollectedCurrentRun);
                float distance = Mathf.Sqrt(
                    Mathf.Pow(targetArticulationBody.transform.position.x - dropZone.transform.position.x, 2) +
                    Mathf.Pow(targetArticulationBody.transform.position.z - dropZone.transform.position.z, 2)
                );
                Academy.Instance.StatsRecorder.Add("Custom/Distance between target and drop area", distance/initDistance);
                
                if(energyCollectedCurrentRun < 1)
                    AddReward(-1);
            
                AddReward((1 - (distance/initDistance)) / 2);

                // End Episode
                EndEpisode();
            }

            // If the target leaves platform, the robot could not finish the episode successfully
            if (targetArticulationBody.transform.localPosition.y < -0.5f)
            {
                targetFailure++;

                // Tensorflow Stats
                Academy.Instance.StatsRecorder.Add("Custom/Failure/Target has left platform", targetFailure);
                Academy.Instance.StatsRecorder.Add("Custom/Maximal energy collected", energyCollectedCurrentRun);

                float distance = Mathf.Sqrt(
                    Mathf.Pow(targetArticulationBody.transform.position.x - dropZone.transform.position.x, 2) +
                    Mathf.Pow(targetArticulationBody.transform.position.z - dropZone.transform.position.z, 2)
                );

                Academy.Instance.StatsRecorder.Add("Custom/Distance between target and drop area", distance/initDistance);

                // End Episode
                if(energyCollectedCurrentRun < 1)
                    AddReward(-1);
                
                EndEpisode();
            }
        }

        public void OnBoxTouched()
        {
            if(!boxTouched)
                AddReward(0.3f);
            boxTouched = true;
        }

        // The robot successfully finishes an episode, if the target cube is collected
        public void OnTargetCollected()
        {
            targetCollected++;
            Academy.Instance.StatsRecorder.Add("Custom/Target collected", targetCollected);
            Academy.Instance.StatsRecorder.Add("Custom/Battery capacity after collection", BatteryCapacity);

            // Reward robot for collection target cube
            AddReward(1);

            // Reward robot for collection target cube fast
            AddReward(BatteryCapacity / 100);

            energyCollectedCurrentRun++;
            Academy.Instance.StatsRecorder.Add("Custom/Maximal energy collected", energyCollectedCurrentRun);
            Academy.Instance.StatsRecorder.Add("Custom/Distance between target and drop area", 0);

            if (energyCollectedCurrentRun < maxEnergyCollectedTillEpisodeEnd)
            {
                BatteryCapacity = 100;
                randomizer.RandomizeObstacles(Academy.Instance.EnvironmentParameters.GetWithDefault("levelDifficulty", 0));
                SetMaxEnergy(Academy.Instance.EnvironmentParameters.GetWithDefault("levelDifficulty", 0));
            }
            else
            {
                AddReward(1); //extra point for a succesfull run
                EndEpisode();
            }
        }

        public void OnCollisionWithBoundary()
        {
            boundaryFailure++;

            // Tensorflow Stats
            Academy.Instance.StatsRecorder.Add("Custom/Failure/Robot touched boundary", boundaryFailure);
            Academy.Instance.StatsRecorder.Add("Custom/Maximal energy collected", energyCollectedCurrentRun);
            float distance = Mathf.Sqrt(
                Mathf.Pow(targetArticulationBody.transform.position.x - dropZone.transform.position.x, 2) +
                Mathf.Pow(targetArticulationBody.transform.position.z - dropZone.transform.position.z, 2)
            );
            Academy.Instance.StatsRecorder.Add("Custom/Distance between target and drop area", distance/initDistance);

            // End Episode
            if(energyCollectedCurrentRun < 1)
                AddReward(-1);
            EndEpisode();
        }

        // Defines how the robot can be controlled during heursitic teach-in mode
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var continuousActionsOut = actionsOut.ContinuousActions;

            if (Input.GetAxis("Horizontal") != 0 && Input.GetAxis("Vertical") != 0)
            {
                if (Input.GetAxis("Horizontal") > 0 && Input.GetAxis("Vertical") > 0)
                {
                    continuousActionsOut[0] = +Input.GetAxis("Vertical") * .2f;
                    continuousActionsOut[1] = -Input.GetAxis("Vertical");
                }
                else if (Input.GetAxis("Horizontal") < 0 && Input.GetAxis("Vertical") > 0)
                {
                    continuousActionsOut[0] = -Input.GetAxis("Vertical");
                    continuousActionsOut[1] = +Input.GetAxis("Vertical") * .2f;
                }
                else if (Input.GetAxis("Horizontal") > 0 && Input.GetAxis("Vertical") < 0)
                {
                    continuousActionsOut[0] = +Input.GetAxis("Vertical") * .2f;
                    continuousActionsOut[1] = -Input.GetAxis("Vertical");
                }
                else if (Input.GetAxis("Horizontal") < 0 && Input.GetAxis("Vertical") < 0)
                {
                    continuousActionsOut[0] = -Input.GetAxis("Vertical");
                    continuousActionsOut[1] = +Input.GetAxis("Vertical") * .2f;
                }
            }
            else if (Input.GetAxis("Horizontal") != 0 && Input.GetAxis("Vertical") == 0)
            {
                float horizontal = Input.GetAxis("Horizontal");
                if(horizontal < 0)
                {
                    continuousActionsOut[0] = horizontal < 0 ? +Input.GetAxis("Horizontal") : 0;
                    continuousActionsOut[1] = horizontal < 0 ? -Input.GetAxis("Horizontal") : 0;
                
                }
                else
                {
                    continuousActionsOut[0] = horizontal > 0 ? +Input.GetAxis("Horizontal") : 0;
                    continuousActionsOut[1] = horizontal > 0 ? -Input.GetAxis("Horizontal") : 0;
                
                }
            } 
            else if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") != 0)
            {
                continuousActionsOut[0] = continuousActionsOut[1] = -Input.GetAxis("Vertical");
            }
        }

        public void OnCollisionWithWall()
        {
            wallFailure++;
            Academy.Instance.StatsRecorder.Add("Custom/Failure/Wall failure", wallFailure);
            AddReward(-1.00f);
            EndEpisode();
        }

        public void OnCollisionWithFence()
        {
            fenceFailure++;
            Academy.Instance.StatsRecorder.Add("Custom/Failure/Wall failure", fenceFailure);
            AddReward(-1);
            EndEpisode();
        }

        public void OnCollisionWithPenaltyArea()
        {
            penaltyArea += Time.deltaTime;
            Academy.Instance.StatsRecorder.Add("Custom/Failure/Time in penaltyArea", penaltyArea);
            AddReward(-0.0025f);
        }

        private void SetMaxEnergy(float difficulty)
        {
            switch ((int) difficulty)
            {
                case 1:  maxEnergyCollectedTillEpisodeEnd = 1; break;
                case 2:  maxEnergyCollectedTillEpisodeEnd = 1; break;
                case 3:  maxEnergyCollectedTillEpisodeEnd = 1; break;
                case 4:  maxEnergyCollectedTillEpisodeEnd = 1; break;
                case 5:  maxEnergyCollectedTillEpisodeEnd = 1; break;
                case 6:  maxEnergyCollectedTillEpisodeEnd = 1; break;
                case 7:  maxEnergyCollectedTillEpisodeEnd = 1; break;
                case 8:  maxEnergyCollectedTillEpisodeEnd = 1; break;
                case 9:  maxEnergyCollectedTillEpisodeEnd = 1; break;
                case 10: maxEnergyCollectedTillEpisodeEnd = 1; break;
                case 11: maxEnergyCollectedTillEpisodeEnd = 1; break;
                default: maxEnergyCollectedTillEpisodeEnd = 1; break;
            }
        }
    }
}

        /*private void RandomizeTargetPosition(float difficulty)
        {
            Vector3 randomTarget;

            switch ((int) difficulty)
            {
                case 1:  randomTarget = new Vector3(0, 0, Random.Range(3.2f, 4f)); maxEnergyCollectedTillEpisodeEnd = 1; break;
                case 2:  randomTarget = new Vector3(0, 0, Random.Range(3.2f, 4f)); maxEnergyCollectedTillEpisodeEnd = 1; break;
                case 3:  randomTarget = new Vector3(0, 0, Random.Range(1f, 4f)); maxEnergyCollectedTillEpisodeEnd = 1; break;
                case 4:  navMesh.GetRandomPoint(3f, new Vector3(0, 0, 5), out randomTarget); maxEnergyCollectedTillEpisodeEnd = 1; break;
                case 5:  navMesh.GetRandomPoint(4f, new Vector3(0, 0, 5), out randomTarget); maxEnergyCollectedTillEpisodeEnd = 1; break;
                case 6:  navMesh.GetRandomPoint(6f, new Vector3(0, 0, 5), out randomTarget); maxEnergyCollectedTillEpisodeEnd = 2; break;
                case 7:  navMesh.GetRandomPoint(8f, Vector3.zero, out randomTarget); maxEnergyCollectedTillEpisodeEnd = 10; break;
                case 8:  navMesh.GetRandomPoint(10f, Vector3.zero, out randomTarget); maxEnergyCollectedTillEpisodeEnd = 10; break;
                case 9:  navMesh.GetRandomPoint(12f, Vector3.zero, out randomTarget); maxEnergyCollectedTillEpisodeEnd = 10; break;
                case 10: navMesh.GetRandomPoint(13.5f, Vector3.zero, out randomTarget); maxEnergyCollectedTillEpisodeEnd = 10; break;
                case 11: navMesh.GetRandomPoint(15f, Vector3.zero, out randomTarget); maxEnergyCollectedTillEpisodeEnd = 10; break;
                default: randomTarget = new Vector3(0, 0, 2f); maxEnergyCollectedTillEpisodeEnd = 1; break;
            }

            randomTarget += (Vector3.up * 0.3f);
            targetArticulationBody.gameObject.SetActive(true);
            targetArticulationBody.TeleportRoot(randomTarget, Quaternion.identity);
        }*/

        /*private void RandomizeDropPosition(float difficulty)
        {
            Vector3 randomDrop;

            switch ((int) difficulty)
            {
                case 1:  randomDrop = new Vector3(0, 0, Random.Range(4.5f, 6f)); break;
                case 2:  randomDrop = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(4.5f, 6f)); break;
                case 3:  randomDrop = new Vector3(Random.Range(-4f, 4f), 0, Random.Range(4.5f, 6f)); break;
                case 4:  navMesh.GetRandomPoint(3f, new Vector3(0, 0, 5), out randomDrop); break;
                case 5:  navMesh.GetRandomPoint(4f, new Vector3(0, 0, 5), out randomDrop); break;
                case 6:  navMesh.GetRandomPoint(6f, new Vector3(0, 0, 5), out randomDrop); break;
                case 7:  navMesh.GetRandomPoint(8f, Vector3.zero, out randomDrop); break;
                case 8:  navMesh.GetRandomPoint(10f, Vector3.zero, out randomDrop); break;
                case 9:  navMesh.GetRandomPoint(12f, Vector3.zero, out randomDrop); break;
                case 10: navMesh.GetRandomPoint(13.5f, Vector3.zero, out randomDrop); break;
                case 11: navMesh.GetRandomPoint(15f, Vector3.zero, out randomDrop); break;
                default: randomDrop = new Vector3(0, 0, 3f); break;
            }

            randomDrop += Vector3.up * 0.005f;
            dropZone.gameObject.SetActive(true);
            dropZone.transform.position = randomDrop;
        }*/