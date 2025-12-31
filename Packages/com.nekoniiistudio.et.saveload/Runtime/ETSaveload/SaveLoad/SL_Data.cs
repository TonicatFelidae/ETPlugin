using System;

namespace ET.Saveload
{
    //niii note: Implement to K
    //SAVE series class : SAVE File
    [Serializable]
    public abstract class SAVE_File
    {
        public VersionControl fileVersionControl;

        public abstract SAVE_MetaData metaData { get; set; }
        public virtual void ResetAll()
        {

        }
        public void ApplyVersionControl(int curVersion)
        {
            fileVersionControl.currentDataVersion = curVersion;
        }

    }
    [Serializable]
    public class SAVE_MetaData
    {
        public string filename;
        public string FileName { get => string.IsNullOrEmpty(filename)? DEFAULT_FILE_NAME : filename; set => filename = value; }
        public long savedTime; // Store Unix timestamp
        public string saveFileName;
        public SaveFileMarker fileMarker;

        public const string DEFAULT_FILE_NAME = "Save File";
        public const string QUICK_SAVE = "QuickSave";
        public const string NEW_SAVE = "NewSave";

        public bool IsNewSave => fileMarker == SaveFileMarker.NewSave;
        public bool IsQuickSave => fileMarker == SaveFileMarker.QuickSave;
        public string SaveFileName
        {
            get
            {
                if (string.IsNullOrEmpty(saveFileName)) // over write all
                {
                    saveFileName = FileName + savedTime;
                }
                return saveFileName;
            }
        }
        public void SetSaveTime(long savedTime)
        {
            this.savedTime = savedTime;
        }
        public void SetFileMarker(SaveFileMarker saveFileMarker)
        {
            fileMarker = saveFileMarker;
            switch (fileMarker)
            {
                case SaveFileMarker.None:
                    break;
                case SaveFileMarker.QuickSave:
                    saveFileName = QUICK_SAVE;
                    break;
                case SaveFileMarker.NewSave:
                    saveFileName = NEW_SAVE;
                    break;
                default:
                    break;
            }
        }
        public void ResetFileMarker()
        {
            fileMarker = SaveFileMarker.None;
            if (saveFileName == QUICK_SAVE || saveFileName == NEW_SAVE) saveFileName = null;
        }
        public SAVE_MetaData DeepCopy()
        {
            return new SAVE_MetaData
            {
                filename = this.filename,
                savedTime = this.savedTime,
                saveFileName = this.saveFileName,
                fileMarker = this.fileMarker// Ensures deep copy
            };
        }

    }
    public enum SaveFileMarker
    {
        None,
        QuickSave,
        NewSave
    }
    [Serializable]
    public class SAVELOAD_Manager_Item
    {
        public string path;
        public int survivedTime;
        public int totalSurvivor;
        //
        public int fileDataVersion;
    }
    [Serializable]
    public struct VersionControl
    {
        public int currentDataVersion;
    }


}



