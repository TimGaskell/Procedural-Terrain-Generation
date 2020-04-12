using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Creates a GUI window in the inspector, Located in the Window tab on the top bar
/// This GUI is used to generate a 2D texture based on 2D Perlin noise. The texture can be saved to the Editor
/// </summary>
public class TextureCreatorWindow : EditorWindow
{
    string fileName = "myProceduralTexture";
    float perlinXScale;
    float perlinYScale;
    int perlinOctaves;
    float perlinPersistance;
    float perlinHeightScale;
    int perlinOffsetX;
    int perlinOffsetY;
    bool alphaToggle = false;
    bool seamlessToggle = false;
    bool mapToggle = false;

    Texture2D pTexture;
  
    [MenuItem("Window/TextureCreatorWindow")]
    public static void ShowWindow() {
        EditorWindow.GetWindow(typeof(TextureCreatorWindow));
    }

    private void OnEnable() {
        pTexture = new Texture2D(513, 513, TextureFormat.ARGB32, false);
    }

    private void OnGUI() {

        GUILayout.Label("Settings", EditorStyles.boldLabel);
        fileName = EditorGUILayout.TextField("Texture Name", fileName);

        int wSize = (int)(EditorGUIUtility.currentViewWidth - 100);

        //All Sliders and toggles needed to set the noise being created
        perlinXScale = EditorGUILayout.Slider("X Scale", perlinXScale, 0, 0.1f);
        perlinYScale = EditorGUILayout.Slider("Y Scale", perlinYScale, 0, 0.1f);
        perlinOctaves = EditorGUILayout.IntSlider("Octaves", perlinOctaves, 1, 10);
        perlinPersistance = EditorGUILayout.Slider("Persistence", perlinPersistance, 1, 10);
        perlinHeightScale = EditorGUILayout.Slider("Height Scale", perlinHeightScale, 0, 1);
        perlinOffsetX = EditorGUILayout.IntSlider("Offset X", perlinOffsetX, 0, 10000);
        perlinOffsetY = EditorGUILayout.IntSlider("Offset Y", perlinOffsetY, 0, 10000);
        alphaToggle = EditorGUILayout.Toggle("Alpha?", alphaToggle);
        mapToggle = EditorGUILayout.Toggle("Map?", mapToggle);
        seamlessToggle = EditorGUILayout.Toggle("Seamless", seamlessToggle);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        // Button responsible for generating the noise texture
        if (GUILayout.Button("Generate", GUILayout.Width(wSize))) {

            int w = 513;
            int h = 513;
            float pValue;
            Color pixCol = Color.white;
            for(int y = 0 ; y < h ; y++) {
                for(int x = 0; x < w; x++) {

                    if (seamlessToggle) {

                        float u = (float)x / (float)w;
                        float v = (float)y / (float)h;

                        float noise00 = Utility.fBM((x + perlinOffsetX) * perlinXScale, (y + perlinOffsetY) * perlinYScale,
                                             perlinOctaves,
                                             perlinPersistance) * perlinHeightScale;

                        float noise01 = Utility.fBM((x + perlinOffsetX) * perlinXScale, (y + perlinOffsetY + h) * perlinYScale,
                                            perlinOctaves,
                                            perlinPersistance) * perlinHeightScale;

                        float noise10 = Utility.fBM((x + perlinOffsetX + w) * perlinXScale, (y + perlinOffsetY) * perlinYScale,
                                            perlinOctaves,
                                            perlinPersistance) * perlinHeightScale;

                        float noise11 = Utility.fBM((x + perlinOffsetX + w) * perlinXScale, (y + perlinOffsetY + h) * perlinYScale,
                                            perlinOctaves,
                                            perlinPersistance) * perlinHeightScale;

                        float noiseTotal = u * v * noise00 +
                                           u * (1 - v) * noise01 +
                                           (1 - u) * v * noise10 +
                                           (1 - u) * (1 - v) * noise11;

                        float value = (int)(256 * noiseTotal) + 50;
                        float r = Mathf.Clamp((int)noise00, 0, 255);
                        float g = Mathf.Clamp(value, 0, 255);
                        float b = Mathf.Clamp(value + 50, 0, 255);
                        float a = Mathf.Clamp(value + 100, 0, 255);

                        pValue = (r + g + b) / (3 * 255.0f);

                    }
                    else {
                        pValue = Utility.fBM((x + perlinOffsetX) * perlinXScale, (y + perlinOffsetY) * perlinYScale,
                                             perlinOctaves,
                                             perlinPersistance) * perlinHeightScale;              
                    }
                    float colValue = pValue;
                    pixCol = new Color(colValue, colValue, colValue, alphaToggle ? colValue : 1);
                    pTexture.SetPixel(x, y, pixCol);
                }

            }
            pTexture.Apply(false, false);
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(pTexture, GUILayout.Width(wSize), GUILayout.Height(wSize));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        //Button responsible for saving the current texture to a png on the editor
        if (GUILayout.Button("Save", GUILayout.Width(wSize))) {

            byte[] bytes = pTexture.EncodeToPNG();
            System.IO.Directory.CreateDirectory(Application.dataPath + "/SavedTextures");
            File.WriteAllBytes(Application.dataPath + "/SavedTextures/" + fileName + ".png", bytes);

        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }


}
