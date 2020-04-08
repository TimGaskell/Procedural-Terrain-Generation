using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour
{ 
    public Vector2 randomHeightRange = new Vector2(0, 0.1f);
    public Texture2D heightMapImage;
    public Vector3 heightMapScale = new Vector3(1, 1, 1);

    public bool resetTerrain = true;

    //PERLIN NOISE ----------------------------------
    public float perlinXScale = 0.01f;
    public float perlinYScale = 0.01f;
    public int perlinOffsetX = 0;
    public int perlinOffsetY = 0;
    public int perlinOctaves = 3;
    public float perlinPersistance = 8;
    public float perlinHeightScale = 0.09f;

    //MULTIPLE PERLIN -------------
    [System.Serializable]
    public class PerlinParamters {
        public float mPerlinXScale = 0.01f;
        public float mPerlinYScale = 0.01f;
        public int mPerlinOctaves = 3;
        public float mPerlinPersistance = 8;
        public float mPerlinHeighScale = 0.09f;
        public int mPerlinOffsetX = 0;
        public int mPerlinOffsetY = 0;
        public bool remove = false;
    
    }

    public List<PerlinParamters> perlinParamters = new List<PerlinParamters>() {
        new PerlinParamters()
    };

    public Terrain terrain;
    public TerrainData terrainData;


    private void OnEnable() {

        Debug.Log("Initializing Terrain Data");
        terrain = this.GetComponent<Terrain>();
        terrainData = Terrain.activeTerrain.terrainData;
    }

    private void Awake() {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        AddTag(tagsProp, "Terrain");
        AddTag(tagsProp, "Cloud");
        AddTag(tagsProp, "Shore");

        //Apply tag changes to tag database
        tagManager.ApplyModifiedProperties();

        //take this object
        this.gameObject.tag = "Terrain";
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Adds a Tag into the Unity Editor.
    /// </summary>
    /// <param name="tagsProp"> Tag Manager for unity </param>
    /// <param name="newTag"> Name of tag being created </param>
    void AddTag(SerializedProperty tagsProp, string newTag) {

        bool found = false;

        //Ensure the tag doesn't already exist
        for(int i = 0; i < tagsProp.arraySize; i++) {

            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if(t.stringValue.Equals(newTag)) { found = true; break; }
        }

        //Add new tag
        if (!found) {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
            newTagProp.stringValue = newTag;
        }
    }

    float[,] GetHeightMap() {

        if (!resetTerrain) {
            return terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        }
        else {
            return new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
        }

    }

    /// <summary>
    /// Generates a random terrain from a height range.
    /// </summary>
    public void RandomTerrain() {

        float[,] heightMap = GetHeightMap();

        for(int x = 0; x < terrainData.heightmapWidth; x++) {
            for(int z = 0; z < terrainData.heightmapHeight; z++) {
                heightMap[x, z] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    /// <summary>
    /// Resets the terrain heights back to 0
    /// </summary>
    public void ResetTerrain() {

        float[,] heightMap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
        
        for (int x = 0; x < terrainData.heightmapWidth; x++) {
            for (int z = 0; z < terrainData.heightmapHeight; z++) {
                heightMap[x, z] = 0;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    /// <summary>
    /// Generates a terrain by reading in a texture
    /// </summary>
    public void LoadTexture() {
        
        float[,] heightMap = GetHeightMap();

        for (int x = 0; x < terrainData.heightmapWidth; x++) {
            for (int z = 0; z < terrainData.heightmapHeight; z++) {
                heightMap[x, z] += heightMapImage.GetPixel((int)(x * heightMapScale.x), (int)(z * heightMapScale.z)).grayscale * heightMapScale.y;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    /// <summary>
    /// Generates a terrain by using multiple 2D perlin noises of the same parameters 
    /// </summary>
    public void Perlin() {

        float[,] heightMap = GetHeightMap();
       

        for (int x = 0; x < terrainData.heightmapWidth; x++) {
            for (int y = 0; y < terrainData.heightmapHeight; y++) {
               
                heightMap[x, y] += Utility.fBM((x+perlinOffsetX)* perlinXScale, (y + perlinOffsetY) * perlinYScale, perlinOctaves, perlinPersistance) * perlinHeightScale;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    /// <summary>
    /// Generates a terrain by using multiple 2D perlin noises that have varying parameters
    /// </summary>
    public void MultiplePerlinTerrain() {

        float[,] heightMap = GetHeightMap();

        for (int x = 0; x < terrainData.heightmapWidth; x++) {
            for (int y = 0; y < terrainData.heightmapHeight; y++) {

                foreach(PerlinParamters p in perlinParamters) {
                    heightMap[x, y] += Utility.fBM((x + p.mPerlinOffsetX) * p.mPerlinXScale , (y + p.mPerlinOffsetY) * p.mPerlinYScale, p.mPerlinOctaves, p.mPerlinPersistance) * p.mPerlinHeighScale;
                }
               
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    /// <summary>
    /// Adds a new perlin noise set of parameters to a list
    /// </summary>
    public void AddNewPerlin() {
        perlinParamters.Add(new PerlinParamters());
    }
     
    /// <summary>
    /// Removes a perlin noise set of parameters if it has selected to be removed. List must have 1 set of parameters at all times.
    /// </summary>
    public void RemovePerlin() {
        List<PerlinParamters> keptPerlinParameters = new List<PerlinParamters>();

        for (int i =0; i < perlinParamters.Count; i++) {
            if (!perlinParamters[i].remove) {
                keptPerlinParameters.Add(perlinParamters[i]);
            }
        }
        if(keptPerlinParameters.Count == 0) { // don't want to keep any
            keptPerlinParameters.Add(perlinParamters[0]); // add at least 1
        }
        perlinParamters = keptPerlinParameters;
    }

    /// <summary>
    /// Generates a random mountain on the terrain
    /// </summary>
    public void Voronoi() {

        float[,] heightMap = GetHeightMap();
        float fallOff = 0.5f;
        Vector3 peak = new Vector3(UnityEngine.Random.Range(0, terrainData.heightmapWidth),
                                   UnityEngine.Random.Range(0.0f, 1.0f),
                                   UnityEngine.Random.Range(0, terrainData.heightmapHeight)); //random location on the map
        heightMap[(int)peak.x, (int)peak.z] = peak.y;

        Vector2 peakLocation = new Vector2(peak.x, peak.z);
        float maxDistance = Vector2.Distance(new Vector2(0, 0), new Vector2(terrainData.heightmapWidth, terrainData.heightmapHeight)); //distance from corner to corner

        for(int y = 0; y < terrainData.heightmapHeight; y++) {
            for (int x = 0; x < terrainData.heightmapWidth; x++) {
                if(!(x == peak.x && y == peak.z)) { // not at the peak
                    float distanceToPeak = Vector2.Distance(peakLocation, new Vector2(x, y)) * fallOff;
                    heightMap[x, y] = peak.y - (distanceToPeak / maxDistance); // increases height around the map by its proximity to the peak
                }
            }

        }
        terrainData.SetHeights(0, 0, heightMap);
    }
    
}
