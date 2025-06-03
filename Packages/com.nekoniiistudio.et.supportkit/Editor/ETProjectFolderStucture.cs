using System.IO;
using UnityEditor;
using UnityEngine;

public class ETProjectFolderStucture : EditorWindow
{
    [MenuItem("ETools/Project structure")]
    public static void ShowWindow()
    {
        GetWindow<ETProjectFolderStucture>("Project structure");
    }

    private void OnGUI()
    {
        GUILayout.Label("Project structure", EditorStyles.boldLabel);
        if (GUILayout.Button("Construct structure"))
        {
            CreateFolders();
        }
    }

    private void CreateFolders()
    {
        string assetsPath = Application.dataPath;
        CreateFolder(assetsPath, "Game");
        CreateFolder(Path.Combine(assetsPath, "Game"), "___Animation___");
        CreateFolder(Path.Combine(assetsPath, "Game"), "___Art___");
        CreateFolder(Path.Combine(assetsPath, "Game"), "___Data___");
        CreateFolder(Path.Combine(assetsPath, "Game"), "___Font___");
        CreateFolder(Path.Combine(assetsPath, "Game"), "___Material___");
        CreateFolder(Path.Combine(assetsPath, "Game"), "___Prefab___");
        CreateFolder(Path.Combine(assetsPath, "Game"), "___Render___");
        CreateFolder(Path.Combine(assetsPath, "Game"), "___Script___");
        CreateFolder(Path.Combine(assetsPath, "Game"), "___Sound___");
        CreateFolder(Path.Combine(assetsPath, "Game"), "___Texture___");
        CreateFolder(Path.Combine(assetsPath, "Game"), "___Tile___");
        CreateFolder(assetsPath, "StreamingAssets");
        AssetDatabase.Refresh();
        Debug.Log("Folders created successfully!");
    }

    private void CreateFolder(string parentPath, string folderName)
    {
        string fullPath = Path.Combine(parentPath, folderName);
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
            Debug.Log($"Created folder: {fullPath}");
        }
        else
        {
            Debug.LogWarning($"Folder already exists: {fullPath}");
        }
    }
}