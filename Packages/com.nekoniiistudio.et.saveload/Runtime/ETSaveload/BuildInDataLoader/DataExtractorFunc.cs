using ET.SupportKit.Collection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET.FileSystem.FileReader
{
    public static class DataExtractorFunc
    {
        public static DictionaryItem KeyData(int i, List<string> list_column) // if 1 row has 12 cells, each cell is a column
        {
            DictionaryItem dictionaryItem = new DictionaryItem(list_column[0].Trim(), list_column[1].Trim());
            return dictionaryItem;
        }
        public static DictionaryItem IData(int i, List<string> list_column) // if 1 row has 12 cells, each cell is a column
        {
            for (int j = 0; j < list_column.Count; j++)
            {
                list_column[j] = list_column[j].Trim();
            }
            DictionaryItem dictionaryItem = new DictionaryItem(i.ToString(), list_column);
            return dictionaryItem;
        }
    }
    public static class DataExtractorConverterFunc
    { 
        public static Func<object, string> String = x => (string)x;
        public static Func<object, List<string>> ListString = x => (List<string>)x;
    }

}