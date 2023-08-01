using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Net.Sockets;
using System;
using System.Text;

public class RemoteControl : MonoBehaviour
{
    public string ip = "painbot.local";
    public int port = 8000;
    private TcpClient socketConnection;
    [SerializeField] float throttle = 1f;
    [SerializeField] int speed = 150;
    [SerializeField] int turningSpeed = 150;
    // Start is called before the first frame update
    void Start()
    {
        socketConnection = new TcpClient();
        socketConnection.Connect(ip, port);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            SendLogic(speed, speed);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            SendLogic(-speed, -speed);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            SendLogic(0, turningSpeed);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            SendLogic(turningSpeed, 0);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            SendLogic(0, 0);
        }

    }

    void SendLogic(int intM1, int intM2)
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
                    var m1IntToSend = Mathf.RoundToInt(intM1 * throttle);
                    var m2IntToSend = Mathf.RoundToInt(intM2 * throttle);
                    string clientMessage = $"{m1IntToSend} {m2IntToSend}";
                    Debug.Log(clientMessage.PadRight(12).Substring(0, 12));
                    string msgToSend = clientMessage.PadLeft(12).Substring(0, 12);
                    byte[] clientMessagAsByteArray = Encoding.UTF8.GetBytes(msgToSend);
                    stream.Write(clientMessagAsByteArray, 0, clientMessagAsByteArray.Length);
                }
            }
            catch (SocketException socketException)
            {
                Debug.Log("Socket exception: " + socketException);
            }
        }
    }
}
