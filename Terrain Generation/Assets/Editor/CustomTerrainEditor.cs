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
    SerializedProperty heightMapScale;
    SerializedProperty heightMapImage;
    SerializedProperty perlinXScale;
    SerializedProperty perlinYScale;
    SerializedProperty perlinOffsetX;
    SerializedProperty perlinOffsetY;


    //Fold Outs ------
    bool showRandom = false;
    bool showLoadHeights = false;
    bool showPerlinNoise = false;

    private void OnEnable() {

        randomHeightRange = serializedObject.FindProperty("randomHeightRange"); //Gets random Height range from linked script in CustomEditor
        heightMapScale = serializedObject.FindProperty("heightMapScale");
        heightMapImage = serializedObject.FindProperty("heightMapImage");
        perlinXScale= serializedObject.FindProperty("perlinXScale");
        perlinYScale = serializedObject.FindProperty("perlinYScale");
        perlinOffsetX = serializedObject.FindProperty("perlinOffsetX");
        perlinOffsetY = serializedObject.FindProperty("perlinOffsetY");
    }

    /// <summary>
    /// Method used for creating a modified Unity Editor. Linked to Custom Terrain (Shown up top)
    /// </summary>
    public override void OnInspectorGUI() {

        serializedObject.Update();

        CustomTerrain terrain = (CustomTerrain)target;

        showRandom = EditorGUILayout.Foldout(showRandom, "Random");
       
        //Items included in the foldout show Random
        if (showRandom) {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // Creates Break between items
            GUILayout.Label("Set Heights Between Random Values", EditorStyles.boldLabel); // Text label
            EditorGUILayout.PropertyField(randomHeightRange); //SerializedProperty to be edited
           
            if(GUILayout.Button("Random Heights")) { //Generates Button 
                terrain.RandomTerrain(); //If button pressed will execute code
            }
        }


        showLoadHeights = EditorGUILayout.Foldout(showLoadHeights, "Load Heights");

        // Items included in the foldout for setting terrain from image.
        if (showLoadHeights) {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Load Heights From Texture", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(heightMapImage);
            EditorGUILayout.PropertyField(heightMapScale);
            if(GUILayout.Button("Load Texture")) {
                terrain.LoadTexture();
            }
        }

        showPerlinNoise = EditorGUILayout.Foldout(showPerlinNoise, "Single Perlin Noise");

        // Items included in the foldout for generating terrain from 2D Perlin noise
        if (showPerlinNoise) {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("PerlinNoise", EditorStyles.boldLabel);
            EditorGUILayout.Slider(perlinXScale, 0, 1, new GUIContent("X Scale"));
            EditorGUILayout.Slider(perlinYScale, 0, 1, new GUIContent("Y Scale"));
            EditorGUILayout.IntSlider(perlinOffsetX, 0, 10000, new GUIContent("Offset X"));
            EditorGUILayout.IntSlider(perlinOffsetY, 0, 10000, new GUIContent("Offset Y"));
            if (GUILayout.Button("Perlin")) {
                terrain.Perlin();
            }
        }

        // Creates the reset button
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); 
        if (GUILayout.Button("Reset Terrain")) { 
            terrain.ResetTerrain();
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
