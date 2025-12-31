using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using ET.Module.Dialogue;
using ET.SupportKit.Collection;
using ET.FileSystem;
using System.Linq;
using ET.FileSystem.FileReader;
using System.Text.RegularExpressions;
using UnityEngine.Windows;
using DG.Tweening.Plugins.Core.PathCore;

namespace ET.Module.Dialogue
{
    public static class DataExtractorFunc_Dialogue
    {
        public static DictionaryItem KeyConversation(int i, List<string> list_column) // if 1 row has 12 cells, each cell is a column
        {
            // key||nara||description||content
            // content{
            // lily: hehehhe
            // mia: dont touch my penis!!!}

            DictionaryItem dictionaryItem = new DictionaryItem();
            //[0]
            dictionaryItem.key = list_column[0];
            //[3]
            string[] dialoguelines = list_column[3].Split("\n", list_column[3].Count());
            List<Dialogue> dialogues = new();
            for (int j = 0; j < dialoguelines.Length; j++)
            {
                dialogues.Add(ReadDialogue(dialoguelines[j]));
            }
            //
            Conversation conversationData = new Conversation();
            conversationData.ID = list_column[0];
            conversationData.dialogues = dialogues;
            dictionaryItem.value = conversationData;
            return dictionaryItem;
        }
        static Dialogue ReadDialogue(string input)
        {
            List<string> dats = new List<string>();
            if (input.Contains(":"))
            {
                string pattern = @"(\w+)\[(\w+)]:\s(.+)";
                MatchCollection matches = Regex.Matches(input, pattern);

                foreach (Match match in matches)
                {
                    if (match.Groups.Count == 4)
                    {
                        dats.Add(match.Groups[1].Value);
                        dats.Add(match.Groups[2].Value);
                        dats.Add(match.Groups[3].Value);
                    }
                }
            }
            else
            {
                dats.Add("");
                dats.Add("");
                dats.Add(input);
            }
            Dialogue ret = new Dialogue();
            try
            {
                ret = new Dialogue(dats[0], dats[1], dats[2]);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"ReadDialogue error at line: >>|{input}|<<");
            }
            return ret;


        }
    }
    public static class DataExtractorConverterFunc_Dialogue
    {
        public static Func<object, Conversation> objConversation = data => (Conversation)data;
    }
}
