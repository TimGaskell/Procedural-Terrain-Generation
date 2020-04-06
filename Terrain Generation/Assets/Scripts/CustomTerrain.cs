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

    public void RandomTerrain() {

    }
}
