using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveLoad 
{
    void Init(string path);
    void Save<T>(T gameData) where T: class;    
    void Load<T>(ref T gameData) where T : class;
    void CleanData();
}

public interface ISaveLoadManager
{
    void Save<T>(T gameData) where T : class;
    void Load<T>(ref T gameData) where T : class;
    void CleanData();
}
