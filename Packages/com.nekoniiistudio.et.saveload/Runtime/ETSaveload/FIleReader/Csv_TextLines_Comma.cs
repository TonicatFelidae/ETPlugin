using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ET.SupportKit.Collection;
using UnityEngine;

namespace ET.FileSystem.FileReader
{
    public class Csv_TextLines_Comma : IFileReader
    {
        public delegate object ReadLineData(List<string> data);
        // Start is called before the first frame update

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
            string text = textAsset.Replace("\r\n", "\n").Replace("\"\"", "[_quote_]");
            var matches = Regex.Matches(text, "\"[\\s\\S]+?\""); // perfect line seperator
            foreach (Match match in matches)
            {
                text = text.Replace(match.Value, match.Value.Replace("\"", null).Replace(",", "[_comma_]").Replace("\n", "[_newline_]"));
            }

            // Making uGUI line breaks to work in asian texts.
            text = text.Replace("。", "。 ").Replace("、", "、 ").Replace("：", "： ").Replace("！", "！ ").Replace("（", " （").Replace("）", "） ").Trim();

            var lines = text.Split('\n').Where(i => i != "").ToList();
            for (var i = 1; i < lines.Count; i++)
            {
                List<string> columns = lines[i].Split(',').Select(j => j.Trim()).Select(j => j.Replace("[_quote_]", "\"").Replace("[_comma_]", ",").Replace("[_newline_]", "\n")).ToList();
                DictionaryItem item = new DictionaryItem();
                item = dictionaryItemDataExtractor(i,columns);
                ret.Add((string)item.key, item.value);
            }
            //stream file asset setting and constructure varial by project 
            return ret;
        }
    }
}

