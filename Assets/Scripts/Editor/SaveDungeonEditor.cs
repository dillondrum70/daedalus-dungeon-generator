using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Palmmedia.ReportGenerator.Core;
using Unity.VisualScripting;

#if UNITY_EDITOR
using UnityEditor;

public class EditorProperties : ScriptableObject
{
    public string _savePath = "";
    public string _fileName = "";
    public GameObject _dungeonParent;
}

public class SaveDungeonEditor : EditorWindow
{
    EditorProperties obj;

    SerializedObject editorObject;
    SerializedProperty savePath;
    SerializedProperty fileName;
    SerializedProperty dungeonParent;

    //GameObject dungeonParent;

    DungeonGenerator generator;

    const string DUNGEON_GAMEOBJECT_NAME = "DungeonManager";
    const string DUNGEON_PARENT_NAME = "Dungeon";

    private void OnEnable()
    {
        AssemblyReloadEvents.afterAssemblyReload += Reload;

        Reload();
    }

    private void OnDisable()
    {
        AssemblyReloadEvents.afterAssemblyReload -= Reload;

        Reload();
    }

    void Reload()
    {
        obj = CreateInstance<EditorProperties>();
        editorObject = new SerializedObject(obj);

        savePath = editorObject.FindProperty("_savePath");
        fileName = editorObject.FindProperty("_fileName");
        dungeonParent = editorObject.FindProperty("_dungeonParent");

        dungeonParent.objectReferenceValue = GameObject.Find(DUNGEON_PARENT_NAME);

        //dungeonParent = GameObject.Find(DUNGEON_PARENT_NAME);
    }

    [MenuItem("Tools/Dungeon Editor")]
    public static void ShowDungeonEditor()
    {
        EditorWindow wnd = GetWindow<SaveDungeonEditor>();
        wnd.titleContent = new GUIContent("Dungeon Editor");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Run Generator"))
        {
            if (!generator)
            {
                //generator may have been reloaded after start or quit in editor
                generator = FindObjectOfType<DungeonGenerator>();
            }

            if (generator)
            {
                generator.Generate();
            }
            else
            {
                Debug.LogError("DungeonGenerator is null");
            }
        }

        if (GUILayout.Button("Clear Dungeon"))
        {
            if(!generator)
            {
                //generator may have been reloaded after start or quit in editor
                generator = FindObjectOfType<DungeonGenerator>();
            }

            if (generator)
            {
                generator.Clear();
            }
            else
            {
                Debug.LogError("DungeonGenerator is null");
            }
        }

        EditorGUILayout.LabelField("Save Path", savePath.stringValue, EditorStyles.wordWrappedLabel);
        
        if (GUILayout.Button("Choose Folder"))
        {
            string path = EditorUtility.OpenFolderPanel("Choose Save Path", "", "");
            savePath.stringValue = path + "/";
        }
        
        EditorGUILayout.PropertyField(fileName, new GUIContent("File Name"));
        
        EditorGUILayout.PropertyField(dungeonParent, new GUIContent("Dungeon Parent Object"));

        if (GUILayout.Button("Save to Prefab"))
        {
            SavePrefab();
        }
    }

    private void SavePrefab()
    {
        PrefabUtility.SaveAsPrefabAsset(dungeonParent.objectReferenceValue.GameObject(),
            savePath.stringValue + fileName.stringValue + ".prefab",
            out bool success);

        if(!success)
        {
            Debug.LogError("Save Failed");
        }
        else
        {
            Debug.Log("Save Complete");
        }
    }
}
#endif
