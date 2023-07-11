using UnityEngine;
using UnityEditor;

namespace StuPro{
    [CustomEditor(typeof(Randomizer))]
    [CanEditMultipleObjects]
    public class RandomizerEditor : Editor 
    {
        SerializedProperty randomizer;
        int difficulty;
        int loadingLevelPercentage;
        string loadingStateDirName = "preGernated_Platforms";

        void OnEnable()
        {
            randomizer = serializedObject.FindProperty("randomizer");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            Randomizer randomizer = (Randomizer)target;
            EditorGUILayout.LabelField(" ", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Randomizer Level Controller", EditorStyles.boldLabel);
            loadingLevelPercentage = EditorGUILayout.IntSlider("Probability load state (%)", loadingLevelPercentage, 0, 100);
            randomizer.loadingLevelPercentage = loadingLevelPercentage;
            loadingStateDirName = EditorGUILayout.TextField("Directory name:", loadingStateDirName);
            randomizer.loadingStateDirName = loadingStateDirName;
            EditorGUILayout.LabelField(" ", EditorStyles.boldLabel);
            difficulty = EditorGUILayout.IntSlider("Difficulty: ", difficulty, 0, 11);

            if(GUILayout.Button("Randomize (Diff: " + difficulty + ")")){
                randomizer.RandomizeObstacles(difficulty);
            }
            if(GUILayout.Button("Save to file")){
                randomizer.SaveObstaclesToFile();
            }
            if(GUILayout.Button("Load from file")){
                randomizer.LoadObstaclesFromRandomFile();
            }
        }
    }
}