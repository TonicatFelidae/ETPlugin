using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace ET.FileSystem
{
    ///////////////////SAVE SYSTEM/////////////////////////
    public enum SaveFileType
    {
        GameFile, // secure // this will acceding auto add 0,1,2,3,4
        QuickGameFile, // secure
        AutoGameFile, // secure
        UserFile, // secure
        SystemFile, //editable
        GameDataExportJson,
        SaveLoadManager, //quick load data
        SaveFile, //savefile use name not auto add 0,1,2,3,4
    }
    public enum DataPathLocation
    {
        PersistentData,
        StreamingAssetsData,
        Resources
    }
    public enum ReaderProtocol
    {
        ReadSettingFile, // value:filename >> to dictionary : ||niii setting file
        ReadCsvFile,
        ReadTsvFile,
    }
    public enum SaveFileAutoNameType
    {
        None,
        Ascending,
    }
    public enum OutDateDataControlAction
    {
        Ignore,
        Warning,
        ResetToDefault
    }
    public struct SaveFileSetting
    {
        public SaveFileType saveFileType;
        public string filePath;
        public string fileAlias;
        public SaveFileAutoNameType saveFileAutoNameType;
        public string fileExtension;
        public string fileNameExtension;
        public SaveFileSetting(SaveFileType saveFileType, string filePath, string fileNameExtension, SaveFileAutoNameType saveFileAutoNameType)
        {
            this.saveFileType = saveFileType;
            this.filePath = filePath;
            this.saveFileAutoNameType = saveFileAutoNameType;
            this.fileNameExtension = fileNameExtension; 
            var elements = fileNameExtension.Split(".");
            fileAlias = elements[0];
            fileExtension = elements[1];
        }
    }
    public struct OutDateDataControlSetting
    {
        public SaveFileType saveFileType;
        public OutDateDataControlAction outDateDataControlAction;

        public OutDateDataControlSetting(SaveFileType saveFileType, OutDateDataControlAction outDateDataControlAction)
        {
            this.saveFileType = saveFileType;
            this.outDateDataControlAction = outDateDataControlAction;
        }
    }
    ///////////////////FILE PATH///////////////////////////
    public enum FolderPath
    {
        None,
        DialogueData,
        GResourceData,
        Resources,

    }///////////////////READER PROTOCOL/////////////////////
    
    public static class FolderPathSP
    {
        public static Dictionary<FolderPath, string> FolderPaths = new()
        {
            { FolderPath.None, "" }, // for nexus
            { FolderPath.DialogueData, "DialogueData" }, // for conversation data
            { FolderPath.GResourceData, "GResourceData" }, // for game data
            { FolderPath.Resources, "Resources" }, // for android
        };
        public static string GetFolderPath(FolderPath folderPath)
        {
            if(folderPath == FolderPath.None)
            {
                return "";
            }
            else
            {
                return FolderPaths[folderPath] + Path.DirectorySeparatorChar;
            }

        }
    }
}
