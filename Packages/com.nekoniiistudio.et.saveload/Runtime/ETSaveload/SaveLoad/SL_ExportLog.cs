using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ETSBuglogExporter
{
    public List<string> logData;
    public void ExportFile()
    {
        string path = Application.persistentDataPath + Path.AltDirectorySeparatorChar + "DebugLog";
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        FileStream file = File.Create(path);
        StreamWriter writerIntance = new StreamWriter(file);
        // This method automatically opens the file, writes to it, and closes file
        for (int i = 0; i < logData.Count; i++)
        {
            writerIntance.WriteLine(logData[i]);
        }
        Debug.Log($"[DEBUG LOG] Saved at {path}");
        writerIntance.Close();
        file.Close();
    }
    public void Log(string tx)
    {
        logData.Add(tx);
    }
}
