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
            string[] files = Directory.GetFiles(persistentPath);
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
            Debug.Log("All files in persistent data path deleted.");
        }
        else
        {
            Debug.LogWarning("Persistent data path does not exist.");
        }
    }
}
