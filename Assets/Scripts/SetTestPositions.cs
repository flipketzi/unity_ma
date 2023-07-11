using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class SerializableList<T>
{
    public List<T> list;
}
public class SetTestPositions : MonoBehaviour
{

    [System.Serializable]
    public struct Objects
    {
        public string name;
        public GameObject gameObject;
        public ArticulationBody articulationBody;
    }
    [System.Serializable]
    public struct Pose
    {
        [HideInInspector] public int number;
        public Vector3[] position;
        public Quaternion[] rotation;
        public Vector3[] rotationEuler;
    }
    [SerializeField] List<Objects> objects = new List<Objects>();
    [SerializeField] SerializableList<Pose> posesEnvironment = new SerializableList<Pose>();
    [SerializeField] SerializableList<Pose> posesMeasurement = new SerializableList<Pose>();
    [SerializeField] int currentPose = 0;
    [SerializeField] string savedPose;
    [Header("Testing")]
    [SerializeField] string fileNameTestEnvironment;
    [SerializeField] int currentTestEnvironment = 1;
    int currentTestRunInEnvironment = 0;
    [SerializeField] int maxTestRunsPerEnvironemt = 10;
    bool firstEnvAndRun = true;
    void Start()
    {
        LoadPoseList();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GetPose(posesEnvironment);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetPose(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SavePoseList(posesEnvironment, "TestEnvironments");
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            LoadPoseList();
        }
    }
    //to be called from the Agent to log the current Data
    public void Log()
    {
        GetPose(posesMeasurement);
    }

    public void LogFirstStep()
    {
        GetInitialPose(posesMeasurement);
    }

    void GetInitialPose(SerializableList<Pose> _poseList)
    {
        //initalize new Pose struct and add it to list
        Pose initPose = new Pose();
        initPose.number = currentPose;
        initPose.position = new Vector3[objects.Count];
        initPose.rotation = new Quaternion[objects.Count];
        initPose.rotationEuler = new Vector3[objects.Count];
        _poseList.list.Add(initPose);

        int counter = 0;
        foreach (Objects o in objects)
        {
            _poseList.list[currentPose].position[counter] = posesEnvironment.list[currentTestEnvironment].position[counter];
            _poseList.list[currentPose].rotation[counter] = posesEnvironment.list[currentTestEnvironment].rotation[counter];
            counter++;
        }
        currentPose++;
    }
    public void GetPoseExternal()
    {
        GetPose(posesEnvironment);
    }

    public void SavePoseListExternal(string name)
    {
        SavePoseList(posesEnvironment, name);
    }

    void GetPose(SerializableList<Pose> _poseList)
    {
        //initalize new Pose struct and add it to list
        Pose initPose = new Pose();
        initPose.number = currentPose;
        initPose.position = new Vector3[objects.Count];
        initPose.rotation = new Quaternion[objects.Count];
        initPose.rotationEuler = new Vector3[objects.Count];
        _poseList.list.Add(initPose);

        //add the position and rotation values of the objects to the
        int counter = 0;
        foreach (Objects o in objects)
        {
            _poseList.list[currentPose].position[counter] = o.gameObject.transform.localPosition;
            _poseList.list[currentPose].rotation[counter] = o.gameObject.transform.localRotation;
            _poseList.list[currentPose].rotationEuler[counter] = o.gameObject.transform.localEulerAngles;
            counter++;
        }
        currentPose++;
    }

    void SavePoseList(SerializableList<Pose> poseList, string fileName)
    {
        savedPose = JsonUtility.ToJson(poseList, false);
        System.DateTime dateAndTime = System.DateTime.Now;
        string dateAndTimeString = dateAndTime.ToString("yyyy-MM-dd_HH-mm-ss_");
        if (poseList == posesMeasurement)
        {
            File.WriteAllText(Application.dataPath + "/Data/Measurements/" + dateAndTimeString + fileName + ".json", savedPose);
        }
        else
        {
            File.WriteAllText(Application.dataPath + "/Data/TestEnvironments/" + fileName + ".json", savedPose);
        }
    }
    void LoadPoseList()
    {
        savedPose = File.ReadAllText(Application.dataPath + "/Data/TestEnvironments/" + fileNameTestEnvironment + ".json");
        posesEnvironment = JsonUtility.FromJson<SerializableList<Pose>>(savedPose);
    }

    void SetPose(int poseIndex)
    {
        int counter = 0;
        foreach (Objects o in objects)
        {
            if (o.gameObject != null)
            {
                TeleportObjectToPosition(o.gameObject, posesEnvironment.list[poseIndex].position[counter], posesEnvironment.list[poseIndex].rotation[counter]);
            }
            if (o.articulationBody != null)
            {
                TeleportObjectToPosition(o.articulationBody, posesEnvironment.list[poseIndex].position[counter], posesEnvironment.list[poseIndex].rotation[counter]);
            }
            counter++;
        }
    }

    public void SetAndSaveMeasurementPose()
    {
        //dont save first set
        if (firstEnvAndRun)
        {
            SetPose(currentTestEnvironment);
            firstEnvAndRun = false;
        }
        else if ((currentTestRunInEnvironment + 1) < maxTestRunsPerEnvironemt)
        {
            SavePoseList(posesMeasurement, "Env-" + currentTestEnvironment + "_Run-" + currentTestRunInEnvironment);
            Debug.Log("Env: " + currentTestEnvironment + " Run: " + currentTestRunInEnvironment);
            currentTestRunInEnvironment++;
            SetPose(currentTestEnvironment);
        }
        else if ((currentTestEnvironment + 1) < posesEnvironment.list.Count)
        {
            SavePoseList(posesMeasurement, "Env-" + currentTestEnvironment + "_Run-" + currentTestRunInEnvironment);
            Debug.Log("Env: " + currentTestEnvironment + " Run: " + currentTestRunInEnvironment);
            currentTestEnvironment++;
            currentTestRunInEnvironment = 0;
            SetPose(currentTestEnvironment);
        }
        else
        {
            SavePoseList(posesMeasurement, "Env-" + currentTestEnvironment + "_Run-" + currentTestRunInEnvironment);
            Debug.Log("Quit");
            UnityEditor.EditorApplication.isPlaying = false;
        }

        //Clears the list and resets the list number counter
        currentPose = 0;
        posesMeasurement.list.Clear();
    }

    public void SetRandomPoseTraining(float levelDifficulty)
    {
        switch (levelDifficulty)
        {
            case 1:
                TeleportObjectToPosition(objects[0].articulationBody, new Vector3(0, .45f, -1));
                TeleportObjectToPosition(objects[1].articulationBody, new Vector3(0, .3f, 0));
                TeleportObjectToPosition(objects[2].gameObject, new Vector3(0, .005f, 1));
                break;
            case 2:
                TeleportObjectToPosition(objects[0].articulationBody, new Vector3(0, .45f, -2));
                TeleportObjectToPosition(objects[1].articulationBody, new Vector3(0, .3f, 0));
                TeleportObjectToPosition(objects[2].gameObject, new Vector3(0, .005f, 2));
                break;
            case 3:
                TeleportObjectToPosition(objects[0].articulationBody, new Vector3(0, .45f, -2));
                TeleportObjectToPosition(objects[1].articulationBody, new Vector3(0, .3f, 0));
                TeleportObjectToPosition(objects[2].gameObject, new Vector3(Random.Range(-1f, 1f), .005f, 2));
                break;
            case 4:
                TeleportObjectToPosition(objects[0].articulationBody, new Vector3(0, .45f, -2));
                TeleportObjectToPosition(objects[1].articulationBody, new Vector3(0, .3f, 0));
                TeleportObjectToPosition(objects[2].gameObject, new Vector3(Random.Range(-2f, 2f), .005f, 2));
                break;
            case 5:
                TeleportObjectToPosition(objects[0].articulationBody, new Vector3(0, .45f, -2));
                TeleportObjectToPosition(objects[1].articulationBody, new Vector3(0, .3f, 0));
                TeleportObjectToPosition(objects[2].gameObject, new Vector3(Random.Range(-4f, 4f), .005f, 2));
                break;
            //Transission was hard in Training make it smoother
            case 6:
                TeleportObjectToPosition(objects[0].articulationBody, new Vector3(0, .45f, -2));
                TeleportObjectToPosition(objects[1].articulationBody, new Vector3(0, .3f, 0), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                TeleportObjectToPosition(objects[2].gameObject, new Vector3(Random.Range(-6f, 6f), .005f, 2), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                break;
            case 7:
                TeleportObjectToPosition(objects[0].articulationBody, new Vector3(0, .45f, -2), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                TeleportObjectToPosition(objects[1].articulationBody, new Vector3(0, .3f, 0), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                TeleportObjectToPosition(objects[2].gameObject, new Vector3(Random.Range(-6f, 6f), .005f, 2), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                break;
            case 8:
                TeleportObjectToPosition(objects[0].articulationBody, new Vector3(0, .45f, -2), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                TeleportObjectToPosition(objects[1].articulationBody, new Vector3(Random.Range(-6f, 6f), .3f, Random.Range(-2f, 2f)), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                TeleportObjectToPosition(objects[2].gameObject, new Vector3(Random.Range(-6f, 6f), .005f, Random.Range(-2f, 2f)), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                break;
            case 9:
                TeleportObjectToPosition(objects[0].articulationBody, new Vector3(Random.Range(-6f, 6f), .45f, Random.Range(-2f, 2f)), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                TeleportObjectToPosition(objects[1].articulationBody, new Vector3(Random.Range(-6f, 6f), .3f, Random.Range(-4f, 4f)), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                TeleportObjectToPosition(objects[2].gameObject, new Vector3(Random.Range(-6f, 6f), .005f, Random.Range(-4f, 4f)), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                break;
            case 10:
                TeleportObjectToPosition(objects[0].articulationBody, new Vector3(Random.Range(-6f, 6f), .45f, Random.Range(-8f, 8f)), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                TeleportObjectToPosition(objects[1].articulationBody, new Vector3(Random.Range(-6f, 6f), .3f, Random.Range(-8f, 8f)), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                TeleportObjectToPosition(objects[2].gameObject, new Vector3(Random.Range(-6f, 6f), .005f, Random.Range(-8f, 8f)), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                break;
            case 11:
                TeleportObjectToPosition(objects[0].articulationBody, new Vector3(Random.Range(-6f, 6f), .45f, Random.Range(-10f, 10f)), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                TeleportObjectToPosition(objects[1].articulationBody, new Vector3(Random.Range(-6f, 6f), .3f, Random.Range(-10f, 10f)), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                TeleportObjectToPosition(objects[2].gameObject, new Vector3(Random.Range(-6f, 6f), .005f, Random.Range(-10f, 10f)), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                break;
            case 69:
                TeleportObjectToPosition(objects[0].gameObject, new Vector3(Random.Range(-6f, 6f), .45f, Random.Range(-10f, 10f)), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                TeleportObjectToPosition(objects[1].gameObject, new Vector3(Random.Range(-6f, 6f), .3f, Random.Range(-10f, 10f)), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                TeleportObjectToPosition(objects[2].gameObject, new Vector3(Random.Range(-6f, 6f), .005f, Random.Range(-10f, 10f)), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                break;
        }
    }
    void TeleportObjectToPosition(GameObject gameObject, Vector3 objectPositon, Quaternion objectRotation)
    {
        gameObject.transform.localPosition = objectPositon;
        gameObject.transform.rotation = objectRotation;
    }
    void TeleportObjectToPosition(ArticulationBody articulationBody, Vector3 objectPositon, Quaternion objectRotation)
    {
        articulationBody.TeleportRoot(transform.parent.transform.position + objectPositon, objectRotation);
        articulationBody.jointPosition = new ArticulationReducedSpace(0f, 0f, 0f);
        articulationBody.jointAcceleration = new ArticulationReducedSpace(0f, 0f, 0f);
        articulationBody.jointForce = new ArticulationReducedSpace(0f, 0f, 0f);
        articulationBody.jointVelocity = new ArticulationReducedSpace(0f, 0f, 0f);
        articulationBody.velocity = Vector3.zero;
        articulationBody.angularVelocity = Vector3.zero;
    }
    void TeleportObjectToPosition(GameObject gameObject, Vector3 objectPositon)
    {
        gameObject.transform.localPosition = objectPositon;
        gameObject.transform.rotation = Quaternion.identity;
    }
    void TeleportObjectToPosition(ArticulationBody articulationBody, Vector3 objectPositon)
    {
        articulationBody.TeleportRoot(transform.parent.transform.position + objectPositon, Quaternion.identity);
        articulationBody.jointPosition = new ArticulationReducedSpace(0f, 0f, 0f);
        articulationBody.jointAcceleration = new ArticulationReducedSpace(0f, 0f, 0f);
        articulationBody.jointForce = new ArticulationReducedSpace(0f, 0f, 0f);
        articulationBody.jointVelocity = new ArticulationReducedSpace(0f, 0f, 0f);
        articulationBody.velocity = Vector3.zero;
        articulationBody.angularVelocity = Vector3.zero;
    }
    int RandomNegativePositiveSign()
    {
        if (Random.value > 0.5f)
        {
            return 1;
        }
        else
        {
            return -1;
        }
    }
}
