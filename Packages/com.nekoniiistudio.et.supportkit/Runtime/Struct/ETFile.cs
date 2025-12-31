using System;
using System.IO;
using System.Text;
using UnityEngine;
using ET.SupportKit.Collection;

namespace ET.FileSystem
{
    /*
     * Note for fileUntil:
     * - Please use Path.DirectorySeparatorChar so your code uses the preferred separator on that system
     * when develop applications that run on multiple platforms:
     * 
     */
    public static class ETFile
    {

        private static SaveFileSetting[] _saveFileSettings =
        {
            new SaveFileSetting(
                SaveFileType.GameFile,
                 $"{Path.DirectorySeparatorChar}Memory",
                 $"CuteSlotNumber.dat",
                 SaveFileAutoNameType.Ascending
                ),
            new SaveFileSetting(
                SaveFileType.AutoGameFile,
                 $"{Path.DirectorySeparatorChar}Memory",
                 $"QuickCuteSlot.dat",
                 SaveFileAutoNameType.None
                ),
            new SaveFileSetting(
                SaveFileType.QuickGameFile,
                 $"{Path.DirectorySeparatorChar}Memory",
                 $"AutoCuteSlotNumber.dat",
                 SaveFileAutoNameType.None
                ),
            new SaveFileSetting(
                SaveFileType.UserFile,
                 $"",
                 $"0.dat",
                 SaveFileAutoNameType.None
                ),
            new SaveFileSetting(
                SaveFileType.SystemFile,
                 $"",
                 $"SystemSetting.json",
                 SaveFileAutoNameType.None
                ),
            new SaveFileSetting(
                SaveFileType.GameDataExportJson,
                 $"",
                 $"GameDataExport.json",
                 SaveFileAutoNameType.Ascending
                ),
            new SaveFileSetting(
                SaveFileType.SaveLoadManager,
                 $"",
                 $"SaveLoadManager.json",
                 SaveFileAutoNameType.None
                ),
            new SaveFileSetting(
                SaveFileType.SaveFile,
                 $"{Path.DirectorySeparatorChar}Memory",
                 $"SaveFile.dat",
                 SaveFileAutoNameType.None
                )
        };
        static OutDateDataControlSetting[] OutDateDataControlSettings =
        {
            new OutDateDataControlSetting(
                SaveFileType.GameFile,
                OutDateDataControlAction.Warning
                ),
            new OutDateDataControlSetting(
                SaveFileType.AutoGameFile,
                OutDateDataControlAction.Warning
                ),
            new OutDateDataControlSetting(
                SaveFileType.QuickGameFile,
                OutDateDataControlAction.Warning
                ),
            new OutDateDataControlSetting(
                SaveFileType.SystemFile,
                OutDateDataControlAction.ResetToDefault
                ),
            new OutDateDataControlSetting(
                SaveFileType.UserFile,
                OutDateDataControlAction.ResetToDefault
                ),
            new OutDateDataControlSetting(
                SaveFileType.GameDataExportJson,
                OutDateDataControlAction.Ignore
                )

        };

        public static string CurrentReadingFileName = "";
        public static string CurrentReadingPath = "";
        private static bool _isUnityEditor
        {
            get
            {
#if UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }
        //savefile
        /// <summary>
        /// Mono save path system, all game save file in one place and separate with system save file
        /// NEW name type
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="saveFileType"></param>
        /// <returns></returns>
        public static string GetSaveFilePath(SaveFileType saveFileType, string fileName)
        {
            SaveFileSetting saveFileSetting = _saveFileSettings.Find(x => x.saveFileType == saveFileType);
            string filePath = saveFileSetting.filePath;
            string fileNameExtension = $"{saveFileSetting.fileAlias}_{fileName}.{saveFileSetting.fileExtension}";
            return GetSaveFilePath(filePath, fileNameExtension);
        }
        /// <summary>
        /// Mono save path system, all game save file in one place and separate with system save file
        /// OLD slot type
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="saveFileType"></param>
        /// <returns></returns>
        public static string GetSaveFilePath(SaveFileType saveFileType, byte slot = 0)
        {
            SaveFileSetting saveFileSetting = _saveFileSettings.Find(x => x.saveFileType == saveFileType);
            string filePath = saveFileSetting.filePath;
            string fileNameExtension = saveFileSetting.fileNameExtension;
            switch (saveFileSetting.saveFileAutoNameType)
            {
                case SaveFileAutoNameType.None:
                    break;
                case SaveFileAutoNameType.Ascending:
                    fileNameExtension = saveFileSetting.fileAlias + slot.ToString() + "." + saveFileSetting.fileExtension;
                    break;
                default:
                    break;
            }
            return GetSaveFilePath(filePath, fileNameExtension);
        }
        public static string GetSaveFilePath(string filePath, string fileNameExtension)
        {
            if (!Directory.Exists(Application.persistentDataPath + filePath))
            {
                Directory.CreateDirectory(Application.persistentDataPath + filePath);
            }
            return $"{filePath}{Path.DirectorySeparatorChar}{fileNameExtension}";
        }
        //Persistent Data
        /// <summary>
        /// DataPathLocation + Folder path
        /// </summary>
        /// <param name="relativeFilePath"></param>
        /// <param name="pathType"></param>
        /// <returns></returns>
        public static string GetWritablePath(string relativeFilePath, DataPathLocation pathType)
        {
            return pathType switch
            {
                DataPathLocation.PersistentData => GetPersistentDataPath() + Path.DirectorySeparatorChar + relativeFilePath,
                DataPathLocation.StreamingAssetsData => GetStreamingAssetsDataPath() + Path.DirectorySeparatorChar + relativeFilePath,
                DataPathLocation.Resources => relativeFilePath, // REsorece is not writeable
                _ => GetStreamingAssetsDataPath() + Path.DirectorySeparatorChar + relativeFilePath,
            };
        }
        private static string GetPersistentDataPath()
        {
            if (_isUnityEditor)
                return Application.dataPath.Replace("Assets", "ExternalData");

            return Application.persistentDataPath;
        }
        //Streaming Data
        private static string GetStreamingAssetsDataPath()
        {
            return Application.dataPath + Path.DirectorySeparatorChar + "StreamingAssets";
        }
        public static byte[] LoadFile(string absolutePath)
        {
            if (absolutePath == null || absolutePath.Length == 0)
            {
                return null;
            }
            if (File.Exists(absolutePath))
            {
                return File.ReadAllBytes(absolutePath);
            }
            else
            {
                return null;
            }
        }

        public static bool CheckFileExist(string filePath)
        {
            bool ret = File.Exists(filePath);
            if (!ret) D.Sys.FindError(CurrentReadingFileName, CurrentReadingPath);
            return ret;
        }

        public static bool CreateNewFile(string content, string path)
        {
            try
            {
                using (FileStream fs = File.Create(path))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(content);
                    fs.Write(info, 0, info.Length);
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
                return false;
            }
        }
        public static OutDateDataControlAction GetOutDateDataControlAction(SaveFileType saveFileType)
        {
            return OutDateDataControlSettings.Find(x => x.saveFileType == saveFileType).outDateDataControlAction;
        }
        public static string CleanFilePath(string fullFileName)
        {
            int dotIndex = fullFileName.IndexOf('.');
            return dotIndex >= 0 ? fullFileName.Substring(0, dotIndex) : fullFileName;
        }
    }
    
}