using ET.FileSystem;
using ET.SupportKit.Collection;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET.FileSystem.FileReader
{
    [Serializable]
    public struct DataLoadingProtocol //pathType/fileFolder/fileName.type(defined by protocol)
    {
        public string loadingProtocolID;
        public string dataFileID;
        [Tooltip("/dataPathLocation/folderPath")] 
        public DataPathLocation dataPathLocation;
        [Tooltip("/dataPathLocation/folderPath")] 
        public FolderPath folderPath;
        [Tooltip("How to read the file, read document guide for more detail")] 
        public FileReaderID fileReaderID;
        public DataExtractorFuncID dataExtractorFuncID;
        [NonSerialized] public IFileReader fileReader;
        [NonSerialized] public Func<int, List<string>, DictionaryItem> dataExtractorFunc;
        
        public DataLoadingProtocol(
            string loadingProtocolID,
            string dataFileID,
            DataPathLocation dataPathLocation,
            FolderPath folderPath,
            FileReaderID fileReaderID,
            IFileReader fileReader,
            DataExtractorFuncID dataExtractorFuncID,
            Func<int, List<string>, DictionaryItem> dataExtractorFunc)
        {
            this.loadingProtocolID = loadingProtocolID;
            this.dataFileID = dataFileID;
            this.dataPathLocation = dataPathLocation;
            this.folderPath = folderPath;
            this.fileReaderID = fileReaderID;
            this.fileReader = fileReader;
            this.dataExtractorFuncID = dataExtractorFuncID;
            this.dataExtractorFunc = dataExtractorFunc;
        }

        public DataLoadingProtocol(
            string loadingProtocolID,
            string dataFileID,
            DataPathLocation dataPathLocation,
            FolderPath folderPath,
            FileReaderID fileReaderID,
            DataExtractorFuncID dataExtractorFuncID)
            : this(
                  loadingProtocolID,
                  dataFileID,
                  dataPathLocation,
                  folderPath,
                  fileReaderID,
                  null,
                  dataExtractorFuncID,
                  null
                  )
        {
        }

        public DataLoadingProtocol(
            string loadingProtocolID,
            string dataFileID,
            DataPathLocation dataPathLocation,
            FolderPath folderPath,
            FileReaderID fileReaderID,
            Func<int, List<string>, DictionaryItem> dataExtractorFunc)
            : this(
                  loadingProtocolID,
                  dataFileID,
                  dataPathLocation,
                  folderPath,
                  fileReaderID,
                  null,
                  default,
                  dataExtractorFunc
                  )
        {
        }
    }
}