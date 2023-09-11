/*
        Copyright (c) 2022 - 2023 Dillon Drummond

        Permission is hereby granted, free of charge, to any person obtaining
        a copy of this software and associated documentation files (the
        "Software"), to deal in the Software without restriction, including
        without limitation the rights to use, copy, modify, merge, publish,
        distribute, sublicense, and/or sell copies of the Software, and to
        permit persons to whom the Software is furnished to do so, subject to
        the following conditions:

        The above copyright notice and this permission notice shall be
        included in all copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
        EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
        MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
        NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
        LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
        OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
        WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

/*
        Daedalus Dungeon Generator: 3D Dungeon Generator Tool
	    By Dillon W. Drummond

	    SaveDungeonEditor.cs

	    ********************************************
	    ***      Editor Tool Implementation      ***
	    ********************************************
 */


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
        PrefabUtility.SaveAsPrefabAsset((GameObject)dungeonParent.objectReferenceValue,
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
