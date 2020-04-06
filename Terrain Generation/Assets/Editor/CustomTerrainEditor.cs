using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EditorGUITable;
using UnityEditor;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]
public class CustomTerrainEditor : Editor
{
    //Properties ------
    SerializedProperty randomHeightRange;

    //Fold Outs ------
    bool showRandom = false;

    private void OnEnable() {

        randomHeightRange = serializedObject.FindProperty("randomHeightRange"); //Gets random Height range from linked script in CustomEditor
    }

    /// <summary>
    /// Method used for creating a modified Unity Editor. Linked to Custom Terrain (Shown up top)
    /// </summary>
    public override void OnInspectorGUI() {

        serializedObject.Update();

        CustomTerrain terrain = (CustomTerrain)target;

        showRandom = EditorGUILayout.Foldout(showRandom, "Random");
       
        if (showRandom) {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // Creates Break between items
            GUILayout.Label("Set Heights Between Random Values", EditorStyles.boldLabel); // Text label
            EditorGUILayout.PropertyField(randomHeightRange); //SerializedProperty to be edited
           
            if(GUILayout.Button("Random Heights")) { //Generates Button 
                terrain.RandomTerrain(); //If button pressed will execute code
            }
        }

        serializedObject.ApplyModifiedProperties();
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
