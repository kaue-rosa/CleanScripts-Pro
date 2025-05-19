using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class CleanScripts : EditorWindow
{
    private List<GameObject> objectsWithMissingScripts = new List<GameObject>();
    private bool foundMissingScripts = false;

    private bool scanScene = true;
    private bool scanPrefabs = false;
    private string prefabFolderPath = "Assets/";

    [MenuItem("Tools/AtlaStudio Tools/CleanScripts Pro")]
    public static void ShowWindow()
    {
        GetWindow<CleanScripts>("AtlaStudio");
    }

    void OnGUI()
    {
        GUILayout.Label("CleanScripts Pro", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // Scan Options
        GUILayout.Label("Scan Options", EditorStyles.miniBoldLabel);
        scanScene = EditorGUILayout.Toggle("Scan Active Scene", scanScene);
        scanPrefabs = EditorGUILayout.Toggle("Scan Prefabs in Folder", scanPrefabs);

        if (scanPrefabs)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Prefab Folder Path:");
            prefabFolderPath = GUILayout.TextField(prefabFolderPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Folder", "Assets/", "");
                if (!string.IsNullOrEmpty(path))
                {
                    // Convert full path to relative
                    if (path.StartsWith(Application.dataPath))
                        prefabFolderPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Find Missing Scripts"))
        {
            FindMissingScripts();
        }

        GUILayout.Space(10);
        if (foundMissingScripts && objectsWithMissingScripts.Count > 0)
        {
            GUILayout.Label($"Found {objectsWithMissingScripts.Count} object(s) with missing scripts:");
            foreach (var obj in objectsWithMissingScripts)
            {
                EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Remove All Missing Scripts"))
            {
                RemoveMissingScripts();
            }
        }
    }

    void FindMissingScripts()
    {
        objectsWithMissingScripts.Clear();

        if (scanScene)
        {
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(obj) > 0)
                {
                    objectsWithMissingScripts.Add(obj);
                }
            }
        }

        if (scanPrefabs)
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabFolderPath });

            foreach (string guid in prefabGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                if (prefab != null && GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab) > 0)
                {
                    objectsWithMissingScripts.Add(prefab);
                }
            }
        }

        foundMissingScripts = objectsWithMissingScripts.Count > 0;

        if (!foundMissingScripts)
            EditorUtility.DisplayDialog("Scan Complete", "No missing scripts found!", "OK");
    }

    void RemoveMissingScripts()
    {
        foreach (GameObject obj in objectsWithMissingScripts)
        {
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
            EditorUtility.SetDirty(obj);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        objectsWithMissingScripts.Clear();
        foundMissingScripts = false;
        EditorUtility.DisplayDialog("Cleanup Complete", "All missing scripts have been removed.", "OK");
    }
}
