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

    public enum VoronoiType { Linear = 0, Power = 1, Combined = 2, SinPow = 3 }
    public VoronoiType voronoiType = VoronoiType.Linear;

    public List<PerlinParamters> perlinParamters = new List<PerlinParamters>() {
        new PerlinParamters()
    };


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
        public float minSlope = 0;
        public float maxSlope = 1.5f;
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

    //Vegetation --------------------------

    [System.Serializable]
    public class Vegetation {
        public GameObject mesh;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0;
        public float maxSlope = 90;
        public float minScale = 0.5f;
        public float maxScale = 1.0f;
        public Color colour1 = Color.white;
        public Color colour2 = Color.white;
        public Color lightColour = Color.white;
        public float minRotation = 0;
        public float maxRotation = 360;
        public float density = 0.5f;
        public bool remove = false;
    }

    public List<Vegetation> vegetation = new List<Vegetation>() {
        new Vegetation()
    };

    public int maxTrees = 5000;
    public int treeSpacing = 5;


    //Details ------------------------------------

    [System.Serializable]
    public class Detail {
        public GameObject prototype = null;
        public Texture2D prototypeTexture = null;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0;
        public float maxSlope = 1;
        public Color dryColor = Color.white;
        public Color healthyColor = Color.white;
        public Vector2 heightRange = new Vector2(1, 1);
        public Vector2 widthRange = new Vector2(1, 1);
        public float noiseSpread = 0.5f;
        public float overlap = 0.01f;
        public float feather = 0.05f;
        public float density = 0.5f;
        public bool remove = false;
    }

    public List<Detail> details = new List<Detail>() {
        new Detail()
    };

    public int maxDetails = 5000;
    public int detailSpacing = 5;



    public Terrain terrain;
    public TerrainData terrainData;

    public enum TagType { Tag = 0, Layer = 1}
    [SerializeField]
    int terrainLayer = 0;


    private void OnEnable() {

        Debug.Log("Initializing Terrain Data");
        terrain = this.GetComponent<Terrain>();
        terrainData = Terrain.activeTerrain.terrainData;
    }

    private void Awake() {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        AddTag(tagsProp, "Terrain",TagType.Tag);
        AddTag(tagsProp, "Cloud",TagType.Tag);
        AddTag(tagsProp, "Shore",TagType.Tag);

        //Apply tag changes to tag database
        tagManager.ApplyModifiedProperties();

        SerializedProperty layerProp = tagManager.FindProperty("layers");
        terrainLayer = AddTag(layerProp, "Terrain", TagType.Layer);
        tagManager.ApplyModifiedProperties();

        //take this object
        this.gameObject.tag = "Terrain";
        this.gameObject.layer = terrainLayer;
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
    /// <param name="tType"> Differs if it is a tag or a layer </param>
    int AddTag(SerializedProperty tagsProp, string newTag, TagType tType) {

        bool found = false;

        //Ensure the tag doesn't already exist
        for(int i = 0; i < tagsProp.arraySize; i++) {

            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if(t.stringValue.Equals(newTag)) { found = true; return i; }
        }

        //Add new tag
        if (!found && tType == TagType.Tag) {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
            newTagProp.stringValue = newTag;
        }
        else if(!found && tType == TagType.Layer) { //Adds new layer to layers
            for(int j = 8; j < tagsProp.arraySize; j++) {
                SerializedProperty newLayer = tagsProp.GetArrayElementAtIndex(j);
                //Add layer in next empty slot
                {
                    Debug.Log("Adding New Layer " + newTag);
                    newLayer.stringValue = newTag;
                    return j;
                }

            }
        }
        return -1;
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
                for(int i =0; i < splatHeights.Count; i++) {

                    float noise = Mathf.PerlinNoise(x * splatHeights[i].SplatNoiseXScale, y * splatHeights[i].SplatNoiseYScale) * splatHeights[i].SplatNoiseScalar;
                    float offset = splatHeights[i].SplatOffSet + noise;

                    //Sets the height ranges for texture of where it can be placed.
                    float thisHeightStart = splatHeights[i].minHeight - offset;
                    float thisHeightStop = splatHeights[i].maxHeight + offset;

                    // Another way to manually get the steepness of the terrain based on its neighbors; 
                    //float steepness = GetSteepness(heightMap, x, y, terrainData.heightmapWidth, terrainData.heightmapHeight); 

                    //Sets the steepness of the terrain. Used for texturing based if steepness is too great
                    float steepness = terrainData.GetSteepness(y / (float)terrainData.alphamapHeight, x / (float)terrainData.alphamapWidth);

                    if((heightMap[x,y] >= thisHeightStart && heightMap[x,y] <= thisHeightStop)&&
                        (steepness >= splatHeights[i].minSlope && steepness <= splatHeights[i].maxSlope)) {
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
    /// Gets the gradient between a current point and its neighboring point by (x+1,y) and (x,y+1)
    /// </summary>
    /// <param name="heightmap"> Current height map array </param>
    /// <param name="x"> x coordinate on height map</param>
    /// <param name="y"> y coordinate on height map</param>
    /// <param name="width"> Width of height map</param>
    /// <param name="height"> Height of height map</param>
    /// <returns></returns>
    float GetSteepness( float[,] heightmap, int x, int y, int width, int height) {

        float h = heightmap[x, y];
        int nx = x + 1;
        int ny = y + 1;

        //if on the upper edge of the map find gradient by going backward;
        if(nx > width - 1) {
            nx = x - 1;
        }
        if(ny > height -1) {
            ny = y - 1;
        }

        float dx = heightmap[nx, y] - h;
        float dy = heightmap[x, ny] - h;
        Vector2 gradient = new Vector2(dx, dy);

        float steep = gradient.magnitude;

        return steep;


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

    /// <summary>
    /// Generates the tree vegetation on the terrain. Loops through all trees that are to be added and places them based on height and slope constraints.
    /// </summary>
    public void plantVegetation() {

        TreePrototype[] newTreePrototypes;
        newTreePrototypes = new TreePrototype[vegetation.Count];
        int tindex = 0;
        foreach (Vegetation t in vegetation) {
            newTreePrototypes[tindex] = new TreePrototype();
            newTreePrototypes[tindex].prefab = t.mesh;
            tindex++;
        }
        terrainData.treePrototypes = newTreePrototypes;

        List<TreeInstance> allVegetation = new List<TreeInstance>();
        for (int z = 0; z < terrainData.size.z; z += treeSpacing) { //Obtains the position in world space relative to the terrain location
            for (int x = 0; x < terrainData.size.x; x += treeSpacing) {
                for (int tp = 0; tp < terrainData.treePrototypes.Length; tp++) {
                    if (UnityEngine.Random.Range(0.0f, 1.0f) > vegetation[tp].density) break;

                    float thisHeight = terrainData.GetHeight((int)(x * terrainData.alphamapWidth / terrainData.size.x),(int)(z * terrainData.alphamapHeight / terrainData.size.z)) / terrainData.size.y; ; 
                    float thisHeightStart = vegetation[tp].minHeight;
                    float thisHeightEnd = vegetation[tp].maxHeight;

                    float steepness = terrainData.GetSteepness(x / (float)terrainData.size.x,
                                                               z / (float)terrainData.size.z);

                    if ((thisHeight >= thisHeightStart && thisHeight <= thisHeightEnd) && // Determines if tree can be on a specific height set or slope
                        (steepness >= vegetation[tp].minSlope && steepness <= vegetation[tp].maxSlope)) {
                        TreeInstance instance = new TreeInstance();
                        instance.position = new Vector3((x + UnityEngine.Random.Range(-5.0f, 5.0f)) / terrainData.size.x,
                                                        terrainData.GetHeight(x, z) / terrainData.size.y,
                                                        (z + UnityEngine.Random.Range(-5.0f, 5.0f)) / terrainData.size.z);

                        Vector3 treeWorldPos = new Vector3(instance.position.x * terrainData.size.x,
                            instance.position.y * terrainData.size.y,
                            instance.position.z * terrainData.size.z)
                                                         + this.transform.position;

                        RaycastHit hit;
                        int layerMask = 1 << terrainLayer;

                        if (Physics.Raycast(treeWorldPos + new Vector3(0, 100, 0), -Vector3.up, out hit, 100, layerMask) || //Performs raycast check to ensure tree origins aren't places above or below the terrain
                            Physics.Raycast(treeWorldPos - new Vector3(0, 100, 0), Vector3.up, out hit, 100, layerMask)) { // If they aren't detected then they wont be drawn 
                            float treeHeight = (hit.point.y - this.transform.position.y) / terrainData.size.y;
                            instance.position = new Vector3(instance.position.x,
                                                             treeHeight,
                                                             instance.position.z);



                            instance.rotation = UnityEngine.Random.Range(vegetation[tp].minRotation,
                                                                         vegetation[tp].maxRotation);
                            instance.prototypeIndex = tp;
                            instance.color = Color.Lerp(vegetation[tp].colour1,
                                                        vegetation[tp].colour2,
                                                        UnityEngine.Random.Range(0.0f, 1.0f));
                            instance.lightmapColor = vegetation[tp].lightColour;
                            float s = UnityEngine.Random.Range(vegetation[tp].minScale, vegetation[tp].maxScale);
                            instance.heightScale = s;
                            instance.widthScale = s;

                            allVegetation.Add(instance);
                            if (allVegetation.Count >= maxTrees) goto TREESDONE;
                        }


                    }
                }
            }
        }
    TREESDONE:
        terrainData.treeInstances = allVegetation.ToArray();

    }

    /// <summary>
    /// Adds a new vegetation set of data into the list
    /// </summary>
    public void AddNewVegetation() {
        vegetation.Add(new Vegetation());
    }

    /// <summary>
    /// Removes the vegetation data from the list if it has its remove bool checked. Will always have one item left in the list
    /// </summary>
    public void RemoveVegetation() {


        List<Vegetation> keptVegetation = new List<Vegetation>();
        for (int i = 0; i < vegetation.Count; i++) {
            if (!vegetation[i].remove) {
                keptVegetation.Add(vegetation[i]);
            }
        }

        if (keptVegetation.Count == 0) {
            keptVegetation.Add(vegetation[0]); // add at least 1
        }
        vegetation = keptVegetation;
    }

    /// <summary>
    /// Function responsible for adding in grass and other details onto the terrain. Adds in every detail object that is added into the details list and trys to place it onto the terrain.
    /// Each details can have their propeties changed to affect how it looks on the terrain and where it can be placed. 
    /// </summary>
    public void AddDetails() {
        DetailPrototype[] newDetailPrototypes;
        newDetailPrototypes = new DetailPrototype[details.Count];
        int dindex = 0;
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        foreach (Detail d in details) {
            newDetailPrototypes[dindex] = new DetailPrototype();
            newDetailPrototypes[dindex].prototype = d.prototype;
            newDetailPrototypes[dindex].prototypeTexture = d.prototypeTexture;
            newDetailPrototypes[dindex].healthyColor = d.healthyColor;
            newDetailPrototypes[dindex].dryColor = d.dryColor;
            newDetailPrototypes[dindex].minHeight = d.heightRange.x;
            newDetailPrototypes[dindex].maxHeight = d.heightRange.y;
            newDetailPrototypes[dindex].minWidth = d.widthRange.x;
            newDetailPrototypes[dindex].maxWidth = d.widthRange.y;
            newDetailPrototypes[dindex].noiseSpread = d.noiseSpread;


            if (newDetailPrototypes[dindex].prototype) {
                newDetailPrototypes[dindex].usePrototypeMesh = true;
                newDetailPrototypes[dindex].renderMode = DetailRenderMode.VertexLit;
            }
            else {
                newDetailPrototypes[dindex].usePrototypeMesh = false;
                newDetailPrototypes[dindex].renderMode = DetailRenderMode.GrassBillboard;
            }
            dindex++;
        }
        terrainData.detailPrototypes = newDetailPrototypes;

        for (int i = 0; i < terrainData.detailPrototypes.Length; i++) {
            int[,] detailMap = new int[terrainData.detailWidth, terrainData.detailHeight];

            for(int y = 0; y < terrainData.detailHeight; y += detailSpacing) {
                for(int x = 0; x < terrainData.detailWidth; x += detailSpacing) {
                    if (UnityEngine.Random.Range(0.0f, 1.0f) > details[i].density) continue;
                    int xHM = (int)(x / (float)terrainData.detailWidth * terrainData.heightmapWidth);
                    int yHM = (int)(y / (float)terrainData.detailHeight * terrainData.heightmapHeight);

                    float thisNoise = Utility.Map(Mathf.PerlinNoise(x * details[i].feather,
                                                                    y * details[i].feather),
                                                                    0, 1, 0.5f, 1);

                    float thisHeightStart = details[i].minHeight * thisNoise -
                                            details[i].overlap * thisNoise;

                    float nextHeightStart = details[i].maxHeight * thisNoise +
                                            details[i].overlap * thisNoise;

                    float thisHeight =  heightMap[yHM, xHM];
                    float steepness = terrainData.GetSteepness(xHM / (float)terrainData.size.x,
                                                               yHM / (float)terrainData.size.y);

                    if((thisHeight >= thisHeightStart && thisHeight <= nextHeightStart) &&
                        (steepness >= details[i].minSlope && steepness <= details[i].maxSlope)) {

                        detailMap[y, x] = 1;
                    }                 
                }
            }
            terrainData.SetDetailLayer(0, 0, i, detailMap);
        }

    }

    /// <summary>
    /// Adds new details properties to the details list.
    /// </summary>
    public void AddNewDetails() {

        details.Add(new Detail());
    }

    /// <summary>
    /// Removes details property from the details list if its remove bool is checked. 
    /// </summary>
    public void RemoveDetails() {
        List<Detail> keptDetails = new List<Detail>();
        for(int i = 0; i < details.Count; i++) {
            if (!details[i].remove) {
                keptDetails.Add(details[i]);
            }
        }
        if(keptDetails.Count == 0) {
            keptDetails.Add(details[0]); // add at least 1
        }
        details = keptDetails;

    }

}
