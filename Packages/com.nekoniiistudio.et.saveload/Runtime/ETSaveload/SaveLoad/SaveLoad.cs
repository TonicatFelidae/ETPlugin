using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using ET.FileSystem;
using System.Security.Cryptography;
using System.Text;
using ET.SupportKit.Collection;
using Newtonsoft.Json;
using System.Collections.Generic;
//using Unity.Plastic.Newtonsoft.Json;

//ver 2023.01.0001
//require TMpro to run

namespace ET.Saveload
{
    public static class CONST_ETSaveload
    {
        public static string SAVE_NAME_FILE = "NiiiSlot";
    }

    //Type 4 feature module ET save load 
    public class ETSaveload
    {
        public SAVELOAD_Manager saveload_manager;
        public void Init(ApplicationSetting applicationSetting)
        {
            _applicationSetting = applicationSetting;
            if (saveload_manager == null) InitSaveLoadManagerFile();
        }
        private ApplicationSetting _applicationSetting;
        #region SaveloadManager
        private void InitSaveLoadManagerFile()
        {
            if (File.Exists(Application.persistentDataPath + ETFile.GetSaveFilePath(SaveFileType.SaveLoadManager, 0))) 
            {
                LoadFile(ref saveload_manager, SaveFileType.SaveLoadManager);
            }
            else
            {
                saveload_manager = new SAVELOAD_Manager(); 
                SaveFile(saveload_manager, SaveFileType.SaveLoadManager);
            }
        }
        /// <summary>
        /// Check and remove deleted file
        /// </summary>
        public void UpdateSaveLoadManagerFile()
        {
            bool isChanged = false;
            for (int i = 0; i < saveload_manager.items.Count; i++)
            {
                var item = saveload_manager.items[i];
                string path = Application.persistentDataPath + ETFile.GetSaveFilePath(SaveFileType.SaveFile, item.SaveFileName);
                if (!File.Exists(path))
                {
                    saveload_manager.items.Remove(item);
                    isChanged = true;   
                }
            }
            if (isChanged) SaveFile(saveload_manager, SaveFileType.SaveLoadManager);
        }
        #endregion

        #region Saveload Quick Access
        public void PrepareSaveFile<T>(T gamedat, out SAVE_MetaData newCopyMetaData) where T : SAVE_File
        {
            gamedat.ApplyVersionControl(_applicationSetting.dataVersionControl.currentDataVersion);
            gamedat.metaData.SetSaveTime(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            newCopyMetaData = gamedat.metaData.DeepCopy();
        }
        public bool SaveFile_WithMetaData<T>(T gamedat, SAVE_MetaData metaData, bool isQuickSave)
        {
            if (isQuickSave)
            {
                metaData.SetFileMarker(SaveFileMarker.QuickSave);
            }
            else
            {
                metaData.ResetFileMarker();
            }
            try
            {
                ETSaveload.SaveFileCryptography(gamedat, SaveFileType.SaveFile, metaData.SaveFileName);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Save data error with code: " + e);
                Debug.LogError("Maybe meta data is missing.");

                return false;
            }
        }
        public void AfterSaveFileSuccessed(SAVE_MetaData metaData)
        {
            saveload_manager.AddOrUpdate(metaData);
            SaveFile(saveload_manager, SaveFileType.SaveLoadManager);
        }
        public (bool,T) LoadFile_WithMetaData<T>(SAVE_MetaData metaData)
        {
            T gamedata = default;
            try
            {
                LoadFileCryptography(ref gamedata, SaveFileType.SaveFile, metaData.SaveFileName);
                return (true,gamedata);
            }
            catch (Exception)
            {
                return (false, gamedata);
            }
        }
        #endregion
        #region Saveload Main
        public void LoadFile<T>(ref T gamedata, SaveFileType saveFileType = SaveFileType.GameFile, byte slot = 0) where T : SAVE_File
        {
            if (File.Exists(Application.persistentDataPath + ETFile.GetSaveFilePath(saveFileType, slot)))
            {
                try
                {
                    switch (saveFileType)
                    {
                        case ET.FileSystem.SaveFileType.GameFile:
                            ETSaveload.LoadFileCryptography(ref gamedata, saveFileType, slot);
                            break;
                        case ET.FileSystem.SaveFileType.QuickGameFile:
                            ETSaveload.LoadFileCryptography(ref gamedata, saveFileType, slot);
                            break;
                        case ET.FileSystem.SaveFileType.AutoGameFile:
                            ETSaveload.LoadFileCryptography(ref gamedata, saveFileType, slot);
                            break;
                        case ET.FileSystem.SaveFileType.UserFile:
                            ETSaveload.LoadFileCryptography(ref gamedata, saveFileType, slot);
                            break;
                        case ET.FileSystem.SaveFileType.SystemFile:
                            ETSaveload.LoadFileJson(ref gamedata, saveFileType, slot);
                            break;
                        case ET.FileSystem.SaveFileType.SaveLoadManager:
                            ETSaveload.LoadFileJson(ref gamedata, saveFileType, slot);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Data outdate with code: " + e);
                    gamedata.ResetAll();
                    SaveFile(gamedata, saveFileType, slot);
                    return; // need this
                }
            }
            else
            {
                Debug.LogError("File not exist, reset data, create new");
                gamedata.ResetAll();
                SaveFile(gamedata, saveFileType, slot);
                return;
            }

            D.Sys.File("Loaded " + saveFileType.ToString());
            //set ourdate version controls
            DataVersionControlDataVersionRequire dataVersionRequire = _applicationSetting.dataVersionControl.dataVersionRequire.Find(x => x.saveFileType == saveFileType);
            if (gamedata.fileVersionControl.currentDataVersion < dataVersionRequire.requireDataVersion)
            {
                switch (ETFile.GetOutDateDataControlAction(saveFileType))
                {
                    case OutDateDataControlAction.Ignore:
                        break;
                    case OutDateDataControlAction.Warning:
                        break;
                    case OutDateDataControlAction.ResetToDefault:
                        gamedata.ResetAll();
                        SaveFile(gamedata, saveFileType, slot);
                        break;
                    default:
                        break;
                }
            }

        }
        public bool TryLoadFile<T>(ref T gamedata, SaveFileType saveFileType = SaveFileType.GameFile, byte slot = 0) where T : SAVE_File
        {
            try
            {
                switch (saveFileType)
                {
                    case ET.FileSystem.SaveFileType.GameFile:
                        ETSaveload.LoadFileCryptography(ref gamedata, saveFileType, slot);
                        break;
                    case ET.FileSystem.SaveFileType.QuickGameFile:
                        ETSaveload.LoadFileCryptography(ref gamedata, saveFileType, slot);
                        break;
                    case ET.FileSystem.SaveFileType.AutoGameFile:
                        ETSaveload.LoadFileCryptography(ref gamedata, saveFileType, slot);
                        break;
                    case ET.FileSystem.SaveFileType.UserFile:
                        ETSaveload.LoadFileCryptography(ref gamedata, saveFileType, slot);
                        break;
                    case ET.FileSystem.SaveFileType.SystemFile:
                        ETSaveload.LoadFileJson(ref gamedata, saveFileType, slot);
                        break;
                    case ET.FileSystem.SaveFileType.SaveLoadManager:
                        ETSaveload.LoadFileJson(ref gamedata, saveFileType, slot);
                        break;
                    case ET.FileSystem.SaveFileType.SaveFile:
                        ETSaveload.LoadFileCryptography(ref gamedata, saveFileType, slot);
                        break;
                    default:
                        break;
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Data outdate with code: " + e);
                gamedata.ResetAll();
                SaveFile(gamedata, saveFileType, slot);
                return false;
            }
        }
        public void SaveFile<T>(T gamedata, SaveFileType saveFileType = SaveFileType.GameFile, byte slot = 0) where T : SAVE_File
        {
            gamedata.ApplyVersionControl(_applicationSetting.dataVersionControl.currentDataVersion);
            try
            {
                switch (saveFileType)
                {
                    case ET.FileSystem.SaveFileType.GameFile:
                        ETSaveload.SaveFileCryptography(gamedata, saveFileType, slot);
                        break;
                    case ET.FileSystem.SaveFileType.QuickGameFile:
                        ETSaveload.SaveFileCryptography(gamedata, saveFileType, slot);
                        break;
                    case ET.FileSystem.SaveFileType.AutoGameFile:
                        ETSaveload.SaveFileCryptography(gamedata, saveFileType, slot);
                        break;
                    case ET.FileSystem.SaveFileType.UserFile:
                        ETSaveload.SaveFileCryptography(gamedata, saveFileType, slot);
                        break;
                    case ET.FileSystem.SaveFileType.SystemFile:
                        ETSaveload.SaveFileJson(gamedata, saveFileType, slot);
                        break;
                    case ET.FileSystem.SaveFileType.GameDataExportJson:
                        ETSaveload.SaveFileJson(gamedata, saveFileType, slot);
                        break;
                    case ET.FileSystem.SaveFileType.SaveLoadManager:
                        ETSaveload.SaveFileJson(gamedata, saveFileType, slot);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Save data error with code: " + e);
                return;
            }
        }
        #endregion
        //crypto
        static byte[] GetCryptoKey()
        {
            byte[] bytes = Encoding.ASCII.GetBytes("Enchanter TonicatFelidae");
            foreach (byte b in bytes)
            {
                Console.WriteLine(b);
            }
            return bytes;
        }
        //test
        public static void SaveFileCryptography<T>(T gamedata, SaveFileType saveFileType, byte slot)=> SaveFileCryptography<T>(gamedata, saveFileType, slot ,null);
        public static void SaveFileCryptography<T>(T gamedata, SaveFileType saveFileType, string saveFileName) => SaveFileCryptography<T>(gamedata, saveFileType, 0, saveFileName);
        private static void SaveFileCryptography<T>(T gamedata, SaveFileType saveFileType, byte slot, string saveFileName)
        {
            string path;
            if (saveFileName != null)
            {
                //save use time stam
                path = Application.persistentDataPath + ETFile.GetSaveFilePath(saveFileType, saveFileName);
            }
            else
            {
                path = Application.persistentDataPath + ETFile.GetSaveFilePath(saveFileType, slot);
            }
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            FileStream file = File.Create(path);
            Aes aesIntance = Aes.Create();
            byte[] inputIV = aesIntance.IV;
            file.Write(inputIV, 0, inputIV.Length);
            CryptoStream streamIntance = new CryptoStream(
                    file,
                    aesIntance.CreateEncryptor(GetCryptoKey(), aesIntance.IV),
                    CryptoStreamMode.Write);
            StreamWriter writerIntance = new StreamWriter(streamIntance);
            string jsonString = JsonUtility.ToJson(gamedata);
            writerIntance.Write(jsonString);
            writerIntance.Close();
            streamIntance.Close();
            file.Close();
            D.Sys.Confirm($"File saved at {path}");
        }
        public static void LoadFileCryptography<T>(ref T gamedata, SaveFileType saveFileType, byte slot) => LoadFileCryptography<T>(ref gamedata, saveFileType, slot, false);
        public static void LoadFileCryptography<T>(ref T gamedata, SaveFileType saveFileType, string saveFileName) => LoadFileCryptography<T>(ref gamedata, saveFileType, 0, true, saveFileName);
        public static void LoadFileCryptography<T>(ref T gamedata, SaveFileType saveFileType, byte slot, bool useMetadataAutoName, string saveFileName = null)
        {
            string path;
            if (useMetadataAutoName)
            {
                //save use time stam
                path = Application.persistentDataPath + ETFile.GetSaveFilePath(saveFileType, saveFileName);
            }
            else
            {
                path = Application.persistentDataPath + ETFile.GetSaveFilePath(saveFileType, slot);
            }
            FileStream file = File.Open(path, FileMode.Open);
            Aes aesIntance = Aes.Create();
            byte[] outputIV = new byte[aesIntance.IV.Length];
            file.Read(outputIV, 0, outputIV.Length);
            CryptoStream streamIntance = new CryptoStream(
                   file,
                   aesIntance.CreateDecryptor(GetCryptoKey(), outputIV),
                   CryptoStreamMode.Read);
            StreamReader readerIntance = new StreamReader(streamIntance);
            string text = readerIntance.ReadToEnd();
            readerIntance.Close();
            string jsonString = JsonUtility.ToJson(gamedata);
            gamedata = JsonUtility.FromJson<T>(text);
            streamIntance.Close();
            file.Close();
            D.Sys.Confirm($"File Loaded from {path}");
        }
        public static void SaveFileTestBinary<T>(T gamedata, SaveFileType saveFileType, byte slot = 0)
        {
            string path = Application.persistentDataPath + ETFile.GetSaveFilePath(saveFileType, slot);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Application.persistentDataPath + ETFile.GetSaveFilePath(saveFileType, slot));
            Debug.Log(Application.persistentDataPath + "/Playerdata");
            Debug.Log(Application.persistentDataPath + ETFile.GetSaveFilePath(saveFileType, slot));
            bf.Serialize(file, gamedata);
            file.Close();
            D.Sys.Confirm($"File saved at {path}");
        }
        public static void LoadFileTestBinary<T>(ref T gamedata, SaveFileType saveFileType, byte slot = 0) where T : SAVE_File
        {
            SAVE_File fileget = null;
            if (File.Exists(Application.persistentDataPath + ETFile.GetSaveFilePath(saveFileType, slot)))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + ETFile.GetSaveFilePath(saveFileType, slot), FileMode.Open);
                fileget = (SAVE_File)bf.Deserialize(file);

                file.Close();

            }
            if (fileget != null) gamedata = (T)Convert.ChangeType(fileget, typeof(T));
            else gamedata = null;
        }
        public static void SaveFileJson<T>(T gamedata, SaveFileType saveFileType, byte slot = 0)
        {
            string path = Application.persistentDataPath + ETFile.GetSaveFilePath(saveFileType, slot);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            FileStream file = File.Create(Application.persistentDataPath + ETFile.GetSaveFilePath(saveFileType, slot));
            StreamWriter writerIntance = new StreamWriter(file);
            writerIntance.Write(JsonConvert.SerializeObject(gamedata));
            writerIntance.Close();
            file.Close();
            D.Sys.Confirm($"File saved at {path}");
        }
        public static void LoadFileJson<T>(ref T gamedata, SaveFileType saveFileType, byte slot = 0) where T : class
        {
            if (File.Exists(Application.persistentDataPath + ETFile.GetSaveFilePath(saveFileType, slot)))
            {
                FileStream file = File.Open(Application.persistentDataPath + ETFile.GetSaveFilePath(saveFileType, slot), FileMode.Open);
                StreamReader readerIntance = new StreamReader(file);
                string text = readerIntance.ReadToEnd();
                readerIntance.Close();
                gamedata = JsonConvert.DeserializeObject<T>(text);
                file.Close();
            }
            else
            {
                gamedata = null;
            }
        }
        /// <summary>
        /// QuickSaveFileJson for fast debug
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gamedata"></param>
        public static void QuickSaveFileJson<T>(T gamedata)
        {
            string path = Application.persistentDataPath + Path.DirectorySeparatorChar + "";
            FileStream file = File.Create(path);
            StreamWriter writerIntance = new StreamWriter(file);
            writerIntance.Write(JsonConvert.SerializeObject(gamedata));
            writerIntance.Close();
            file.Close();
            Debug.Log("Saved file at: " + path);
        }
        //base
        /// <summary>
        /// Delete save file from slot.
        /// </summary>
        /// <param name="slot"></param>
        public static void DeleteFile(SaveFileType saveFileType, byte slot = 0)
        {
            if (File.Exists(Application.persistentDataPath + ETFile.GetSaveFilePath(saveFileType, slot)))
            {
                File.Delete(Application.persistentDataPath + ETFile.GetSaveFilePath(saveFileType, slot));
            }
        }
        public static bool DeleteFile(string saveFileName, SaveFileType saveFileType = SaveFileType.SaveFile)
        {
            string path = Application.persistentDataPath + ETFile.GetSaveFilePath(saveFileType, saveFileName);
            if (File.Exists(path))
            {
                File.Delete(path);
                D.Sys.Confirm($"File deleted at {path}");
                return true;
            }
            return false;
        }
    }
    public class ETDatamanager : MonoBehaviour
    {
    }
    [Serializable]
    public class SAVE_SerVector3
    {
        public float x;
        public float y;
        public float z;

    }
    [Serializable]
    public class ApplicationSetting
    {
        public DataVersionControl dataVersionControl;
    }
    [Serializable]
    public struct DataVersionControl //control version of data// out date controls
    {
        public int currentDataVersion;
        public DataVersionControlDataVersionRequire[] dataVersionRequire;
    }
    [Serializable]
    public struct DataVersionControlDataVersionRequire //control version of data// out date controls
    {
        public SaveFileType saveFileType;
        public int requireDataVersion;
    }
    [Serializable]
    public class SAVELOAD_Manager: SAVE_File
    {
        public override SAVE_MetaData metaData { get; set; } = new();
        public List<SAVE_MetaData> items = new();
        public int Count => items.Count;


        public void AddOrUpdate(SAVE_MetaData newData)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].SaveFileName == newData.SaveFileName)
                {
                    items[i] = newData;
                    return;
                }
            }
            items.Add(newData);
        }
    }
}
