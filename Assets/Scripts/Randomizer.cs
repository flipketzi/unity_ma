using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;
using System;

namespace StuPro
{
    
    [System.Serializable]
    public class GameObjectData
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;
    }
    [System.Serializable]
    public class GameObjectsDataList
    {
        public List<GameObjectData> gameObjectsData;
    }

    public class Randomizer : MonoBehaviour, IMove
    {
        // Struct 'Obstacle' defines props of the obstacles on the platform
        [System.Serializable]
        public class Obstacle
        {
            public string id;
            public GameObject obstacle;
            public ArticulationBody articulationBody;
            public bool global;
            public bool randomize;
            public bool randomizeRotation;
            public bool rotated;
            public float y;
            public float minDistanceX;
            public float minDistanceZ;
            public void rotate(Quaternion euler)
            {
                obstacle.transform.rotation = euler;
                rotated = !rotated;

                float tempWidth = minDistanceX;
                minDistanceX = minDistanceZ;
                minDistanceZ = tempWidth;
            }
        }
        
        private Obstacle robot;
        private Obstacle target;
        private Obstacle dropoff;

        public string loadingStateDirName;
        public int loadingLevelPercentage;
        

        [SerializeField] List<Obstacle> obstacles = new List<Obstacle>();
        List<GameObject> notRandObstacles = new List<GameObject>();
        [SerializeField] List<Obstacle> randomized = new List<Obstacle>();
        [SerializeField] List<GameObject> randomAppearanceObjects = new List<GameObject>();

        [SerializeField] GameObject boundary1, boundary2;
        [SerializeField] bool demonstrationOn = false;
        //[SerializeField] List<string> files = new List<string>();
        

        void Start()
        {
            if (demonstrationOn) Time.timeScale = 1;
        }

        private int ctr = 0;

        public void SaveObstaclesToFile(){
            System.DateTime dateAndTime = System.DateTime.Now;
            string dateAndTimeString = dateAndTime.ToString("yyyy-MM-dd_HH-mm-ss_");
            string filename = dateAndTimeString + "obstaclestate.json";
            GameObjectsDataList data = new GameObjectsDataList();
            data.gameObjectsData = new List<GameObjectData>();

            foreach (Obstacle o in obstacles){
                GameObject go = o.obstacle;
                GameObjectData goData = new GameObjectData();
                goData.name = go.name;
                goData.position = go.transform.position;
                goData.rotation = go.transform.rotation;
                data.gameObjectsData.Add(goData);
            }

            string json = JsonUtility.ToJson(data, true);
            Debug.Log(json);
            File.WriteAllText(Application.dataPath + "/" + loadingStateDirName + "/"+ filename, json);

            Debug.Log("Wrote obstacle states to file!");
        }

        public void LoadObstaclesFromFile(string filename){
            string json = File.ReadAllText( Application.dataPath  + "/" + loadingStateDirName + "/" + filename);
            GameObjectsDataList data = JsonUtility.FromJson<GameObjectsDataList>(json);
            foreach (Obstacle o in obstacles){
                GameObjectData objectData = data.gameObjectsData.Find(x => x.name == o.obstacle.name);
                o.obstacle.transform.position = objectData.position;
                o.obstacle.transform.rotation = objectData.rotation;
                data.gameObjectsData.Remove(objectData);
            }
        }

        public void LoadObstaclesFromRandomFile(){
            string dirPath = Application.dataPath  + "/" + loadingStateDirName + "/";
            DirectoryInfo info = new DirectoryInfo(dirPath);
            FileInfo[] files = info.GetFiles();
            
            if(files.Length == 0){
                return;
            }
            List<FileInfo> filteredFiles = new List<FileInfo>();
            foreach(FileInfo f in files){
                if(!f.Name.Contains("meta")){
                    filteredFiles.Add(f);
                }
            }
            int randomIndex = UnityEngine.Random.Range(0, filteredFiles.Count);
            Debug.Log(filteredFiles[randomIndex].Name);
            LoadObstaclesFromFile(filteredFiles[randomIndex].Name);
        }

        public void RandomizeObstacles(float difficulty)
        {
            Debug.Log("diff: " + difficulty);

            if (demonstrationOn) difficulty = 11;

            for (int i = 0; i < obstacles.Count; i++)
            {
                switch (obstacles[i].id)
                {
                    case "AI-Bot": robot = obstacles[i]; break;
                    case "Target": target = obstacles[i]; break;
                    case "DropZone": dropoff = obstacles[i]; break;
                }
            }

            target.articulationBody.velocity = Vector3.zero;
            target.articulationBody.angularVelocity = Vector3.zero;


            //Load the previous difficulty to avoid a reward of 0
            if (difficulty >= 2)
            {
                float randomNumber = UnityEngine.Random.Range(0f, 1f);
                if(randomNumber < 0.3f)
                    difficulty--;
            }

            notRandObstacles.Clear();

            switch (difficulty)
            {
                case 1:
                    robot.randomize = false;
                    target.randomize = false;
                    dropoff.randomize = false;
                    notRandObstacles.Add(robot.obstacle);
                    notRandObstacles.Add(target.obstacle);
                    notRandObstacles.Add(dropoff.obstacle);
                    TeleportObstacleToPosition(robot, new Vector3(0, .2f, -2.5f));
                    TeleportObstacleToPosition(target, new Vector3(0, .3f, 0));
                    TeleportObstacleToPosition(dropoff, new Vector3(0, .005f, 2));
                    RandomizeAll(1,1,1,1);
                    break;

                case 2:
                    robot.randomize = false;
                    target.randomize = false;
                    dropoff.randomize = false;
                    notRandObstacles.Add(robot.obstacle);
                    notRandObstacles.Add(target.obstacle);
                    notRandObstacles.Add(dropoff.obstacle);
                    TeleportObstacleToPosition(robot, new Vector3(0, .2f, UnityEngine.Random.Range(-2.5f,-3f)));
                    TeleportObstacleToPosition(target, new Vector3(UnityEngine.Random.Range(-1f,1f), .3f, 0));
                    TeleportObstacleToPosition(dropoff, new Vector3(UnityEngine.Random.Range(-1f,1f), .005f, 2));
                    RandomizeAll(1,1,1,1);
                    break;

                case 3:
                    robot.randomize = false;
                    target.randomize = false;
                    dropoff.randomize = false;
                    notRandObstacles.Add(robot.obstacle);
                    notRandObstacles.Add(target.obstacle);
                    notRandObstacles.Add(dropoff.obstacle);
                    TeleportObstacleToPosition(robot, new Vector3(0, .2f, UnityEngine.Random.Range(-2.5f,-4f)));
                    TeleportObstacleToPosition(target, new Vector3(UnityEngine.Random.Range(-1.5f,1.5f), .3f, 0));
                    TeleportObstacleToPosition(dropoff, new Vector3(UnityEngine.Random.Range(-2f,2f), .005f, 2));
                    RandomizeAll(1,1,1,1);
                    break;

                case 4:
                    robot.randomize = false;
                    target.randomize = false;
                    dropoff.randomize = false;
                    notRandObstacles.Add(robot.obstacle);
                    notRandObstacles.Add(target.obstacle);
                    notRandObstacles.Add(dropoff.obstacle);
                    TeleportObstacleToPosition(robot, new Vector3(0, .2f, UnityEngine.Random.Range(-2.5f,-4f)));
                    TeleportObstacleToPosition(target, new Vector3(UnityEngine.Random.Range(-2f,2f), .3f, 0));
                    TeleportObstacleToPosition(dropoff, new Vector3(UnityEngine.Random.Range(-3f,3f), .005f, 2));
                    RandomizeAll(1,1,1,1);
                    break;

                case 5:
                    robot.randomize = false;
                    target.randomize = false;
                    dropoff.randomize = false;
                    notRandObstacles.Add(robot.obstacle);
                    notRandObstacles.Add(target.obstacle);
                    notRandObstacles.Add(dropoff.obstacle);
                    TeleportObstacleToPosition(robot, new Vector3(0, .2f, UnityEngine.Random.Range(-2.5f,-4f)));
                    TeleportObstacleToPosition(target, new Vector3(UnityEngine.Random.Range(-2f,2f), .3f, 0));
                    TeleportObstacleToPosition(dropoff, new Vector3(UnityEngine.Random.Range(-4f,4f), .005f, 2));
                    RandomizeAll(1,1,1,1);
                    break;

                case 6:
                    
                    for (int i = 0; i < obstacles.Count; i++)
                    {
                        Obstacle obst;
                        obst = obstacles[i];
                        switch (obst.id)
                        {
                            case "AI-Bot": 
                                obst.randomize = false;
                                break;
                            case "Target": 
                                obst.randomize = false;
                                break;
                            case "DropZone": 
                                obst.randomize = true;
                                break;
                        }
                    }

            
                    for (int i = 0; i < obstacles.Count; i++)
                    {
                        Obstacle obst = obstacles[i];
                        //obst.randomize = true;
                    }

                    notRandObstacles.Add(robot.obstacle);
                    notRandObstacles.Add(target.obstacle);
                    TeleportObstacleToPosition(robot, new Vector3(0, .2f, UnityEngine.Random.Range(-2.5f,-4f)));
                    TeleportObstacleToPosition(target, new Vector3(UnityEngine.Random.Range(-3f,3f), .3f, 0));
                    RandomizeAll(1,1,0,.2f);
                    break;

                case 7:
                    robot.randomize = false;
                    target.randomize = false;
                    dropoff.randomize = true;
                    notRandObstacles.Add(robot.obstacle);
                    notRandObstacles.Add(target.obstacle);
                    TeleportObstacleToPosition(robot, new Vector3(0, .2f, UnityEngine.Random.Range(-2.5f,-4f)));
                    TeleportObstacleToPosition(target, new Vector3(UnityEngine.Random.Range(-4f,4f), .3f, 0));
                    RandomizeAll(1,1,.3f,.3f);
                    break;

                case 8:

                    robot.randomize = true;
                    target.randomize = true;
                    dropoff.randomize = true;
                    RandomizeAll(1,1,.3f,.3f);
                    break;


                case 9:
                    robot.randomize = true;
                    target.randomize = true;
                    dropoff.randomize = true;
                    RandomizeAll(1,1,.5f,.5f);
                    break;

                case 10:
                    robot.randomize = true;
                    target.randomize = true;
                    dropoff.randomize = true;
                    RandomizeAll(1,1,.75f,.75f);
                    break;

                case 11:
                    robot.randomize = true;
                    target.randomize = true;
                    dropoff.randomize = true;
                    RandomizeAll(1,1,1,1);
                    break;

                default:
                    robot.randomize = false;
                    target.randomize = false;
                    dropoff.randomize = false;
                    notRandObstacles.Add(robot.obstacle);
                    notRandObstacles.Add(target.obstacle);
                    notRandObstacles.Add(dropoff.obstacle);
                    TeleportObstacleToPosition(robot, new Vector3(0, .2f, -2));
                    TeleportObstacleToPosition(target, new Vector3(0, .3f, 0));
                    TeleportObstacleToPosition(dropoff, new Vector3(0, .005f, 1));
                    RandomizeAll(1,1,1,1);
                    break;
            }
        }

        public void RandomizeAppearances(){
            foreach(GameObject o in randomAppearanceObjects){
                AppearanceComponent ac = o.GetComponent<AppearanceComponent>();
                MeshRenderer[] objectRenderers = o.GetComponents<MeshRenderer>();
                MeshRenderer[] childRenderers = o.GetComponentsInChildren<MeshRenderer>();
                //select random material
                int nextIndex = UnityEngine.Random.Range(0, ac.materialList.Count);
                Material nextMaterial = Instantiate(ac.materialList[nextIndex]);
                //change color or brightness
                if(ac.colorVariance){
                    if(ac.uniformColorVariance){
                    float uniformColorOffset = UnityEngine.Random.Range(0.0f, ac.materialColorVariance);
                    nextMaterial.color = new Color(1.0f - uniformColorOffset, 1.0f - uniformColorOffset, 1.0f - uniformColorOffset, 1f);
                    }else{
                        nextMaterial.color = new Color(UnityEngine.Random.Range(1.0f - ac.materialColorVariance, 1.0f), UnityEngine.Random.Range(1.0f - ac.materialColorVariance, 1.0f), UnityEngine.Random.Range(1.0f - ac.materialColorVariance, 1.0f), 1f);
                    }
                }
                
                //apply for each renderer
                foreach(MeshRenderer renderer in objectRenderers){
                    Destroy(renderer.material);
                    renderer.material = nextMaterial;
                }
                foreach(MeshRenderer renderer in childRenderers){
                    Destroy(renderer.material);
                    renderer.material = nextMaterial;
                }
            }
        }

        private void TeleportObstacleToPosition(Obstacle obstacle, Vector3 point)
        {
            obstacle.obstacle.transform.position = transform.parent.transform.position + point;
            if (obstacle.articulationBody) obstacle.articulationBody.TeleportRoot(transform.parent.transform.position + point, Quaternion.identity);
            Debug.DrawLine(point, point + Vector3.up, Color.red, 1f);
        }

        private void RandomizeAll(float xFactor1, float xFactor2, float zFactor1, float zFactor2)
        {
            float x = boundary1.transform.position.x;
            float y = boundary1.transform.position.z;
            float w = Math.Abs(boundary2.transform.position.x - x);
            float h = Math.Abs(boundary2.transform.position.z - y);

            QuadTree quadTree = new QuadTree(0, 0, w, h, this);

            foreach (Obstacle o in obstacles)
            {
                if (!o.randomize)
                    continue;

                if (o.randomizeRotation)
                    o.rotate(Quaternion.Euler(0, UnityEngine.Random.Range(0, 4) * 90, 0));

                WrapperRectangle wr;
                if (o == dropoff || o == target)
                {
                    wr = new WrapperRectangle(o, o.minDistanceX, o.minDistanceZ);
                } else if(o == robot)
                {
                    if(o.rotated)
                        wr = new WrapperRectangle(o, 9, 4);
                    else
                        wr = new WrapperRectangle(o, 4, 9);
                }
                else
                {
                    if (o.rotated)
                        wr = new WrapperRectangle(o, 1f, 3);
                    else
                        wr = new WrapperRectangle(o, 3, 1f);
                }

                quadTree.insert(wr, notRandObstacles);
            }
            quadTree.iterate();
        }

        public void Move(QuadTreeNode toMove)
        {
            Obstacle obstacle = toMove.getData();

            Vector3 newPosition = new Vector3(toMove.getPosX() - 7, obstacle.y, toMove.getPosY() - 11);
            TeleportObstacleToPosition(obstacle, newPosition);
        }
    }

}
