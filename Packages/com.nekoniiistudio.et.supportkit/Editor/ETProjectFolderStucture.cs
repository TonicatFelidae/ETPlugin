using System.IO;
using UnityEditor;
using UnityEngine;

namespace  ET
{
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
            if (GUILayout.Button("Construct Folder Structure"))
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
            // Scripts folder
            CreateFolder(Path.Combine(assetsPath, "Game"), "___Script___");
            CreateAssemblyDefinition(Path.Combine(assetsPath, "Game", "___Script___"), "Game");
            //
            CreateFolder(Path.Combine(assetsPath, "Game"), "___Sound___");
            CreateFolder(Path.Combine(assetsPath, "Game"), "___Texture___");
            CreateFolder(Path.Combine(assetsPath, "Game"), "___Tile___");
            CreateFolder(assetsPath, "StreamingAssets");

            CreateFolder(assetsPath, "Resources");
            string resourcesPath = Path.Combine(assetsPath, "Resources");
            CreateFolder(resourcesPath, "Prefabs");
            string prefabsPath = Path.Combine(resourcesPath, "Prefabs");
            CreateFolder(prefabsPath, "UI");
            string uiPath = Path.Combine(prefabsPath, "UI");
            CreateFolder(uiPath, "Popup");

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
        private void CreateAssemblyDefinition(string folderPath, string assemblyName)
        {
            if (!Directory.Exists(folderPath))
            {
                Debug.LogWarning($"Script folder does not exist, cannot create asmdef: {folderPath}");
                return;
            }

            string asmdefPath = Path.Combine(folderPath, assemblyName + ".asmdef");
            if (File.Exists(asmdefPath))
            {
                Debug.LogWarning($"Assembly definition already exists: {asmdefPath}");
                return;
            }

            // Minimal asmdef content. Unity will import and use this to create an assembly for the folder.
            string asmdefJson = @"{
  ""name"": """ + assemblyName + @""",
  ""rootNamespace"": """",
  ""references"": [],
  ""optionalUnityReferences"": [],
  ""includePlatforms"": [],
  ""excludePlatforms"": [],
  ""allowUnsafeCode"": false,
  ""overrideReferences"": false,
  ""precompiledReferences"": [],
  ""autoReferenced"": true,
  ""defineConstraints"": [],
  ""versionDefines"": [],
  ""noEngineReferences"": false
}";

            File.WriteAllText(asmdefPath, asmdefJson);
            Debug.Log($"Created asmdef: {asmdefPath}");
        }
    }
    
}
