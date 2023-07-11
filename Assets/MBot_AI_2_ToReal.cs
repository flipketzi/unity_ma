using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Net.Sockets;
using System;
using System.Text;

public class MBot_AI_2_ToReal : Agent
{
    public string ip = "painbot.local";
    public int port = 8000;
    private TcpClient socketConnection;
    [SerializeField] ArticulationBody articulationBody;
    [SerializeField] ArticulationBody leftWheel;
    [SerializeField] ArticulationBody rightWheel;
    [Tooltip("The top speed the wheels can acchieve. A speed of 360 accords to 1 revolution per second. The real robot wheel makes 2 revolutions without load.")]
    [SerializeField] float topSpeed = 450;
    [SerializeField] [Range(0, 1)] float throttleM1 = 1;
    [SerializeField] [Range(0, 1)] float throttleM2 = 1;
    [SerializeField] bool sendToRealRobot = false;
    [SerializeField] bool batteryDeathIrrelevant = false;
    [SerializeField] float batteryCapacity = 0;
    ArticulationDrive wheelDrive;
    public float actionM1 = 0;
    public float actionM2 = 0;
    public int intM1 = 0;
    public int intM2 = 0;

    float BatteryCapacity
    {
        get { return batteryCapacity; }
        set { batteryCapacity = Mathf.Clamp(value, 0, 100); }
    }

    public override void OnEpisodeBegin()
    {
        articulationBody.TeleportRoot(transform.parent.transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity); //resetting M-Bot Position
        BatteryCapacity = 100; // resetting Battery Capacity
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = -Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        actionM1 = actionBuffers.ContinuousActions[1];
        actionM2 = actionBuffers.ContinuousActions[0];
        var leftWheelDrive = Mathf.Clamp(actionM1, -1f, 1f);
        var rightWheelDrive = Mathf.Clamp(actionM2, -1f, 1f);

        Debug.Log($"Action M1: {actionM1} Action M2: {actionM2}");

        if (sendToRealRobot)
        {
            SendMessage();
        }

        BatteryCapacity -= 0.2f;
        // BatteryCapacity -= 0.05f * Mathf.Abs(leftWheelDrive); //turning the wheels drains energy
        // BatteryCapacity -= 0.05f * Mathf.Abs(rightWheelDrive); //turning the wheels drains energy

        wheelDrive = leftWheel.xDrive;
        wheelDrive.targetVelocity = topSpeed * leftWheelDrive;
        leftWheel.xDrive = wheelDrive;

        wheelDrive = rightWheel.xDrive;
        wheelDrive.targetVelocity = topSpeed * rightWheelDrive;
        rightWheel.xDrive = wheelDrive;

        if (!batteryDeathIrrelevant && (BatteryCapacity <= 0))
        {
            EndEpisode();
        }
    }

    void SendMessage()
    {
        //M1
        if (actionM1 > 0)
        {
            intM1 = Mathf.RoundToInt(Mathf.Lerp(0, -250, actionM1));
        }
        if (actionM1 <= 0)
        {
            intM1 = Mathf.RoundToInt(Mathf.Lerp(0, 250, Mathf.Abs(actionM1)));
        }
        //M2
        if (actionM2 > 0)
        {
            intM2 = Mathf.RoundToInt(Mathf.Lerp(0, -250, actionM2));
        }
        if (actionM2 <= 0)
        {
            intM2 = Mathf.RoundToInt(Mathf.Lerp(0, 250, Mathf.Abs(actionM2)));
        }

        SendLogic();
    }
    void SendLogic()
    {
        if (socketConnection == null)
        {
            //do nothing
        }
        else
        {
            try
            {
                // Get a stream object for writing. 			
                NetworkStream stream = socketConnection.GetStream();
                if (stream.CanWrite)
                {
                    var m1IntToSend = Mathf.RoundToInt(intM1 * throttleM1);
                    var m2IntToSend = Mathf.RoundToInt(intM2 * throttleM2);
                    string clientMessage = $"{m1IntToSend} {m2IntToSend}";
                    Debug.Log(clientMessage.PadRight(12).Substring(0, 12));
                    string msgToSend = clientMessage.PadLeft(12).Substring(0, 12);
                    byte[] clientMessagAsByteArray = Encoding.UTF8.GetBytes(msgToSend);
                    stream.Write(clientMessagAsByteArray, 0, clientMessagAsByteArray.Length);
                    Debug.Log("Client sent his message - should be received by server");
                }
            }
            catch (SocketException socketException)
            {
                Debug.Log("Socket exception: " + socketException);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            sendToRealRobot = !sendToRealRobot;
            if (sendToRealRobot)
            {
                Debug.Log("Start sending to real robot");
                socketConnection = new TcpClient();
                socketConnection.Connect(ip, port);
            }

            else
            {
                Debug.Log("Stop sending to real robot");
                socketConnection.Close();
            }
        }
    }

}
