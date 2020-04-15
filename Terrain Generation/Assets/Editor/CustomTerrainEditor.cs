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
    SerializedProperty perlinOctaves;
    SerializedProperty perlinPersistance;
    SerializedProperty perlinHeightScale;
    SerializedProperty resetTerrain;
    SerializedProperty voronoiPeakCount;
    SerializedProperty voronoiFallOff;
    SerializedProperty voronoiDropOff;
    SerializedProperty voronoiMinHeight;
    SerializedProperty voronoiMaxHeight;
    SerializedProperty voronoiType;
    SerializedProperty MPHeightMin;
    SerializedProperty MPHeightMax;
    SerializedProperty MPHeightDampenerPower;
    SerializedProperty MPRoughness;
    SerializedProperty SmoothAmount;

    GUITableState perlinParameterTable;
    SerializedProperty perlinParameters;

    GUITableState splatMapTable;
    SerializedProperty splatHeights;

    GUITableState vegetationTable;
    SerializedProperty vegatation;
    SerializedProperty maxTrees;
    SerializedProperty treeSpacing;

    GUITableState DetailTable;
    SerializedProperty details;
    SerializedProperty maxDetails;
    SerializedProperty detailSpacing;

    SerializedProperty waterHeight;
    SerializedProperty waterGO;
    SerializedProperty shoreMaterial;

    SerializedProperty ErosionType;
    SerializedProperty ErosionStrength;
    SerializedProperty SpringsPerRiver;
    SerializedProperty Solubility;
    SerializedProperty Droplets;
    SerializedProperty erosionSmoothAmount;


    //Fold Outs ------
    bool showRandom = false;
    bool showLoadHeights = false;
    bool showPerlinNoise = false;
    bool showMultiplePerlin = false;
    bool showVoroni = false;
    bool showMidPointDisplacement = false;
    bool showSmooth = false;
    bool showSplatMaps = false;
    bool showHeights = false;
    bool showVegetation = false;
    bool showDetail = false;
    bool showWater = false;
    bool showEroision = false;


    Texture2D hmTexture;

    private void OnEnable() {

        randomHeightRange = serializedObject.FindProperty("randomHeightRange"); //Gets random Height range from linked script in CustomEditor
        heightMapScale = serializedObject.FindProperty("heightMapScale");
        heightMapImage = serializedObject.FindProperty("heightMapImage");
        perlinXScale= serializedObject.FindProperty("perlinXScale");
        perlinYScale = serializedObject.FindProperty("perlinYScale");
        perlinOffsetX = serializedObject.FindProperty("perlinOffsetX");
        perlinOffsetY = serializedObject.FindProperty("perlinOffsetY");
        perlinOctaves = serializedObject.FindProperty("perlinOctaves");
        perlinPersistance = serializedObject.FindProperty("perlinPersistance");
        perlinHeightScale = serializedObject.FindProperty("perlinHeightScale");
        resetTerrain = serializedObject.FindProperty("resetTerrain");
        perlinParameterTable = new GUITableState("perlinParameterTable");
        perlinParameters = serializedObject.FindProperty("perlinParamters");
        voronoiPeakCount = serializedObject.FindProperty("voronoiPeakCount");
        voronoiFallOff = serializedObject.FindProperty("voronoiFallOff");
        voronoiDropOff = serializedObject.FindProperty("voronoiDropOff");
        voronoiMinHeight = serializedObject.FindProperty("voronoiMinHeight");
        voronoiMaxHeight = serializedObject.FindProperty("voronoiMaxHeight");
        voronoiType = serializedObject.FindProperty("voronoiType");
        MPHeightMin = serializedObject.FindProperty("MPHeightMin");
        MPHeightMax = serializedObject.FindProperty("MPHeightMax");
        MPHeightDampenerPower = serializedObject.FindProperty("MPHeightDampernerPower");
        MPRoughness = serializedObject.FindProperty("MPRoughness");
        SmoothAmount = serializedObject.FindProperty("SmoothAmount");
        splatMapTable = new GUITableState("splatMapTable");
        splatHeights = serializedObject.FindProperty("splatHeights");
        hmTexture = new Texture2D(513, 513, TextureFormat.ARGB32, false);
        vegetationTable = new GUITableState("VegatationTable");
        vegatation = serializedObject.FindProperty("vegetation");
        maxTrees = serializedObject.FindProperty("maxTrees");
        treeSpacing = serializedObject.FindProperty("treeSpacing");
        DetailTable = new GUITableState("Details Table");
        details = serializedObject.FindProperty("details");
        maxDetails = serializedObject.FindProperty("maxDetails");
        detailSpacing = serializedObject.FindProperty("detailSpacing");
        waterHeight = serializedObject.FindProperty("waterHeight");
        waterGO = serializedObject.FindProperty("waterGO");
        shoreMaterial = serializedObject.FindProperty("shoreLineMaterial");

        ErosionType = serializedObject.FindProperty("erosionType");
        ErosionStrength = serializedObject.FindProperty("erosionStrength");
        SpringsPerRiver = serializedObject.FindProperty("springsPerRiver");
        Solubility = serializedObject.FindProperty("solubility");
        Droplets = serializedObject.FindProperty("droplets");
        erosionSmoothAmount = serializedObject.FindProperty("erosionSmoothAmount");

    }

    /// <summary>
    /// Method used for creating a modified Unity Editor. Linked to Custom Terrain (Shown up top)
    /// </summary>

    Vector2 scrollPos;
    public override void OnInspectorGUI() {

        serializedObject.Update();

        CustomTerrain terrain = (CustomTerrain)target;

        //ScrollBar Starting code
        Rect r = EditorGUILayout.BeginVertical();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(r.width), GUILayout.Height(r.height));
        EditorGUI.indentLevel++;
        

        EditorGUILayout.PropertyField(resetTerrain);

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
            EditorGUILayout.IntSlider(perlinOctaves, 1, 10, new GUIContent("Octaves"));
            EditorGUILayout.Slider(perlinPersistance, 1, 10, new GUIContent("Persistence"));
            EditorGUILayout.Slider(perlinHeightScale, 0, 1, new GUIContent("Height Scale"));
            if (GUILayout.Button("Perlin")) {
                terrain.Perlin();
            }
        }

        showMultiplePerlin = EditorGUILayout.Foldout(showMultiplePerlin, "Multiple Perlin Noise");

        // Items included in the foldout for generating terrain with multiple Perlin noises;
        if (showMultiplePerlin) {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Multiple Perlin Noise", EditorStyles.boldLabel);
            perlinParameterTable = GUITableLayout.DrawTable(perlinParameterTable, perlinParameters);  // creates a table which stores each perlin noise

            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+")){
                terrain.AddNewPerlin();
            }
            if (GUILayout.Button("-")) {
                terrain.RemovePerlin();
            }
            EditorGUILayout.EndHorizontal();
            if(GUILayout.Button("Apply Multiple Perlin")) {

                terrain.MultiplePerlinTerrain();
            }

        }

        showVoroni = EditorGUILayout.Foldout(showVoroni, "Voronoi");

        // Items included in the foldout for generating a Voronoi mountain on the terrain
        if (showVoroni) {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Voronoi", EditorStyles.boldLabel);
            EditorGUILayout.IntSlider(voronoiPeakCount, 1, 10, new GUIContent("Peak Count"));
            EditorGUILayout.Slider(voronoiFallOff, 0, 10, new GUIContent("Fall off"));
            EditorGUILayout.Slider(voronoiDropOff, 0, 10, new GUIContent("Drop off"));
            EditorGUILayout.Slider(voronoiMinHeight, 0, 1, new GUIContent("Min Height"));
            EditorGUILayout.Slider(voronoiMaxHeight, 0, 1, new GUIContent("Max Height"));
            EditorGUILayout.PropertyField(voronoiType);

            if (GUILayout.Button("Voronoi")) {
                terrain.Voronoi();
            }

        }

        showMidPointDisplacement = EditorGUILayout.Foldout(showMidPointDisplacement, "Mid Point Displacement");

        // Items included in the foldout for generating a Mid point displacement mountain on the terrain
        if (showMidPointDisplacement) {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("MidPoint Displacement", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(MPHeightMin);
            EditorGUILayout.PropertyField(MPHeightMax);
            EditorGUILayout.PropertyField(MPHeightDampenerPower);
            EditorGUILayout.PropertyField(MPRoughness);

            if (GUILayout.Button("Mid Point Displacement")) {
                terrain.MidPointDisplacement();
            }
        }

        showSmooth = EditorGUILayout.Foldout(showSmooth, "Smooth Terrain");

        // Items included in the foldout for generating Smoother terrain
        if (showSmooth) {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Smooth Terrain", EditorStyles.boldLabel);
            EditorGUILayout.IntSlider(SmoothAmount, 1, 10, new GUIContent("Smooth Amount"));
            if (GUILayout.Button("Smooth")) {
                terrain.Smooth();
            }
        }


        showSplatMaps = EditorGUILayout.Foldout(showSplatMaps, "Splat Maps");

        // Items included in the foldout for generating SplatMap Textures for terrain
        if (showSplatMaps) {

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Splat Map", EditorStyles.boldLabel);

            splatMapTable = GUITableLayout.DrawTable(splatMapTable, splatHeights);      

            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+")) {
                terrain.AddNewSplatHeight();
            }
            if (GUILayout.Button("-")) {
                terrain.RemoveSplatHeight();
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Apply SplatMaps")) {

                terrain.SplatMaps();
            }

        }


        showHeights = EditorGUILayout.Foldout(showHeights, "Height Map");

        //Items included in the foldout for generating a height map of the current terrain.
        if (showHeights) {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Current Height Map", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            int hmtSize = (int)(EditorGUIUtility.currentViewWidth - 100);
            GUILayout.Label(hmTexture, GUILayout.Width(hmtSize), GUILayout.Height(hmtSize));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh", GUILayout.Width(hmtSize))){
                float[,] heightMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);

                for (int y = 0; y < terrain.terrainData.heightmapHeight; y++) {
                    for (int x = 0; x < terrain.terrainData.heightmapWidth; x++) {
                        hmTexture.SetPixel(x, y, new Color(heightMap[x, y], heightMap[x, y], heightMap[x, y], 1));
                    }
                }
                hmTexture.Apply();
            }
           
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        showVegetation = EditorGUILayout.Foldout(showVegetation, "Vegetation");

        //Items included in the foldout for generating the tree vegetation on the terrain
        if (showVegetation) {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Vegetation", EditorStyles.boldLabel);
            EditorGUILayout.IntSlider(maxTrees, 0, 10000, new GUIContent("Maximum Trees"));
            EditorGUILayout.IntSlider(treeSpacing, 2, 20, new GUIContent("Tree Spacing"));
            vegetationTable = GUITableLayout.DrawTable(vegetationTable, vegatation);

            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("+")) {
                terrain.AddNewVegetation();
            }
            if (GUILayout.Button("-")) {
                terrain.RemoveVegetation();
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Apply Vegetation")) {

                terrain.plantVegetation();
            }


        }

        showDetail = EditorGUILayout.Foldout(showDetail, "Detail");
        //Items included in the foldout for generating the details (e.g. grass) on the terrain

        if (showDetail) {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Detail", EditorStyles.boldLabel);
            EditorGUILayout.IntSlider(maxDetails, 0, 10000, new GUIContent("Max Details"));
            EditorGUILayout.IntSlider(detailSpacing, 2, 20, new GUIContent("Detail Spacing"));
            DetailTable = GUITableLayout.DrawTable(DetailTable, details);

            terrain.GetComponent<Terrain>().detailObjectDistance = maxDetails.intValue;
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("+")) {
                terrain.AddNewDetails();
            }
            if (GUILayout.Button("-")) {
                terrain.RemoveDetails();
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Apply Vegetation")) {

                terrain.AddDetails();
            }
        }

        //Items included in the foldout for generating Water onto the map.
        showWater = EditorGUILayout.Foldout(showWater, "Water");

        if (showWater) {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Water", EditorStyles.boldLabel);
            EditorGUILayout.Slider(waterHeight, 0, 1, new GUIContent("Water Height"));
            EditorGUILayout.PropertyField(waterGO);

            if(GUILayout.Button("Add Water")) {
                terrain.Addwater();
            }

            EditorGUILayout.PropertyField(shoreMaterial);
            if(GUILayout.Button("Add ShoreLine")) {
                terrain.DrawShoreLine();
            }

        }

        //Items included in the foldout for enabling different types of errosion to occur on the terrain.
        showEroision = EditorGUILayout.Foldout(showEroision, "Erosion");

        if (showEroision) {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Erosion", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(ErosionType);
            EditorGUILayout.Slider(ErosionStrength, 0, 1, new GUIContent("Erosion Strength"));
            EditorGUILayout.IntSlider(Droplets, 0, 500, new GUIContent("Droplets"));
            EditorGUILayout.Slider(Solubility, 0.001f, 1, new GUIContent("Solubility"));
            EditorGUILayout.IntSlider(SpringsPerRiver, 0, 20, new GUIContent("Springs Per River"));
            EditorGUILayout.IntSlider(erosionSmoothAmount, 0, 10, new GUIContent("Smooth Amount"));

            if (GUILayout.Button("Erode")) {
                terrain.Erode();
            }

        }
    

        // Creates the reset button
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    
        if (GUILayout.Button("Reset Terrain")) { 
            terrain.ResetTerrain();
        }


        //Scrollbar ending code
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

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
