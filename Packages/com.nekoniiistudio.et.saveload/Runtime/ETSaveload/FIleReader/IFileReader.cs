using Cysharp.Threading.Tasks;
using ET.SupportKit.Collection;
using System.Collections.Generic;
using System;

namespace ET.FileSystem.FileReader
{
    public interface IFileReader
    {
        public Dictionary<string, object> ReadFromResource(
            string path, 
            Func<int, List<string>, 
            DictionaryItem> dictionaryItemDataExtractor,
            ReadType readType
            );
    }
    public enum ReadType
    {
        FileRead,
        ResourcesRead,
    }
}