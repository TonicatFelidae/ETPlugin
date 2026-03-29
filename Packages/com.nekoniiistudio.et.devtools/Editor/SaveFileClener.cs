using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SaveFileClener : Editor
{
    [MenuItem("ETools/Delete Persistent Data")]
    public static void DeleteAllPersistentData()
    {
        string persistentPath = Application.persistentDataPath;
        if (Directory.Exists(persistentPath))
        {
            // Delete all files
            string[] files = Directory.GetFiles(persistentPath, "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (IOException ex)
                {
                    Debug.LogError($"Failed to delete file: {file}\n{ex}");
                }
            }

            // Delete all directories
            string[] directories = Directory.GetDirectories(persistentPath, "*", SearchOption.AllDirectories);
            foreach (string dir in directories)
            {
                try
                {
                    Directory.Delete(dir, true);
                }
                catch (IOException ex)
                {
                    Debug.LogError($"Failed to delete directory: {dir}\n{ex}");
                }
            }

            Debug.Log("All files and folders in persistent data path deleted.");
        }
        else
        {
            Debug.LogWarning("Persistent data path does not exist.");
        }
    }
}
