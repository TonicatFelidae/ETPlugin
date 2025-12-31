using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ET.FileSystem;
using ET.SupportKit.Collection;
using System.Linq;
using ET.FileSystem.FileReader;

namespace ET.FileSystem.FileReader
{
    /// <summary>
    /// FileID is not FileName
    /// </summary>
    public class FileReaderManager : MonoBehaviour
    {
        public DataExtractor _dataExtractor = new();
        [SerializeField] private DataPathLocation _nexusFileLocation = DataPathLocation.StreamingAssetsData; 
        [Header("File Reader Protocols")]
        [SerializeField] private DataLoadingProtocol[] loadProtocols;
        private Dictionary<string, DataLoadingProtocol> _loadProtocols;
        public Dictionary<string, DataLoadingProtocol> LoadProtocols
        {
            get
            { 
                if (_loadProtocols == null)
                {
                    _loadProtocols = new();
                    _loadProtocols = loadProtocols.ToDictionaryFromList(x => x.loadingProtocolID);
                }
                return _loadProtocols;
            }
            set
            {
                _loadProtocols = value; 
            }
        }
        private Dictionary<string, string> _nexusLinks = new();
        public Dictionary<string, string> NexusLinks
        {
            get
            {
                if (_nexusLinks.Count == 0)
                {
                    LoadFileNexus();
                }
                return _nexusLinks;
            }
        }

        private void Awake()
        {
            NexusLinks.FirstOrDefault();
        }
        public void RefreshNexusLinks()
        {
            LoadFileNexus();
        }
        /// <summary>
        /// Install and will force override if already exist
        /// </summary>
        /// <param name="readProtocol"></param>
        /// <param name="dataLoadingProtocol"></param>
        public void AddDataLoadingProtocol(DataLoadingProtocol dataLoadingProtocol)
        {
            if (LoadProtocols.ContainsKey(dataLoadingProtocol.loadingProtocolID))
            {
                LoadProtocols[dataLoadingProtocol.loadingProtocolID] = dataLoadingProtocol;  
            }
            else
            {
                LoadProtocols.Add(dataLoadingProtocol.loadingProtocolID, dataLoadingProtocol);
            }
        }
        private void LoadFileNexus()
        {
            DataLoadingProtocol nexusLaodingProtocol = new DataLoadingProtocol(
                "FileNexus",
                "FileNexus",
                _nexusFileLocation,
                FolderPath.None,
                FileReaderID.Txt_TextLines_Colon,
                DataExtractorFuncID.KeyData);
            string path = FolderPathSP.GetFolderPath(nexusLaodingProtocol.folderPath) + "FileNexus.txt";
            string writealblePath = ETFile.GetWritablePath(path, nexusLaodingProtocol.dataPathLocation);
            if (!ETFile.CheckFileExist(writealblePath) && _nexusFileLocation != DataPathLocation.Resources) return;
            _nexusLinks = _dataExtractor.ExtractData(new Txt_TextLines_Colon(), DataExtractorFunc.KeyData, writealblePath,
                _nexusFileLocation == DataPathLocation.Resources ? ReadType.ResourcesRead : ReadType.FileRead)
                .ToDictionary_Dynamic(DataExtractorConverterFunc.String);
        }

        public List<Dictionary<string, object>> GetData(string loadingProtocolID)
            => GetData(loadingProtocolID, -1, -1);
        public List<Dictionary<string, object>> GetData(string loadingProtocolID, int atIndex)
            => GetData(loadingProtocolID, atIndex, -1);
        public List<Dictionary<string, object>> GetData(string loadingProtocolID, int fromIndexIncluded,int toIndexExcluded, string optionalFileID = "")
        {
            List<Dictionary<string, object>> ret = new();
            DataLoadingProtocol dataLoadingProtocol = LoadProtocols[loadingProtocolID];
            string fileID = optionalFileID;
            if (fileID == "") fileID = dataLoadingProtocol.dataFileID;
            if (fromIndexIncluded == toIndexExcluded && fromIndexIncluded == -1) // use fileID as key
            {
                var gotData = LoadFile(fileID, dataLoadingProtocol);
                if (gotData != null) ret.Add(gotData);
            }
            else if (fromIndexIncluded != -1 && toIndexExcluded == -1) // use fileID + fromIndex as key
            {
                var gotData = LoadFile(fileID + fromIndexIncluded, dataLoadingProtocol);
                if (gotData != null) ret.Add(gotData);
            }
            else // use multi keys of fileID as keys
            {
                for (int i = fromIndexIncluded; i < toIndexExcluded; i++)
                {
                    string key = fileID + "_" + i;
                    var gotData = LoadFile(key, dataLoadingProtocol);
                    if (gotData != null) ret.Add(gotData);
                }
            }
            return ret;
        }
        public Dictionary<string, object> LoadFile(string fileID, string loadingProtocolID)
        => LoadFile(fileID, LoadProtocols[loadingProtocolID]);
        public Dictionary<string, object> LoadFile(string fileID, DataLoadingProtocol dataLoadingProtocol)
        {
            if (!NexusLinks.ContainsKey(fileID)) return null;
            string path = FolderPathSP.GetFolderPath(dataLoadingProtocol.folderPath) + NexusLinks[fileID];
            string writealblePath = ETFile.GetWritablePath(path, dataLoadingProtocol.dataPathLocation);
            D.Log($"Try read file at: {writealblePath}");
            bool isResourcesFile = dataLoadingProtocol.dataPathLocation == DataPathLocation.Resources;
            if (!isResourcesFile && !ETFile.CheckFileExist(writealblePath)) return null;
            if (dataLoadingProtocol.fileReader == null) dataLoadingProtocol.fileReader = FileReaderSP.GetFileReader(dataLoadingProtocol.fileReaderID);
            if (dataLoadingProtocol.dataExtractorFunc == null) dataLoadingProtocol.dataExtractorFunc = FileReaderSP.GetDataExtractorFunc(dataLoadingProtocol.dataExtractorFuncID);
            if (dataLoadingProtocol.fileReader == null) D.LogError($"FileReader {dataLoadingProtocol.fileReaderID} not found");
            if (dataLoadingProtocol.dataExtractorFunc == null) D.LogError($"DataExtractorFunc {dataLoadingProtocol.dataExtractorFuncID} not found");
            if (isResourcesFile) // for load from resource folder
            {
                return _dataExtractor.ExtractData(
                    dataLoadingProtocol.fileReader,
                    dataLoadingProtocol.dataExtractorFunc,
                    writealblePath,
                    ReadType.ResourcesRead
                    );
            }
            else
            {
                return _dataExtractor.ExtractData(
                    dataLoadingProtocol.fileReader,
                    dataLoadingProtocol.dataExtractorFunc,
                    writealblePath,
                    ReadType.FileRead
                    );
            }
        }
    }
}
