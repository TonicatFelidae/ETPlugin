using ET.FileSystem;
using ET.SupportKit.Collection;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET.FileSystem.FileReader
{
    public class DataExtractor
    {
        /// <summary>
        /// Read document
        /// </summary>
        /// <param name="fileReader"></param>
        /// <param name="dataExtractionFunc"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public Dictionary<string, object> ExtractData(
            IFileReader fileReader, 
            Func<int, List<string>, DictionaryItem> dataExtractionFunc, 
            string path,
            ReadType readType
            )
        {
            Dictionary<string, object> loadedData = new();
            if (readType == ReadType.ResourcesRead || ETFile.CheckFileExist(path))
            {
                loadedData = fileReader.ReadFromResource(path, dataExtractionFunc,readType);
            }
            this.Log($"Extract {path} got {loadedData.Count} items");
            return loadedData;
        }
    }
}
