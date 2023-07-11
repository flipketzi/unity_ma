// Source: https://forum.unity.com/threads/how-to-save-manually-save-a-png-of-a-camera-view.506269/, accessed on 29.06.2022

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.MLAgents.Sensors;

namespace StuPro
{
    [System.Serializable]
    public struct RecorderData
    {
        public byte[] image;
        public float[] actions;
        public float[] roboterPos;
        public float[] roboterRot;
        public float[] targetPos;
        public float[] targetRot;
        public float[] dropPos;
    }

    public class Recorder : MonoBehaviour
    {
        [SerializeField] Camera cam;
        [SerializeField] RobotAI agent;
        [SerializeField] string path;
        public bool record;

        private void Awake()
        {
            this.path = Application.persistentDataPath;
        }

        private byte[] GetCameraImage()
        {
            // The Render Texture in RenderTexture.active is the one
            // that will be read by ReadPixels.
            var currentRT = RenderTexture.active;
            RenderTexture.active = cam.targetTexture;

            // Render the camera's view.
            cam.Render();

            // Make a new texture and read the active Render Texture into it.
            Texture2D image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height);
            image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
            image.Apply();

            var Bytes = image.EncodeToPNG();
            Destroy(image);
 
            return Bytes;
        }

        public void Write(string folder, string file)
        {
            if (!record) return;
            if (!Directory.Exists(path + "/" + folder)) Directory.CreateDirectory(path + "/" + folder);
            
            RecorderData data = new RecorderData();

            // Image
            data.image = GetCameraImage();

            // Actions
            data.actions = new float[2];
            data.actions[0] = agent.actionM1;
            data.actions[1] = agent.actionM2;

            // Roboter Position
            data.roboterPos = new float[3];
            data.roboterPos[0] = agent.articulationBody.transform.position.x;
            data.roboterPos[1] = agent.articulationBody.transform.position.y;
            data.roboterPos[2] = agent.articulationBody.transform.position.z;

            // Roboter Rotation
            data.roboterRot = new float[3];
            data.roboterRot[0] = agent.articulationBody.transform.rotation.x;
            data.roboterRot[1] = agent.articulationBody.transform.rotation.y;
            data.roboterRot[2] = agent.articulationBody.transform.rotation.z;

            // Target Position
            data.targetPos = new float[3];
            data.targetPos[0] = agent.targetArticulationBody.transform.position.x;
            data.targetPos[1] = agent.targetArticulationBody.transform.position.y;
            data.targetPos[2] = agent.targetArticulationBody.transform.position.z;

            // Target Rotation
            data.targetRot = new float[3];
            data.targetRot[0] = agent.targetArticulationBody.transform.rotation.x;
            data.targetRot[1] = agent.targetArticulationBody.transform.rotation.y;
            data.targetRot[2] = agent.targetArticulationBody.transform.rotation.z;

            // Drop-Off Position
            data.dropPos = new float[3];
            data.dropPos[0] = agent.dropZone.transform.position.x;
            data.dropPos[1] = agent.dropZone.transform.position.y;
            data.dropPos[2] = agent.dropZone.transform.position.z;
            
            string json = JsonUtility.ToJson(data);
            StreamWriter sr = new StreamWriter(path + "/" + folder + "/" + file + ".json", false);
            sr.WriteLine(json);
            sr.Close();
        }
    }
}