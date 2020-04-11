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
    //VORONOI -------------
    public int voronoiPeakCount = 5;
    public float voronoiFallOff = 0.2f;
    public float voronoiDropOff = 0.6f;
    public float voronoiMinHeight = 0.1f;
    public float voronoiMaxHeight = 0.5f;

    //MIDPOINT -------------------
    public float MPHeightMin = -5;
    public float MPHeightMax = 5;
    public float MPHeightDampernerPower = 2;
    public float MPRoughness = 2;

    //SMOOTH
    public int SmoothAmount = 1;

    //SPLAT MAPS ---------------------
    [System.Serializable]
    public class SplatHeights {
        public Texture2D texture = null;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public Vector2 tileOffset = new Vector2(0, 0);
        public Vector2 tileSize = new Vector2(50, 50);
        public float SplatOffSet = 0.1f;
        public float SplatNoiseXScale = 0.01f;
        public float SplatNoiseYScale = 0.01f;
        public float SplatNoiseScalar = 0.1f;
        public bool remove = false;
    }

    public List<SplatHeights> splatHeights = new List<SplatHeights>() {
        new SplatHeights()
    };


    public enum VoronoiType {  Linear = 0, Power = 1, Combined =2, SinPow = 3}
    public VoronoiType voronoiType = VoronoiType.Linear;

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

        for (int p = 0; p < voronoiPeakCount; p++) {
            Vector3 peak = new Vector3(UnityEngine.Random.Range(0, terrainData.heightmapWidth),
                                       UnityEngine.Random.Range(voronoiMinHeight, voronoiMaxHeight),
                                       UnityEngine.Random.Range(0, terrainData.heightmapHeight)); //random location on the map
          

            if(heightMap[(int)peak.x, (int)peak.z] < peak.y) {
                heightMap[(int)peak.x, (int)peak.z] = peak.y;
            }
            else {
                continue;
            }

            Vector2 peakLocation = new Vector2(peak.x, peak.z);
            float maxDistance = Vector2.Distance(new Vector2(0, 0), new Vector2(terrainData.heightmapWidth, terrainData.heightmapHeight)); //distance from corner to corner

            for (int y = 0; y < terrainData.heightmapHeight; y++) {
                for (int x = 0; x < terrainData.heightmapWidth; x++) {
                    if (!(x == peak.x && y == peak.z)) { // not at the peak
                        float distanceToPeak = Vector2.Distance(peakLocation, new Vector2(x, y)) / maxDistance;
                        float h;

                        if (voronoiType == VoronoiType.Combined) {
                            h = peak.y - distanceToPeak * voronoiFallOff - Mathf.Pow(distanceToPeak, voronoiDropOff); // Combined
                        }
                        else if (voronoiType == VoronoiType.Power) {

                            h = peak.y - Mathf.Pow(distanceToPeak, voronoiDropOff) * voronoiFallOff; // Power
                        }
                        else if(voronoiType == VoronoiType.SinPow) {
                            h = peak.y - Mathf.Pow(distanceToPeak * 3, voronoiFallOff) - Mathf.Sin(distanceToPeak * 2 * Mathf.PI) / voronoiDropOff; //sin pow
                        }
                        else {
                            h = peak.y - distanceToPeak * voronoiFallOff; // Linear
                        }

                        if (heightMap[x, y] < h) {
                            heightMap[x, y] = h;
                        } 

                    }
                }

            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    /// <summary>
    /// Generates a terrain by taking mid points for each position on the hex and lifting it along with its mid points
    /// </summary>
    public void MidPointDisplacement() {
        float[,] heightMap = GetHeightMap();
        int width = terrainData.heightmapWidth - 1;
        int squareSize = width;
        float heightMin = MPHeightMin;
        float heightMax = MPHeightMax;
        float heightDampener = (float)Mathf.Pow(MPHeightDampernerPower, -1 * MPRoughness);

        int cornerX, cornerY;
        int midX, midY;
        int pmidXL, pmidXR, pmidYU, pmidYD;

        /*heightMap[0, 0] = UnityEngine.Random.Range(0f, 0.2f);
        heightMap[0, terrainData.heightmapHeight - 2] = UnityEngine.Random.Range(0f, 0.2f);
        heightMap[terrainData.heightmapWidth - 2, 0] = UnityEngine.Random.Range(0f, 0.2f);
        heightMap[terrainData.heightmapHeight - 2, terrainData.heightmapWidth - 2] = UnityEngine.Random.Range(0f, 0.2f); // Assign heights to corners of terrain */
       
        while (squareSize > 0) {
            for (int x = 0; x < width; x += squareSize) {
                for (int y = 0; y < width; y += squareSize) {
                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);

                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);

                    heightMap[midX, midY] = (float)((heightMap[x, y] +
                                                     heightMap[cornerX, y] +
                                                     heightMap[x, cornerY] +
                                                     heightMap[cornerX, cornerY]) / 4.0f + 
                                                     UnityEngine.Random.Range(heightMin, heightMax));
                }
            }
            for (int x = 0; x < width; x += squareSize) { 
                 for(int y = 0; y < width; y += squareSize) {

                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);

                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);

                    pmidXR = (int)(midX + squareSize);
                    pmidYU = (int)(midY + squareSize);
                    pmidXL = (int)(midX - squareSize);
                    pmidYD = (int)(midY - squareSize);

                    if(pmidXL <= 0 || pmidYD <=0 || pmidXR >= width -1 || pmidYU >= width - 1) {
                        continue;
                    }

                    //Calculate the square value for the bottom side
                    heightMap[midX, y] = (float)((heightMap[midX, midY] +
                                                 heightMap[x, y] +
                                                 heightMap[midX, pmidYD] +
                                                 heightMap[cornerX, y]) / 4.0f +
                                                 UnityEngine.Random.Range(heightMin, heightMax));

                    //Calculate the square value for the top side
                    heightMap[midX, cornerY] = (float)((heightMap[x, cornerY] +
                                                 heightMap[midX, midY] +
                                                 heightMap[cornerX, cornerY] +
                                                 heightMap[midX, pmidYU]) / 4.0f +
                                                 UnityEngine.Random.Range(heightMin, heightMax));

                    //Calculate the square value for the left side
                    heightMap[x, midY] = (float)((heightMap[x, y] +
                                                 heightMap[pmidXL, midY] +
                                                 heightMap[x, cornerY] +
                                                 heightMap[midX, midY]) / 4.0f +
                                                 UnityEngine.Random.Range(heightMin, heightMax));

                    //Calculate the square value for the right side
                    heightMap[cornerX, midY] = (float)((heightMap[cornerX, y] +
                                                 heightMap[midX, midY] +
                                                 heightMap[cornerX, cornerY] +
                                                 heightMap[pmidXR, midY]) / 4.0f +
                                                 UnityEngine.Random.Range(heightMin, heightMax));

                }
            
            }
            squareSize = (int)(squareSize / 2.0f);
            heightMin *= heightDampener;
            heightMax *= heightDampener;
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    /// <summary>
    /// Smooths out a terrain by averaging the height of a point by the surrounding points.
    /// </summary>
    public void Smooth() {

        float[,] heightMap = GetHeightMap();
        float smoothProgress = 0;
        EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress / SmoothAmount);

        for (int i = 0; i < SmoothAmount; i++) {
            for (int y = 0; y < terrainData.heightmapHeight; y++) {
                for (int x = 0; x < terrainData.heightmapWidth; x++) {

                    float avgHeight = heightMap[x, y];
                    List<Vector2> neighbours = GenerateNeighbours(new Vector2(x, y), terrainData.heightmapWidth, terrainData.heightmapHeight);

                    foreach (Vector2 n in neighbours) {
                        avgHeight += heightMap[(int)n.x, (int)n.y];
                    }
                    heightMap[x, y] = avgHeight / ((float)neighbours.Count + 1);
                }
            }
            smoothProgress++;
            EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress / SmoothAmount);
        }
        terrainData.SetHeights(0, 0, heightMap);
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// Generates a list of neighboring points on the terrain. Checks if the point is on the edge of the terrain. 
    /// </summary>
    /// <param name="Pos"> terrain position, x , y</param>
    /// <param name="width"> Width of terrain </param>
    /// <param name="height"> Height of terrain </param>
    /// <returns> List of surrounding points of that point </returns>
    List<Vector2> GenerateNeighbours(Vector2 Pos, int width, int height) {
        List<Vector2> neighbours = new List<Vector2>();

        for(int y = - 1; y< 2; y++) {
            for(int x = - 1; x < 2; x++) {

                if(!(x==0 && y == 0)) {
                    Vector2 nPos = new Vector2(Mathf.Clamp(Pos.x + x, 0, width - 1), Mathf.Clamp(Pos.y + y, 0, height - 1));
                    if (!neighbours.Contains(nPos)) {
                        neighbours.Add(nPos);
                    }
                }
            }
        }
        return neighbours;
    }

    /// <summary>
    /// Adds a Texture plus texture information into the splat heights list
    /// </summary>
    public void AddNewSplatHeight() {
        splatHeights.Add(new SplatHeights());
    }

    /// <summary>
    /// Removes a specific splat heights item if they have its remove bool checked
    /// </summary>
    public void RemoveSplatHeight() {

        List<SplatHeights> keptSplatHeights = new List<SplatHeights>();
        for(int i =0; i< splatHeights.Count; i++) {
            if (!splatHeights[i].remove) {
                keptSplatHeights.Add(splatHeights[i]);
            }
        }

        if(keptSplatHeights.Count == 0) {
            keptSplatHeights.Add(splatHeights[0]); // add at least 1
        }
        splatHeights = keptSplatHeights;
    }

    /// <summary>
    /// Creates terrain layers for textures to be placed on the map. Places textures based on the height of the terrain
    /// </summary>
    public void SplatMaps() {
        TerrainLayer[] newSplatPrototype;
        newSplatPrototype = new TerrainLayer[splatHeights.Count];
        int spIndex = 0;
        foreach (SplatHeights sh in splatHeights) { //Generates terrain layer textures in the assets folder. Used to color terrain further on
            newSplatPrototype[spIndex] = new TerrainLayer();
            newSplatPrototype[spIndex].diffuseTexture = sh.texture;
            newSplatPrototype[spIndex].tileOffset = sh.tileOffset;
            newSplatPrototype[spIndex].tileSize = sh.tileSize;
            newSplatPrototype[spIndex].diffuseTexture.Apply(true);
            string path = "Assets/New Terrain Layer " + spIndex + ".terrainLayer";
            AssetDatabase.CreateAsset(newSplatPrototype[spIndex], path);
            spIndex++;
            Selection.activeObject = this.gameObject;
        }

        terrainData.terrainLayers = newSplatPrototype;
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
        float[,,] splatMapData = new float[terrainData.alphamapHeight, terrainData.alphamapWidth, terrainData.alphamapLayers];

        for(int y = 0; y < terrainData.alphamapHeight; y++) {
            for(int x = 0; x < terrainData.alphamapWidth; x++) {

                float[] splat = new float[terrainData.alphamapLayers];
                //Determines if a texture is used at a specific height. If multiple fall at the same height, they are equally blended together
                for(int i =0; i < splatHeights.Count; i++) {

                    float noise = Mathf.PerlinNoise(x * splatHeights[i].SplatNoiseXScale, y * splatHeights[i].SplatNoiseYScale) * splatHeights[i].SplatNoiseScalar;
                    float offset = splatHeights[i].SplatOffSet + noise;

                    float thisHeightStart = splatHeights[i].minHeight - offset;
                    float thisHeightStop = splatHeights[i].maxHeight + offset;
                    if((heightMap[x,y] >= thisHeightStart && heightMap[x,y] <= thisHeightStop)) {
                        splat[i] = 1;
                    }
                }
                NormalizeVector(splat);
                for(int j = 0; j < splatHeights.Count; j++) {
                    splatMapData[x, y, j] = splat[j];
                }

            }
        }
        terrainData.SetAlphamaps(0, 0, splatMapData);
    }

    /// <summary>
    /// Normalizes an array of vectors.
    /// </summary>
    /// <param name="v"> Array of Vectors </param>
    void NormalizeVector(float[] v) {
        float total = 0;

        for(int i = 0; i < v.Length; i++) {
            total += v[i];
        }
        for(int i = 0; i < v.Length; i++) {
            v[i] /= total;
        }
    }



}
