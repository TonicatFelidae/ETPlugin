using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using ET.SupportKit.Collection;
using System;
using ET.FileSystem;
using ET.Extension;

namespace ET.FileSystem.FileReader
{
    public class Txt_TextLines_Comma: IFileReader
    {
        public Dictionary<string, object> ReadFromResource(string path, Func<int, List<string>, DictionaryItem> dictionaryItemDataExtractor, ReadType readType)
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();

            string textAsset = "";
            switch (readType)
            {
                case ReadType.FileRead:
                    textAsset = File.ReadAllText(path);
                    break;
                case ReadType.ResourcesRead:
                    var resourceLoad = Resources.Load<TextAsset>(ETFile.CleanFilePath(path));
                    textAsset = resourceLoad.text;
                    break;
                default:
                    textAsset = File.ReadAllText(path);
                    break;
            }
            string[] lines = textAsset.Split("\n"[0]);
            for (var i = 1; i < lines.Length; i++)
            {
                if (!string.IsNullOrEmpty(lines[i]))
                {
                    List<string> columns = lines[i].Split(",").ToList();
                    DictionaryItem item = dictionaryItemDataExtractor(i, columns);
                    ret.Add((string)item.key, item.value);
                }
            }
            return ret;
        }
    }
}
