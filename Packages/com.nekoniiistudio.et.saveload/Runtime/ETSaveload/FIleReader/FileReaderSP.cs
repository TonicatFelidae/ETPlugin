using ET.SupportKit.Collection;
using System;
using System.Collections.Generic;

namespace ET.FileSystem.FileReader
{
    public enum FileReaderID
    {
        Txt_TextLines_Comma,
        Txt_TextLines_Colon,
        Csv_TextLines_Comma,
    }
    public enum DataExtractorFuncID
    {
        KeyData,
        IData,
    }
    public static class FileReaderSP
    {
        private static Dictionary<FileReaderID, IFileReader> _fileReader = new();
        public static IFileReader GetFileReader(FileReaderID fileReaderID)
        {
            if (!_fileReader.ContainsKey(fileReaderID))
            {
                _fileReader.Add(fileReaderID, CreateFileReaderInstance(fileReaderID));
            }
            return _fileReader[fileReaderID];
        }
        private static IFileReader CreateFileReaderInstance(FileReaderID fileReaderID)
        {
            switch (fileReaderID)
            {
                case FileReaderID.Txt_TextLines_Comma:
                    return new Txt_TextLines_Comma();
                case FileReaderID.Txt_TextLines_Colon:
                    return new Txt_TextLines_Colon();
                case FileReaderID.Csv_TextLines_Comma:
                    return new Csv_TextLines_Comma();
                default:
                    return default;
            }
        }

        public static Func<int, List<string>, DictionaryItem> GetDataExtractorFunc(DataExtractorFuncID dataExtractorFuncID)
        {
            switch (dataExtractorFuncID)
            {
                case DataExtractorFuncID.KeyData:
                    return DataExtractorFunc.KeyData;
                case DataExtractorFuncID.IData:
                    return DataExtractorFunc.IData;
                default:
                    return default;
            }
            
        }
    }

}