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

    //PERLIN NOISE ----------------------------------
    public float perlinXScale = 0.01f;
    public float perlinYScale = 0.01f;
    public int perlinOffsetX = 0;
    public int perlinOffsetY = 0;
    
    
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

    /// <summary>
    /// Generates a random terrain from a height range.
    /// </summary>
    public void RandomTerrain() {

        float[,] heightMap = terrainData.GetHeights(0,0,terrainData.heightmapWidth,terrainData.heightmapHeight);

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

        float[,] heightMap;
        heightMap = new float[terrainData.heightmapWidth, terrainData.heightmapWidth];

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
        float[,] heightMap;

        heightMap = new float[terrainData.heightmapWidth, terrainData.heightmapWidth];

        for (int x = 0; x < terrainData.heightmapWidth; x++) {
            for (int z = 0; z < terrainData.heightmapHeight; z++) {
                heightMap[x, z] = heightMapImage.GetPixel((int)(x * heightMapScale.x), (int)(z * heightMapScale.z)).grayscale * heightMapScale.y;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    /// <summary>
    /// Generates a terrain by using a 2D perlin noise
    /// </summary>
    public void Perlin() {
        float[,] heightMap;
        heightMap = new float[terrainData.heightmapWidth, terrainData.heightmapWidth];


        for (int x = 0; x < terrainData.heightmapWidth; x++) {
            for (int y = 0; y < terrainData.heightmapHeight; y++) {
                heightMap[x, y] = Mathf.PerlinNoise((x + perlinOffsetX) * perlinXScale, (y  + perlinOffsetY)* perlinYScale);
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }
}
