using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using StuPro;

public class RobotAI_v2 : Agent
{
    // Debugging
    [SerializeField] bool showDebugMessages = false;

    // Articulation bodies
    [SerializeField] ArticulationBody leftWheel, rightWheel;
    [SerializeField] ArticulationDrive wheelDrive;
    [SerializeField] SetTestPositions randomizer;
    [SerializeField] DecisionRequester decisionRequester;
    int decisionPeriod;

    // Properties for Training
    public float actionM1 = 0;
    public float actionM2 = 0;
    [SerializeField] float _cumulativeReward = 0;
    [SerializeField] int defaultLevelDifficulty = 1;
    int currentStep = 0;
    int currentEpisode = 0;

    //Changes the mode of the robot
    // inference means running the already trained neural network or using player comands (heuristics)
    // testing is like inferencing but runs trough test environments and logs data
    // training is used to train the neural network
    [SerializeField] RobotMode _robotMode = RobotMode.inference;
    enum RobotMode
    {
        inference,
        testing,
        training
    }

    // Settings
    [SerializeField] float topSpeed = 360;
    void Start()
    {
        OnTriggerEvent.OnTrigger += OnCollisionWithObject;
        decisionPeriod = decisionRequester.DecisionPeriod;
    }

    // Defines what happens at the beginning of a new episode (e.g. reset of robot and obstacles positions and rotations)
    public override void OnEpisodeBegin()
    {
        currentEpisode++;
        currentStep = 0;
        //reset wheel velocity
        ResetWheels(leftWheel);
        ResetWheels(rightWheel);

        //reset reward
        SetReward(0);
        switch (_robotMode)
        {
            case RobotMode.inference:
                randomizer.SetRandomPoseTraining(Academy.Instance.EnvironmentParameters.GetWithDefault("levelDifficulty", defaultLevelDifficulty));
                break;
            case RobotMode.testing:
                randomizer.SetAndSaveMeasurementPose();
                break;
            case RobotMode.training:
                randomizer.SetRandomPoseTraining(Academy.Instance.EnvironmentParameters.GetWithDefault("levelDifficulty", defaultLevelDifficulty));
                break;
        }
    }

    // Defines the action that the robot performs each step
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Read next action from action buffer
        actionM1 = actions.ContinuousActions[1];
        actionM2 = actions.ContinuousActions[0];

        // Clamp actions
        var leftWheelDrive = Mathf.Clamp(actionM1, -1f, 1f);
        var rightWheelDrive = Mathf.Clamp(actionM2, -1f, 1f);

        // Add velocity to wheels
        wheelDrive = leftWheel.xDrive;
        wheelDrive.targetVelocity = topSpeed * leftWheelDrive;
        leftWheel.xDrive = wheelDrive;

        wheelDrive = rightWheel.xDrive;
        wheelDrive.targetVelocity = topSpeed * rightWheelDrive;
        rightWheel.xDrive = wheelDrive;

        //Penalize for each step
        AddReward((-1f) * ((1f / (MaxStep / decisionPeriod)) * 1.0f));

        currentStep++;
        //Logs Data if in testing mode, for the first step of an episode it has to be done in late update otherwise the onepisode begin pose is not yet set 
        if (_robotMode == RobotMode.testing && currentStep > 1) randomizer.Log();
        else if(_robotMode == RobotMode.testing) randomizer.LogFirstStep();
    }

    // Defines how the robot can be controlled during heursitic teach-in mode
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        print("Horizontal: " + Input.GetAxis("Horizontal"));
        print("Vertical: " + Input.GetAxis("Vertical"));

        continuousActionsOut[0] = Mathf.Clamp(-Input.GetAxis("Vertical") + (Input.GetAxis("Horizontal")), -1, 1);
        continuousActionsOut[1] = Mathf.Clamp(-Input.GetAxis("Vertical") - (Input.GetAxis("Horizontal")), -1, 1);
    }
    // void LateUpdate()
    // {
    //     if (log)
    //     {
    //         log = false;
    //         Debug.Log("LateUpdate: " + transform.localPosition);
    //     }
    // }
    void OnCollisionWithObject(bool inTrigger, string triggerType)
    {
        switch (triggerType)
        {
            case "DropZone":
                OnTargetDelivered();
                break;
            case "Boundary":
                OnCollisionWithBoundary();
                break;
            case "Wall":
                OnCollisionWithWall();
                break;
            case "Fence":
                OnCollisionWithFence();
                break;
            case "Penalty":
                OnCollisionWithPenaltyArea();
                break;
            default:
                break;
        }
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

    // The robot successfully finishes an episode, if the target cube is delivered to the drop zone
    public void OnTargetDelivered()
    {
        AddReward(1);
        EndEpisode();
    }
    public void OnCollisionWithBoundary()
    {
        SetReward(-1);
        EndEpisode();
    }
    void OnCollisionWithWall()
    {
        SetReward(-1);
        EndEpisode();
    }
    void OnCollisionWithFence()
    {
        SetReward(-1);
        EndEpisode();
    }
    void OnCollisionWithPenaltyArea()
    {
        AddReward(-0.0025f);
    }
}
