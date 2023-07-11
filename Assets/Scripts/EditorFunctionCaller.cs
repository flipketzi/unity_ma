using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EditorFunctionCaller : MonoBehaviour
{
    [SerializeField] SetTestPositions setTestPositions;
    [SerializeField] bool getPose = false;
    [SerializeField] bool savePoseList = false;
    [SerializeField] string fileName = "none";
    [SerializeField] bool setRandomPose = false;
    [SerializeField] int trainingLevel = 0;
    [SerializeField] bool setRandomPoseAndSave = false;
    void Update()
    {
        if (getPose)
        {
            getPose = false;
            Debug.Log("getPose");
            setTestPositions.GetPoseExternal();
        }
        if (savePoseList)
        {
            savePoseList = false;
            setTestPositions.SavePoseListExternal(fileName);
        }
        if (setRandomPose)
        {
            setRandomPose = false;
            setTestPositions.SetRandomPoseTraining(trainingLevel);
        }
        if (setRandomPoseAndSave)
        {
            setRandomPoseAndSave = false;
            setTestPositions.SetRandomPoseTraining(trainingLevel);
            setTestPositions.GetPoseExternal();
        }
    }

}
